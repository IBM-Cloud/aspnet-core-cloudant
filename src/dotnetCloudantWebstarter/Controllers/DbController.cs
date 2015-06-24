using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using MyCouch.Requests;
using MyCouch.Responses;
using dotnetCloudantWebstarter.Models;

namespace CloudantDotNet.Controllers
{
    [Route("api/[controller]")]
    public class DbController : Controller
    {
        // POST: /api/db
        [HttpPost]
        public async Task<dynamic> Create(ToDoItem item)
        {
            try
            {
                //Reference the Cloudant db.
                var myCouch = new DbConnection();
                var client = myCouch.GetClient();

                // Post/Insert to Cloudant using myCouch
                var response = await client.Documents.PostAsync("{\"text\":\"" + item.text + "\"}");
                if (response.IsSuccess)
                {
                    return "{\"id\":\"" + response.Id + "\",\"rev\":\"" + response.Rev + "\",\"text\":\"" + item.text + "\"}";
                }
                else
                {
                    return "{\"msg\": \"Failure to POST. Status Code: " + response.StatusCode + ". Reason: " + response.Reason + "\"}";
                }

            }
            catch (Exception e)
            {
                return "{\"msg\": \"Failure to POST: " + e + "\"}";
            }
        }

        // GET: /api/db
        [HttpGet]
        public async Task<dynamic> GetAll()
        {
            try
            {
                var dbRef = new DbConnection();
                var client = dbRef.GetClient();
                //Create the DB if it does not exist.
                if (client.Database.GetAsync().Result.Error == "not_found")
                {
                    await dbRef.CreateDB();
                    //load with initial data
                    await client.Documents.PostAsync("{\"text\":\"Sample 1\"}");
                    await client.Documents.PostAsync("{\"text\":\"Sample 2\"}");
                    await client.Documents.PostAsync("{\"text\":\"Sample 3\"}");
                }
                //Query for all docs including full content of the docs.
                var query = new QueryViewRequest("_all_docs").Configure(query1 => query1.IncludeDocs(true));

                //GET
                RawResponse response = await client.Views.QueryRawAsync(query);
                if (response.IsSuccess)
                {
                    return response.Content;
                }
                else
                {
                    return "{\"msg\": \"Failure to GET. Status Code: " + response.StatusCode + ". Reason: " + response.Reason + "\"}";
                }
            }
            catch (Exception e)
            {
                return "{\"msg\": \"Failure to GET: " + e + "\"}";
            }
        }

        // PUT: /api/db/
        [HttpPut]
        public async Task<string> update(ToDoItem item)
        {
            try
            {
                var dbRef = new DbConnection();
                var client = dbRef.GetClient();
                //setup updated fields.
                var data = "{\"text\":\"" + item.text + "\"}";

                //Update
                var response = await client.Documents.PutAsync(item.id, item.rev, data);
                if (response.IsSuccess)
                {
                    return "{\"id\":\"" + response.Id + "\",\"rev\":\"" + response.Rev + "\",\"text\":\"" + item.text + "\"}";
                }
                else
                {
                    return "{\"msg\": \"Failure to PUT. Status Code: " + response.StatusCode + ". Reason: " + response.Reason + "\"}";
                }

            }
            catch (Exception e)
            {
                return "{\"msg\": \"Failure to PUT." + e + "\"}";
            }
        }

        // Delete: /api/db/
        [HttpDelete]
        public async Task<dynamic> delete(ToDoItem item)
        {
            try
            {
                var dbRef = new DbConnection();
                var client = dbRef.GetClient();

                //Delete
                var response = await client.Documents.DeleteAsync(item.id, item.rev);
                if (response.IsSuccess)
                {
                    return "{\"id\":\"" + response.Id + "\",\"rev\":\"" + response.Rev + "\"}";
                }
                else
                {
                    return "{\"msg\": \"Failure to DELETE. Status Code: " + response.StatusCode + ". Reason: " + response.Reason + "\"}";
                }

            }
            catch (Exception e)
            {
                return "{\"msg\": \"Failure to DELETE. " + e + "\"}";
            }
        }
    }
}