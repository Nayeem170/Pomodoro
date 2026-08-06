using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Pomodoro.Web.Services;

public interface IApplicationStartupService
{
    /// <param name="builder">The WebAssemblyHostBuilder to configure</param>
    void ConfigureHostBuilder(WebAssemblyHostBuilder builder);

    /// <param name="builder">The configured WebAssemblyHostBuilder</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task InitializeAndRunHostAsync(WebAssemblyHostBuilder builder);
}
