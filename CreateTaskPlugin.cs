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
            try
            { // Check if the plugin is triggered by the create message and the target is an account entity
                if (context.MessageName.ToLower() == "create" && context.PrimaryEntityName.ToLower() == "account")
                {
                    // Get context of account record / Primary entity
                    Entity entity = (Entity)context.InputParameters["Target"];
                    tracingService.Trace($"Target Entity: {entity.LogicalName}");

                    // Create a new task note related to the account
                    Entity task = new Entity("task");
                    task.Attributes["subject"] = "New Account Created";
                    //task.Attributes["description"] = $"A new account with name {entity.Attributes["name"]} has been created.";
                    task.Attributes.Add("description", $"A new account with name {entity.Attributes["name"]} has been created.");

                    //set up lookup value
                    //task.Attributes["regardingobjectid"] = new EntityReference($"{entity.LogicalName}", entity.Id);
                    //task.Attributes["regardingobjectid"] = entity.ToEntityReference();
                    task.Attributes.Add("regardingobjectid", entity.ToEntityReference());

                    // Create the task note record
                    service.Create(task);
                    tracingService.Trace("Task note successfully created!");
                }
            }

            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occured", ex);
            }

            catch (Exception ex)
            {
                tracingService.Trace("Invalid Plugin {0}", ex.ToString());
                throw;
            }

        }
    }
}
