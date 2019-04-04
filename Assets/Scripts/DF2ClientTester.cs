using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Syrus.Plugins.DFV2Client;
using UnityEngine.UI;

public class DF2ClientTester : MonoBehaviour
{
	public InputField session, content;

	public Text chatbotText;

	private DialogFlowV2Client client;

    // Start is called before the first frame update
    void Start()
    {
		client = GetComponent<DialogFlowV2Client>();

		client.ChatbotResponded += LogResponseText;
		client.DetectIntentError += LogError;
		client.ReactToContext("DefaultWelcomeIntent-followup", 
			context => Debug.Log("Reacting to welcome followup"));
		client.SessionCleared += sess => Debug.Log("Cleared session " + session);
		client.AddInputContext(new DF2Context("userdata", 1, ("name", "George")), name);

		Dictionary<string, object> parameters = new Dictionary<string, object>()
		{
			{ "name", "George" }
		};
		client.DetectIntentFromEvent("test-inputcontexts", parameters, name);

    }

	private void LogResponseText(DF2Response response)
	{
		Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
		Debug.Log(name + " said: \"" + response.queryResult.fulfillmentText + "\"");
		chatbotText.text = response.queryResult.fulfillmentText;
	}

	private void LogError(DF2ErrorResponse errorResponse)
	{
		Debug.LogError(string.Format("Error {0}: {1}", errorResponse.error.code.ToString(), 
			errorResponse.error.message));
	}

    
	public void SendText()
	{
		client.DetectIntentFromText(content.text, session.text);
	}


	public void SendEvent()
	{
		client.DetectIntentFromEvent(content.text,
			new Dictionary<string, object>(), session.text);
	}

	public void Clear()
	{
		client.ClearSession(name);
	}
}
