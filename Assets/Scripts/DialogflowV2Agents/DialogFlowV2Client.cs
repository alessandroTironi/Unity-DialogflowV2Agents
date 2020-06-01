using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Syrus.Plugins.DFV2Client
{
	[AddComponentMenu("Syrus/Plugins/DialogflowV2Agents/DialogFlowV2Client")]
	public class DialogFlowV2Client : MonoBehaviour 
	{
		/// <summary>
		/// The object that defines the service settings.
		/// </summary>
		public ServiceSettings accessSettings = null;

		/// <summary>
		/// Delegate for handling errors received after a detectIntent request.
		/// </summary>
		/// <param name="error">The error response.</param>
		public delegate void DetectIntentErrorHandler(DF2ErrorResponse error);

		/// <summary>
		/// Event fired at each error received from DetectIntent.
		/// </summary>
		public event DetectIntentErrorHandler DetectIntentError;

		/// <summary>
		/// Delegate for handling responses from the DF2 server.
		/// </summary>
		/// <param name="response">The received response.</param>
		public delegate void ServerResponseHandler(DF2Response response);

		/// <summary>
		/// Event fired at each response from the chatbot.
		/// </summary>
		public event ServerResponseHandler ChatbotResponded;

		/// <summary>
		/// Delegate for handling session clear.
		/// </summary>
		/// <param name="session">The session ID.</param>
		public delegate void SessionClearedHanlder(string session);

		/// <summary>
		/// Event fired whenever a session is cleared.
		/// </summary>
		public event SessionClearedHanlder SessionCleared;

		/// <summary>
		/// A delegate for handling output contexts.
		/// </summary>
		/// <param name="outContext">The output context the client must react to.</param>
		public delegate void OutputContextHandler(DF2Context outContext);

		/// <summary>
		/// The set of <see cref="DF2Context"/> the client must react to.
		/// </summary>
		internal Dictionary<string, OutputContextHandler> reactionContexts =
			new Dictionary<string, OutputContextHandler>();

		/// <summary>
		/// The list of input contexts to send in the next request.
		/// </summary>
		private List<DF2Context> inputContexts = new List<DF2Context>();

		/// <summary>
		/// The list of entities to send in the next request.
		/// </summary>
		private List<DF2EntityType> inputEntities = new List<DF2EntityType>();

		/// <summary>
		/// The default detectIntent URL where project ID and session ID are missing. 
		/// </summary>
		internal static readonly string PARAMETRIC_DETECT_INTENT_URL = 
			"https://dialogflow.googleapis.com/v2/projects/{0}/agent/sessions/{1}:detectIntent";

		/// <summary>
		/// Makes a POST request to Dialogflow for detecting an intent from text.
		/// </summary>
		/// <param name="text">The input text.</param>
		/// <param name="talker">The ID of the entity who is talking to the bot.</param>
		/// <param name="languageCode">The language code of the request.</param>
		public void DetectIntentFromText(string text, string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = accessSettings.LanguageCode;

			DF2QueryInput queryInput = new DF2QueryInput();
			queryInput.Text = new DF2TextInput();
			queryInput.Text.Text = text;
			queryInput.Text.LanguageCode = languageCode;

			StartCoroutine(DetectIntent(queryInput, talker));
		}
		
		//@hoatong
		/// <summary>
		/// Makes a POST request to Dialogflow for detecting an intent from text.
		/// </summary>
		/// <param name="talker">The ID of the entity who is talking to the bot.</param>
		/// <param name="languageCode">The language code of the request.</param>
		public void DetectIntentFromAudio(string audio64, string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = accessSettings.LanguageCode;

			DF2QueryInput queryInput = new DF2QueryInput();

			queryInput.AudioConfig = new DF2AudioConfig();
			
			queryInput.AudioConfig.LanguageCode = languageCode;
			
			StartCoroutine(DetectIntent(queryInput, talker, audio64));
		}

		/// <summary>
		/// Makes a POST request to Dialogflow for detecting an intent from an event.
		/// </summary>
		/// <param name="eventName">The name of the event.</param>
		/// <param name="parameters">The parameters of the event.</param>
		/// <param name="talker">The ID of the entity who is talking to the bot.</param>
		/// <param name="languageCode">The language code of the request.</param>
		public void DetectIntentFromEvent(string eventName, Dictionary<string, object> parameters, 
			string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = accessSettings.LanguageCode;

			DF2QueryInput queryInput = new DF2QueryInput();
			queryInput.Event = new DF2EventInput();
			queryInput.Event.Name = eventName;
			queryInput.Event.Parameters = parameters;
			queryInput.Event.LanguageCode = languageCode;

			StartCoroutine(DetectIntent(queryInput, talker));
		}

		/// <summary>
		/// Detects an intent from a user-built <see cref="DF2QueryInput"/>.
		/// </summary>
		/// <param name="input">The user-built <see cref="DF2QueryInput"/>.</param>
		/// <param name="talker">The user who is talking to the chatbot.</param>
		/// <param name="languageCode">The language code of the request.</param>
		public void DetectIntentFromGenericInput(DF2QueryInput input, string talker,
			string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = accessSettings.LanguageCode;

			StartCoroutine(DetectIntent(input, talker));
		}

		/// <summary>
		/// Sends a <see cref="DF2QueryInput"/> object as a HTTP request to the remote
		/// chatbot.
		/// </summary>
		/// <param name="queryInput">The input request.</param>
		/// <param name="session">The session ID, i.e., the ID of the user who talks to the chatbot.</param>
		private IEnumerator DetectIntent(DF2QueryInput queryInput, string session, string audio ="")
		{
			// Gets the JWT access token.
			string accessToken = string.Empty;
			while (!JwtCache.TryGetToken(accessSettings.ServiceAccount, out accessToken))
				yield return JwtCache.GetToken(accessSettings.CredentialsFileName,
					accessSettings.ServiceAccount);

			// Prepares the HTTP request.
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			DF2Request request = new DF2Request(session, queryInput);

			// Adds the input contexts and the entities.
			request.QueryParams = new DF2QueryParams();
			request.QueryParams.Contexts = inputContexts.ToArray();
			inputContexts.Clear();
			request.QueryParams.SessionEntityTypes = inputEntities.ToArray();
			inputEntities.Clear();
			
			//@hoatong
			// Add audio string
			
			request.OutputAudioConfig = new DF2OutputAudioConfig();
			request.OutputAudioConfig.AudioEncoding = "OUTPUT_AUDIO_ENCODING_LINEAR_16";

			request.InputAudio = audio;

			string jsonInput = JsonConvert.SerializeObject(request, settings);
			
			Debug.Log("Json: "+ jsonInput);
			
			byte[] body = Encoding.UTF8.GetBytes(jsonInput);

			string url = string.Format(PARAMETRIC_DETECT_INTENT_URL, accessSettings.ProjectId, session);
			UnityWebRequest df2Request = new UnityWebRequest(url, "POST");		
			df2Request.SetRequestHeader("Authorization", "Bearer " + accessToken);
			df2Request.SetRequestHeader("Content-Type", "application/json");
			df2Request.uploadHandler = new UploadHandlerRaw(body);
			df2Request.downloadHandler = new DownloadHandlerBuffer();

			yield return df2Request.SendWebRequest();

			// Processes response.
			if (df2Request.isNetworkError || df2Request.isHttpError)
				DetectIntentError?.Invoke(JsonConvert.DeserializeObject<DF2ErrorResponse>(df2Request.downloadHandler.text));
			else
			{
				string response = Encoding.UTF8.GetString(df2Request.downloadHandler.data);
				DF2Response resp = JsonConvert.DeserializeObject<DF2Response>(response);
				ChatbotResponded?.Invoke(resp);
				if(resp.queryResult.outputContexts !=null)
				{
					foreach (var context in resp.queryResult.outputContexts)
					{
						string[] cName = context.Name.ToLower().Split('/');
						if (reactionContexts.ContainsKey(cName[cName.Length - 1]))
							reactionContexts[cName[cName.Length - 1]](context);
					}
				}
			}
		}

		/// <summary>
		/// Sets the client for reacting to the specified context.
		/// </summary>
		/// <param name="contextName">The context name (last segment).</param>
		/// <param name="handler">What the client must do after the context detection.</param>
		public void ReactToContext(string contextName, OutputContextHandler handler)
		{
			reactionContexts[contextName.ToLower()] = handler;
		}

		/// <summary>
		/// Stops the client from reacting to the specified context.
		/// </summary>
		/// <param name="contextName">The context the client must stop reacting to.</param>
		public void StopReactingToContext(string contextName)
		{
			reactionContexts.Remove(contextName);
		}

		/// <summary>
		/// Adds an input context to the next request.
		/// </summary>
		/// <param name="inputContext">The input context to add.</param>
		/// <param name="session">The current session ID.</param>
		public void AddInputContext(DF2Context inputContext, string session)
		{
			if (!inputContext.Name.StartsWith("projects"))
				inputContext.Name = string.Format(DF2Context.PARAMETRIC_CONTEXT_ID,
					accessSettings.ProjectId, session, inputContext.Name);
			inputContexts.Add(inputContext);
		}


		public void AddEntityType(DF2EntityType entityType, string session)
		{
			if (!entityType.Name.StartsWith("projects"))
				entityType.Name = string.Format(DF2EntityType.PARAMETRIC_ENTITY_TYPE_NAME,
					accessSettings.ProjectId, session, entityType.Name);
			inputEntities.Add(entityType);
		}

		/// <summary>
		/// Resets the specified session.
		/// </summary>
		/// <param name="session">The name of the session to reset.</param>
		public void ClearSession(string session)
		{
			StartCoroutine(ClearSessionRequest(session));
		}

		/// <summary>
		/// Resets the specified session.
		/// </summary>
		/// <param name="session">The name of the session to reset.</param>
		private IEnumerator ClearSessionRequest(string session)
		{
			// Gets the JWT access token.
			string accessToken = string.Empty;
			while (!JwtCache.TryGetToken(accessSettings.ServiceAccount, out accessToken))
				yield return JwtCache.GetToken(accessSettings.CredentialsFileName,
					accessSettings.ServiceAccount);

			string url = string.Format(
				"https://dialogflow.googleapis.com/v2/projects/{0}/agent/sessions/{1}/contexts",
				accessSettings.ProjectId, session);
			UnityWebRequest deleteRequest = new UnityWebRequest(url);
			deleteRequest.method = "DELETE";
			deleteRequest.SetRequestHeader("Authorization", "Bearer " + accessToken);
			deleteRequest.SetRequestHeader("Content-Type", "application/json");
			yield return deleteRequest.SendWebRequest();
			if (deleteRequest.isHttpError || deleteRequest.isNetworkError)
				Debug.LogError(deleteRequest.responseCode + ": " + deleteRequest.error);
			else
				SessionCleared?.Invoke(session);
		}
	}
}


