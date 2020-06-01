using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2QueryInput
	{

		public DF2TextInput Text { get; set; }

		public DF2EventInput Event { get; set; }
		
		public DF2AudioConfig AudioConfig { get; set; }
	}
}

