using Microsoft.AspNet.Builder;
using Microsoft.Framework.DependencyInjection;
using Microsoft.AspNet.StaticFiles;
using CloudantDotNet;

public class Startup
{
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc();
	}

    public void Configure(IApplicationBuilder app)
    {
        System.Net.ServicePointManager.ServerCertificateValidationCallback += (s, ce, ca, p) => true;

        DbConnection db = new DbConnection();
        db.ConfigureDB();

        app.UseFileServer(new FileServerOptions()
        {
            EnableDirectoryBrowsing = false,
        });

        app.UseMvc(routes =>
        {
            routes.MapRoute(
                name: "Default",
                template: "{controller=Home}/{action=Index}");
        });
    }

}
