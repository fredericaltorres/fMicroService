using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace fDotNetCoreContainerHelper
{
    public class HttpHelper
    {
        public async Task<(bool succeeded, string location, HttpResponseMessage httpResponseMessage)> PostJson(Uri uri, string json, string bearerToken = null)
        {
            using (var client = new HttpClient())
            {
                SetJwtToken(bearerToken, client);

                HttpResponseMessage hrp = await client.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
                if (hrp.IsSuccessStatusCode)
                    return (hrp.IsSuccessStatusCode, hrp.Headers.Location.ToString(), hrp);
                else
                    return (hrp.IsSuccessStatusCode, null, hrp);
            }
        }

        public async Task<(bool succeeded, HttpResponseMessage httpResponseMessage)> Get(Uri uri, string bearerToken = null)
        {
            using (var client = new HttpClient())
            {
                SetJwtToken(bearerToken, client);
                HttpResponseMessage hrp = await client.GetAsync(uri);
                if (hrp.IsSuccessStatusCode)
                    return (hrp.IsSuccessStatusCode, hrp);
                else
                    return (hrp.IsSuccessStatusCode, hrp);
            }
        }

        private static void SetJwtToken(string bearerToken, HttpClient client)
        {
            if (bearerToken != null)
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);
        }
    }
}
