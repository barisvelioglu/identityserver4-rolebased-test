using IdentityModel;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MvcClient
{

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = "Cookies";
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";
                    options.Events = new OpenIdConnectEvents()
                    {
                        OnUserInformationReceived = async context =>
                        {
                            // IDS4 returns multiple claim values as JSON arrays, which break the authentication handler
                            if (context.User.TryGetValue(JwtClaimTypes.Role, out JToken role))
                            {
                                var claims = new List<Claim>();
                                if (role.Type != JTokenType.Array)
                                {
                                    claims.Add(new Claim(JwtClaimTypes.Role, (string)role));
                                }
                                else
                                {
                                    foreach (var r in role)
                                        claims.Add(new Claim(JwtClaimTypes.Role, (string)r));
                                }
                                var id = context.Principal.Identity as ClaimsIdentity;
                                id.AddClaims(claims);
                            }

                        }
                    };

                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("api1");
                    options.Scope.Add("roles");
                    options.Scope.Add("offline_access");

                    options.ClaimActions.Add(new JsonKeyClaimAction("role", "role", "role"));
                });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}