using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

public class Startup
{
    public IConfiguration Configuration { get; set; }

    public Startup(IHostingEnvironment env)
    {
        var configBuilder = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true);
        Configuration = configBuilder.Build();

        string vcapServices = System.Environment.GetEnvironmentVariable("VCAP_SERVICES");
        if (vcapServices != null)
        {
            dynamic json = JsonConvert.DeserializeObject(vcapServices);
            foreach (dynamic obj in json.Children()) {
                if (((string)obj.Name).ToLowerInvariant().StartsWith("cloudant")) {
                    dynamic credentials = (((JProperty)obj).Value[0] as dynamic).credentials;
                    if (credentials != null) {
                        string host = credentials.host;
                        string username = credentials.username;
                        string password = credentials.password;
                        Configuration["cloudantNoSQLDB:0:credentials:username"] = username;
                        Configuration["cloudantNoSQLDB:0:credentials:password"] = password;
                        Configuration["cloudantNoSQLDB:0:credentials:host"] = host;
                        break;
                    }
                }
            }
        }
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        // works with VCAP_SERVICES JSON value added to config.json when running locally,
        // and works with actual VCAP_SERVICES env var based on configuration set above when running in CF
        services.Configure<creds>(Configuration.GetSection("cloudantNoSQLDB:0:credentials"));
    }

    public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
    {
        loggerFactory.AddConsole();
        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();
        app.UseMvcWithDefaultRoute();
    }

    public static void Main(string[] args) => WebApplication.Run(args);
}

public class creds
{
    public string username { get; set; }
    public string password { get; set; }
    public string host { get; set; }
}
