using DAL;
using DataModel;
using System;
using System.Web.Mvc;
using InnoAssignment1.ViewModel;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Activities;
using System.Windows.Controls;

namespace InnoAssignment1.Controllers
{
    public class OrderController : Controller
    {
        CRMDAL dal = new CRMDAL();

        public ActionResult Customer()
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDM = (ContactDM)Session["userDM"];
                ViewBag.OrderList = dal.GetOrderByGuid(userDM, "tfq_customer");
                ViewBag.User = Session["userDm"];
                return View();
            }
            else { return RedirectToAction("Login", "Login"); }
        }
        public ActionResult Shopkeeper()
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDM = (ContactDM)Session["userDM"];
                ViewBag.OrderList = dal.ViewOrderForShopkeeper();
                ViewBag.User = Session["userDm"];
                return View();
            }
            else { return RedirectToAction("Login", "Login"); }
        }
        public ActionResult Deliveryboy()
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDM = (ContactDM)Session["userDM"];
                ViewBag.OrderList = dal.ViewOrderDeliveryBoy(userDM);
                ViewBag.User = Session["userdm"];
                return View();
            }
            else { return RedirectToAction("Login", "Login"); }
        }
        public ActionResult CreateNewOrder()
        {
            if (Session["userDm"] != null)
            {
                //ContactDM user = Session["userDm"];
                CRMDAL cdm = new CRMDAL();
                try
                {
                    OrderDM odm = new OrderDM();
              
                    odm.Address = Request["address"];
                    odm.City = Request["city"];
                    odm.State = Request["state"];
                    odm.Landmark = Request["landmark"];
                    odm.ContactNo = Request["mobileno"];
                    odm.PaymentModeValue = Convert.ToInt32(Request["paymentmethod"]);
                    //odm.CustomerId = user.Id;
                    odm.Id = cdm.CreateOrder(odm, (ContactDM)Session["userDM"]);
                    if (odm.Id != Guid.Empty)
                    {
                        Session["order"] = odm;
                        return RedirectToAction("ViewOrder", "Order", new {Id=odm.Id});
                    }
                    else
                    {
                        return Content("Something went wrong...Try again");
                    }
                }
                catch (Exception e)
                {
                    Console.Write("Oooops something went wrong.." + e);
                }
                return RedirectToAction("CreateOrder", "Order");
            }
            else { return RedirectToAction("Login", "Login"); }
        }
        public ActionResult CreateOrder()
        {
            return View();
        }
        public ActionResult UpdateOrderStatus(Guid Id)
        {
            if (Session["userDm"] != null)
            {    OrderDM odm = new OrderDM();
                 odm = dal.GetOrderById(Id);
                ContactDM userDm = (ContactDM)Session["userDm"];

                if (odm.StatusReasonValue == 1)
                {
                    OrderDM order = dal.UpdateOrderStatus(Id, 227830000);
                    return RedirectToAction("Customer", "Order");
                }
                else if (odm.StatusReasonValue == 227830000)
                {
                    OrderDM order = dal.UpdateOrderStatus(Id, 227830001);
                    return RedirectToAction("Shopkeeper", "Order");
                }
                else if (odm.StatusReasonValue == 227830001)
                {
                    OrderDM order = dal.UpdateOrderStatus(Id, 227830002);
                    dal.DeliveryAssigning(Id, userDm.Id);
                    return RedirectToAction("Deliveryboy", "Order");
                }
                else if (odm.StatusReasonValue == 227830002)
                {
                    OrderDM order = dal.UpdateOrderStatus(Id, 227830003);
                    dal.DispatchedAssigning(Id, userDm.Id);
                    return RedirectToAction("Shopkeeper", "Order");
                }
                else if (odm.StatusReasonValue == 227830003)
                {
                    OrderDM order = dal.UpdateOrderStatus(Id, 227830004);
                    return RedirectToAction("Deliveryboy", "Order");
                }
                else
                {
                    return Content("Status Not Mactched Contact customer care..");
                }
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult CancelOrder(Guid Id)
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDm = (ContactDM)Session["userDm"];

                OrderDM order = dal.UpdateOrderStatus(Id, 227830005);
                return RedirectToAction("Customer", "Order");

            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult Undellivered(Guid Id)
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDm = (ContactDM)Session["userDm"];

                OrderDM order = dal.UpdateOrderStatus(Id, 227830006);
                return RedirectToAction("Deliveryboy", "Order");

            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult ViewOrder(Guid Id)
        {
            if (Session["userDm"] != null)
            {

                ViewOrder_VM order_VM = new ViewOrder_VM();
                order_VM.Order = dal.GetOrderById(Id);
                Session["order"] = order_VM.Order;
                order_VM.OrderItems = dal.GetOrderItemsByOrderId(Id);
                order_VM.ItemMasters = dal.GetItemMasterList();
                return View(order_VM);
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult AddItemsInCustomerOrder()
        {
            if (Session["userDm"] != null)
            {
                ContactDM userDm = (ContactDM)Session["userDm"];
                OrderDM orderDm = (OrderDM)Session["order"];
                ItemMasterDM item = null;
                List<ItemMasterDM> lim = new List<ItemMasterDM>();
                lim = dal.GetItemMasterList();

                OrderItemDM orderItemDM = new OrderItemDM();
                orderItemDM.Name = string.Format("Item For {0}", string.Concat(userDm.FirstName, userDm.LastName));
                orderItemDM.OrderId = orderDm.Id;
                orderItemDM.ItemMasterId = Guid.Parse(Request["item"]);
                orderItemDM.Quantity = Convert.ToInt32(Request["quantity"]);

                foreach (var dalt in lim)
                {
                    if(dalt.Id == orderItemDM.ItemMasterId)
                    {
                        item = new ItemMasterDM();
                        item = dalt;
                    }
                }
                orderItemDM.Price = item.Price;
                orderItemDM.Id = dal.AddItemInCustomerOrder(orderItemDM);
                return RedirectToAction("ViewOrder", "Order", new {Id=orderDm.Id});
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult DeleteOrderItem(Guid Id)
        {
            if (Session["userDm"] != null)
            {
                dal.DeleteOrderItemByOrderId(Id);
                return RedirectToAction("ViewOrder", "Order", new { Id = ((OrderDM)Session["order"]).Id });
            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
    }
} 