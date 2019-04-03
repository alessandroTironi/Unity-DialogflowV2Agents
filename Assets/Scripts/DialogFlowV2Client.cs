﻿using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Syrus.Plugins.DFV2Client
{
	public class DialogFlowV2Client : MonoBehaviour 
	{
		/// <summary>
		/// The GCP project ID.
		/// </summary>
		[SerializeField]
		private string projectId = string.Empty;

		/// <summary>
		/// The language used for the chatbot.
		/// </summary>
		[SerializeField]
		private string languageCode = string.Empty;

		/// <summary>
		/// Delegate for handling errors received after a detectIntent request.
		/// </summary>
		/// <param name="errorCode">The HTTP response code.</param>
		/// <param name="errorMessage">The HTTP error message.</param>
		public delegate void DetectIntentErrorHandler(long errorCode, string errorMessage);

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
		/// A delegate for handling output contexts.
		/// </summary>
		/// <param name="outContext">The output context the client must react to.</param>
		public delegate void OutputContextHandler(DF2OutputContext outContext);

		/// <summary>
		/// The set of <see cref="DF2OutputContext"/> the client must react to.
		/// </summary>
		internal Dictionary<string, OutputContextHandler> reactionContexts =
			new Dictionary<string, OutputContextHandler>();

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
				languageCode = this.languageCode;

			DF2QueryInput queryInput = new DF2QueryInput();
			queryInput.Text = new DF2TextInput();
			queryInput.Text.Text = text;
			queryInput.Text.LanguageCode = languageCode;

			StartCoroutine(DetectIntent(queryInput, talker));
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
				languageCode = this.languageCode;

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
				languageCode = this.languageCode;

			StartCoroutine(DetectIntent(input, talker));
		}

		/// <summary>
		/// Sends a <see cref="DF2QueryInput"/> object as a HTTP request to the remote
		/// chatbot.
		/// </summary>
		/// <param name="queryInput">The input request.</param>
		/// <param name="session">The session ID, i.e., the ID of the user who talks to the chatbot.</param>
		private IEnumerator DetectIntent(DF2QueryInput queryInput, string session)
		{
			// Gets the JWT access token.
			string accessToken = string.Empty;
			while (!JwtCache.TryGetToken("smartrpgshopclient@smartrpgshop.iam.gserviceaccount.com",
				out accessToken))
				yield return JwtCache.GetToken("smartrpgshop-ea4bb047937c",
					"smartrpgshopclient@smartrpgshop.iam.gserviceaccount.com");

			// Prepares the HTTP request.
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			DF2Request request = new DF2Request(session, queryInput);
			string jsonInput = JsonConvert.SerializeObject(request, settings);
			Debug.Log(jsonInput);
			byte[] body = Encoding.UTF8.GetBytes(jsonInput);

			string url = string.Format(PARAMETRIC_DETECT_INTENT_URL, projectId, session);
			UnityWebRequest df2Request = new UnityWebRequest(url, "POST");		
			df2Request.SetRequestHeader("Authorization", "Bearer " + accessToken);
			df2Request.SetRequestHeader("Content-Type", "application/json");
			df2Request.uploadHandler = new UploadHandlerRaw(body);
			df2Request.downloadHandler = new DownloadHandlerBuffer();

			yield return df2Request.SendWebRequest();

			// Processes response.
			if (df2Request.isNetworkError || df2Request.isHttpError)
				DetectIntentError?.Invoke(df2Request.responseCode, df2Request.error);
			else
			{
				string response = Encoding.UTF8.GetString(df2Request.downloadHandler.data);
				Debug.Log("Received response: " + response);
				DF2Response resp = JsonConvert.DeserializeObject<DF2Response>(response);
				ChatbotResponded?.Invoke(resp);
				for (int i = 0; i < resp.queryResult.outputContexts.Length; i++)
				{
					DF2OutputContext context = resp.queryResult.outputContexts[i];
					string[] cName = context.Name.ToLower().Split('/');
					if (reactionContexts.ContainsKey(cName[cName.Length - 1]))
						reactionContexts[cName[cName.Length - 1]](context);
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
	}
}


