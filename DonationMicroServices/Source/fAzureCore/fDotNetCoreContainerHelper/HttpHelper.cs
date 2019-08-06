using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace fDotNetCoreContainerHelper
{
    public class HttpHelper
    {
        public async Task<(bool succeeded, string location, HttpResponseMessage httpResponseMessage)> 
            PostJson(Uri uri, string json, string bearerToken = null, System.Net.Http.HttpClient httpClient = null)
        {
            if(httpClient == null)
            {
                // This mode raise exception ex:System.Net.Http.HttpRequestException:System.Net.Sockets.SocketException: Connection reset by peer
                // when running in 4 core machine. See more info in https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/
                using (var client = new HttpClient())
                    return await __PostJson(uri, json, bearerToken, client);
            }
            else
            {
                return await __PostJson(uri, json, bearerToken, httpClient);
            }
        }

        private static async Task<(bool succeeded, string location, HttpResponseMessage httpResponseMessage)>
            __PostJson(Uri uri, string json, string bearerToken, HttpClient client)
        {
            SetJwtToken(bearerToken, client);

            HttpResponseMessage hrp = await client.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"));
            if (hrp.IsSuccessStatusCode)
                return (hrp.IsSuccessStatusCode, hrp.Headers.Location.ToString(), hrp);
            else
                return (hrp.IsSuccessStatusCode, null, hrp);
        }

        public async Task<(bool succeeded, string location, HttpResponseMessage httpResponseMessage)> PostJson_impl1(Uri uri, string json, string bearerToken = null)
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
