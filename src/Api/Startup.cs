// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Security.Claims;

namespace Api
{
    public class Startup
    {
  
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            services.AddAuthentication("Bearer")
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.NameClaimType = "name";
                    options.RoleClaimType = "role";
                    options.SaveToken = true;
                    options.ApiName = "api1";
                }).AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                    options.Authority = "http://localhost:5000";
                    options.MetadataAddress = "http://localhost:5000/.well-known/openid-configuration";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc";
                    options.ClientSecret = "secret";

                    options.ResponseType = "code id_token";
                    options.Scope.Add("offline_access");
                    options.Scope.Add("role");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.GetClaimsFromUserInfoEndpoint = true;// <-- add this

                    options.SaveTokens = true;
                    // Fix for getting roles claims correctly :
                    options.ClaimActions.MapJsonKey("role", "role", "role");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };

                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Premium", policyAdmin =>
                {
                    policyAdmin.RequireClaim("role", "Premium User");
                });

                options.AddPolicy("Normal", policyUser =>
                {
                    policyUser.RequireClaim("role", "Normal User");
                });

            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseAuthentication();

            app.UseMvc();
        }
    }
}