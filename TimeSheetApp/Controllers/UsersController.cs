using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using timesheetapps.Models;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;
using timesheetapps.Library;

namespace timesheetapps.Controllers
{
    public class UsersController : Controller
    {
        private readonly ConnectionStringClass _cc;
        public UsersController(ConnectionStringClass cc)
        {
            _cc = cc;
        }

        public ActionResult Index(IFormCollection fc, string searchEmailID, int? AfterChange, int pageindex = 1)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string radioIsActive = fc["UserIsActive"];
                    bool dispIsActive = true;

                    if (radioIsActive == null && HttpContext.Session.GetString("PageChanged") == "1" && HttpContext.Session.GetString("ActiveFilter") == "0")
                    {
                        //Maintain the filters
                        dispIsActive = false;
                        ViewBag.UserIsActive = "inactive";
                    }
                    else
                    {
                        if (pageindex != 1)
                        {
                            HttpContext.Session.SetString("PageChanged", "1");
                        }

                        if (radioIsActive == "0" || AfterChange == 1)
                        {
                            dispIsActive = false;
                            ViewBag.UserIsActive = "inactive";
                            HttpContext.Session.SetString("ActiveFilter", "0");
                        }
                        else
                        {
                            HttpContext.Session.SetString("ActiveFilter", "1");
                        }
                    }

                    var query = (from u in _cc.Users
                                 where u.IsActive == dispIsActive &&
                                 (String.IsNullOrEmpty(searchEmailID) || (!String.IsNullOrEmpty(searchEmailID) && (u.EmailAddress.Contains(searchEmailID))))
                                 join r in _cc.Roles on u.RoleID equals r.RoleID
                                 select new DisplayUsersClass
                                 {
                                     UserID = u.UserID,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     EmailAddress = u.EmailAddress,
                                     RoleName = r.RoleName,
                                     IsActive = u.IsActive
                                 })
                                            .AsNoTracking()
                                            .OrderBy(q => q.FirstName);

                    var pagedResult = PagingList.Create(query, 10, pageindex);
                    return View(pagedResult);

                    //IEnumerable<DisplayUsersClass> multipletable;
                    //multipletable = from u in _cc.Users
                    //                where u.IsActive == dispIsActive
                    //                join r in _cc.Roles on u.RoleID equals r.RoleID
                    //                select new DisplayUsersClass
                    //                {
                    //                    UserID = u.UserID,
                    //                    FirstName = u.FirstName,
                    //                    LastName = u.LastName,
                    //                    EmailAddress = u.EmailAddress,
                    //                    RoleName = r.RoleName,
                    //                    IsActive = u.IsActive
                    //                };
                    //multipletable = multipletable.OrderBy(u => u.FirstName);
                    //return View(multipletable);  
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View();                  
        }

        void listUserRole()
        {
            List<RolesClass> RoleList = new List<RolesClass>();
            RoleList = (from r in _cc.Roles orderby r.RoleName descending select r).ToList();
            ViewBag.RoleList = RoleList;
        }

        void ErrorLog(Exception ex)
        {
            log4net.ILog log;
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Error(ex.Message, ex);
        }

        public ActionResult Create()
        {
            listUserRole();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserClass user)
        {
            listUserRole();

            if (ModelState.IsValid)
            {
                try
                {
                    var IsExist = _cc.Users.FirstOrDefault(e => e.EmailAddress.ToUpper() == user.EmailAddress.Trim().ToUpper());

                    if (IsExist == default)
                    {
                        user.LastName = user.LastName.Trim();
                        user.FirstName = user.FirstName.Trim();
                        user.EmailAddress = user.EmailAddress.Trim();
                        user.Password = EncryptionLibrary.EncryptText(user.Password.Trim());
                        user.CreateUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                        user.createDate = DateTime.Now;

                        _cc.Add(user);
                        await _cc.SaveChangesAsync();

                        HttpContext.Session.SetString("PageChanged", "0");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        //ViewBag.SuccessMsg = "Email ID Already Exist !";
                        ModelState.AddModelError(string.Empty, "Email ID Already Exist !");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (id == null || id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        listUserRole();

                        var getSelectedRecord = await _cc.Users.FindAsync(id);
                        getSelectedRecord.Password = EncryptionLibrary.DecryptText(getSelectedRecord.Password);
                        return View(getSelectedRecord);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserClass user)
        {
            listUserRole();

            if (ModelState.IsValid)
            {
                try
                {
                    user.LastName = user.LastName.Trim();
                    user.FirstName = user.FirstName.Trim();
                    user.Password = EncryptionLibrary.EncryptText(user.Password.Trim());
                    user.ModifyUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                    user.ModifyDate = DateTime.Now;

                    _cc.Update(user);
                    await _cc.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }               
            }
            return View(user);
        }

        public async Task<IActionResult> ChangeUserStatus(int? id, int? Changeto)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    bool changeIsActive = true;
                    if (Changeto == 0)
                    {
                        changeIsActive = false;
                    }
                    if (id == null || id == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        var getUser = await _cc.Users.FindAsync(id);
                        getUser.IsActive = changeIsActive;
                        getUser.ModifyUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                        getUser.ModifyDate = DateTime.Now;
                        _cc.Update(getUser);
                        await _cc.SaveChangesAsync();
                    }
                    return RedirectToAction("Index", new { AfterChange = Changeto });
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View();
        }
    }
}
