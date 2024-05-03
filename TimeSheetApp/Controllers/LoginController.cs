using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using timesheetapps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using timesheetapps.Library;

namespace timesheetapps.Controllers
{
    public class LoginController : Controller
    {
        private readonly ConnectionStringClass _cc;
        public LoginController(ConnectionStringClass cc)
        {
            _cc = cc;
        }

        public IActionResult Index()
        {
            return View();           
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(LoginClass us)
        {
            if (ModelState.IsValid) //ModelState.Values.SelectMany(v => v.Errors).ElementAt(0); --> Check in immediate window
            {
                try
                {
                    if (!string.IsNullOrEmpty(us.EmailAddress))
                    {
                        var pwd = EncryptionLibrary.EncryptText(us.Password);

                        var query = from u in _cc.Users
                                        where u.EmailAddress.ToUpper() == us.EmailAddress.Trim().ToUpper()
                                        && u.Password == pwd
                                        && u.IsActive == true
                                    select new LoginClass
                                    {
                                            UserID = u.UserID,
                                            RoleID = u.RoleID,
                                            EmailAddress = u.EmailAddress,
                                            FirstName = u.FirstName,
                                            LastName = u.LastName
                                    };
                        var DisplayRecords = query.ToList();
                        if (DisplayRecords.ToList().Count != 0)
                        {
                            HttpContext.Session.SetString(key: "Logout", value: "0");
                            HttpContext.Session.SetInt32("LoginRoleID", Convert.ToInt32(DisplayRecords[0].RoleID));
                            HttpContext.Session.SetInt32("LoginUserID", Convert.ToInt32(DisplayRecords[0].UserID));

                            //var DisplayUser = Convert.ToString(DisplayRecords[0].EmailAddress).Split('@')[0];
                            //DisplayUser = DisplayUser.Replace('.', ' ');
                            //HttpContext.Session.SetString("DisplayUser", DisplayUser);
                            var DisplayUser = DisplayRecords[0].FirstName + ' ' + DisplayRecords[0].LastName;
                            HttpContext.Session.SetString("DisplayUser", DisplayUser);
                            
                            return RedirectToAction("Index", "TimeSheet");
                        }
                        else
                        {
                            ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                            return View();
                        }

                        //var loginUser = _cc.LoginUser.FromSqlRaw("login @p0", Convert.ToString(us.EmailAddress).Trim().ToUpper()).ToList();
                        //if (loginUser.Count != 0)
                        //{
                        //    HttpContext.Session.SetInt32("LoginRoleID", Convert.ToInt32(loginUser[0].RoleID));
                        //    HttpContext.Session.SetInt32("LoginUserID", Convert.ToInt32(loginUser[0].UserID));
                        //    return RedirectToAction("Index", "TimeSheet");
                        //}
                        //else
                        //{
                        //    ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                        //    return View();
                        //}
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View();
        }

        void ErrorLog(Exception ex)
        {
            log4net.ILog log;
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Error(ex.Message, ex);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                HttpContext.Session.Clear();
                HttpContext.Session.SetString(key:"Logout", value:"1");
                ViewData.Clear();
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                ErrorLog(ex);

                return View(ex.Message);
            }
        }
    }
}
