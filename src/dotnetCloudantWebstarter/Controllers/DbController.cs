using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using dotnetCloudantWebstarter.Models;
using Microsoft.Framework.OptionsModel;

namespace CloudantDotNet.Controllers
{
    [Route("api/[controller]")]
    public class DbController : Controller
    {
        private static readonly string dbName = "todos";

        private creds cloudantCreds;

        public DbController(IOptions<creds> options)
        {
            cloudantCreds = options.Value;
        }

        [HttpPost]
        public async Task<dynamic> Create(ToDoItem item)
        {
            using (var client = cloudantClient())
            {
                var response = await client.PostAsJsonAsync(dbName, item);
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

        [HttpGet]
        public async Task<dynamic> GetAll()
        {
            using (var client = cloudantClient())
            {
                // create and populated DB if it doesn't exist
                var response = await client.GetAsync(dbName);
                if (!response.IsSuccessStatusCode)
                {
                    response = await client.PutAsync(dbName, null);
                    if (response.IsSuccessStatusCode)
                    {
                        Task t1 = Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 1' }"));
                        Task t2 = Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 2' }"));
                        Task t3 = Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 3' }"));
                        await Task.WhenAll(t1, t2, t3);
                    }
                    else
                    {
                        throw new Exception("Failed to create database " + dbName + ". Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase);
                    }
                }

                response = await client.GetAsync(dbName + "/_all_docs?include_docs=true");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                string msg = "Failure to GET. Status Code: " + response.StatusCode + ". Reason: " + response.ReasonPhrase;
                Console.WriteLine(msg);
                return JsonConvert.SerializeObject(new { msg = msg });
            }
        }

        [HttpPut]
        public async Task<string> update(ToDoItem item)
        {
            using (var client = cloudantClient())
            {
                var response = await client.PutAsJsonAsync(dbName + "/" + item.id + "?rev=" + item.rev, item);
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

        [HttpDelete]
        public async Task<dynamic> delete(ToDoItem item)
        {
            using (var client = cloudantClient())
            {
                var response = await client.DeleteAsync(dbName + "/" + item.id + "?rev=" + item.rev);
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

        private HttpClient cloudantClient()
        {
            if (cloudantCreds.username == null || cloudantCreds.password == null || cloudantCreds.host == null)
            {
                throw new Exception("Missing Cloudant NoSQL DB service credentials");
            }

            var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(cloudantCreds.username + ":" + cloudantCreds.password));

            HttpClient client = HttpClientFactory.Create(new LoggingHandler());
            client.BaseAddress = new Uri("https://" + cloudantCreds.host);
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