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
		internal string PROJECT_ID;

		/// <summary>
		/// The JSON Web Token (JWT) used for authentication.
		/// </summary>
		[SerializeField]
		internal string ACCESS_TOKEN;

		/// <summary>
		/// The language used for the chatbot.
		/// </summary>
		[SerializeField]
		internal string LANGUAGE_CODE;

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
		public delegate void ServerResponseHandler(QueryResult response);

		/// <summary>
		/// Event fired at each response from the chatbot.
		/// </summary>
		public event ServerResponseHandler ChatbotResponded;

		/// <summary>
		/// The default detectIntent URL where project ID and session ID are missing. 
		/// </summary>
		internal static readonly string PARAMETRIC_URL = 
			"https://dialogflow.googleapis.com/v2/projects/{0}/agent/sessions/{1}:detectIntent";

		public void DetectIntentFromText(string text, string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = LANGUAGE_CODE;

			DF2QueryInput queryInput = new DF2QueryInput();
			queryInput.Text = new DF2TextInput();
			queryInput.Text.Text = text;
			queryInput.Text.LanguageCode = languageCode;
		}


		public void DetectIntentFromEvent(string eventName, Dictionary<string, string> parameters, 
			string talker, string languageCode = "")
		{
			if (languageCode.Length == 0)
				languageCode = LANGUAGE_CODE;

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
			// Prepares the HTTP request.
			var settings = new JsonSerializerSettings();
			settings.NullValueHandling = NullValueHandling.Ignore;
			settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			string jsonInput = JsonConvert.SerializeObject(request, settings);
			byte[] body = Encoding.UTF8.GetBytes(jsonInput);

			string url = string.Format(PARAMETRIC_URL, PROJECT_ID, session);
			UnityWebRequest df2Request = new UnityWebRequest(url, "POST");
			df2Request.SetRequestHeader("Authorization", "Bearer " + ACCESS_TOKEN);
			df2Request.SetRequestHeader("Content-Type", "application/json");
			df2Request.uploadHandler = new UploadHandlerRaw(body);
			df2Request.downloadHandler = new DownloadHandlerBuffer();

			yield return df2Request.SendWebRequest();

			// Processes response.
			if (df2Request.isNetworkError || df2Request.isHttpError)
				DetectIntentError?.Invoke(df2Request.responseCode, df2Request.error);
			else
			{
				string result = Encoding.UTF8.GetString(df2Request.downloadHandler.data);
				ChatbotResponded?.Invoke(JsonConvert.DeserializeObject<QueryResult>(result));
			}
		}
	}
}


