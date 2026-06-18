using CurrencyExchange.Blazor.Components;
using CurrencyExchange.Blazor.Services;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HTTP client pointing at the API
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7100/";
builder.Services.AddHttpClient<ApiAuthService>              (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiCurrencyService>          (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiTransactionService>       (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiWalletService>            (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiExchangeRateService>      (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiCurrencyExchangeService>  (c => c.BaseAddress = new Uri(apiBaseUrl));
builder.Services.AddHttpClient<ApiNbpService>               (c => c.BaseAddress = new Uri(apiBaseUrl));

// Local storage (in-memory for server-side; swap with Blazored.LocalStorage for production)
builder.Services.AddSingleton<ILocalStorageService, InMemoryLocalStorageService>();

// JWT Authentication state provider
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

// Required by Blazor auth components (AuthorizeView, AuthorizeRouteView, etc.)
builder.Services.AddCascadingAuthenticationState();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
