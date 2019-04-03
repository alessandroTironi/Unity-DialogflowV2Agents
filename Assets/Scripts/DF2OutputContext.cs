using System.Collections.Generic;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2OutputContext 
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public int LifespanCount { get; set; }

		[JsonProperty]
		public Dictionary<string, string> Parameters { get; set; }
	}
}

