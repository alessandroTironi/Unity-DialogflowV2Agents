# UnityDialogflowV2Agents

Package for implementing [Dialogflow](https://dialogflow.com/) V2 clients in a Unity project. This package provides a MonoBehaviour script which turns a gameobject into a client for a Dialogflow chatbot that uses the V2 APIs described [here](https://cloud.google.com/dialogflow-enterprise/docs/reference/rest/v2-overview) by Google. Since this library is meant to be used only to implement DFV2 clients, it is only possible to query a chatbot by sending text or an event, with optional additional input contexts and/or entities. For additional functionalities (e.g., creating new intents) I suggest to use the official [SDK](https://developers.google.com/api-client-library/dotnet/apis/dialogflow/v2), or your Dialogflow bot page.

This library has been designed to be lightweight, so we do not include the DFV2 SDK as an additional plugins (which is more than 7MB including all its references), but it exclusively relies on HTTP requests. Thanks to [this post](http://leoncvlt.com/blog/json-web-token-jwt-for-google-cloud-platform-in-unity/) from Leonardo Cavaletti for helping me in understanding the Google JWT authentication process and thanks to [this DFV2 client project](https://unitylist.com/p/i1a/dialogflow-2.0-Unity-client), which enlightened me on the DFV2 HTTP requests format.

### Set up

First, you need to install the library DLL (Syrus.Plugins.DialogflowV2Agents.dll) and the required Newtonsoft.Json.dll in the `Assets/Plugins` folder. Note that, in order to use the Newtonsoft.Json.dll, you need a version of Unity that supports the version 4.X of the .NET framework. 

After having installed the plugin, create a folder named `DialogflowV2` in the `Assets/Resources` directory and place a .p12 private access key file associated to the GCP service account you want to use for the chatbot client. I suggest to create a dedicated service account for your chatbot clients. After having generated a .p12 file and having placed it into the `DialogflowV2` folder, rename it to change its extension from `.p12` to `.bytes`, otherwise Unity Resources Manager will not be able to read it. 

### Detecting intents

Intent detection is the core of the library and is done via the `DialogflowV2Client.cs` script, which you should attach to each gameobject you want to turn into a Dialogflow V2 client. This gameobject will act as a local instance of the chatbot you created on Dialogflow, allowing you to send it inputs (along different sessions) and to report its answers. 

This script requies a service access settings object for specifying the authorization parameters of the chatbot client. To create an access settings object, right-click on the Assets window and select `Create/DialogflowV2/Access Settings`. You will need to provide the GCP project ID associated to your chatbot, the service account and the filename of the .p12 private key file you placed into the `Resources` folder.

Intent detection requests are asynchronous calls whose result is reported by subscribing to two events:
* Subscribe to the `DialogflowV2Client.ChatbotResponded` event for receiving the chatbot's response to your inputs.
* Subscribe to the `DialogflowV2Client.DetectIntentError` event for receiving error messages.

After having subscribed to an appropriate event, you can start querying your chatbot. Intent detection requests can be done via text inputs, event inputs or generic requests entirely made by the user.

```csharp
void Start()
{
    client = GetComponent<DialogFlowV2Client>();

    client.ChatbotResponded += LogResponseText;
    client.DetectIntentError += LogError;

    // Send additional parameters to event
    Dictionary<string, object> parameters = new Dictionary<string, object>()
    {
        { "name", "George" }
    };
    client.DetectIntentFromEvent("event-name", parameters, sessionName);
    client.DetectIntentFromText("Hi bot, can you answer to this?", sessionName);
}

private void LogResponseText(DF2Response response)
{
    Debug.Log(JsonConvert.SerializeObject(response, Formatting.Indented));
    Debug.Log(name + " said: \"" + response.queryResult.fulfillmentText + "\"");
    chatbotText.text = response.queryResult.fulfillmentText;
}

private void LogError(long responseCode, string errorMessage)
{
    Debug.LogError(string.Format("Error {0}: {1}", responseCode.ToString(), errorMessage));
}
```

The `sessionName` is a string that identifies a dialog session and it is used by Dialogflow to store the state of a conversation. 

### Adding input contexts and entities

To provide additional input contexts to the next DFV2 request, you need to invoke the `DialogflowV2Client.AddInputContext(DF2Context context, string sessionName)` and provide a new context that will be sent in the next intent detection request. 

```csharp
client.AddInputContext(new DF2Context("a-context", 1));
client.DetectIntentFromText("Hi bot!", sessionName); // will contain a-context as input context
client.DetectIntentFromText("Hi bot!", sessionName); // will not contain a-context as input context
```

You can also provide additional entities to your request with the method `DialogflowV2Client.AddEntityType(DF2EntityType entityType, string session)`. The entity will be added only to the successive request, as with input contexts.

```csharp
DF2Entity name0 = new DF2Entity("George", "George");
DF2Entity name1 = new DF2Entity("Greg", "Greg");
DF2EntityType names = new DF2EntityType("names", 
    DF2EntityType.DF2EntityOverrideMode.ENTITY_OVERRIDE_MODE_SUPPLEMENT, 
    new DF2Entity[] { name0, name1 }
);
client.AddEntityType(names, sessionName);
client.DetectIntentFromText("My name is George", sessionName);
```

### Reacting to output contexts

You can set your DF client to react to certain output contexts in order to notify you when the chatbot returns a certain output context. You just need to invoke the method `DialogflowV2Client.ReactToContext(string contextName, OutputContextHandler handler)`, where `handler` is a function with the prototype `void HandleOutputContext(DF2Context context)`. 

```csharp
client.ReactToContext("a-context", context => Debug.Log("Chatbot provided a-context!"));
client.AddInputContext(new DF2Context("a-context", 1));
client.DetectIntentFromText("Hi bot!", sessionName); 

client.StopReactingToContext("a-context");
```

### Clearing a session

If you want to clear the state of a given conversation, you just need to specify the session name when calling `DialogflowV2Client.ClearSession(string sessionName)`. You can subscribe to the `DialogflowV2Client.SessionCleared` event to be updated on the session clear success.
