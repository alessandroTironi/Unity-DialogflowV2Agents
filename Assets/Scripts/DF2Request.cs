using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Request
	{
		[JsonProperty]
		public DF2QueryInput QueryInput { get; set; }

		public DF2Request(DF2QueryInput queryInput)
		{
			QueryInput = queryInput;
		}
	}
}
