using DAL;
using DataModel;
using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace InnoAssignment1.Controllers
{
    public class LoginController : Controller
    {

       
        

        public ActionResult LoginSubmit()
        {
            CRMDAL crmdl = new CRMDAL();
            try
            {
                string email = Request["email"];
                string password = Request["password"];

                ContactDM contactDM = crmdl.GetContactByEmailAndPassword(email, password);
                if (contactDM != null)
                {
                    Session["userDM"] = contactDM;
                    if (contactDM.ContactTypeText == "Customer")
                    {
                        return RedirectToAction("Customer", "Order");
                    }
                    else if (contactDM.ContactTypeText == "Store keeper")
                    {
                        return RedirectToAction("Shopkeeper", "Order");
                    }
                    else
                    {
                        return RedirectToAction("Deliveryboy", "Order");
                    }
                }
                else
                {
                    return Content("Inncorrect Email or password");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
        public ActionResult Login()
        {
            return View();
        }
        public ActionResult ForgetPassword()
        {
            CRMDAL crmd = new CRMDAL();

            try
            {


                String email = Request["email"];
                ContactDM conPM = crmd.GetContactByEmail(email);
                conPM.ForgetPassword = true;
                if (conPM != null)
                {
                    Guid guid = crmd.ChangePassword(conPM);
                    return Content("new password has been sent to your email address");
                }
                else
                {
                    return Content("Email doesn't match check your email. ( Register first if you haven't ) ");
                }
            }
            catch (Exception e)
            {
                return Content("Ooops somthing went wrong.." + e.Message);
            }

        }
        public ActionResult Forget()
        {
            return View();
        }
        public ActionResult Logout()
        {
            Session["userDM"] = null;
            Session["order"] = null;
            return RedirectToAction("Login", "Login");
        }
        public ActionResult ChangePassword()
        {
            CRMDAL crmdl = new CRMDAL();
            if (Session["userDm"] != null)
            {
                ContactDM cdm = (ContactDM)Session["userDm"];    
                
                if (Request["newPassword"] != Request["password"])
                {
                    crmdl.ChagnePassAfterLogin(cdm.Id, Request["newPassword"]);
                    return Content("Password Changed Sucessfully. CLick here to login again : https://localhost:44364/");
                    

                }
                else
                {
                    return Content("New Password and Old Password Should not be same..");
                }

            }
            else
            {
                return RedirectToAction("Login", "Login");
            }
        }
        public ActionResult ChangePass()
        {
            return View();
        }
    }
}