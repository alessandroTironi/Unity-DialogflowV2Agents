using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Request
	{
		[JsonProperty]
		public string Session { get; set; }

		[JsonProperty]
		public DF2QueryInput QueryInput { get; set; }

		[JsonProperty]
		public DF2QueryParams QueryParams { get; set; }
		
		//@hoatong
		[JsonProperty]
		public DF2OutputAudioConfig OutputAudioConfig { get; set; }
		
		[JsonProperty]
		public string InputAudio { get; set; }
		
		public DF2Request(string session, DF2QueryInput queryInput)
		{
			Session = session;
			QueryInput = queryInput;
		}
	}


	[JsonObject]
	public class DF2QueryParams
	{
		[JsonProperty]
		public DF2Context[] Contexts { get; set; }

		[JsonProperty]
		public DF2EntityType[] SessionEntityTypes { get; set; }
	}
}
