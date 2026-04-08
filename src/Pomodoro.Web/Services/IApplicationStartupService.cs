using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Pomodoro.Web.Services;

/// <summary>
/// Service responsible for handling application startup and host configuration
/// </summary>
public interface IApplicationStartupService
{
    /// <summary>
    /// Configures the WebAssemblyHostBuilder with root components and services
    /// </summary>
    /// <param name="builder">The WebAssemblyHostBuilder to configure</param>
    void ConfigureHostBuilder(WebAssemblyHostBuilder builder);

    /// <summary>
    /// Initializes and runs the application host
    /// </summary>
    /// <param name="builder">The configured WebAssemblyHostBuilder</param>
    /// <returns>A task representing the asynchronous operation</returns>
    Task InitializeAndRunHostAsync(WebAssemblyHostBuilder builder);
}