using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2TextInput
	{
		[JsonProperty]
		public string Text { get; set; }

		[JsonProperty]
		public string LanguageCode { get; set; }
	}
}
