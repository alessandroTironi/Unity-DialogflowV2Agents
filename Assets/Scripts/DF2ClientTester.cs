using System.Collections.Generic;
using UnityEngine;
using Syrus.Plugins.DFV2Client;
using Newtonsoft.Json;
using Google.Cloud.Dialogflow.V2;
using Google.Protobuf.WellKnownTypes;

public class DF2ClientTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		DialogFlowV2Client client = GetComponent<DialogFlowV2Client>();

		client.ChatbotResponded += LogResponseText;
		client.DetectIntentError += LogError;

		Dictionary<string, object> parameters = new Dictionary<string, object>()
		{
			{ "name", "George" }
		};
		client.DetectIntentFromEvent("Welcome", parameters, "test");
    }

	private void LogResponseText(DF2Response response)
	{
		Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
		Debug.Log(name + " said: \"" + response.queryResult.queryText + "\"");
	}

	private void LogError(long responseCode, string errorMessage)
	{
		Debug.LogError(string.Format("Error {0}: {1}", responseCode.ToString(), errorMessage));
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
