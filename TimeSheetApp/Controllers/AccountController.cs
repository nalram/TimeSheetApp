using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using timesheetapps.Models;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace timesheetapps.Controllers
{
    public class AccountController : Controller
    {
        private readonly ConnectionStringClass _cc;
        public AccountController(ConnectionStringClass cc)
        {
            _cc = cc;
        }

        public ActionResult Index(IFormCollection fc, string searchAccount, int? AfterChange,  int pageindex=1)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string radioIsActive = fc["AccountIsActive"];
                    bool dispIsActive = true;

                    if (radioIsActive == null && HttpContext.Session.GetString("PageChanged") == "1" && HttpContext.Session.GetString("ActiveFilter") == "0")
                    {
                        //Maintain the filters
                        dispIsActive = false;
                        ViewBag.AccountIsActive = "inactive";
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
                            ViewBag.AccountIsActive = "inactive";
                            HttpContext.Session.SetString("ActiveFilter", "0");
                        }
                        else
                        {
                            HttpContext.Session.SetString("ActiveFilter", "1");
                        }
                    }

                    var query = from a in _cc.Account
                                where a.IsActive == dispIsActive &&
                                (String.IsNullOrEmpty(searchAccount) || (!String.IsNullOrEmpty(searchAccount) && (a.AccountName.Contains(searchAccount))))
                                orderby a.AccountName
                                select a;
                    var pagedResult = PagingList.Create(query, 10, pageindex);
                    return View(pagedResult);
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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountClass account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var IsExist = _cc.Account.FirstOrDefault(e => e.AccountName.ToUpper() == account.AccountName.Trim().ToUpper());

                    if (IsExist == default)
                    {
                        account.AccountName = account.AccountName.Trim();
                        account.CreateUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                        account.createDate = DateTime.Now;

                        _cc.Add(account);
                        await _cc.SaveChangesAsync();

                        HttpContext.Session.SetString("PageChanged", "0");
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Account Already Exist !");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View(account);
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
                        var getSelectedRecord = await _cc.Account.FindAsync(id);
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
        public async Task<IActionResult> Edit(AccountClass account)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var IsExist = _cc.Account.FirstOrDefault(e => e.AccountName.ToUpper() == account.AccountName.Trim().ToUpper());

                    if (IsExist == default)
                    {
                        account.AccountName = account.AccountName.Trim();
                        account.ModifyUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                        account.ModifyDate = DateTime.Now;

                        _cc.Update(account);
                        await _cc.SaveChangesAsync();

                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Account Already Exist !");
                        return View();
                    }
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View(account);
        }
        public async Task<IActionResult> ChangeAccountStatus(int? id, int? Changeto)
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
                        var getAccount = await _cc.Account.FindAsync(id);
                        getAccount.IsActive = changeIsActive;
                        getAccount.ModifyUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                        getAccount.ModifyDate = DateTime.Now;
                        _cc.Update(getAccount);
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
