﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Builder.TraceExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AspNetCore_EchoBot_With_State
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Bind appsettings.json values to a class that can be injected into Index.cshtml
            services.Configure<ApplicationConfiguration>(Configuration);

            services.AddMvc();

            // The Memory Storage used here is for local bot debugging only. When the bot
            // is restarted, anything stored in memory will be gone. 
            IStorage dataStore = new MemoryStorage();

            // The File data store, shown here, is suitable for bots that run on 
            // a single machine and need durable state across application restarts.                 
            // IStorage dataStore = new FileStorage(System.IO.Path.GetTempPath());

            // For production bots use the Azure Table Store, Azure Blob, or 
            // Azure CosmosDB storage provides, as seen below. To include any of 
            // the Azure based storage providers, add the Microsoft.Bot.Builder.Azure 
            // Nuget package to your solution. That package is found at:
            //      https://www.nuget.org/packages/Microsoft.Bot.Builder.Azure/

            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureTableStorage("AzureTablesConnectionString", "TableName");
            // IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage("AzureBlobConnectionString", "containerName");

            // Create the Conversation Sate
            var conversationState = new ConversationState(dataStore);
            // Create the property which will manage the ConversationState
            var accessor = conversationState.CreateProperty<EchoState>("ConversationState", () => new EchoState());
            
            services.AddBot<EchoBot>(options =>
            {
                options.CredentialProvider = new ConfigurationCredentialProvider(Configuration);
                

                options.Middleware.Add(conversationState);
            });

            // Add the accessor to the services for future use in the Bot constructor, so the bot
            // has access to the ConversationState inside the turn context
            services.AddSingleton(accessor);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();
        }
    }

    // This class is used to retrieve strings from appsettings.json on demand
    public class ApplicationConfiguration
    {
        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
    }
}
