using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

namespace Syrus.Plugins.DFV2Client
{
	public static class JwtCache 
	{
		/// <summary>
		/// Maps each project to an access token.
		/// </summary>
		private static Dictionary<string, JwtToken> tokens;

		static JwtCache()
		{
			tokens = new Dictionary<string, JwtToken>();
		}

		/// <summary>
		/// Tries to acquire a JWT token from the cache.
		/// </summary>
		/// <param name="serviceAccount">The Google Service Account that wants to use the chatbot.</param>
		/// <param name="token">The access token.</param>
		/// <returns>True if the acccess token was found, false otherwise.</returns>
		public static bool TryGetToken(string serviceAccount, out string token)
		{
			if (tokens.TryGetValue(serviceAccount, out JwtToken jwt) && Time.time < jwt.expireTime)
			{
				token = jwt.token;
				return true;
			}

			token = string.Empty;
			return false;
		}

		/// <summary>
		/// Acquires a new JWT token and updates the cache.
		/// </summary>
		/// <param name="credentialsFileName">The name of the .p12 file that contains the credentials.</param>
		/// <param name="serviceAccount">The name of the service account which is making the request.</param>
		public static IEnumerator GetToken(string credentialsFileName, string serviceAccount)
		{
			TextAsset p12File = Resources.Load<TextAsset>("DialogflowV2/" + credentialsFileName);
			var jwt = GoogleJsonWebToken.GetJwt(serviceAccount, p12File.bytes,
				GoogleJsonWebToken.SCOPE_DIALOGFLOWV2);
			UnityWebRequest tokenRequest = GoogleJsonWebToken.GetAccessTokenRequest(jwt);
			yield return tokenRequest.SendWebRequest();
			if (tokenRequest.isNetworkError || tokenRequest.isHttpError)
			{
				Debug.LogError("Error " + tokenRequest.responseCode + ": " + tokenRequest.error);
				yield break;
			}
			string serializedToken = Encoding.UTF8.GetString(tokenRequest.downloadHandler.data);
			var jwtJson = JsonConvert.DeserializeObject<GoogleJsonWebToken.JwtTokenResponse>(serializedToken);
			tokens[serviceAccount] = new JwtToken(jwtJson.access_token,
				Time.time + jwtJson.expires_in);
		}

		internal struct JwtToken
		{
			/// <summary>
			/// The JWT token used for DF2 access.
			/// </summary>
			internal string token;

			/// <summary>
			/// The expire time in Unity time.
			/// </summary>
			internal float expireTime;

			internal JwtToken(string token, float expireTime)
			{
				this.token = token;
				this.expireTime = expireTime;
			}
		}
	}


}

