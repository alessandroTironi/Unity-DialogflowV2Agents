using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Entity 
	{
		[JsonProperty]
		public string Value { get; set; }

		[JsonProperty]
		public string[] Synonyms { get; set; }

		public DF2Entity(string value, params string[] synonyms)
		{
			Value = value;
			Synonyms = synonyms;
		}
	}
}
