using System.Collections.Generic;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2QueryResult
	{
		[JsonProperty]
		public string queryText { get; set; }

		[JsonProperty]
		public string action { get; set; }

		[JsonProperty]
		public Dictionary<string, object> parameters { get; set; }

		[JsonProperty]
		public bool allRequiredParamsPresent { get; set; }

		[JsonProperty]
		public string fulfillmentText { get; set; }

		[JsonProperty]
		public Dictionary<string, object>[] fulfillmentMessages { get; set; }

		[JsonProperty]
		public DF2Context[] outputContexts { get; set; }

		[JsonProperty]
		public Dictionary<string, object> intent { get; set; }

		[JsonProperty]
		public float intentDetectionConfidence { get; set; }

		[JsonProperty]
		public string languageCode { get; set; }

	}
}

