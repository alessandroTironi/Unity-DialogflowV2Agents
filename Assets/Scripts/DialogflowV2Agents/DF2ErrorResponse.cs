using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2ErrorResponse
	{
		[JsonProperty]
		public DF2Error error { get; set; }
	}

	[JsonObject]
	public class DF2Error 
	{
		[JsonProperty]
		public long code { get; set; }

		[JsonProperty]
		public string message { get; set; }

		[JsonProperty]
		public string status { get; set; }
	}
}

