using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace DMSNPlugins
{
    public class DuplicateCheck : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Get the execution context
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            //Extract tracing service for debugging
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            Entity entity = (Entity)context.InputParameters["Target"];



            // Check if the plugin is triggered by the create or update message and the target is an account entity
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
            {
                if (context.MessageName.ToLower() == "create" || context.MessageName.ToLower() == "update")
                {

                    try
                    {
                        // Get the organization service
                        IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                        IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                        var email = string.Empty;
                        //check if attribute has value
                        if (entity.Attributes.Contains("emailaddress1"))
                            email = entity.Attributes["emailaddress1"].ToString();

                        // Retrieve all records in the contact entity where the emailaddress is equal to email input"
                        QueryExpression query = new QueryExpression("contact");
                        query.ColumnSet = new ColumnSet("emailaddress1");
                        query.Criteria.AddCondition("emailaddress1", ConditionOperator.Equal, email);
                        EntityCollection collection = service.RetrieveMultiple(query);

                        // Check if the email exist 
                        if (collection.Entities.Count > 0)
                            throw new InvalidPluginExecutionException("Contact with email already exist!");

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
}
