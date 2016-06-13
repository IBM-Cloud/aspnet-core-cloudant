using CloudantDotNet.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace CloudantDotNet.Services
{
    public class CloudantService : ICloudantService
    {
        private static readonly string _dbName = "todos";
        private readonly Creds _cloudantCreds;

        public CloudantService(Creds creds)
        {
            _cloudantCreds = creds;
        }

        public async Task<dynamic> CreateAsync(ToDoItem item)
        {
            using (var client = CloudantClient())
            {
                var response = await client.PostAsJsonAsync(_dbName, item);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsAsync<ToDoItem>();
                    return JsonConvert.SerializeObject(new { id = responseJson.id, rev = responseJson.rev });
                }
                string msg = "Failure to POST. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
                Console.WriteLine(msg);
                return JsonConvert.SerializeObject(new { msg = "Failure to POST. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase });
            }
        }

        public async Task<dynamic> DeleteAsync(ToDoItem item)
        {
            using (var client = CloudantClient())
            {
                var response = await client.DeleteAsync(_dbName + "/" + item.id + "?rev=" + item.rev);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsAsync<ToDoItem>();
                    return JsonConvert.SerializeObject(new { id = responseJson.id, rev = responseJson.rev });
                }
                string msg = "Failure to DELETE. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
                Console.WriteLine(msg);
                return JsonConvert.SerializeObject(new { msg = msg });
            }
        }

        public async Task<dynamic> GetAllAsync()
        {
            using (var client = CloudantClient())
            {
                var response = await client.GetAsync(_dbName + "/_all_docs?include_docs=true");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                string msg = "Failure to GET. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
                Console.WriteLine(msg);
                return JsonConvert.SerializeObject(new { msg = msg });
            }
        }

        public async Task<string> UpdateAsync(ToDoItem item)
        {
            using (var client = CloudantClient())
            {
                var response = await client.PutAsJsonAsync(_dbName + "/" + item.id + "?rev=" + item.rev, item);
                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsAsync<ToDoItem>();
                    return JsonConvert.SerializeObject(new { id = responseJson.id, rev = responseJson.rev });
                }
                string msg = "Failure to PUT. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
                Console.WriteLine(msg);
                return JsonConvert.SerializeObject(new { msg = msg });
            }
        }

        public async Task PopulateTestData()
        {
            using (var client = CloudantClient())
            {
                // create and populate DB if it doesn't exist
                var response = await client.GetAsync(_dbName);
                if (!response.IsSuccessStatusCode)
                {
                    response = await client.PutAsync(_dbName, null);
                    if (response.IsSuccessStatusCode)
                    {
                        Task t1 = CreateAsync(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 1' }"));
                        Task t2 = CreateAsync(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 2' }"));
                        Task t3 = CreateAsync(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 3' }"));
                        await Task.WhenAll(t1, t2, t3);
                    }
                    else
                    {
                        throw new Exception("Failed to create database " + _dbName + ". Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase);
                    }
                }
            }
        } 

        private HttpClient CloudantClient()
        {
            if (_cloudantCreds.username == null || _cloudantCreds.password == null || _cloudantCreds.host == null)
            {
                throw new Exception("Missing Cloudant NoSQL DB service credentials");
            }

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(_cloudantCreds.username + ":" + _cloudantCreds.password));

            HttpClient client = HttpClientFactory.Create(new LoggingHandler());
            client.BaseAddress = new Uri("https://" + _cloudantCreds.host);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
            return client;
        }
    }

    class LoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            Console.WriteLine("{0}\t{1}", request.Method, request.RequestUri);
            var response = await base.SendAsync(request, cancellationToken);
            Console.WriteLine(response.StatusCode);
            return response;
        }
    }
}