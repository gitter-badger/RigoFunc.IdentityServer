﻿using System;
using System.Collections.Generic;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Hosting.Cors;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RigoFunc.IdentityServer {
    public static class IIdentityServerBuilderExtensions {
        public static IIdentityServerBuilder UseAspNetCoreIdentity<TUser, TKey>(this IIdentityServerBuilder builder) where TUser : IdentityUser<TKey> where TKey : IEquatable<TKey> {
            var services = builder.Services;

            services.AddTransient<SignInManager<TUser>, IdentityServerSignInManager<TUser>>();
            services.AddTransient<IProfileService, IdentityProfileService<TUser, TKey>>();
            services.AddTransient<IResourceOwnerPasswordValidator, IdentityResourceOwnerPasswordValidator<TUser>>();
            services.AddTransient<ICorsPolicyService, IdentityCorsPolicyService>();

            var paths = new List<string>(Constants.RoutePaths.CorsPaths);
            
            // can this by reflection rather than hard code?
            paths.Add("api/oauth/register");
            paths.Add("api/oauth/sendcode");
            paths.Add("api/oauth/login");
            paths.Add("api/oauth/verifycode");
            paths.Add("api/oauth/changepassword");
            paths.Add("api/oauth/resetpassword");
            paths.Add("api/oauth/update");
            
            // just for allow CORS for Api
            services.AddTransient<ICorsPolicyProvider>(provider => {
                return new PolicyProvider(
                    provider.GetRequiredService<ILogger<PolicyProvider>>(),
                    paths,
                    provider.GetRequiredService<ICorsPolicyService>());
            });
            services.AddCors();

            services.Configure<IdentityOptions>(options => {
                options.Cookies.ApplicationCookie.AuthenticationScheme = Constants.PrimaryAuthenticationType;

                options.Cookies.ApplicationCookie.LoginPath = new PathString("/" + Constants.RoutePaths.Login);
                options.Cookies.ApplicationCookie.LogoutPath = new PathString("/" + Constants.RoutePaths.Logout);

                options.ClaimsIdentity.UserIdClaimType = JwtClaimTypes.Subject;
                options.ClaimsIdentity.UserNameClaimType = JwtClaimTypes.Name;
                options.ClaimsIdentity.RoleClaimType = JwtClaimTypes.Role;
            });

            services.Configure<IdentityServerOptions>(
               options => {
                   options.AuthenticationOptions.PrimaryAuthenticationScheme = Constants.PrimaryAuthenticationType;
               });

            return builder;
        }
    }
}
