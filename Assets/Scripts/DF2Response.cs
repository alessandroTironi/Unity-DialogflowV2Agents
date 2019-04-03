using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Google.Cloud.Dialogflow.V2;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Response
	{
		[JsonProperty]
		public string responseId { get; set; }

		[JsonProperty]
		public QueryResult queryResult { get; set; }
	}
}

