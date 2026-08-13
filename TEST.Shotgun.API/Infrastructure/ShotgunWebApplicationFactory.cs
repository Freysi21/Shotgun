using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TEST.Shotgun.API.Infrastructure;

// Generic in-process ASP.NET Core host for E2E-testing any Shotgun<TEntity, TRepository, TId>
// controller. Deliberately bypasses WebApplicationFactory's entry-point resolution (there is no
// Program.cs to discover here) by building the IHostBuilder from scratch and letting the caller
// register whichever DbContext/repository/controller-assembly a given entity needs.
public class ShotgunWebApplicationFactory : WebApplicationFactory<ShotgunWebApplicationFactory>
{
    private readonly Action<IServiceCollection> _configureServices;
    private readonly Assembly _controllerAssembly;

    public ShotgunWebApplicationFactory(Action<IServiceCollection> configureServices, Assembly? controllerAssembly = null)
    {
        _configureServices = configureServices;
        _controllerAssembly = controllerAssembly ?? Assembly.GetCallingAssembly();
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder.UseTestServer();
                webBuilder.ConfigureServices(services =>
                {
                    services.AddControllers().AddApplicationPart(_controllerAssembly);
                    _configureServices(services);
                });
                webBuilder.Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints => endpoints.MapControllers());
                });
            });
    }
}
