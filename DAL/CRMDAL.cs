using DataModel;
using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Metadata;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Activities.Statements;
using System.Diagnostics;
using System.Activities.Presentation.Model;

namespace DAL
{
    public class CRMDAL
    {

        public IOrganizationService _orgService;
        public CRMDAL()
        {
            string connectionString = ConfigurationManager.AppSettings["CRMConnectionString"];
            CrmServiceClient _crmConnection;

            _crmConnection = new CrmServiceClient(connectionString);

            _orgService = (IOrganizationService)_crmConnection.OrganizationWebProxyClient != null ? (IOrganizationService)_crmConnection.OrganizationWebProxyClient : (IOrganizationService)_crmConnection.OrganizationServiceProxy;

        }

        #region CommonMethods
        public DateTime ConvertToGivenTimeZone(DateTime dtUTC, string timezone)
        {
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(timezone);
            return TimeZoneInfo.ConvertTimeFromUtc(dtUTC, timeZone);
        }

        #endregion

        #region SignUp and Login

        //Code For New Customer Signup (Any new account created with this method automatically assign as Customer)
        public Guid CreateContactByEmailAndPassword(ContactDM dn)
        {
            Guid newGuid = Guid.Empty;
            try
            {
                Entity en = new Entity("contact");
                if (!string.IsNullOrWhiteSpace(dn.FirstName))
                    en["firstname"] = dn.FirstName;
                if (!string.IsNullOrWhiteSpace(dn.LastName))
                    en["lastname"] = dn.LastName;
                if (dn.GenderValue > 0)
                    en["gendercode"] = new OptionSetValue(dn.GenderValue);
                if (!string.IsNullOrWhiteSpace(dn.Email))
                    en["emailaddress1"] = dn.Email;
                if (!string.IsNullOrWhiteSpace(dn.MobilePhone))
                    en["mobilephone"] = dn.MobilePhone;
                if (!string.IsNullOrWhiteSpace(dn.Password))
                    en["tfq_password"] = dn.Password;
                if (dn.DateOfBirth != DateTime.MinValue)
                    en["birthdate"] = dn.DateOfBirth;
                if (dn.ContactTypeValue > 0)
                    en["tfq_contacttype"] = new OptionSetValue(dn.ContactTypeValue);
                newGuid = _orgService.Create(en);

            }
            catch (Exception e)
            {
                Console.WriteLine("Opps Something went wrong.." + e);
            }
            return newGuid;
        }

        //Code for Login 0f Customer, Storekeeper, Deliveryboy
        public ContactDM GetContactByEmailAndPassword(string email, string password)
        {
            ContactDM contactDM = null;
            try
            {

                string qry = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                 <entity name='contact'>
                                   <attribute name='firstname' />
                                   <attribute name='lastname' />
                                   <attribute name='contactid' />
                                   <attribute name='emailaddress1' />
                                   <attribute name='address1_city' />
                                   <attribute name='address1_line1' />
                                   <attribute name='address1_postalcode' />
                                   <attribute name='address1_stateorprovince' />
                                   <attribute name='tfq_contacttype' />
                                   <order attribute='firstname' descending='false' />
                                   <filter type='and'>
                                     <condition attribute='emailaddress1' operator='eq' value='{0}' />
                                     <condition attribute='tfq_password' operator='eq' value='{1}' />
                                   </filter>
                                 </entity>
                               </fetch>";

                qry = string.Format(qry, email, password);
                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(qry));
                if (entityCol != null && entityCol.Entities.Count > 0)
                {
                    Entity entity = entityCol.Entities[0];
                    contactDM = new ContactDM();
                    contactDM.Id = entity.Id;

                    if (entity.Contains("emailaddress1"))
                        contactDM.Email = (string)entity["emailaddress1"];
                    if (entity.Contains("firstname"))
                        contactDM.FirstName = (string)entity["firstname"];
                    if (entity.Contains("lastname"))
                        contactDM.LastName = (string)entity["lastname"];
                    if (entity.Contains("tfq_contacttype"))
                    {
                        contactDM.ContactTypeValue = ((OptionSetValue)entity["tfq_contacttype"]).Value;
                        contactDM.ContactTypeText = entity.FormattedValues["tfq_contacttype"];
                    }
                    if (entity.Contains("address1_city"))
                        contactDM.City = (string)entity["address1_city"];
                    if (entity.Contains("address1_line1"))
                        contactDM.Street = (string)entity["address1_line1"];
                    if (entity.Contains("address1_postalcode"))
                        contactDM.ZipCode = (string)entity["address1_postalcode"];
                    if (entity.Contains("address1_stateorprovince"))
                        contactDM.State = (string)entity["address1_stateorprovince"];
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Opps something went wrong.." + ex);
            }
            return contactDM;
        }
        #endregion

        #region ForgetPassword & ChangePassword 
        public ContactDM GetContactByEmail(string email)
        {
            ContactDM contactDM = null;
            try
            {
                string qry = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                            <entity name='contact'>
                               <attribute name='firstname' />
                               <attribute name='lastname' />
                               <attribute name='contactid' />
                               <attribute name='emailaddress1' />
                               <attribute name='tfq_contacttype' />
                               <order attribute='fullname' descending='false' />
                               <filter type='and'>
                               <condition attribute='emailaddress1' operator='eq' value='{0}' />
                             </filter>
                            </entity>
                          </fetch>";
                qry = string.Format(qry, email);

                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(qry));
                if (entityCol != null && entityCol.Entities.Count > 0)
                {
                    Entity entity = entityCol.Entities[0];
                    contactDM = new ContactDM();
                    contactDM.Id = entity.Id;

                    if (entity.Contains("emailaddress1"))
                        contactDM.Email = (string)entity["emailaddress1"];

                }
            }
            catch (Exception ex) { Console.WriteLine("Opps something went wrong.." + ex); }
            return contactDM;
        }

        //This chagnge password is attached code for the forget password. This code will automatically call when someone click the forget password code.
        public Guid ChangePassword(ContactDM con)
        {
            Guid guid = con.Id;

            try
            {
                Entity ecn = new Entity("contact");
                ecn.Id = guid;
                const string valid = "@#$abcdefGhIjKlMnopqrstUvwxyZ1234567890";
                StringBuilder res = new StringBuilder();
                Random rand = new Random();
                int i = 0;
                while (i < 10)
                {
                    res.Append(valid[rand.Next(valid.Length)]);
                    i++;
                }
                string newPassword = res.ToString();

                if (!string.IsNullOrWhiteSpace(newPassword))
                    ecn["tfq_password"] = newPassword;
                ecn["tfq_forgetpassword"] = con.ForgetPassword;
                _orgService.Update(ecn);
                return guid;

            }
            catch (Exception e)
            {
                Console.WriteLine("Oppps something went wrong....." + e);
            }
            return guid;
        }

        //Change Password After login all contacts
        public void ChagnePassAfterLogin(Guid Id, string password)
        {
            Entity en = new Entity("contact");
            en.Id = Id;
            if (!string.IsNullOrWhiteSpace(password))
                en["tfq_password"] = password;
            _orgService.Update(en);
        }
        #endregion

        #region ViewOrder (Customer, Deliveryboy, Storekeeper)

        //For customer
        public List<OrderDM> GetOrderByGuid(ContactDM cdm, string customerType)
        {
            List<OrderDM> orderList = new List<OrderDM>();
            OrderDM orderDM = null;
            try
            {

                string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                   <entity name='tfq_order'>
                                     <attribute name='tfq_orderid' />
                                     <attribute name='tfq_name' />
                                     <attribute name='tfq_totalamount' />
                                     <attribute name='statuscode' />
                                     <attribute name='tfq_paymentmode' />
                                     <attribute name='tfq_orderno' />
                                     <attribute name='tfq_mobilephone' />
                                     <attribute name='tfq_city' />
                                     <attribute name='tfq_address' />
                                     <attribute name='tfq_state' />

                                     <attribute name='tfq_customer' />
                                     <attribute name='tfq_diapatchedby' />
                                     <attribute name='tfq_deliveredby' />

                                     <order attribute='tfq_name' descending='false' />
                                     <filter type='and'>
                                       <condition attribute='{1}' operator='eq' value='{0}' />
                                     </filter>
                                   </entity>
                                 </fetch>";

                query = string.Format(query, cdm.Id, customerType);
                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(query));

                if (entityCol != null && entityCol.Entities.Count > 0)
                {

                    foreach (var entity in entityCol.Entities)
                    {
                        orderDM = new OrderDM();
                        orderDM.Id = entity.Id;

                        if (entity.Contains("tfq_orderno"))
                            orderDM.OrderNo = (string)entity["tfq_orderno"];
                        switch (customerType)
                        {
                            case "tfq_customer":

                                orderDM.CustomerId = cdm.Id;
                                orderDM.Name = string.Concat(cdm.FirstName, " ", cdm.LastName);

                                if (entity.Contains("tfq_totalamount"))
                                    orderDM.TotalAmount = ((Money)entity["tfq_totalamount"]).Value;

                                if (entity.Contains("tfq_mobilephone"))
                                    orderDM.ContactNo = (string)entity["tfq_mobilephone"];

                                if (entity.Contains("tfq_address"))
                                    orderDM.Address = (string)entity["tfq_address"];

                                if (entity.Contains("tfq_city"))
                                    orderDM.City = (string)entity["tfq_city"];

                                if (entity.Contains("tfq_state"))
                                    orderDM.State = (string)entity["tfq_state"];

                                if (entity.Contains("tfq_diapatchedby"))
                                {
                                    orderDM.DispatchedById = ((EntityReference)entity["tfq_diapatchedby"]).Id;
                                    orderDM.DispatchedByName = ((EntityReference)entity["tfq_diapatchedby"]).Name;
                                }

                                if (entity.Contains("tfq_deliveredby"))
                                {
                                    orderDM.DeliveredById = ((EntityReference)entity["tfq_deliveredby"]).Id;
                                    orderDM.DeliveredByName = ((EntityReference)entity["tfq_deliveredby"]).Name;
                                }

                                if (entity.Contains("statuscode"))
                                {
                                    orderDM.StatusReasonValue = ((OptionSetValue)entity["statuscode"]).Value;
                                    orderDM.StatusReasonText = entity.FormattedValues["statuscode"];
                                }

                                if (entity.Contains("tfq_paymentmode"))
                                {
                                    orderDM.PaymentModeValue = ((OptionSetValue)entity["tfq_paymentmode"]).Value;
                                    orderDM.PaymentModeText = entity.FormattedValues["tfq_paymentmode"];
                                }
                                break;

                        }
                        if (entity.Contains("statuscode"))
                        {
                            orderDM.StatusReasonValue = ((OptionSetValue)entity["statuscode"]).Value;
                            orderDM.StatusReasonText = entity.FormattedValues["statuscode"];
                        }
                        orderList.Add(orderDM);

                    }
                }
            }

            catch (Exception e) { Console.WriteLine(e); }

            return orderList;
        }

        //For Storekeeper
        public List<OrderDM> ViewOrderForShopkeeper()
        {
            List<OrderDM> orderList = new List<OrderDM>();
            OrderDM orderDM = null;
            try
            {

                string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                   <entity name='tfq_order'>
                                       <attribute name='tfq_orderid' />
                                       <attribute name='tfq_name' />
                                       <attribute name='statuscode' />
                                       <attribute name='tfq_orderno' />
                                       <attribute name='tfq_customer'/>
                                       <attribute name='tfq_mobilephone' />
                                       <attribute name='tfq_landmark' />
                                       <attribute name='tfq_deliveredby' />
                                       <attribute name='tfq_diapatchedby' />
                                       <attribute name='tfq_contactno' />
                                       <attribute name='tfq_city' />
                                       <attribute name='tfq_amount' />
                                       <attribute name='tfq_totalamount' />
                                       <attribute name='tfq_address' />
                                     <order attribute='tfq_name' descending='false' />
                                     <filter type='and'>
                                       <condition attribute='statuscodename' operator='not-like' value='%New%' />
                                     </filter>
                                   </entity>
                                 </fetch>";

                query = string.Format(query);
                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(query));

                if (entityCol != null && entityCol.Entities.Count > 0)
                {

                    foreach (var entity in entityCol.Entities)
                    {
                        orderDM = new OrderDM();
                        orderDM.Id = entity.Id;

                        if (entity.Contains("tfq_orderno"))
                            orderDM.OrderNo = (string)entity["tfq_orderno"];

                        if (entity.Contains("tfq_customer"))
                        {
                            orderDM.CustomerId = ((EntityReference)entity["tfq_customer"]).Id;
                            orderDM.CustomerName = ((EntityReference)entity["tfq_customer"]).Name;
                        }

                        if (entity.Contains("tfq_totalamount"))
                            orderDM.TotalAmount = ((Money)entity["tfq_totalamount"]).Value;

                        if (entity.Contains("tfq_mobilephone"))
                            orderDM.ContactNo = (string)entity["tfq_mobilephone"];

                        if (entity.Contains("tfq_address"))
                            orderDM.Address = (string)entity["tfq_address"];

                        if (entity.Contains("tfq_city"))
                            orderDM.City = (string)entity["tfq_city"];

                        if (entity.Contains("tfq_landmark"))
                            orderDM.State = (string)entity["tfq_landmark"];
                        
                        if (entity.Contains("tfq_state"))
                            orderDM.State = (string)entity["tfq_state"];

                        if (entity.Contains("tfq_name"))
                            orderDM.Name = (string)entity["tfq_name"];

                        if (entity.Contains("tfq_diapatchedby"))
                        {
                            orderDM.DispatchedById = ((EntityReference)entity["tfq_diapatchedby"]).Id;
                        orderDM.DispatchedByName = ((EntityReference)entity["tfq_diapatchedby"]).Name;
                        }
                        if (entity.Contains("tfq_deliveredby"))
                        {
                            orderDM.DeliveredById = ((EntityReference)entity["tfq_deliveredby"]).Id;
                            orderDM.DeliveredByName = ((EntityReference)entity["tfq_deliveredby"]).Name;
                        }

                        if (entity.Contains("statuscode"))
                        {
                            orderDM.StatusReasonValue = ((OptionSetValue)entity["statuscode"]).Value;
                            orderDM.StatusReasonText = entity.FormattedValues["statuscode"];
                        }

                        if (entity.Contains("tfq_paymentmode"))
                        {
                            orderDM.PaymentModeValue = ((OptionSetValue)entity["tfq_paymentmode"]).Value;
                            orderDM.PaymentModeText = entity.FormattedValues["tfq_paymentmode"];
                        }
                        orderList.Add(orderDM);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }

            return orderList;
        }

        //For DeliveryBOy
        public List<OrderDM> ViewOrderDeliveryBoy(ContactDM user)
        {
            List<OrderDM> orderList = new List<OrderDM>();
            OrderDM orderDM = null;
            try
            {

                string query = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                   <entity name='tfq_order'>
                                     <attribute name='tfq_orderid' />
                                       <attribute name='tfq_name' />
                                       <attribute name='statuscode' />
                                       <attribute name='tfq_orderno' />
                                       <attribute name='tfq_customer'/>
                                       <attribute name='tfq_mobilephone' />
                                       <attribute name='tfq_landmark' />
                                       <attribute name='tfq_deliveredby' />
                                       <attribute name='tfq_diapatchedby' />
                                       <attribute name='tfq_contactno' />
                                       <attribute name='tfq_city' />
                                       <attribute name='tfq_amount' />
                                       <attribute name='tfq_totalamount' />
                                       <attribute name='tfq_address' />
                                     <order attribute='tfq_name' descending='false' />
                                     <filter type='and'>
                                       <filter type='or'>
                                         <condition attribute='tfq_deliveredby' operator='eq' value='{0}' />
                                         <condition attribute='tfq_deliveredby' operator='null' />
                                       </filter>
                                       <condition attribute='statuscode' operator='in'>
                                         <value>227830004</value>
                                         <value>227830001</value>
                                         <value>227830002</value>
                                         <value>227830003</value>
                                         <value>227830006</value>
                                       </condition>
                                     </filter>
                                   </entity>
                                 </fetch>";

                query = string.Format(query, user.Id);
                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(query));

                if (entityCol != null && entityCol.Entities.Count > 0)
                {

                    foreach (var entity in entityCol.Entities)
                    {
                        orderDM = new OrderDM();
                        orderDM.Id = entity.Id;

                        if (entity.Contains("tfq_orderno"))
                            orderDM.OrderNo = (string)entity["tfq_orderno"];

                        if (entity.Contains("tfq_customer"))
                        {
                            orderDM.CustomerId = ((EntityReference)entity["tfq_customer"]).Id;
                            orderDM.CustomerName = ((EntityReference)entity["tfq_customer"]).Name;
                        }

                        if (entity.Contains("tfq_totalamount"))
                            orderDM.TotalAmount = ((Money)entity["tfq_totalamount"]).Value;

                        if (entity.Contains("tfq_mobilephone"))
                            orderDM.ContactNo = (string)entity["tfq_mobilephone"];

                        if (entity.Contains("tfq_address"))
                            orderDM.Address = (string)entity["tfq_address"];

                        if (entity.Contains("tfq_city"))
                            orderDM.City = (string)entity["tfq_city"];

                        if (entity.Contains("tfq_state"))
                            orderDM.State = (string)entity["tfq_state"];

                        if (entity.Contains("tfq_name"))
                            orderDM.Name = (string)entity["tfq_name"];

                        if (entity.Contains("tfq_diapatchedby"))
                        {
                            orderDM.DispatchedById = ((EntityReference)entity["tfq_diapatchedby"]).Id;
                        orderDM.DispatchedByName = ((EntityReference)entity["tfq_diapatchedby"]).Name;
                        }
                        if (entity.Contains("tfq_deliveredby"))
                        {
                            orderDM.DeliveredById = ((EntityReference)entity["tfq_deliveredby"]).Id;
                            orderDM.DeliveredByName = ((EntityReference)entity["tfq_deliveredby"]).Name;
                        }

                        if (entity.Contains("statuscode"))
                        {
                            orderDM.StatusReasonValue = ((OptionSetValue)entity["statuscode"]).Value;
                            orderDM.StatusReasonText = entity.FormattedValues["statuscode"];
                        }

                        if (entity.Contains("tfq_paymentmode"))
                        {
                            orderDM.PaymentModeValue = ((OptionSetValue)entity["tfq_paymentmode"]).Value;
                            orderDM.PaymentModeText = entity.FormattedValues["tfq_paymentmode"];
                        }
                        orderList.Add(orderDM);
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e); }

            return orderList;
        }
        #endregion

        #region CreateOrder

        //Adding Additional details for Customer to place order
        public Guid CreateOrder(OrderDM odm, ContactDM usr)
        {

            Guid newGuid = Guid.Empty;
            try
            {
                Entity en = new Entity("tfq_order");
                if (!string.IsNullOrWhiteSpace(odm.Name))
                    en["tfq_name"] = odm.Name;

                if (Guid.Empty != usr.Id)
                    en["tfq_customer"] = new EntityReference("contact", usr.Id);

                if (!string.IsNullOrWhiteSpace(odm.Address))
                    en["tfq_address"] = odm.Address;

                if (!string.IsNullOrWhiteSpace(odm.City))
                    en["tfq_city"] = odm.City;

                if (!string.IsNullOrWhiteSpace(odm.State))
                    en["tfq_state"] = odm.State;

                if (!string.IsNullOrWhiteSpace(odm.ContactNo))
                    en["tfq_mobilephone"] = odm.ContactNo;

                if (!string.IsNullOrWhiteSpace(odm.Landmark))
                    en["tfq_landmark"] = odm.Landmark;

                if (odm.PaymentModeValue > 0)
                {
                    en["tfq_paymentmode"] = new OptionSetValue(odm.PaymentModeValue);
                }

                newGuid = _orgService.Create(en);
            }

            catch (Exception e)
            {
                Console.WriteLine("oops something went wrong..." + e);
            }
            return newGuid;
        }
        //Getting all the Item Master List from CRM Database  In the form of option set so that customer can pick one by one item form here to place order...
        public List<ItemMasterDM> GetItemMasterList()
        {
            List<ItemMasterDM> itemlist = new List<ItemMasterDM>();
            try
            {
                string qry = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                               <entity name='tfq_itemmaster'>
                               <attribute name='tfq_itemmasterid' />
                               <attribute name='tfq_name' />
                               <attribute name='tfq_type' />
                               <attribute name='tfq_price' />
                               <attribute name='tfq_description' />
                               <attribute name='tfq_category' />
                               <order attribute='tfq_description' descending='false' />
                               </entity>
                               </fetch>";

                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(qry));
                if (entityCol != null && entityCol.Entities.Count > 0)
                {
                    foreach (var entity in entityCol.Entities)
                    {
                        ItemMasterDM itemDM = new ItemMasterDM();
                        itemDM.Id = entity.Id;

                        if (entity.Contains("tfq_name"))
                            itemDM.Name = (string)entity["tfq_name"];
                        if (entity.Contains("tfq_description"))
                            itemDM.Description = (string)entity["tfq_description"];
                        if (entity.Contains("tfq_type"))
                        {
                            itemDM.TypeValue = ((OptionSetValue)entity["tfq_type"]).Value;
                            itemDM.TypeText = entity.FormattedValues["tfq_type"];
                        }
                        if (entity.Contains("tfq_price"))
                            itemDM.Price = decimal.Parse((((Money)entity["tfq_price"]).Value).ToString("0.00"));
                        if (entity.Contains("mon_category"))
                        {
                            itemDM.CategoryId = ((EntityReference)entity["tfq_category"]).Id;
                            itemDM.CategoryName = ((EntityReference)entity["tfq_category"]).Name;
                        }
                        itemlist.Add(itemDM);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Opps something went wrong.." + ex);
            }
            return itemlist;
        }


        //Adding item Master To create order
        public Guid AddItemInCustomerOrder(OrderItemDM orderItem)
        {
            Guid guid = Guid.Empty;
            try
            {
                Entity ecn = new Entity("tfq_orderitem");
                if (!string.IsNullOrWhiteSpace(orderItem.Name))
                    ecn["tfq_name"] = orderItem.Name;
                if (Guid.Empty != orderItem.OrderId)
                    ecn["tfq_order"] = new EntityReference("tfq_order", orderItem.OrderId);
                if (Guid.Empty != orderItem.ItemMasterId)
                    ecn["tfq_itemmaster"] = new EntityReference("tfq_order", orderItem.ItemMasterId);
                if (!string.IsNullOrWhiteSpace(Convert.ToString(orderItem.Quantity)))
                    ecn["tfq_quantity"] = orderItem.Quantity;
                if (orderItem.Price > -1)
                    ecn["tfq_price"] = new Money(orderItem.Price);

                guid = _orgService.Create(ecn);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Opps something went wrong.." + ex);
            }
            return guid;
        }
        //
        //Code for Deleting OrderItems
        public void DeleteOrderItemByOrderId(Guid Id)
        {
            _orgService.Delete("tfq_orderitem", Id);
        }
        #endregion

        #region GetOrderItemsByOrderId
        public List<OrderItemDM> GetOrderItemsByOrderId(Guid Id)
        {
            OrderItemDM itemDM = null;
            List<OrderItemDM> itemList = new List<OrderItemDM>();
            try
            {
                string qrey = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                   <entity name='tfq_orderitem'>
                                     <attribute name='tfq_orderitemid' />
                                     <attribute name='tfq_name' />
                                     <attribute name='tfq_quantity' />
                                     <attribute name='tfq_price' />
                                     <attribute name='tfq_itemmaster' />
                                     <attribute name='tfq_amount' />
                                     <order attribute='tfq_name' descending='false' />
                                     <filter type='and'>
                                     <condition attribute='tfq_order' operator='eq' value='{0}' />
                                     </filter>
                                   </entity>
                                </fetch>";
                qrey = string.Format(qrey, Id);
                EntityCollection entityCol = _orgService.RetrieveMultiple(new FetchExpression(qrey));
                if (entityCol != null && entityCol.Entities.Count > 0)
                {
                    foreach (var entity in entityCol.Entities)
                    {
                        itemDM = new OrderItemDM();
                        itemDM.Id = entity.Id;
                        if (entity.Contains("tfq_name"))
                            itemDM.Name = (string)entity["tfq_name"];
                        if (entity.Contains("tfq_quantity"))
                            itemDM.Quantity = (int)entity["tfq_quantity"];
                        if (entity.Contains("tfq_itemmaster"))
                        {
                            itemDM.ItemMasterId = ((EntityReference)entity["tfq_itemmaster"]).Id;
                            itemDM.ItemMasterName = ((EntityReference)entity["tfq_itemmaster"]).Name;
                        }
                        if (entity.Contains("tfq_amount"))
                            itemDM.Amount = decimal.Parse((((Money)entity["tfq_amount"]).Value).ToString("0.00"));
                        if (entity.Contains("tfq_price"))
                            itemDM.Price = decimal.Parse((((Money)entity["tfq_price"]).Value).ToString("0.00"));
                        itemList.Add(itemDM);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Opps something went wrong.." + ex);
            }
            return itemList;
        }
        #endregion

        #region GetOrderById
        public OrderDM GetOrderById(Guid Id)
        {
            OrderDM itemDM = null;
            try
            {
               
                Entity entity = _orgService.Retrieve("tfq_order", Id, new ColumnSet(true));
                if (entity != null)
                {
                    
                        itemDM = new OrderDM();
                        itemDM.Id = entity.Id;
                        if (entity.Contains("tfq_name"))
                            itemDM.Name = (string)entity["tfq_name"];
                        if (entity.Contains("tfq_address"))
                            itemDM.Address = (string)entity["tfq_address"];
                        if (entity.Contains("tfq_mobilephone"))
                            itemDM.ContactNo = (string)entity["tfq_mobilephone"];
                        if (entity.Contains("statuscode"))
                        {
                            itemDM.StatusReasonValue = ((OptionSetValue)entity["statuscode"]).Value;
                            itemDM.StatusReasonText = entity.FormattedValues["statuscode"];
                        }
                        if (entity.Contains("tfq_paymentmode"))
                        {
                            itemDM.PaymentModeValue = ((OptionSetValue)entity["tfq_paymentmode"]).Value;
                            itemDM.PaymentModeText = entity.FormattedValues["tfq_paymentmode"];
                        }
                        if (entity.Contains("tfq_amount"))
                            itemDM.Amount = decimal.Parse((((Money)entity["tfq_amount"]).Value).ToString("0.00"));
                        if (entity.Contains("tfq_totalamount"))
                            itemDM.TotalAmount = decimal.Parse((((Money)entity["tfq_totalamount"]).Value).ToString("0.00"));
                    
                    return itemDM;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Opps something went wrong.." + ex);
            }
            return itemDM;
        }
        #endregion

        #region UpdateOrderStatus 

        //For In-Progress,Ready To Move,  Assigned, Shipped, Delivered, Cancelled, Undelivered
        public OrderDM UpdateOrderStatus(Guid Id, int statusno)
        {
            OrderDM order = new OrderDM();
            order=GetOrderById(Id);
            Entity en = new Entity("tfq_order");
            en.Id = Id;
            if (order.StatusReasonValue > 0)
                en["statuscode"] = new OptionSetValue(statusno);
            _orgService.Update(en);
            return order;

        }
            //Delivery Boy Assigning when he pick the order
        public void DeliveryAssigning(Guid Id, Guid Uid)
        {
            OrderDM order = new OrderDM();
            Entity en = new Entity("tfq_order");
            en.Id = Id;
            order.DeliveredById = Uid;
            if (order.DeliveredById != Guid.Empty)
                en["tfq_deliveredby"] = new EntityReference("contact", order.DeliveredById);
            _orgService.Update(en);
        }
            //Diapatched By Assigning
        public void DispatchedAssigning(Guid Id, Guid Uid)
        {
            OrderDM order = new OrderDM();
            Entity en = new Entity("tfq_order");
            en["tfq_orderid"] = Id;
            order.DispatchedById = Uid;
            if (order.DispatchedById != Guid.Empty)
                en["tfq_diapatchedby"] = new EntityReference("contact", order.DispatchedById);
            _orgService.Update(en);
        }
        #endregion
    }
}
