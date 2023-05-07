using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.ServiceModel;

namespace DMSNPlugins
{
    public class CopyField : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            //Extract tracing service for debugging
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            //get the reference of primary entity record
            Entity entity = (Entity)context.InputParameters["Target"];

            // Check if the plugin is being triggered by the creation of a contact record.
            if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity && entity.LogicalName == "contact" && context.MessageName.ToLower() == "update")
            {

                try
                {
                    // Obtain the organization service from the service provider.
                    IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                    IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                    // Obtain the account record.
                    EntityReference accountRef = entity.GetAttributeValue<EntityReference>("parentcustomerid");
                    if (accountRef == null)
                    {
                        throw new InvalidPluginExecutionException("The contact must be associated with an account.");
                    }

                    //get the context of account entity
                    Entity account = service.Retrieve(accountRef.LogicalName, accountRef.Id, new ColumnSet(true));

                    var emailAddress = string.Empty;
                    var telephone = string.Empty;
                    var addressline1 = string.Empty;
                    var addressline2 = string.Empty;
                    var city = string.Empty;
                    var state = string.Empty;
                    var postalCode = string.Empty;
                    var country = string.Empty;


                    //Get the attributes values
                    if (account.Attributes.Contains("emailaddress"))
                        emailAddress = account.Attributes["emailaddress"].ToString();
                    if (account.Attributes.Contains("telephone1"))
                        telephone = account.Attributes["telephone1"].ToString();
                    if (account.Attributes.Contains("mobilephone"))
                        entity.Attributes["mobilephone"] = telephone;
                    if (account.Attributes.Contains("address1_line1"))
                        addressline1 = account.Attributes["address1_line1"].ToString();
                    if (account.Attributes.Contains("address1_line2"))
                        addressline2 = account.Attributes["address1_line2"].ToString();
                    if (account.Attributes.Contains("address1_city"))
                        city = account.Attributes["address1_city"].ToString();
                    if (account.Attributes.Contains("address1_stateorprovince"))
                        state = account.Attributes["address1_stateorprovince"].ToString();
                    if (account.Attributes.Contains("address1_postalcode"))
                        postalCode = account.Attributes["address1_postalcode"].ToString();
                    if (account.Attributes.Contains("address1_country"))
                        country = account.Attributes["address1_country"].ToString();

                    // Copy the account fields to the contact record.
                    entity.Attributes["address1_line1"] = addressline1;
                    entity.Attributes["address1_line2"] = addressline2;
                    entity.Attributes["address1_city"] = city;
                    entity.Attributes["address1_stateorprovince"] = state;
                    entity.Attributes["address1_postalcode"] = postalCode;
                    entity.Attributes["address1_country"] = country;
                    entity.Attributes["emailaddress1"] = emailAddress;
                    entity.Attributes["telephone1"] = telephone;
                    entity.Attributes["mobilephone"] = telephone;

                    // Copy the account fields to the contact record.
                    /*
                    entity.Attributes["address1_line1"] = account.GetAttributeValue<string>("address1_line1");
                    entity.Attributes["address1_line2"] = account.GetAttributeValue<string>("address1_line2");
                    entity.Attributes["address1_city"] = account.GetAttributeValue<string>("address1_city");
                    entity.Attributes["address1_stateorprovince"] = account.GetAttributeValue<string>("address1_stateorprovince");
                    entity.Attributes["address1_postalcode"] = account.GetAttributeValue<string>("address1_postalcode");
                    entity.Attributes["address1_country"] = account.GetAttributeValue<string>("address1_country");
                    entity.Attributes["emailaddress1"] = account.GetAttributeValue<string>("emailaddress1");
                    entity.Attributes["telephone1"] = account.GetAttributeValue<string>("telephone1");
                    entity.Attributes["mobilephone"] = account.GetAttributeValue<string>("telephone1");
                    */


                    // Save the contact record.
                    service.Update(entity);
                    tracingService.Trace("Successfully copied!");
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
