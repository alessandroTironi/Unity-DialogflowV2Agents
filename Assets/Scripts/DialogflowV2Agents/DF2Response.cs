using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Response
	{
		[JsonProperty]
		public string responseId { get; set; }

		[JsonProperty]
		public DF2QueryResult queryResult { get; set; }
		
		[JsonProperty]
		public string OutputAudio { get; set; }
		
		[JsonProperty]
		public DF2OutputAudioConfig OutputAudioConfig { get; set; }
	}
}

