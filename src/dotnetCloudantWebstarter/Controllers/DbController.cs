using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.IO;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using dotnetCloudantWebstarter.Models;

namespace CloudantDotNet.Controllers
{
    [Route("api/[controller]")]
    public class DbController : Controller
    {
		private static readonly string dbName = "todos";

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
					var body = new StringContent("", Encoding.UTF8, "application/json");
					response = await client.PutAsync(dbName, body);
					if (response.IsSuccessStatusCode)
					{
						await Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 1' }"));
						await Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 2' }"));
						await Create(JsonConvert.DeserializeObject<ToDoItem>("{ 'text': 'Sample 3' }"));
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
			var vcapServices = System.Environment.GetEnvironmentVariable("VCAP_SERVICES");
			if (vcapServices == null)
            {
                throw new Exception("VCAP_SERVICES environment variable not set.");
            }
			else
			{
				dynamic json = JsonConvert.DeserializeObject(vcapServices);
				if (json.cloudantNoSQLDB == null) {
					throw new Exception("Did not find Cloudant credentials in VCAP_SERVICES environment variable.");
				}
				else
				{
					string host = json.cloudantNoSQLDB[0].credentials.host;
					string username = json.cloudantNoSQLDB[0].credentials.username;
					string password = json.cloudantNoSQLDB[0].credentials.password;

					var auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));

					HttpClient client = HttpClientFactory.Create(new LoggingHandler());
					client.BaseAddress = new Uri("https://" + host + "/");
					client.DefaultRequestHeaders.Accept.Clear();
					client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
					client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);
					return client;
				}
			}			
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