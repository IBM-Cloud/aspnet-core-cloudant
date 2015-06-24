using MyCouch;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace CloudantDotNet
{
    public class DbConnection
    {
        string cloudantURL;
        string user;
        string password;
        static string dbName;
        static MyCouchServerClient db;
        static Uri uri;

        public void ConfigureDB()
        {
            dbName = "todos";
            var settings = System.Environment.GetEnvironmentVariable("VCAP_SERVICES");
            if (settings == null)
            {
                Console.WriteLine("VCAP_SERVICES environment variable not set.");
            }
            var parsed = JObject.Parse(settings);
            if (parsed["cloudantNoSQLDB"] != null && parsed["cloudantNoSQLDB"][0] != null)
            {
                user = parsed["cloudantNoSQLDB"][0]["credentials"]["username"].ToString();
                password = parsed["cloudantNoSQLDB"][0]["credentials"]["password"].ToString();
                cloudantURL = parsed["cloudantNoSQLDB"][0]["credentials"]["url"].ToString();
                var uriBuilder = new MyCouchUriBuilder(cloudantURL).SetBasicCredentials(user, password);
                uri = uriBuilder.Build();
                db = new MyCouchServerClient(uriBuilder.Build());
            }
        }

        public async Task<dynamic> CreateDB()
        {
            await DbConnection.db.Databases.PutAsync(DbConnection.dbName);
            return "Created DB: " + DbConnection.dbName;
        }

        public MyCouchClient GetClient()
        {
            return new MyCouchClient(DbConnection.uri + DbConnection.dbName);
        }
    }
}