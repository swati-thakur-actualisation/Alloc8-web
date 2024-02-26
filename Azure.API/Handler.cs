using Azure.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.API
{
    public class Handler
    {
        private readonly HttpClient httpClient;

        public Handler()
        {
            httpClient = new HttpClient();
        }
        public async Task<AzureAuthorization> getAccessToken(string clientId, string clientSecret)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("resource", "https://management.azure.com/")
            });

                    var response = await client.PostAsync("https://login.microsoftonline.com/a95e2dff-914a-459c-bae9-5678d07956f7/oauth2/token", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<AzureAuthorization>(data);

                    }
                    else
                    {

                        return null;
                    }
                }

            }
            catch (Exception ex)
            {


            }
            return null;
        }
        public async Task<WebJobs> getWebJobs(string bearerToken)
        {


            try
            {

                string url = $"https://management.azure.com/subscriptions/127d751f-b18c-40ed-8aef-e839782df01b/resourceGroups/actulisation-shared/providers/Microsoft.Web/sites/actualisation-dashboard/webjobs?api-version=2022-03-01";


                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);


                HttpResponseMessage response = await httpClient.GetAsync(url);


                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                var allAzureWebJobData = JsonConvert.DeserializeObject<WebJobs>(responseBody);

                return allAzureWebJobData;
            }
            catch (Exception e)
            {

                return null;
            }
        }

        public async Task<string> getLogsApi(string logUrl, string bearerToken)
        {
            try
            {
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);


                HttpResponseMessage response = await httpClient.GetAsync(logUrl);


                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                return responseBody;

            }
            catch (Exception ex)
            {

                return null;
            }
        }

        public async Task<bool> runWebJobs(string webJob, string bearerToken)
        {
            try
            {

                string url = $"https://management.azure.com/subscriptions/127d751f-b18c-40ed-8aef-e839782df01b/resourceGroups/actulisation-shared/providers/Microsoft.Web/sites/actualisation-dashboard/continuouswebjobs/{webJob}/start?api-version=2022-03-01";


                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", bearerToken);


                HttpResponseMessage response = await httpClient.PostAsync(url, null);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {

                return false;
            }



        }
    }
}
