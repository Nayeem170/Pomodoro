using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Pomodoro.Web;
using Pomodoro.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Register application startup service
builder.Services.AddScoped<IApplicationStartupService, ApplicationStartupService>();

// Configure host builder without early service provider build
var startupService = new ApplicationStartupService(null);
startupService.ConfigureHostBuilder(builder);

// Initialize and run host
await startupService.InitializeAndRunHostAsync(builder);
