using CrmEarlyBound;
using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;
namespace DMSNPlugins
{
    public class CreateTaskPlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get the execution context
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Extract tracing service for debugging
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            // Get the organization service
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName.ToLower() == "account")
            {
                try
                { // Check if the plugin is triggered by the create message and the target is an account entity

                    // Get context of account record / Primary entity
                    Account account = (context.InputParameters["Target"] as Entity).ToEntity<Account>(); //using early bind

                    tracingService.Trace(account.LogicalName);

                    // Create a new task note related to the account
                    Task task = new Task();
                    task.Subject = "Early Bind";
                    task.Description = $"A new account with name {account.Name} has been created using early binding.";

                    //set up lookup value
                    task.RegardingObjectId = new EntityReference(account.LogicalName, account.Id);

                    // Create the task note record
                    service.Create(task);
                    tracingService.Trace("Task note successfully created!");
                }

                catch (FaultException<OrganizationServiceFault> ex)
                {
                    throw new InvalidPluginExecutionException("An error occured", ex);
                }

                catch (Exception ex)
                {
                    tracingService.Trace("Invalid Plugin {0}", ex);
                    throw;
                }
            }

        }
    }
}
