using Cosmos.Chat.GPT.Options;
using Cosmos.Chat.GPT.Services;
using Microsoft.Extensions.Options;

///using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using System.Configuration;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.RegisterConfiguration();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.RegisterServices();
builder.Services.ConfigureServices(builder.Configuration.GetSection("AzureAd"));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.RunAsync();

static class ProgramExtensions
{
    public static void RegisterConfiguration(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<CosmosDb>()
            .Bind(builder.Configuration.GetSection(nameof(CosmosDb)));

        builder.Services.AddOptions<OpenAi>()
            .Bind(builder.Configuration.GetSection(nameof(OpenAi)));
    }

    public static void RegisterServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<CosmosDbService, CosmosDbService>((provider) =>
        {
            var cosmosDbOptions = provider.GetRequiredService<IOptions<CosmosDb>>();
            if (cosmosDbOptions is null)
            {
                throw new ArgumentException($"{nameof(IOptions<CosmosDb>)} was not resolved through dependency injection.");
            }
            else
            {
                var authenticationStateProvider = provider.GetRequiredService<AuthenticationStateProvider>();
                return new CosmosDbService(
                    endpoint: cosmosDbOptions.Value?.Endpoint ?? String.Empty,
                    key: cosmosDbOptions.Value?.Key ?? String.Empty,
                    databaseName: cosmosDbOptions.Value?.Database ?? String.Empty,
                    containerName: cosmosDbOptions.Value?.Container ?? String.Empty,
                    authenticationStateProvider: authenticationStateProvider
                );
            }
        });
        services.AddScoped<OpenAiService, OpenAiService>((provider) =>
        {
            var openAiOptions = provider.GetRequiredService<IOptions<OpenAi>>();
            if (openAiOptions is null)
            {
                throw new ArgumentException($"{nameof(IOptions<OpenAi>)} was not resolved through dependency injection.");
            }
            else
            {
                return new OpenAiService(
                    endpoint: openAiOptions.Value?.Endpoint ?? String.Empty,
                    key: openAiOptions.Value?.Key ?? String.Empty,
                    modelName: openAiOptions.Value?.ModelName ?? String.Empty
                );
            }
        });
        services.AddScoped<ChatService>((provider) =>
        {
            var openAiOptions = provider.GetRequiredService<IOptions<OpenAi>>();
            if (openAiOptions is null)
            {
                throw new ArgumentException($"{nameof(IOptions<OpenAi>)} was not resolved through dependency injection.");
            }
            else
            {
                var cosmosDbService = provider.GetRequiredService<CosmosDbService>();
                var openAiService = provider.GetRequiredService<OpenAiService>();
                var authenticationStateProvider = provider.GetRequiredService<AuthenticationStateProvider>();
                return new ChatService(
                    openAiService: openAiService,
                    cosmosDbService: cosmosDbService,
                    maxConversationTokens: openAiOptions.Value?.MaxConversationTokens ?? String.Empty,
                    authenticationStateProvider: authenticationStateProvider
                );
            }
        });


    }

    public static void ConfigureServices(this IServiceCollection services, IConfigurationSection configuration)
    {
        services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(configuration);

        services.AddRazorPages().AddMvcOptions(options =>
        {
        var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
        }).AddMicrosoftIdentityUI();
    }
}
