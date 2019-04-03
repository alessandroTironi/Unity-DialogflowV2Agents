using System.Collections.Generic;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2Context 
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public int LifespanCount { get; set; }

		[JsonProperty]
		public Dictionary<string, object> Parameters { get; set; }

		/// <summary>
		/// A string that must be formatted to obtain a context ID.
		/// </summary>
		public static readonly string PARAMETRIC_CONTEXT_ID =
			"projects/{0}/agent/sessions/{1}/contexts/{2}";

		public DF2Context()
		{

		}

		public DF2Context(string name, int lifespanCount, Dictionary<string, object> parameters)
		{
			Name = name;
			LifespanCount = lifespanCount;
			Parameters = parameters;
		}

		public DF2Context(string name, int lifespanCount, params (string, object)[] parameters)
		{
			Name = name;
			LifespanCount = lifespanCount;
			if (parameters.Length > 0)
			{
				Parameters = new Dictionary<string, object>();
				for (int i = 0; i < parameters.Length; i++)
					Parameters[parameters[i].Item1] = parameters[i].Item2;
			}
			
		}
	}
}

