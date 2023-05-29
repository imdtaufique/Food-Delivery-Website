using DAL;
using DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Navigation;

namespace InnoAssignment1.Controllers
{
    public class SignupController : Controller
    {
        // Creating new account only Customer may create new account....
        public ActionResult Signup()
        {
           return View();   
        } 
        public ActionResult SignupSubmit() 
        {
         CRMDAL cdn = new CRMDAL();
            try
            {
                ContactDM contactdm = new ContactDM();
                contactdm.FirstName = Request["firstname"];
                contactdm.LastName = Request["lastname"];
                contactdm.GenderValue = Convert.ToInt32(Request["gender"]);
                contactdm.DateOfBirth = Convert.ToDateTime(Request["birthdate"]);
                contactdm.Email= Request["email"];
                contactdm.MobilePhone = Request["mobile"];
                contactdm.Password= Request["password"];
                contactdm.ContactTypeValue = 227830000;

                Guid guid = cdn.CreateContactByEmailAndPassword(contactdm);

                if (guid != Guid.Empty) 
                {
                    return Content("Registered Succesfully");
                }
                else
                {
                    return Content("Try again");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("OOps somethinbg went wrong" +e.Message);
            }
            return View();
        }

       
    }
}