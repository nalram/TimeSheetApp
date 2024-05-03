using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using timesheetapps.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using ClosedXML.Excel;
using System.IO;
using ReflectionIT.Mvc.Paging;

namespace timesheetapps.Controllers
{
    public class TimeSheetController : Controller
    {
        private readonly ConnectionStringClass _cc;
        public TimeSheetController(ConnectionStringClass cc)
        {
            _cc = cc;          
        }       

        public IActionResult Index(string RecordStatus, DateTime? start, DateTime? end, string NameFilter, string AccountFilter, bool excel, int pageindex = 1)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int LoginRoleId = Convert.ToInt32(HttpContext.Session.GetInt32("LoginRoleID"));
                    int LoginUserId = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));

                    listUsersandAccount();

                    if (pageindex != 1)
                    {
                        HttpContext.Session.SetString("PageChanged", "1");
                    }

                    if (RecordStatus == null && HttpContext.Session.GetString("PageChanged") == "1")
                    {
                        //Maintain the filters
                        RecordStatus = HttpContext.Session.GetString("RecordFilter");
                        start = Convert.ToDateTime(HttpContext.Session.GetString("StartDateFilter"));
                        end = Convert.ToDateTime(HttpContext.Session.GetString("EndDateFilter"));
                        NameFilter = HttpContext.Session.GetString("dispNameFilter");
                        AccountFilter = HttpContext.Session.GetString("dispAccountFilter");
                    }

                    //Record Filter
                    ViewData["RecordFilter"] = RecordStatus;
                    int dispRecordStatus = 1;
                    if (!String.IsNullOrEmpty(RecordStatus))
                    {
                        dispRecordStatus = Convert.ToInt32(RecordStatus);
                        HttpContext.Session.SetString("RecordFilter", RecordStatus);
                    }
                    else
                    {
                        HttpContext.Session.SetString("RecordFilter", "1");
                    }

                    //Date Filter
                    DateTime dispStartDate;
                    DateTime dispEndDate;
                    int deltaSunday = DayOfWeek.Sunday - DateTime.Now.DayOfWeek;
                    //On Sunday
                    if (deltaSunday == 0)
                    {
                        // On Sunday shows current week monday to sunday
                        int delta = DayOfWeek.Monday - DateTime.Now.DayOfWeek;
                        //dispStartDate = DateTime.Now.AddDays(delta == 1 ? -6 : delta);
                        dispStartDate = DateTime.Today.AddDays(delta == 1 ? -6 : delta);
                        dispEndDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Sunday);
                    }
                    else
                    {
                        // On Sunday shows next week monday to sunday, other days this shows correctly
                        dispStartDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
                        dispEndDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 7);
                    }

                    if (start != null)
                    {
                        dispStartDate = (DateTime)start;
                        ViewData["StartDateFilter"] = String.Format("{0:yyyy-MM-dd}", start);
                        HttpContext.Session.SetString("StartDateFilter", (String.Format("{0:yyyy-MM-dd}", start)));
                    }
                    else
                    {
                        ViewData["StartDateFilter"] = dispStartDate.ToString("yyyy-MM-dd");
                        HttpContext.Session.SetString("StartDateFilter", (String.Format("{0:yyyy-MM-dd}", dispStartDate)));
                    }
                    if (end != null)
                    {
                        dispEndDate = (DateTime)end;
                        ViewData["EndDateFilter"] = String.Format("{0:yyyy-MM-dd}", end);
                        HttpContext.Session.SetString("EndDateFilter", (String.Format("{0:yyyy-MM-dd}", end)));
                    }
                    else
                    {
                        ViewData["EndDateFilter"] = dispEndDate.ToString("yyyy-MM-dd");
                        HttpContext.Session.SetString("EndDateFilter", (String.Format("{0:yyyy-MM-dd}", dispEndDate)));
                    }

                    //User Name Filter
                    ViewData["NameFilter"] = NameFilter;
                    int dispNameFilter = 0;

                    if (LoginRoleId != 1)
                    {
                        dispNameFilter = LoginUserId;
                        ViewData["NameFilter"] = LoginUserId;
                        HttpContext.Session.SetString("dispNameFilter", LoginUserId.ToString());
                    }
                    else
                    {
                        if (!String.IsNullOrEmpty(NameFilter))
                        {
                            dispNameFilter = Convert.ToInt32(NameFilter);
                            HttpContext.Session.SetString("dispNameFilter", NameFilter);
                        }
                        else
                        {
                            HttpContext.Session.SetString("dispNameFilter", "0");
                        }
                    }

                    //Account Filter
                    ViewData["AccountFilter"] = AccountFilter;
                    int dispAccountFilter = 0;
                    if (!String.IsNullOrEmpty(AccountFilter))
                    {
                        dispAccountFilter = Convert.ToInt32(AccountFilter);
                        HttpContext.Session.SetString("dispAccountFilter", AccountFilter);
                    }
                    else
                    {
                        HttpContext.Session.SetString("dispAccountFilter", "0");
                    }

                    var questionQuery = (from ts in _cc.TimeSheet
                                         where ts.RecordStatus == dispRecordStatus
                                         //&& ts.createDate.Date >= dispStartDate && ts.createDate.Date <= dispEndDate
                                         && ts.StartDate.Date >= dispStartDate && ts.StartDate.Date <= dispEndDate
                                         && ((LoginRoleId == 1 && dispNameFilter == 0) ||
                                         (LoginRoleId == 1 && dispNameFilter != 0 && ts.UserID == dispNameFilter) ||
                                         (LoginRoleId != 1 && ts.UserID == dispNameFilter))
                                         && (dispAccountFilter == 0 || ts.AccountID == dispAccountFilter)
                                         join u in _cc.Users on ts.UserID equals u.UserID
                                         join a in _cc.Account on ts.AccountID equals a.AccountID
                                         select new DisplayTimeSheetClass
                                         {
                                             RecordID = ts.RecordID,
                                             UserID = ts.UserID,
                                             FullName = u.FullName,
                                             AccountID = ts.AccountID,
                                             AccountName = a.AccountName,
                                             ID = ts.ID,
                                             Job = ts.Job,
                                             StartDate = ts.StartDate,
                                             Hours = ts.Hours,
                                             Rate = ts.Rate,
                                             Chargeable = ts.Chargeable,
                                             RecordStatus = ts.RecordStatus,
                                             Description = ts.Description,
                                             CreateUserID = ts.CreateUserID,
                                             createDate = ts.createDate,
                                             ModifyUserID = ts.ModifyUserID,
                                             ModifyDate = ts.ModifyDate,
                                             LastName = u.LastName,
                                             FirstName = u.FirstName
                                         })
                                            .AsNoTracking()
                                            .OrderBy(q => q.StartDate).ThenBy(q => q.FirstName);
                                            //.OrderByDescending(q => q.createDate).ThenBy(q => q.FirstName);

                    //Export to Excel
                    if (excel == true)
                    {
                        //var DisplayRecords = _cc.DisplayTimeSheet.FromSqlRaw("DisplayRecords @p0, @p1, @p2, @p3, @p4, @p5, @p6", LoginUserId, LoginRoleId, dispRecordStatus, 
                        //    dispStartDate, dispEndDate, dispNameFilter, dispAccountFilter).ToList();

                        var DisplayRecords = questionQuery.ToList();
                        if (DisplayRecords.Count != 0)
                        {
                            return ExportToExcel(DisplayRecords);                            
                        }
                    }
                    //return View(DisplayRecords);
                    var pagedResult = PagingList.Create(questionQuery, 25, pageindex);
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

        FileContentResult ExportToExcel(List<DisplayTimeSheetClass> DisplayRecords)
        {
            string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            string fileName = "TimeSheet.xlsx";
            using (var workbook = new XLWorkbook())
            {
                IXLWorksheet worksheet =
                workbook.Worksheets.Add("TimeSheet");
                worksheet.Row(1).Style.Font.Bold = true;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#C0C0C0");
                worksheet.Column(7).Style.NumberFormat.Format = "######0.00";
                worksheet.Column(8).Style.NumberFormat.Format = "######0.00";

                worksheet.Cell(1, 1).Value = "Last";
                worksheet.Cell(1, 2).Value = "First";
                worksheet.Cell(1, 3).Value = "ID";
                worksheet.Cell(1, 4).Value = "Start date";
                worksheet.Cell(1, 5).Value = "Chargeable Y/N";
                worksheet.Cell(1, 6).Value = "Account";
                worksheet.Cell(1, 7).Value = "Hours";
                worksheet.Cell(1, 8).Value = "Rate";
                worksheet.Cell(1, 9).Value = "Job";
                worksheet.Cell(1, 10).Value = "Description";
                for (int index = 1; index <= DisplayRecords.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value =
                    DisplayRecords[index - 1].LastName;
                    worksheet.Cell(index + 1, 2).Value =
                    DisplayRecords[index - 1].FirstName;
                    worksheet.Cell(index + 1, 3).Value =
                    DisplayRecords[index - 1].ID;
                    worksheet.Cell(index + 1, 4).Value =
                    DisplayRecords[index - 1].StartDate;
                    if (DisplayRecords[index - 1].Chargeable)
                        worksheet.Cell(index + 1, 5).Value = "Chargeable";
                    else
                        worksheet.Cell(index + 1, 5).Value = "Non-Chargeable";
                    worksheet.Cell(index + 1, 6).Value =
                    DisplayRecords[index - 1].AccountName;
                    worksheet.Cell(index + 1, 7).Value =
                    DisplayRecords[index - 1].Hours;
                    worksheet.Cell(index + 1, 8).Value =
                    DisplayRecords[index - 1].Rate;
                    worksheet.Cell(index + 1, 9).Value =
                    DisplayRecords[index - 1].Job;
                    worksheet.Cell(index + 1, 10).Value =
                    DisplayRecords[index - 1].Description;
                }

                if (HttpContext.Session.GetInt32("LoginRoleID") != 1)
                {
                    worksheet.Column(1).Delete();
                    worksheet.Column(1).Delete();
                    worksheet.Column(1).Delete();
                    worksheet.Column(2).Delete();
                    worksheet.Column(4).Delete();
                    worksheet.Column(4).Delete();
                }

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, contentType, fileName);
                }
            }
        }

        void listUsersandAccount()
        {
            List<UserClass> UserList = new List<UserClass>();
            UserList = (from u in _cc.Users where u.IsActive == true orderby u.FirstName select u).ToList();
            ViewBag.UserList = UserList;

            List<AccountClass> AccountList = new List<AccountClass>();
            AccountList = (from a in _cc.Account where a.IsActive == true orderby a.AccountName select a).ToList();
            ViewBag.AccountList = AccountList;
        }

        void ErrorLog(Exception ex)
        {
            log4net.ILog log;
            log4net.Config.XmlConfigurator.Configure();
            log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            log.Error(ex.Message, ex);
        }

        public IActionResult Create()
        {
            listUsersandAccount();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TimeSheetClass ts, String hndLock)
        {
            listUsersandAccount();

            if (!ModelState.IsValid)
            {
                if(!String.IsNullOrEmpty(ts.Description))
                {
                    ts.Description = ts.Description.Replace("\r\n", " "); //.Substring(0, 250);
                    ts.Description = ts.Description.Replace("\n", " ");
                }

                if (!String.IsNullOrEmpty(ts.NonChargeReason) && !string.IsNullOrWhiteSpace(ts.NonChargeReason))
                {
                    ts.NonChargeReason = ts.NonChargeReason.Replace("\r\n", " ");
                    ts.NonChargeReason = ts.NonChargeReason.Replace("\n", " ");
                }

                ModelState.Clear();
                TryValidateModel(ts);
            }
            else
            {
                try
                {
                    if (HttpContext.Session.GetInt32("LoginRoleID") == 1)
                    {                        
                        if (!String.IsNullOrEmpty(hndLock))
                        {
                            ts.RecordStatus = Convert.ToInt32(hndLock);
                        }

                        if (ts.Chargeable == true)
                        {
                            ts.NonChargeReason = "";
                            if (ts.Rate == 0 || ts.Rate == null)
                            {
                                ts.Rate = Convert.ToDecimal(0.00);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(ts.NonChargeReason) && !string.IsNullOrWhiteSpace(ts.NonChargeReason))
                            {
                                ts.NonChargeReason = ts.NonChargeReason.Trim();
                            }
                            ts.Rate = Convert.ToDecimal(0.00);
                        }
                    }
                    else
                    {
                        ts.Rate = Convert.ToDecimal(0.00);
                        ts.NonChargeReason = "";
                        ts.UserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                    }

                    if (!string.IsNullOrWhiteSpace(ts.Description))
                    {
                        ts.Description = ts.Description.Trim();
                    }
                    else
                    {
                        ts.Description = "";
                    }                                       

                    ts.CreateUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                    ts.createDate = DateTime.Now;

                    _cc.Add(ts);
                    await _cc.SaveChangesAsync();
                    ViewBag.SuccessMsg = "Record successfully added !";

                    ModelState.Clear();
                    return View();
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }               
            }
            return View(ts);
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
                        listUsersandAccount();

                        var getSelectedRecord = await _cc.TimeSheet.FindAsync(id);
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
        public async Task<IActionResult> Edit(TimeSheetClass ts, String hndLock)
        {
            listUsersandAccount();

            if (!ModelState.IsValid)
            {                
                if (!String.IsNullOrEmpty(ts.Description))
                {
                    ts.Description = ts.Description.Replace("\r\n", " ");
                    ts.Description = ts.Description.Replace("\n", " ");
                }

                if (!String.IsNullOrEmpty(ts.NonChargeReason) && !string.IsNullOrWhiteSpace(ts.NonChargeReason))
                {
                    ts.NonChargeReason = ts.NonChargeReason.Replace("\r\n", " ");
                    ts.NonChargeReason = ts.NonChargeReason.Replace("\n", " ");
                }

                ModelState.Clear();
                TryValidateModel(ts);
            }
            else
            {
                try
                {
                    if (HttpContext.Session.GetInt32("LoginRoleID") == 1)
                    {
                        if (!String.IsNullOrEmpty(hndLock))
                        {
                            ts.RecordStatus = Convert.ToInt32(hndLock);
                        }

                        if (ts.Chargeable == true)
                        {
                            ts.NonChargeReason = "";
                            if (ts.Rate == 0 || ts.Rate == null)
                            {
                                ts.Rate = Convert.ToDecimal(0.00);
                            }
                        }
                        else
                        {
                            if (!String.IsNullOrEmpty(ts.NonChargeReason) && !string.IsNullOrWhiteSpace(ts.NonChargeReason))
                            {
                                ts.NonChargeReason = ts.NonChargeReason.Trim();
                            }
                            ts.Rate = Convert.ToDecimal(0.00);
                        }
                    }
                    else
                    {
                        ts.Rate = Convert.ToDecimal(0.00);
                        ts.NonChargeReason = "";
                    }

                    if (!string.IsNullOrWhiteSpace(ts.Description))
                    {
                        ts.Description = ts.Description.Trim();
                    }
                    else
                    {
                        ts.Description = "";
                    }

                    ts.ModifyUserID = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));
                    ts.ModifyDate = DateTime.Now;

                    _cc.Update(ts);
                    await _cc.SaveChangesAsync();

                    return RedirectToAction("Index", new
                    {
                        RecordStatus = HttpContext.Session.GetString("RecordFilter"),
                        start = HttpContext.Session.GetString("StartDateFilter"),
                        end = HttpContext.Session.GetString("EndDateFilter"),
                        NameFilter = HttpContext.Session.GetString("dispNameFilter"),
                        AccountFilter = HttpContext.Session.GetString("dispAccountFilter")
                    });
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }                
            }
            return View(ts);
        }

        public async Task<IActionResult> Delete (int? id)
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
                        var getSelectedRecord = await _cc.TimeSheet.FindAsync(id);

                        _cc.Remove(getSelectedRecord);
                        await _cc.SaveChangesAsync();

                        return RedirectToAction("Index", new
                        {
                            RecordStatus = getSelectedRecord.RecordStatus,
                            start = HttpContext.Session.GetString("StartDateFilter"),
                            end = HttpContext.Session.GetString("EndDateFilter"),
                            NameFilter = HttpContext.Session.GetString("dispNameFilter"),
                            AccountFilter = HttpContext.Session.GetString("dispAccountFilter")
                        });
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

        public async Task<IActionResult> MoveToArchive(DateTime? start, DateTime? end, string NameFilter, string AccountFilter)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int LoginUserId = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));

                    //string conn = "Data Source=LAPTOP-BH62EAR9;Initial Catalog=timesheetapps;Integrated Security=True";
                    //SqlConnection sqlcon = new SqlConnection(conn);
                    //SqlCommand sqlcomm = new SqlCommand("MoveToArchive", sqlcon);
                    //sqlcomm.CommandType = CommandType.StoredProcedure;
                    //sqlcomm.Parameters.Add("@start", SqlDbType.DateTime).Value = start;
                    //sqlcomm.Parameters.Add("@end", SqlDbType.DateTime).Value = end;
                    //sqlcomm.Parameters.Add("@NameFilter", SqlDbType.Int).Value = NameFilter;
                    //sqlcomm.Parameters.Add("@AccountFilter", SqlDbType.Int).Value = AccountFilter;
                    //sqlcomm.Parameters.Add("@ModifyUserID", SqlDbType.Int).Value = LoginUserId;
                    //sqlcomm.Parameters.Add("@ModifyDate", SqlDbType.DateTime).Value = DateTime.Now;
                    //sqlcon.Open();
                    //await sqlcomm.ExecuteNonQueryAsync();
                    //sqlcon.Close();

                    //var command = _cc.Database.GetDbConnection().CreateCommand();
                    //command.CommandText = "update timesheet set RecordStatus = 3 where recordid = 86";
                    //_cc.Database.OpenConnection();
                    //command.ExecuteNonQuery();
                    //_cc.Database.CloseConnection();

                    var command = _cc.Database.GetDbConnection().CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    List<SqlParameter> prm = new List<SqlParameter>()
                     {
                         new SqlParameter("@start", SqlDbType.DateTime) {Value = start},
                         new SqlParameter("@end", SqlDbType.DateTime) {Value = end},
                         new SqlParameter("@NameFilter", SqlDbType.Int) {Value = NameFilter},
                         new SqlParameter("@AccountFilter", SqlDbType.Int) {Value = AccountFilter},
                         new SqlParameter("@ModifyUserID", SqlDbType.Int) {Value = LoginUserId},
                         new SqlParameter("@ModifyDate", SqlDbType.DateTime) {Value = DateTime.Now},
                     };
                    command.Parameters.AddRange(prm.ToArray());
                    command.CommandText = "MoveToArchive";
                    await _cc.Database.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync();
                    _cc.Database.CloseConnection();

                    return RedirectToAction("Index", new
                    {
                        RecordStatus = HttpContext.Session.GetString("RecordFilter"),
                        start = HttpContext.Session.GetString("StartDateFilter"),
                        end = HttpContext.Session.GetString("EndDateFilter"),
                        NameFilter = HttpContext.Session.GetString("dispNameFilter"),
                        AccountFilter = HttpContext.Session.GetString("dispAccountFilter")
                    });
                }
                catch (Exception ex)
                {
                    ErrorLog(ex);

                    return View(ex.Message);
                }
            }
            return View();            
        }

        public async Task<IActionResult> BulkLock(DateTime? start, DateTime? end, string NameFilter, string AccountFilter)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int LoginUserId = Convert.ToInt32(HttpContext.Session.GetInt32("LoginUserID"));

                    var command = _cc.Database.GetDbConnection().CreateCommand();
                    command.CommandType = CommandType.StoredProcedure;
                    List<SqlParameter> prm = new List<SqlParameter>()
                     {
                         new SqlParameter("@start", SqlDbType.DateTime) {Value = start},
                         new SqlParameter("@end", SqlDbType.DateTime) {Value = end},
                         new SqlParameter("@NameFilter", SqlDbType.Int) {Value = NameFilter},
                         new SqlParameter("@AccountFilter", SqlDbType.Int) {Value = AccountFilter},
                         new SqlParameter("@ModifyUserID", SqlDbType.Int) {Value = LoginUserId},
                         new SqlParameter("@ModifyDate", SqlDbType.DateTime) {Value = DateTime.Now},
                     };
                    command.Parameters.AddRange(prm.ToArray());
                    command.CommandText = "BulkLock";
                    await _cc.Database.OpenConnectionAsync();
                    await command.ExecuteNonQueryAsync();
                    _cc.Database.CloseConnection();

                    return RedirectToAction("Index", new
                    {
                        RecordStatus = HttpContext.Session.GetString("RecordFilter"),
                        start = HttpContext.Session.GetString("StartDateFilter"),
                        end = HttpContext.Session.GetString("EndDateFilter"),
                        NameFilter = HttpContext.Session.GetString("dispNameFilter"),
                        AccountFilter = HttpContext.Session.GetString("dispAccountFilter")
                    });
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
