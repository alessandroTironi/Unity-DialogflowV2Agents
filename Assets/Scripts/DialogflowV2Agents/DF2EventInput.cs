using System.Collections.Generic;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2EventInput 
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public Dictionary<string, object> Parameters { get; set; }

		[JsonProperty]
		public string LanguageCode { get; set; }
	}
}

