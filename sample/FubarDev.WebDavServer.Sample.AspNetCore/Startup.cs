﻿using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Account;
using FubarDev.WebDavServer.AspNetCore;
using FubarDev.WebDavServer.AspNetCore.Logging;
using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.FileSystem.DotNet;
using FubarDev.WebDavServer.Locking;
using FubarDev.WebDavServer.Locking.InMemory;
using FubarDev.WebDavServer.Props.Store;
using FubarDev.WebDavServer.Props.Store.TextFile;

using idunno.Authentication;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Sample.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddOptions()
                .AddAuthentication()
                .Configure<WebDavHostOptions>(
                    opt =>
                    {
                        var hostSection = Configuration.GetSection("Host");
                        hostSection?.Bind(opt);
                    })
                .Configure<DotNetFileSystemOptions>(
                    opt =>
                    {
                        opt.RootPath = Path.Combine(Path.GetTempPath(), "webdav");
                        opt.AnonymousUserName = "anonymous";
                    })
                .AddTransient<IPropertyStoreFactory, TextFilePropertyStoreFactory>()
                .AddSingleton<IFileSystemFactory, DotNetFileSystemFactory>()
                .AddSingleton<ILockManager, InMemoryLockManager>()
                .AddTransient(sp =>
                {
                    var factory = sp.GetRequiredService<IFileSystemFactory>();
                    var context = sp.GetRequiredService<IWebDavContext>();
                    return factory.CreateFileSystem(context.User);
                })
                .AddMvcCore()
                .AddAuthorization()
                .AddWebDav();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (Program.IsKestrel && !Program.IsWindows)
            {
                app.UseBasicAuthentication(new BasicAuthenticationOptions()
                {
                    Events = new BasicAuthenticationEvents()
                    {
                        OnValidateCredentials = ValidateCredentialsAsync,
                    }
                });
            }

            app.UseMiddleware<RequestLogMiddleware>();

            app.UseForwardedHeaders(new ForwardedHeadersOptions()
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseMvc();
        }

        private static Task ValidateCredentialsAsync(ValidateCredentialsContext context)
        {
            if (!Npam.NpamUser.Authenticate("passwd", context.Username, context.Password))
                return Task.FromResult(0);

            var groups = Npam.NpamUser.GetGroups(context.Username).ToList();
            var accountInfo = Npam.NpamUser.GetAccountInfo(context.Username);
            var identity = new GenericIdentity(accountInfo.Username, "passwd");
            var groupNames = groups.Select(x => x.GroupName).ToArray();
            var principal = new LocalUserPrincipal(identity, accountInfo.HomeDir, groupNames);
            context.Ticket = new AuthenticationTicket(principal, new AuthenticationProperties(), "Basic");
            context.State = EventResultState.HandledResponse;

            return Task.FromResult(0);
        }
    }
}
