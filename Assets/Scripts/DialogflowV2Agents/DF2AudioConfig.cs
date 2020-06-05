using Newtonsoft.Json;

//@hoatong
namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2AudioConfig
	{
		[JsonProperty]
		public string LanguageCode { get; set; }
	}
}
