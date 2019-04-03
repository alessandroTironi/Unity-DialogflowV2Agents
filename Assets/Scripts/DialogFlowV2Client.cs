using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Google.Cloud.Dialogflow.V2;
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
		/// The default detectIntent URL where project ID and session ID are missing. 
		/// </summary>
		internal static readonly string PARAMETRIC_URL = 
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
		}

		/// <summary>
		/// Makes a POST request to Dialogflow for detecting an intent from an event.
		/// </summary>
		/// <param name="eventName">The name of the event.</param>
		/// <param name="parameters">The parameters of the event.</param>
		/// <param name="talker">The ID of the entity who is talking to the bot.</param>
		/// <param name="languageCode">The language code of the request.</param>
		public void DetectIntentFromEvent(string eventName, Dictionary<string, string> parameters, 
			string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = this.languageCode;

			DF2QueryInput queryInput = new DF2QueryInput();
			queryInput.Event = new DF2EventInput();
			queryInput.Event.Name = eventName;
			queryInput.Event.Parameters = parameters;
			queryInput.Event.LanguageCode = languageCode;

			StartCoroutine(DetectIntent(new DF2Request(queryInput), "test"));
		}

		/// <summary>
		/// Sends a <see cref="QueryInput"/> object as a HTTP request to the remote
		/// chatbot.
		/// </summary>
		/// <param name="request">The input request.</param>
		/// <param name="session">The session ID, i.e., the ID of the user who talks to the chatbot.</param>
		private IEnumerator DetectIntent(DF2Request request, string session)
		{
			// Gets the JWT access token.
			TextAsset p12File = Resources.Load<TextAsset>("DialogflowV2/smartrpgshop-ea4bb047937c");
			var jwt = GoogleJsonWebToken.GetJwt("smartrpgshopclient@smartrpgshop.iam.gserviceaccount.com",
				p12File.bytes,
				GoogleJsonWebToken.SCOPE_DIALOGFLOWV2);
			UnityWebRequest tokenRequest = GoogleJsonWebToken.GetAccessTokenRequest(jwt);
			yield return tokenRequest.SendWebRequest();
			if (tokenRequest.isNetworkError || tokenRequest.isHttpError)
				Debug.LogError("Error " + tokenRequest.responseCode + ": " + tokenRequest.error);
			string serializedToken = Encoding.UTF8.GetString(tokenRequest.downloadHandler.data);
			var jwtJson = JsonConvert.DeserializeObject<Dictionary<string, object>>(serializedToken);
			Debug.Log(jwtJson["access_token"] + " expires in " + jwtJson["expires_in"]);

			// Prepares the HTTP request.
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			string jsonInput = JsonConvert.SerializeObject(request, settings);
			byte[] body = Encoding.UTF8.GetBytes(jsonInput);

			string url = string.Format(PARAMETRIC_URL, projectId, session);
			UnityWebRequest df2Request = new UnityWebRequest(url, "POST");
			
			df2Request.SetRequestHeader("Authorization", "Bearer " + jwtJson["access_token"]);
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
				ChatbotResponded?.Invoke(JsonConvert.DeserializeObject<DF2Response>(response));
			}
		}
	}
}


