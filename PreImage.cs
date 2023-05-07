using Microsoft.Xrm.Sdk;
using System;
using System.ServiceModel;

namespace DMSNPlugins
{
    public class PreImage : IPlugin
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
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && context.PrimaryEntityName.ToLower() == "lead" && context.MessageName.ToLower() == "update")
            {
                try
                { // Check if the plugin is triggered by the create message and the target is an account entity



                    // Get context of Primary entity
                    Entity entity = (Entity)context.InputParameters["Target"];
                    var modifiedBusinessPhone = entity.Attributes["telephone1"].ToString();

                    //Get the context of last value of the record
                    Entity PreImageEntity = (Entity)context.PreEntityImages["PreImage"];
                    var oldBusinessPhone = PreImageEntity.Attributes["telephone1"].ToString();

                    throw new InvalidPluginExecutionException($"Phone number is changed from {oldBusinessPhone} to {modifiedBusinessPhone}");
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
}
