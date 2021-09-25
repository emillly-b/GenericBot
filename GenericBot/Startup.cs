using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.HttpOverrides;

namespace SaturnBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddAuthentication(options =>
            {
                // If an authentication cookie is present, use it to get authentication information
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                // If authentication is required, and no cookie is present, use Okta (configured below) to sign in
                options.DefaultChallengeScheme = "discord";
            })
            .AddCookie()// cookie authentication middleware first
            .AddOAuth("discord", options =>
            {
                // Oauth authentication middleware is second

                // When a user needs to sign in, they will be redirected to the authorize endpoint
                options.AuthorizationEndpoint = $"https://discord.com/api/oauth2/authorize";

                options.Scope.Add("identify");
                options.Scope.Add("guilds");

                // After the user signs in, an authorization code will be sent to a callback
                // in this app. The OAuth middleware will intercept it
                options.CallbackPath = new PathString("/api/v1/callback");

                // The OAuth middleware will send the ClientId, ClientSecret, and the
                // authorization code to the token endpoint, and get an access token in return
                options.ClientId = Core.GlobalConfig.OAuthClientId;
                options.ClientSecret = Core.GlobalConfig.OAuthClientSecret;
                options.TokenEndpoint = "https://discord.com/api/oauth2/token";

                // Below we call the userinfo endpoint to get information about the user
                options.UserInformationEndpoint = $"https://discord.com/api/users/@me";

                // Describe how to map the user info we receive to user claims
                options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                options.ClaimActions.MapJsonKey(ClaimTypes.Name, "username");

                options.Events = new OAuthEvents
                {
                    OnCreatingTicket = async context =>
                    {
                        // Get user info from the userinfo endpoint and use it to populate user claims
                        var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

                        var response = await context.Backchannel.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                        response.EnsureSuccessStatusCode();

                        var user = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

                        context.RunClaimActions(user.RootElement);
                    }
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            var fordwardedHeaderOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            fordwardedHeaderOptions.KnownNetworks.Clear();
            fordwardedHeaderOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(fordwardedHeaderOptions);

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
