using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	[JsonObject]
	public class DF2EntityType 
	{
		[JsonProperty]
		public string Name { get; set; }

		[JsonProperty]
		public DF2EntityOverrideMode EntityOverrideMode { get; set; }

		[JsonProperty]
		public DF2Entity[] Entities { get; set; }

		public DF2EntityType(string name, DF2EntityOverrideMode overrideMode, DF2Entity[] entities = null)
		{
			Name = name;
			EntityOverrideMode = overrideMode;
			if (entities != null)
				Entities = entities;
		}

		[System.Serializable]
		public enum DF2EntityOverrideMode
		{
			ENTITY_OVERRIDE_MODE_UNSPECIFIED,
			ENTITY_OVERRIDE_MODE_OVERRIDE,
			ENTITY_OVERRIDE_MODE_SUPPLEMENT
		}

		public const string PARAMETRIC_ENTITY_TYPE_NAME =
			"projects/{0}/agent/sessions/{1}/entityTypes/{2}";

	}
}

