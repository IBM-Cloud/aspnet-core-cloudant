using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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
            if (json.cloudantNoSQLDB != null)
            {
                string host = json.cloudantNoSQLDB[0].credentials.host;
                string username = json.cloudantNoSQLDB[0].credentials.username;
                string password = json.cloudantNoSQLDB[0].credentials.password;
                Configuration["cloudantNoSQLDB:0:credentials:username"] = username;
                Configuration["cloudantNoSQLDB:0:credentials:password"] = password;
                Configuration["cloudantNoSQLDB:0:credentials:host"] = host;
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
