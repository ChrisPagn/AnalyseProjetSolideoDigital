using Client;
using Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// En production, le Client est servi par le même conteneur que l'Api (Prompt Maître 7.3) :
// l'origine courante suffit. En dev local, l'Api tourne sur un port séparé (voir
// wwwroot/appsettings.Development.json et Api/Program.cs : CORS + cookie SameSite=Lax
// activés uniquement en environnement Development).
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl),
});

builder.Services.AddMudServices();

builder.Services.AddScoped<AuthClient>();
builder.Services.AddScoped<ClientsApiClient>();
builder.Services.AddScoped<ProjetsApiClient>();
builder.Services.AddScoped<PhasesApiClient>();
builder.Services.AddScoped<RegistresApiClient>();
builder.Services.AddScoped<DomaineApiClient>();
builder.Services.AddScoped<TracabiliteApiClient>();
builder.Services.AddScoped<CookieAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CookieAuthenticationStateProvider>());
builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
