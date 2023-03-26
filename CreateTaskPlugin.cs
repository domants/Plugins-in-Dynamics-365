using System;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;

namespace DMSNPlugins
{
    public class CreateTaskPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get the execution context
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Extract tracing service
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Check if the plugin is triggered by the create message and the target is an account entity
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName.ToLower() == "account")
            {
                // Get context of account record
                Entity account = (Entity)context.InputParameters["Target"];

                // Create a new task note related to the account
                Entity task = new Entity("task");
                task.Attributes["subject"] = "New Account Created";
                task.Attributes["description"] = "A new account with name '" + account.Attributes["name"] + "' has been created.";
                //set up lookup value
                task.Attributes["regardingobjectid"] = new EntityReference("account", account.Id);
                
                // Get the organization service
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                // Create the task note record
                service.Create(task);
                tracingService.Trace("Task note successfully created!");
            }
        }
    }
}