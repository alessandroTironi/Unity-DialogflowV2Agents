using Newtonsoft.Json;

//@hoatong
namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2OutputAudioConfig
	{
		[JsonProperty]
		public string AudioEncoding { get; set; }
	}
}
