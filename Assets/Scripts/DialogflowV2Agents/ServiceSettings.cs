using UnityEngine;

namespace Syrus.Plugins.DFV2Client
{
	[CreateAssetMenu(fileName = "DF2AccessSettings", menuName = "DialogflowV2/Access Settings")]
	public class ServiceSettings : ScriptableObject
	{
		[SerializeField]
		private string projectId = "";
		/// <summary>
		/// The GCP project ID.
		/// </summary>
		public string ProjectId { get { return projectId; } }

		[SerializeField]
		private string credentialsFileName = "";
		/// <summary>
		/// The name of the .p12 file that contains the service account credentials.
		/// </summary>
		public string CredentialsFileName { get { return credentialsFileName; } }

		[SerializeField]
		private string serviceAccount = "";
		/// <summary>
		/// The service account address.
		/// </summary>
		public string ServiceAccount { get { return serviceAccount; } }

		[SerializeField]
		private string languageCode = "";
		/// <summary>
		/// The language code of requests and responses.
		/// </summary>
		public string LanguageCode { get { return languageCode; } set { languageCode = value; } }
	}
}

