using AppZim.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using PagedList;
using PdfSharp;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using static AppZim.Controllers.PushAutoController;

namespace AppZim.Areas.Admin.Controllers
{
    public class StaffController : BaseController
    {
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddMoneyReceiver(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var checkUserReceiver = MoneyReceiverTable.checkUserReceiverExists(f.Get("ddl-full-name").ToInt(0));
            if (checkUserReceiver == true)
            {
                SetAlert("Người này đã tồn tại trong danh sách nhận hộ", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }    
            //Người nhận hộ
            var receiver = AccountTable.getbyID(f.Get("ddl-full-name").ToInt(0));
            int receiverId = 0;
            if (receiver != null)
            {
                receiverId = receiver.ID;
            }
            string note = "";
            if (!string.IsNullOrEmpty(f.Get("txt-note")))
            {
                note = f.Get("txt-note").ToString();
            }
            string bankInfo = "";
            if (!string.IsNullOrEmpty(f.Get("txt-bank")))
            {
                bankInfo = f.Get("txt-bank").ToString();
            }
            var rs = MoneyReceiverTable.Insert(receiverId, note, bankInfo, aclog.FullName);
            if (rs != null)
            {
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            else
                SetAlert("Thêm mới thất bại", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }




        //Danh sách thông tin ng thu hộ
        public ActionResult MoneyReceiverList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });


            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
            }
            ViewBag.Search = Request.QueryString["search"];
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slSort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["slSort"].ToInt(0);
            }
            ViewBag.Sort = Request.QueryString["slSort"].ToInt(0);

            //sort type
            int sorttype = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slSortType"]))
            {
                ViewBag.SortType = sorttype = Request.QueryString["slSortType"].ToInt(0);
            }
            ViewBag.SortType = Request.QueryString["slSortType"].ToInt(0);

            int pageSize = 20;
            List<Admin_MoneyReceiver_GetList_Result> l = new List<Admin_MoneyReceiver_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_MoneyReceiver_GetList(search, sort, sorttype, pageSize, page).ToList();
            }

            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Any())
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = total;

            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }

        //cập nhật người thu hộ
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UpdateReceiverAjax(int id, int? userReceiverId,string note, string bankInfomation, double payment)
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1)// Superadmin/Manager/Saler
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                var rs = MoneyReceiverTable.getbyID(id);
                if (rs == null)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
                if (userReceiverId == 0)
                    userReceiverId = null;
                rs = MoneyReceiverTable.Update(id, userReceiverId, note, bankInfomation, aclog.FullName, payment);
                if (rs != null)
                    return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                int uid = 0;
                if (Session["UID"] != null)
                    uid = Session["UID"].ToString().ToInt(0);
                AssetCRM.Writelog("StaffController", "UpdateReceiverAjax", uid, ex.Message);
                //return RedirectToAction("Signin", "Login", new { area = "" });
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

        }

        /// <summary>
        /// Xóa thông tin người nhận hộ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteReceiver(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var rs = MoneyReceiverTable.Delete(id, aclog.UserName);
            if (rs == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }


        // tạo mới nhân viên
        public ActionResult CreateStaff()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            return View();
        }

        // danh sách nhân viên
        public ActionResult StaffList(int page = 1)
        {
            var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
            }
            ViewBag.Search = Request.QueryString["search"];
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            ViewBag.Sort = Request.QueryString["sort"].ToInt(0);
            List<get_list_of_staff_Result> l = new List<get_list_of_staff_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_staff(search, sort, pageSize, (page - 1) * pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        // danh sach thanh vien trong nhóm
        public ActionResult ListMember(int LeaderID, int page = 1)
        {
            var l = new List<Admin_Seller_GetListMember_Result>();
            double revenue = 0;
            int pageSize = 10;
            using (var db = new ZimEntities())
            {
                l = db.Admin_Seller_GetListMember(LeaderID, (page - 1) * pageSize, pageSize).ToList();
                revenue = db.Admin_GetTotalRevenue(LeaderID).FirstOrDefault() ?? 0;
            }
            var leader = AccountTable.getbyID(LeaderID);
            ViewBag.Revenue = revenue + leader.Revenue ?? 0;
            ViewBag.LeaderID = leader.ID;
            ViewBag.LeaderName = leader.FullName;
            ViewBag.Role2ID = leader.LeaderID;

            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        // danh sach thanh vien trong phòng
        public ActionResult ListMemberInRoom(int LeaderID)
        {
            var l = new List<tbl_Account>();
            double revenue = 0;
            using (var db = new ZimEntities())
            {
                l = db.tbl_Account.Where(x => x.IsDelete != true && x.LeaderID == LeaderID).ToList();
                revenue = db.Admin_GetTotalRevenue(LeaderID).FirstOrDefault() ?? 0;
            }
            var leader = AccountTable.getbyID(LeaderID);
            ViewBag.Revenue = revenue + leader.Revenue ?? 0;
            ViewBag.LeaderID = leader.ID;
            ViewBag.LeaderName = leader.FullName;
            return View(l);
        }
        [HttpPost]
        public ActionResult AddMember(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int leaderID = f.Get("LeaderID").ToInt(0);
            if (aclog.RoleID != 1)
            {
                if (aclog.ID != leaderID)
                {
                    return RedirectToAction("Signin", "Login", new { area = "" });
                }
            }

            int memberID = f.Get("MemberID").ToInt(0);
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(x => x.ID == memberID).FirstOrDefault();
                ck.LeaderID = leaderID;
                db.SaveChanges();
            }
            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }
        // nhân viên chi tiết
        public ActionResult StaffDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var staff = AccountTable.getbyID(id);
            if (staff == null)
            {
                SetAlert("Không tìm thấy nhân viên", "e");
                return RedirectToAction("StaffList", "Staff");
            }
            return View(staff);
        }
        [HttpPost]
        public ActionResult AssignSchool(int UID, int SchoolID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                bool isHide = false;
                var ck = AssignManagerSpecializeToSchoolTable.checkschool(UID, SchoolID);
                if (ck != null)
                    isHide = !ck.isHide.Value;
                AssignManagerSpecializeToSchoolTable.insertschool(UID, SchoolID, isHide, aclog.UserName);
            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }
        //cấu hình giáo viên
        public ActionResult ConfigurationTeacher(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4 || aclog.RoleID == 3 || aclog.RoleID == 6 || aclog.RoleID == 9)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var t = AccountTable.getbyID(id);
            if (t == null)
            {
                SetAlert("Không tìm thấy giáo viên này :(", "e");
                return RedirectToAction("TeacherList", "Staff");
            }
            if (t.RoleID != 4)
            {
                SetAlert("Không tìm thấy giáo viên này :(", "e");
                return RedirectToAction("TeacherList", "Staff");
            }

            int? month = null;
            if (!string.IsNullOrEmpty(Request.QueryString["slMonth"]))
            {
                month = Request.QueryString["slMonth"].ToInt(0);
            }
            ViewBag.Month = month;

            int? year = null;
            if (!string.IsNullOrEmpty(Request.QueryString["slYear"]))
            {
                year = Request.QueryString["slYear"].ToInt(0);
            }
            ViewBag.Year = year;

            int pageSize = 20;

            List<Admin_TeacherSalaryApproved_GetList_Result> listSalaryApproved = new List<Admin_TeacherSalaryApproved_GetList_Result>();
            using (var db = new ZimEntities())
            {
                listSalaryApproved = db.Admin_TeacherSalaryApproved_GetList(id, month, year, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (listSalaryApproved.Count > 0)
            {
                total = listSalaryApproved[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            ViewBag.ListSalaryApproved = listSalaryApproved;
            List<Admin_StatisticsTeacherSalaryApproved_GetList_Result> listSalaryApprovedStatistics = new List<Admin_StatisticsTeacherSalaryApproved_GetList_Result>();
            using (var db = new ZimEntities())
            {
                listSalaryApprovedStatistics = db.Admin_StatisticsTeacherSalaryApproved_GetList(id, year).ToList();
            }
            ViewBag.MonthLabel = JsonConvert.SerializeObject(listSalaryApprovedStatistics.Select(x => x.Month));
            ViewBag.TotalSalary = JsonConvert.SerializeObject(listSalaryApprovedStatistics.Select(x => x.TotalSalary));
            return View(t);
        }

        public ActionResult GetStatisticsSalary(int teacherId, int? year)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4 || aclog.RoleID == 3 || aclog.RoleID == 6 || aclog.RoleID == 9)
                return RedirectToAction("Signin", "Login", new { area = "" });

            if (year == 0)
                year = DateTime.Now.Year;

            List<Admin_StatisticsTeacherSalaryApproved_GetList_Result> listSalaryApprovedStatistics = new List<Admin_StatisticsTeacherSalaryApproved_GetList_Result>();
            using (var db = new ZimEntities())
            {
                listSalaryApprovedStatistics = db.Admin_StatisticsTeacherSalaryApproved_GetList(teacherId, year).ToList();
            }
            var monthLabel = listSalaryApprovedStatistics.Select(x => x.Month);
            var totalSalary = listSalaryApprovedStatistics.Select(x => x.TotalSalary);
            return Json(new { rs = true, totalSalary = totalSalary, monthLabel = monthLabel }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetListSalaryApproved(int teacherId, int? month, int? year, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4 || aclog.RoleID == 3 || aclog.RoleID == 6 || aclog.RoleID == 9)
                return RedirectToAction("Signin", "Login", new { area = "" });

            if (month == 0)
                month = null;
            ViewBag.Month = month;
            if (year == 0)
                year = null;
            ViewBag.Year = year;

            int pageSize = 20;

            List<Admin_TeacherSalaryApproved_GetList_Result> listSalaryApproved = new List<Admin_TeacherSalaryApproved_GetList_Result>();
            using (var db = new ZimEntities())
            {
                listSalaryApproved = db.Admin_TeacherSalaryApproved_GetList(teacherId, month, year, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (listSalaryApproved.Count > 0)
            {
                total = listSalaryApproved[0].TotalRow.Value;
            }
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;

            return Json(new { data = listSalaryApproved });
        }

        // danh sách giáo viên viên
        public ActionResult TeacherList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = search = Request.QueryString["search"].ToString();
            }
            int language = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slLanguage"]))
            {
                ViewBag.Language = language = Request.QueryString["slLanguage"].ToInt(0);
            }

            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                ViewBag.Sort = sort = Request.QueryString["sort"].ToInt(0);
            }
            List<ASW_Teacher_List_Result> l = new List<ASW_Teacher_List_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_Teacher_List(search, language, sort, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;

            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow.Value;
            }
            ViewBag.Total = l.Count();
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;

            return View(l);
        }

        //Danh sách giáo viên cần kiểm tra lương
        public ActionResult TeacherSalaryDetailList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            int course = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                course = Request.QueryString["slCourse"].ToInt(0);
            }
            ViewBag.Course = course;

            int teacher = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTeacher"]))
            {
                teacher = Request.QueryString["slTeacher"].ToInt(0);
            }
            ViewBag.Teacher = teacher;

            // thời gian bắt đầu từ
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = date.FirstDayOfMonth();
            var lastDayOfMonth = date.LastDayOfMonth();

            string fromdate = firstDayOfMonth.ToString("yyyy-MM-dd 00:00:00.000");
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                try
                {
                    DateTime fdate = DateTime.ParseExact(Request.QueryString["from"].ToString(), "dd/MM/yyyy", null);
                    ViewBag.From = fdate.ToString("dd/MM/yyyy");
                    fromdate = fdate.ToString("yyyy-MM-dd 00:00:00.000");
                }
                catch { }
            }

            string todate = lastDayOfMonth.ToString("yyyy-MM-dd 23:59:59.998");
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
            // đến
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                try
                {
                    DateTime tdate = DateTime.ParseExact(Request.QueryString["to"].ToString(), "dd/MM/yyyy", null);
                    ViewBag.To = tdate.ToString("dd/MM/yyyy");
                    todate = tdate.ToString("yyyy-MM-dd 23:59:59.998");
                }
                catch { }
            }

            var l = new List<Get_ListTeacherSalaryIsReject_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Get_ListTeacherSalaryIsReject(course, teacher, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
            }
            int total = 0;
            if (l.Count > 0)
                total = l[0].TotalRow.Value;
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }
        public ActionResult checkoutSalaryDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var teacher = TeacherSalaryDetailTable.getbyid(id);
            if (teacher == null)
            {
                SetAlert("Không tìm thấy nhân viên", "e");
                return RedirectToAction("StaffList", "Staff");
            }
            return View(teacher);
        }

        public ActionResult createCheckSalary(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int courseID = f.Get("dll-course").ToInt(0);
            int coursesScheduleID = f.Get("dll-coursesSchedule").ToInt();
            int teacherID = f.Get("dll-teacher").ToInt();
            float salary = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-salary")))
            {
                salary = f.Get("txt-salary").ToFloat();
            }
            DateTime date = DateTime.ParseExact(f.Get("txt-date"), "dd/MM/yyyy", null);
            int type = f.Get("txt-type").ToInt();
            string reasonNote = f.Get("txt-reasonNOte").ToString();

            var rs = TeacherSalaryDetailTable.insertCheckSalary(aclog.FullName, courseID, coursesScheduleID, teacherID, salary, date, type, reasonNote);
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        // danh sách nhân viên kinh doanh
        public ActionResult SellerList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            //key search
            string search = "";
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = ViewBag.Search = Request.QueryString["search"];
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
            }
            ViewBag.Sort = sort;
            var l = new List<Admin_Seller_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_Seller_GetList(search, sort, (page - 1) * pageSize, pageSize).ToList();
            }
            int total = 0;
            if (l.Count > 0)
                total = l[0].TotalRow.Value;
            ViewBag.Total = total;
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            return View(l);
        }

        // danh sách giáo viên
        public ActionResult SellerDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var ac = AccountTable.getbyID(id);
            if (ac == null)
            {
                SetAlert("Không tìm thấy nhân viên", "e");
                return RedirectToAction("SellerList", "Staff");
            }
            if (ac.RoleID != 6)
            {
                SetAlert("Không tìm thấy nhân viên", "e");
                return RedirectToAction("SellerList", "Staff");
            }
            return View(ac);
        }

        #region  chiến dịch sale
        public ActionResult CampaignSale(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            //page size
            int pageSize = 20;

            string stime = GetDateTime.Now.ToString("dd/MM/yyyy");
            var lf = CampaignSaleTable.getall();
            lf = lf.OrderByDescending(n => n.eTime).ToList();
            if (lf.Count > 0)
                stime = lf[0].eTime.Value.AddDays(1).ToString("dd/MM/yyyy");
            //ViewBag.sTime = stime;
            //lấy bài mới 
            //List<tbl_ExerciseAddonStudent> l = new List<tbl_ExerciseAddonStudent>();
            IEnumerable<tbl_CampaignSale> l = CampaignSaleTable.getall().OrderByDescending(n => n.sTime).ToList();

            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.Note).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.CampaignName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.sTime).ToList();
                else if (sort == 2)
                    l = l.OrderByDescending(n => n.sTime).ToList();
                else if (sort == 3)
                    l = l.OrderBy(n => n.eTime).ToList();
                else if (sort == 4)
                    l = l.OrderByDescending(n => n.eTime).ToList();
                else if (sort == 5)
                    l = l.OrderBy(n => n.CampaignName).ToList();
                else if (sort == 6)
                    l = l.OrderByDescending(n => n.CampaignName).ToList();
            }
            int trangthai = 2;
            if (!string.IsNullOrEmpty(Request.QueryString["trangthai"]))
            {
                trangthai = Request.QueryString["trangthai"].ToInt(0);
                if (trangthai == 0)
                    l = l.Where(n => n.IsHide == false).ToList();
                else if (trangthai == 1)
                    l = l.Where(n => n.IsHide == true).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.TrangThai = trangthai;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        [HttpPost]
        public ActionResult AddCampaignSale(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            string name = f.Get("txt-name").ToString();
            DateTime stime = DateTime.ParseExact(f.Get("txt-stime"), "dd/MM/yyyy", null);
            DateTime etime = DateTime.ParseExact(f.Get("txt-etime"), "dd/MM/yyyy", null);
            var camp = CampaignSaleTable.getall().OrderByDescending(x => x.eTime).FirstOrDefault();
            DateTime date = DateTime.Now.Date;
            if (camp != null)
            {
                date = camp.eTime.Value.AddDays(1);
            }
            if (stime < date)
            {
                SetAlert("Ngày bắt đầu phải lớn hơn ngày hiện tại", "e");
                return RedirectToAction("CampaignSale", "Staff");
            }
            if (etime <= stime)
            {
                SetAlert("Ngày kết thúc phải lớn hơn ngày bắt đầu", "e");
                return RedirectToAction("CampaignSale", "Staff");
            }
            string note = f.Get("txt-note").ToString();
            //var rs = CampaignSaleTable.insert(name, stime, etime, note, aclog.UserName);
            //if (rs != null)
            //    SetAlert("Thành công", "s");
            //else
            //    SetAlert("Không thành công", "e");
            return RedirectToAction("CampaignSale", "Staff");
        }

        public ActionResult CampaignSaleDetail(int id, int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CampaignSaleTable.getbyid(id);
            if (ck == null)
            {
                SetAlert("Không tìm thấy chiến dịch này", "e");
                return RedirectToAction("CampaignSale", "Staff");
            }
            //page size
            int pageSize = 20;
            ViewBag.ID = id;
            // lấy học viên trong chiến dịch
            // lay tu 2 table : tbl_StudentAppointment and tbl_CoursesStudent
            // 
            IEnumerable<tbl_CoursesStudent> l = CoursesStudentTable.getall().Where(n => n.CampaignSaleID == id).OrderByDescending(n => n.ID).ToList();
            var l2 = StudentAppointmentTable.getByCampaignSaleID(id);
            List<ChienDich> fl = new List<ChienDich>();
            // tbl_CoursesStudent
            foreach (var item in l)
            {
                ChienDich x = new ChienDich();
                x.TenHocVien = item.StudentName;
                x.TVV = AccountTable.getbyID(item.SupportUID.Value).FullName;
                x.TenKhoaHoc = item.CourseName;
                x.Gia = (double)item.PriceCourse + (double)item.CommitmentPrice;
                x.DuocGiam = (double)item.PriceDiscout;
                x.DaThanhToan = (double)item.Deposit;
                x.ConThieu = (x.Gia - x.DuocGiam - x.DaThanhToan);
                x.SupportID = item.SupportUID.Value;
                x.NgayTao = item.CreatedDate.Value;
                fl.Add(x);
            }
            foreach (var item in l2)
            {
                ChienDich x = new ChienDich();
                x.TenHocVien = item.StudentName;
                x.TVV = AccountTable.getbyusername(item.CreatedBy).FullName;
                x.TenKhoaHoc = item.ClassName;
                x.Gia = (double)item.ClassPrice + (double)item?.CommitmentPrice;
                x.DuocGiam = (double)item.DiscountPrice;
                x.DaThanhToan = (double)item.Deposit;
                x.ConThieu = (x.Gia - x.DuocGiam - x.DaThanhToan);
                x.SupportID = AccountTable.getbyusername(item.CreatedBy).ID;
                x.NgayTao = item.CreatedDate.Value;
                fl.Add(x);
            }
            IEnumerable<ChienDich> fll = fl.AsEnumerable();
            if (aclog.RoleID == 6)//saler
                fl = fl.Where(n => n.SupportID == aclog.ID).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string search = ViewBag.Search = Request.QueryString["search"].ToString();
                fll = from ex in fll
                      where AssetCRM.RemoveUnicode(ex.TenHocVien).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                      select ex;
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    fll = fll.OrderBy(n => n.TenHocVien).ToList();
                if (sort == 2)
                    fll = fll.OrderByDescending(n => n.TenHocVien).ToList();
                if (sort == 3)
                    fll = fll.OrderBy(n => n.ConThieu).ToList();
                if (sort == 4)
                    fll = fll.OrderByDescending(n => n.ConThieu).ToList();
            }
            ViewBag.Sort = sort;
            return View(fll.ToPagedList(page, pageSize));
        }

        public ActionResult CampaignSaleSalaryDetail()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });


            return View();
        }
        #endregion

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddStaff(FormCollection f, HttpPostedFileBase certificate, HttpPostedFileBase cv)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var school = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            int schoolid = 0;
            string schoolname = "";
            if (school != null)
            {
                schoolid = school.ID;
                schoolname = school.SchoolName;
            }

            int role = f.Get("hdfRole").ToInt(0);
            var d = DistrictTable.getbyID(f.Get("ddl-district").ToInt(0));
            string fullname = f.Get("txt-full-name").ToString();
            int language = f.Get("dll-language").ToInt(0);
            string email = f.Get("txt-email").ToString();
            string phone = f.Get("txt-phone").ToString();
            string address = f.Get("txt-address").ToString();
            string linkfb = f.Get("txt-fb").ToString();
            string datestartwork = f.Get("txt-dw").ToString();
            string zoomID = "";
            if (!string.IsNullOrEmpty(f.Get("txt-zoomid")))
            {
                zoomID = f.Get("txt-zoomid").ToString();
            }
            int position = 0;
            if (!string.IsNullOrEmpty(f.Get("ddl-position")))
            {
                position = f.Get("ddl-position").ToInt(0);
            }
            double revenue = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue")))
            {
                revenue = Convert.ToDouble(f.Get("txt-revenue").Replace(",", ""));
            }
            string zoomPassword = "";
            if (!string.IsNullOrEmpty(f.Get("txt-zoompass")))
            {
                zoomPassword = f.Get("txt-zoompass").ToString();
            }
            string bank = "";
            if (!string.IsNullOrEmpty(f.Get("txt-bank")))
            {
                bank = f.Get("txt-bank").ToString();
            }
            var certificatelink = "";
            var cvlink = "";
            if (certificate != null)
            {
                string ext = Path.GetExtension(certificate.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                {
                    SetAlert("Vui lòng chỉ Upload với định dạng hình ảnh!", "e");
                    return RedirectToAction("ProfileUser", "Account");
                }

                string fileName = certificate.FileName; // getting File Name
                string fileExtension = Path.GetExtension(fileName).ToLower();
                var result = AssetCRM.isValidImageAndVideo(fileExtension); // Validate Header
                if (result)
                {
                    fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(Server.MapPath("~/Upload/useravatar/"), fileName);
                    certificatelink = "/Upload/useravatar/" + fileName;
                    certificate.SaveAs(path);
                }
                else
                    SetAlert("Ảnh không đúng định dạng", "e");
            }
            if (cv != null)
            {
                string ext = Path.GetExtension(cv.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                {
                    SetAlert("Vui lòng chỉ Upload với định dạng hình ảnh!", "e");
                    return RedirectToAction("ProfileUser", "Account");
                }

                string fileName = cv.FileName; // getting File Name
                string fileExtension = Path.GetExtension(fileName).ToLower();
                var result = AssetCRM.isValidImageAndVideo(fileExtension); // Validate Header
                if (result)
                {
                    fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(Server.MapPath("~/Upload/useravatar/"), fileName);
                    cvlink = "/Upload/useravatar/" + fileName;
                    cv.SaveAs(path);
                }
                else
                    SetAlert("Ảnh không đúng định dạng", "e");
            }
            var rs = AccountTable.insertstaff(d, schoolid, schoolname, fullname, email, phone, address, role,
                2, "", "", aclog.UserName, language, certificatelink, cvlink, zoomID, zoomPassword, bank,
                linkfb, datestartwork, position, revenue);

            if (rs != null)
            {
                if (rs.RoleID == 4)
                {
                    Thread tassign = new Thread(() => AssignClassNew(rs, aclog));
                    tassign.Start();

                    var contract = ContractTable.getbyid(2);
                    string content = contract.ContractContent;
                    content = content.Replace("[ten-giao-vien]", rs.FullName);
                    content = content.Replace("[email]", rs.Email);
                    content = content.Replace("[tai-khoan]", rs.Email);
                    content = content.Replace("[mat-khau]", "iccacademy");
                    content = content.Replace("[ngay]", DateTime.Now.Day.ToString());
                    content = content.Replace("[thang]", DateTime.Now.Month.ToString());
                    content = content.Replace("[nam]", DateTime.Now.Year.ToString());
                    content = content.Replace("[ngay]", DateTime.Now.Day.ToString());
                    content = content.Replace("[thang]", DateTime.Now.Month.ToString());
                    content = content.Replace("[nam]", DateTime.Now.Year.ToString());
                    var nn = Language.GetByID(language);
                    content = content.Replace("[ngon-ngu]", nn?.Name);
                    ContractTeacherTable.insert(rs.ID, content, 1, false, aclog.UserName);
                }
                var tt = f.Get("hdtt").ToString();
                var ct = f.Get("hdct").ToString();
                AssetCRM.SendMail(email, tt, ct);
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            else
                SetAlert("Không thành công, tài khoản \"" + email + "\" đã tồn tại trên hệ thống", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        [ValidateInput(false)]
        public ActionResult AddStaffByLeader(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var school = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            int schoolid = 0;
            string schoolname = "";
            if (school != null)
            {
                schoolid = school.ID;
                schoolname = school.SchoolName;
            }
            int position = 0;
            if (!string.IsNullOrEmpty(f.Get("ddl-position")))
            {
                position = f.Get("ddl-position").ToInt(0);
            }
            double revenue = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue")))
            {
                revenue = Convert.ToDouble(f.Get("txt-revenue").Replace(",", ""));
            }
            int leader = f.Get("LeaderID").ToString().ToInt(0);
            string fullname = f.Get("txt-full-name").ToString();
            string email = f.Get("txt-email").ToString();
            string phone = f.Get("txt-phone").ToString();
            string address = f.Get("txt-address").ToString();
            string linkfb = f.Get("txt-fb").ToString();
            string datestartwork = f.Get("txt-dw").ToString();
            var rs = AccountTable.insertstaff(null, schoolid, schoolname, fullname, email, phone, address, 6,
                2, "", "", aclog.UserName, 0, "", "", "", "", "", linkfb, datestartwork, position, revenue);


            if (rs != null)
            {
                AccountTable.UpdateLeader(rs.ID, leader);
                //int id;
                //switch (rs.Language)
                //{
                //    case 7:
                //        id = 21;
                //        break;
                //    case 6:
                //        id = 14;
                //        break;
                //    default:
                //        id = 7;
                //        break;
                //}
                //var emailTemplate = EmailTemplateTable.GetByID(id);
                //var tt = emailTemplate.EmailTitle;
                //var ct = emailTemplate.EmailContent;
                //ct = ct.Replace("[ten-nhan-vien]", fullname);
                //ct = ct.Replace("[tai-khoan]", email);
                //ct = ct.Replace("[mat-khau]", "iccacademy");
                var tt = f.Get("hdtt").ToString();
                var ct = f.Get("hdct").ToString();
                AssetCRM.SendMail(email, tt, ct);
                SetAlert("Thành công", "s");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }
            else
                SetAlert("Không thành công, tài khoản \"" + email + "\" đã tồn tại trên hệ thống", "e");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
        [HttpPost]
        public ActionResult EditStaff(FormCollection f, HttpPostedFileBase certificate, HttpPostedFileBase cv)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 3)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ID = f.Get("hdfID").ToInt(0);
            var acc = AccountTable.getbyID(ID);
            if (acc == null)
            {
                SetAlert("Không tìm thấy tài khoản này", "e");
                return RedirectToAction("Center", "Option");
            }
            int status = f.Get("ddl-status").ToInt(0);
            string fullname = f.Get("txt-full-name").ToString();
            var gender = f.Get("sex").ToString().ToInt(3);
            string email = f.Get("txt-email").ToString();
            string phone = f.Get("txt-phone").ToString();
            string pass = f.Get("txt-password").ToString();
            string address = f.Get("txt-address").ToString();
            string linkfb = f.Get("txt-fb").ToString();
            string datestartwork = f.Get("txt-dw").ToString();
            string zoomID = f.Get("txt-zoomid")?.ToString();
            string zoomPassword = f.Get("txt-zoompass")?.ToString();
            string bank = f.Get("txt-bank")?.ToString();
            int language = f.Get("dll-language").ToInt(0);
            var school = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            var role = acc.RoleID.Value;
            int position = 0;
            if (!string.IsNullOrEmpty(f.Get("ddl-position")))
            {
                position = f.Get("ddl-position").ToInt(0);
            }

            double revenue = acc.Revenue ?? 0;
            if (!string.IsNullOrEmpty(f.Get("txt-revenue")) && aclog.RoleID == 1)
            {
                revenue = Convert.ToDouble(f.Get("txt-revenue").Replace(",", ""));
            }

            if (!string.IsNullOrEmpty(f.Get("ddl-role")))
            {
                role = f.Get("ddl-role").ToInt(0);
            }

            if (school == null)
            {
                school = new tbl_School();
                school.ID = 0;
                school.CityName = "";
            }
            int loc = f.Get("ddl-location").ToInt(0);

            var location = LocationTable.getbyID(loc);
            if (location == null)
            {
                location = new tbl_City();
                location.ID = 0;
                location.CityName = "";
            }

            var district = DistrictTable.getbyID(0);
            if (district == null)
            {
                district = new tbl_District();
                district.ID = 0;
                district.DistrictName = "";
            }
            DateTime bd = GetDateTime.Now.Date;
            try
            {
                string s = f.Get("txt-birthday").ToString().Replace(" ", "");
                bd = DateTime.ParseExact(s, "dd/MM/yyyy", null);
            }
            catch
            {
                bd = GetDateTime.Now.Date;
            }
            var certificatelink = "";
            var cvlink = "";
            string strPathAndQuery = Request.Url.PathAndQuery;
            string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
            if (certificate != null)
            {
                string ext = Path.GetExtension(certificate.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                {
                    SetAlert("Vui lòng chỉ Upload với định dạng hình ảnh!", "e");
                    return RedirectToAction("ProfileUser", "Account");
                }

                string fileName = certificate.FileName; // getting File Name
                string fileExtension = Path.GetExtension(fileName).ToLower();
                var result = AssetCRM.isValidImageAndVideo(fileExtension); // Validate Header
                if (result)
                {
                    fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(Server.MapPath("~/Upload/useravatar/"), fileName);
                    certificatelink = strUrl + "Upload/useravatar/" + fileName;
                    certificate.SaveAs(path);
                }
                else
                    SetAlert("Ảnh không đúng định dạng", "e");
            }
            if (cv != null)
            {
                string ext = Path.GetExtension(cv.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                {
                    SetAlert("Vui lòng chỉ Upload với định dạng hình ảnh!", "e");
                    return RedirectToAction("ProfileUser", "Account");
                }

                string fileContentType = cv.ContentType; // getting ContentType
                byte[] tempFileBytes = new byte[cv.ContentLength];
                string fileName = cv.FileName; // getting File Name
                string fileExtension = Path.GetExtension(fileName).ToLower();
                var result = AssetCRM.isValidImageAndVideo(fileExtension); // Validate Header
                if (result)
                {
                    fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(Server.MapPath("~/Upload/useravatar/"), fileName);
                    cvlink = strUrl + "Upload/useravatar/" + fileName;
                    cv.SaveAs(path);
                }
                else
                    SetAlert("Ảnh không đúng định dạng", "e");
            }
            var rs = AccountTable.updatestaff(acc.ID, location.ID, location.CityName, school.ID, school.SchoolName, fullname, pass, email, phone, address, role,
                status, bd, gender, aclog.UserName, district.ID, district.DistrictName, language, certificatelink, cvlink,
                zoomID, zoomPassword, bank, linkfb, datestartwork, position, revenue);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            if (rs.RoleID == 4)// giáo viên
                return RedirectToAction("ConfigurationTeacher", "Staff", new { id = acc.ID });
            if (rs.RoleID == 5)// học viên
                return RedirectToAction("CustomerDetai", "Customer", new { id = acc.ID });
            if (rs.RoleID == 6)//saler
                return RedirectToAction("SellerDetail", "Staff", new { id = acc.ID });
            else
                return RedirectToAction("StaffDetail", "Staff", new { id = acc.ID });
        }

        [HttpPost]
        public ActionResult AssignClass(int UID, int ClassID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            AssignTeacherClassTable.insertclass(UID, ClassID, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AssignSubject(int UID, int ClassID, int SubjectID, int LevelID)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            AssignTeacherClassTable.insertsubject(UID, ClassID, SubjectID, LevelID, aclog.UserName);
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TeachHours(int page = 1)
        {
            if (Session["UID"] == null)
            {
                SetAlert("Your session has expired", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }

            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 9 && aclog.RoleID != 2 && aclog.RoleID != 8 && aclog.RoleID != 7)
            {
                SetAlert("Access is denied", "e");
                return RedirectToAction("Signin", "Login", new { area = "" });
            }
            string search = "";
            List<int> ListTeacher = new List<int>();
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                search = Request.QueryString["search"].ToString();
                ListTeacher = AccountTable.getbyrole(4).Where(n => n.FullName.ToUpper().Contains(search.ToUpper()) || n.UserName.ToUpper().Contains(search.ToUpper())).Select(n => n.ID).ToList();
            }
            ViewBag.Search = search;

            string dateview3 = "";
            int type = 1;
            if (Request.QueryString["type"] != null)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["type"]))
                {
                    try
                    {
                        type = Request.QueryString["type"].ToString().ToInt(1);
                    }
                    catch
                    {
                        type = 1;
                    }
                }
            }
            ViewBag.Type = type;
            DateTime d = DateTime.Now.Date;
            if (Request.QueryString["date"] != null)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["date"]))
                {
                    try
                    {
                        d = DateTime.ParseExact(Request.QueryString["date"].ToString(), "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        d = DateTime.Now.Date;
                    }
                }
            }
            ViewBag.Date = d.ToString("dd/MM/yyyy");
            StringBuilder gvTeaching = new StringBuilder();
            List<tbl_AccountTotalHourTeaching> ltrTeaching = new List<tbl_AccountTotalHourTeaching>();
            if (type == 1)//ngày
            {
                dateview3 = d.ToString("dd/MM/yyy") + " - " + d.ToString("dd/MM/yyyy");
                d = d.AddDays(1).Date;
                ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 1 && n.DateRun.Value.Date == d.Date).ToList();
            }
            if (type == 2)//tuần
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 2 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddDays(-7).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 2 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 3)//tháng
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 3 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddMonths(-1).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 3 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 4)//quý
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 4 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddMonths(-3).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 4 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 5)//năm
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 5 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddYears(-1).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 5 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (ListTeacher.Count > 0)
            {
                ltrTeaching = ltrTeaching.Where(n => ListTeacher.Contains(n.UID.Value)).ToList();
            }
            int total = ViewBag.Total = ltrTeaching.Count;

            int pageSize = 20;
            if ((page - 1) * pageSize < ltrTeaching.Count)
            {
                ltrTeaching = ltrTeaching.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                ltrTeaching = new List<tbl_AccountTotalHourTeaching>();
            }
            int PageCount = 0;
            if (total % pageSize == 0)
                PageCount = total / pageSize;
            else
                PageCount = total / pageSize + 1;
            ViewBag.Page = page;
            ViewBag.PageCount = PageCount;
            ViewBag.DateView = dateview3;
            return View(ltrTeaching);
        }
        [HttpGet]
        public ActionResult ExportTeachHours(string search, string date, int type)
        {

            if (Session["UID"] == null)
            {
                return Json(new { rs = false, message = "Your session has expired" }, JsonRequestBehavior.AllowGet);
            }

            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 9)
            {
                return Json(new { rs = false, message = "Access is denied" }, JsonRequestBehavior.AllowGet);
            }
            List<int> ListTeacher = new List<int>();
            if (!string.IsNullOrEmpty(search))
            {
                ListTeacher = AccountTable.getbyrole(4).Where(n => n.FullName.ToUpper().Contains(search.ToUpper()) || n.UserName.ToUpper().Contains(search.ToUpper())).Select(n => n.ID).ToList();
            }
            string dateview3 = "";
            DateTime d = DateTime.Now.Date;
            if (date != null)
            {
                if (!string.IsNullOrEmpty(date))
                {
                    try
                    {
                        d = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        d = DateTime.Now.Date;
                    }
                }
            }
            StringBuilder gvTeaching = new StringBuilder();
            List<tbl_AccountTotalHourTeaching> ltrTeaching = new List<tbl_AccountTotalHourTeaching>();
            if (type == 1)//ngày
            {
                dateview3 = d.ToString("dd/MM/yyy") + " - " + d.ToString("dd/MM/yyyy");
                d = d.AddDays(1).Date;
                ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 1 && n.DateRun.Value.Date == d.Date).ToList();
            }
            if (type == 2)//tuần
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 2 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddDays(-7).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 2 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 3)//tháng
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 3 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddMonths(-1).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 3 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 4)//quý
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 4 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddMonths(-3).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 4 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (type == 5)//năm
            {
                var firt = AccountRankTable.getbyrole(4).Where(n => n.Type == 5 && n.DateRun > d.Date).OrderBy(n => n.DateRun).FirstOrDefault();
                if (firt != null)
                {
                    dateview3 = firt.DateRun.Value.AddYears(-1).ToString("dd/MM/yyy") + "-" + firt.DateRun.Value.AddDays(-1).ToString("dd/MM/yyyy");
                    ltrTeaching = AccountTotalHourTeachingTable.getall().Where(n => n.Type == 5 && n.DateRun.Value.Date == firt.DateRun.Value.Date).ToList();
                }
                else
                {
                    dateview3 = "Không có dữ liệu";
                }
            }
            if (ListTeacher.Count > 0)
            {
                ltrTeaching = ltrTeaching.Where(n => ListTeacher.Contains(n.UID.Value)).ToList();
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">");
            strExport.Append("      <tr>");
            strExport.Append("            <th scope=\"col\">Giáo viên</th>");
            strExport.Append("            <th scope=\"col\">Loại lớp</th>");
            strExport.Append("            <th scope=\"col\">Giờ dạy</th>");
            strExport.Append("            <th scope=\"col\">Điểm danh</th>");
            strExport.Append("            <th scope=\"col\">Không điểm danh</th>");
            strExport.Append("            <th scope=\"col\">Tổng giờ</th>");
            strExport.Append("            <th scope=\"col\">Giờ trực</th>");
            strExport.Append("            <th scope=\"col\">Tổng giờ trực</th>");
            strExport.Append("      </tr>");
            int countTeacher = 0;
            foreach (var item in ltrTeaching)
            {
                countTeacher++;
                tbl_AccountTotalHourTeaching bf = AccountTotalHourTeachingTable.getbyuid(item.UID.Value).Where(n => n.Type == type && n.DateRun < item.DateRun).OrderByDescending(n => n.DateRun).FirstOrDefault();
                var ac = AccountTable.getbyID(item.UID.Value);
                if (ac != null)
                {
                    strExport.Append("<tr>");
                    strExport.Append("    <td>" + ac.FullName + "</td>");
                    strExport.Append("    <td>Offline</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.Hours + "</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.RealHour + "</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.AbsentHour + "</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.TotalHour + "</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.HoursCourseSelfStudy + "</td>");
                    strExport.Append("    <td class=\"center-align\">" + item.TotalHourCourseSelfStudy + "</td>");
                    strExport.Append("</tr>");
                    //strExport.Append("<tr>");
                    //strExport.Append("    <td>Online</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.HoursOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.RealHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.AbsentHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.TotalHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.HoursCourseSelfStudyOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.TotalHourCourseSelfStudyOnline + "</td>");
                    //strExport.Append("</tr>");
                    //strExport.Append("<tr>");
                    //strExport.Append("    <td rowspan=\"2\">Leader</td>");
                    //strExport.Append("    <td>Offline</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderHoursOffline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderRealHour + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderAbsentHour + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderTotalHour + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderHoursCourseSelfStudy + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderTotalHourCourseSelfStudy + "</td>");
                    //strExport.Append("</tr>");
                    //strExport.Append("<tr>");
                    //strExport.Append("    <td>Offline</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderHoursOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderRealHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderAbsentHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderTotalHourOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderHoursCourseSelfStudyOnline + "</td>");
                    //strExport.Append("    <td class=\"center-align\">" + item.LeaderTotalHourCourseSelfStudyOnline + "</td>");
                    //strExport.Append("</tr>");
                }

            }
            strExport.Append("</table>");
            var filename = "Gio_Day_Giao_Vien" + " " + dateview3;
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// Xoá giáo viên
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteUser(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                AccountTable.DeleteAccount(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteTeacher(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                var check = TeacherCourseTable.GetAllByTeacherUID(id);
                if (check.Count() > 0)
                {
                    foreach (var i in check)
                    {
                        var c = CoursesTable.getbyid(i.CourseID.Value);
                        if (c != null)
                        {
                            if (c.TypeCourse == 2)
                            {
                                if (c.EndingDate >= DateTime.Now)
                                {
                                    return Json(new { rs = false, message = "Không thể xóa giáo viên đang có khóa học!" }, JsonRequestBehavior.AllowGet);
                                }
                            }
                            else
                            {
                                //IEnumerable<tbl_CoursesSchedule> l = CoursesScheduleTable.getbycourseid(i.CourseID.Value);
                                //var ll = from t in l
                                //         group t by t.Date;
                                //int dem = 0;
                                //foreach (var ageGroup in ll)
                                //{
                                //    var ne = from t in ageGroup
                                //             group t by t.StudyTimeID;
                                //    dem += ne.GroupBy(n => n.Key).Count();
                                //}
                                //if(c.OpeningDate>=DateTime.Now||)
                            }
                        }
                    }
                }
                AccountTable.DeleteAccount(id, true, aclog.UserName);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = false, message = "Thất bại" }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// ImportExcel Saler
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportExcelSaler()
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                if (aclog.RoleID != 1)
                {
                    return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
                }
                if (Request.Files["FileUpload1"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();
                    string connString = "";
                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };
                    string newfile = GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Request.Files["FileUpload1"].FileName;
                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/Upload/Excel"), newfile);
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Upload/Excel"));
                    }
                    if (validFileTypes.Contains(extension))
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1);
                        }
                        Request.Files["FileUpload1"].SaveAs(path1);
                        System.Data.DataTable dt = new System.Data.DataTable();
                        if (extension == ".csv")
                        {
                            dt = ConvertCSVtoDataTable(path1);
                        }
                        //Connection String to Excel Workbook  
                        else if (extension.Trim() == ".xls")
                        {
                            connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        else if (extension.Trim() == ".xlsx")
                        {
                            connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        foreach (DataRow item in dt.Rows)
                        {
                            if (string.IsNullOrEmpty(item[1].ToString()) || string.IsNullOrEmpty(item[2].ToString()) ||
                             string.IsNullOrEmpty(item[3].ToString()) || string.IsNullOrEmpty(item[4].ToString()))
                            {
                                continue;
                            }
                            tbl_Account pe = new tbl_Account();
                            if (!string.IsNullOrEmpty(item[0].ToString()))
                            {
                                pe.LocationID = item[0].ToString().ToInt(0);
                                pe.FullName = item[1].ToString();
                                pe.Email = item[2].ToString();
                                pe.Phone = item[3].ToString();
                                pe.Address = item[4].ToString();
                                var d = DistrictTable.getbyID(pe.LocationID.Value);
                                var checkphone = AccountTable.getbyusername(pe.Phone);
                                if (checkphone == null)
                                {
                                    AccountTable.insertstaff(d, 0, "", pe.FullName, pe.Email, pe.Phone, pe.Address, 6, 2, "",
                                        "", aclog.UserName, 0, "", "", "", "", "", "", "", 0, 0);
                                }
                            }
                        }
                        return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = false, message = "Vui lòng upload file có định dạng .xls, .xlsx or .csv" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("StaffController", "SellerList", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, message = "Something error!" }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// ImportExcel Staff
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportExcelStaff()
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                if (aclog.RoleID != 1)
                {
                    return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
                }
                if (Request.Files["FileUpload1"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();
                    string connString = "";
                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };
                    string newfile = GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Request.Files["FileUpload1"].FileName;
                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/Upload/Excel"), newfile);
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Upload/Excel"));
                    }
                    if (validFileTypes.Contains(extension))
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1);
                        }
                        Request.Files["FileUpload1"].SaveAs(path1);
                        System.Data.DataTable dt = new System.Data.DataTable();
                        if (extension == ".csv")
                        {
                            dt = ConvertCSVtoDataTable(path1);
                        }
                        //Connection String to Excel Workbook  
                        else if (extension.Trim() == ".xls")
                        {
                            connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        else if (extension.Trim() == ".xlsx")
                        {
                            connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        foreach (DataRow item in dt.Rows)
                        {
                            if (string.IsNullOrEmpty(item[1].ToString()) || string.IsNullOrEmpty(item[2].ToString()) ||
                             string.IsNullOrEmpty(item[3].ToString()) || string.IsNullOrEmpty(item[4].ToString()))
                            {
                                continue;
                            }
                            tbl_Account pe = new tbl_Account();
                            if (!string.IsNullOrEmpty(item[0].ToString()))
                            {
                                pe.SchoolID = item[0].ToString().ToInt(0);
                                pe.RoleID = item[1].ToString().ToInt(0);
                                pe.FullName = item[2].ToString();
                                pe.Email = item[3].ToString();
                                pe.Phone = item[4].ToString();
                                pe.Address = item[5].ToString();
                                if (pe.SchoolID != 1)
                                {
                                    return Json(new { rs = false, message = "Không tìm thấy trung tâm!" }, JsonRequestBehavior.AllowGet);
                                }
                                if (pe.RoleID != 7 && pe.RoleID != 1)
                                {
                                    return Json(new { rs = false, message = "Không tìm thấy chức vụ!" }, JsonRequestBehavior.AllowGet);
                                }
                                var checkphone = AccountTable.getbyusername(pe.Phone);
                                if (checkphone == null)
                                {
                                    var sch = SchoolTable.getbyID(pe.SchoolID.Value);
                                    AccountTable.insertstaff(null, pe.SchoolID.Value, sch.SchoolName, pe.FullName, pe.Email,
                                        pe.Phone, pe.Address, pe.RoleID.Value, 2, "", "", aclog.UserName, 0, "", "", "", "",
                                        "", "", "", 0, 0);
                                }
                            }
                        }
                        return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = false, message = "Vui lòng upload file có định dạng .xls, .xlsx or .csv" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("StaffController", "SellerList", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, message = "Something error!" }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Import Teacher
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ImportExcelTeacher()
        {
            try
            {
                if (Session["UID"] == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog == null)
                    return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
                if (aclog.RoleID != 1)
                {
                    return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
                }
                if (Request.Files["FileUpload1"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["FileUpload1"].FileName).ToLower();
                    string connString = "";
                    string[] validFileTypes = { ".xls", ".xlsx", ".csv" };
                    string newfile = GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "_" + Request.Files["FileUpload1"].FileName;
                    string path1 = string.Format("{0}/{1}", Server.MapPath("~/Upload/Excel"), newfile);
                    if (!Directory.Exists(path1))
                    {
                        Directory.CreateDirectory(Server.MapPath("~/Upload/Excel"));
                    }
                    if (validFileTypes.Contains(extension))
                    {
                        if (System.IO.File.Exists(path1))
                        {
                            System.IO.File.Delete(path1);
                        }
                        Request.Files["FileUpload1"].SaveAs(path1);
                        System.Data.DataTable dt = new System.Data.DataTable();
                        if (extension == ".csv")
                        {
                            dt = ConvertCSVtoDataTable(path1);
                        }
                        //Connection String to Excel Workbook  
                        else if (extension.Trim() == ".xls")
                        {
                            connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + path1 + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        else if (extension.Trim() == ".xlsx")
                        {
                            connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + path1 + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                            dt = ConvertXSLXtoDataTable(path1, connString);
                        }
                        foreach (DataRow item in dt.Rows)
                        {
                            if (string.IsNullOrEmpty(item[1].ToString()) || string.IsNullOrEmpty(item[2].ToString()) ||
                             string.IsNullOrEmpty(item[3].ToString()) || string.IsNullOrEmpty(item[4].ToString()))
                            {
                                continue;
                            }
                            tbl_Account pe = new tbl_Account();
                            if (!string.IsNullOrEmpty(item[0].ToString()))
                            {
                                pe.LocationID = item[0].ToString().ToInt(0);
                                pe.FullName = item[1].ToString();
                                pe.Email = item[2].ToString();
                                pe.Phone = item[3].ToString();
                                pe.Address = item[4].ToString();
                                var d = DistrictTable.getbyID(pe.LocationID.Value);
                                var checkphone = AccountTable.getbyusername(pe.Phone);
                                if (checkphone == null)
                                {
                                    AccountTable.insertstaff(d, 0, "", pe.FullName, pe.Email, pe.Phone, pe.Address, 4, 2, "",
                                        "", aclog.UserName, 0, "", "", "", "", "", "", "", 0, 0);
                                }
                            }
                        }
                        return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { rs = false, message = "Vui lòng upload file có định dạng .xls, .xlsx or .csv" }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("StaffController", "SellerList", Session["UID"].ToString().ToInt(0), ex.Message);
                return Json(new { rs = false, message = "Something error!" }, JsonRequestBehavior.AllowGet);
            }

        }

        public static System.Data.DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            System.Data.DataTable dt = new System.Data.DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }

                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows.Length > 1)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i].Trim();
                        }
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }
        public static System.Data.DataTable ConvertXSLXtoDataTable(string strFilePath, string connString)
        {
            OleDbConnection oledbConn = new OleDbConnection(connString);
            System.Data.DataTable dt = new System.Data.DataTable();
            try
            {

                oledbConn.Open();
                using (OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn))
                {
                    OleDbDataAdapter oleda = new OleDbDataAdapter();
                    oleda.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    oleda.Fill(ds);

                    dt = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {

                oledbConn.Close();
            }

            return dt;

        }
        /// <summary>
        /// Xuất excel danh sách giáo viên
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelTeacher(string search, int sort)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            var l = AccountTable.getbyrole(4).Where(x => x.IsDelete == false);
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(search))
            {
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.FullName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Email).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Phone).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.LocationName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            //sort
            if (!string.IsNullOrEmpty(sort.ToString()))
            {
                if (sort == 1)
                    l = l.OrderBy(n => n.LocationName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.LocationName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.FullName).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.FullName).ToList();
            }
            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Tỉnh/TP</th>");
            strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Email</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Ngày nhận việc</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.LocationName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Email + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Giao_Vien_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất excel danh sách nhân viên
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelStaff(string search, int sort)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            IEnumerable<tbl_Account> l = AccountTable.GetStaff();
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(search))
            {
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.FullName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Email).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.SchoolName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Phone).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.LocationName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            //sort
            if (!string.IsNullOrEmpty(sort.ToString()))
            {
                if (sort == 1)
                    l = l.OrderBy(n => n.SchoolName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.SchoolName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.FullName).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.FullName).ToList();
            }
            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Trung tâm</th>");
            strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
            strExport.Append("                        <th scope=\"col\">Tài khoản</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Email</th>");
            strExport.Append("                        <th scope=\"col\">Chức vụ</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Ngày nhận việc</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SchoolName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.UserName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Email + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.RoleName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Nhan_Vien_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất excel danh sách saler
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelSaler(string search, int sort)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            var l = AccountTable.getbyrole(6).Where(x => x.IsDelete == false);
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }
            if (!string.IsNullOrEmpty(search))
            {
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.FullName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Email).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.Phone).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.LocationName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            //sort
            if (!string.IsNullOrEmpty(sort.ToString()))
            {
                if (sort == 1)
                    l = l.OrderBy(n => n.LocationName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.LocationName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.FullName).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.FullName).ToList();
            }
            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Tỉnh/TP</th>");
            strExport.Append("                        <th scope=\"col\">Họ và tên</th>");
            strExport.Append("                        <th scope=\"col\">Số điện thoại</th>");
            strExport.Append("                        <th scope=\"col\">Email</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Ngày nhận việc</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.LocationName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.FullName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Phone + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Email + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.StatusName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "Danh_Sach_Saler_Vien_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xóa chiến dịch sale
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult DeleteCampaignSale(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                var cam = CampaignSaleTable.getbyid(id);
                if (cam.IsHide == true)
                {
                    return Json(new { rs = false, message = "Không thể xóa chiến dịch đã hoàn thành!" }, JsonRequestBehavior.AllowGet);
                }
                else if (cam.IsHide == false && cam.sTime <= DateTime.Now)
                {
                    return Json(new { rs = false, message = "Không thể xóa chiến dịch đang diễn ra!" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    CampaignSaleTable.Delete(id, true, aclog.UserName);
                    return Json(new { rs = true, message = "Thành công!" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất excel danh sách chiến dịch
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelCampaignSale(string search, int sort, int trangthai)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            if (aclog.RoleID != 1)
            {
                return Json(new { rs = false, message = "Truy cập bị từ chối!" }, JsonRequestBehavior.AllowGet);
            }
            List<ASW_CampaignSale_ExportExcel_Result> l = new List<ASW_CampaignSale_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CampaignSale_ExportExcel(search, sort, trangthai).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Chiến dịch</th>");
            strExport.Append("                        <th scope=\"col\">Bắt đầu</th>");
            strExport.Append("                        <th scope=\"col\">Kết thúc</th>");
            strExport.Append("                        <th scope=\"col\">Tổng ngày</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CampaignName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.sTime.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.eTime.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.TotalDay + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.TrangThai + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.Note + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Chien_Dich_Kinh_Doanh_" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RemoveMember(int id)
        {
            try
            {
                using (var db = new ZimEntities())
                {
                    var check = db.tbl_Account.Where(x => x.ID == id).FirstOrDefault();
                    if (check != null)
                    {
                        check.LeaderID = 0;
                        db.SaveChanges();
                    }
                }

                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            catch (Exception e)
            {
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
        }
        [HttpPost]
        public ActionResult LoadEmail(int id, int language)
        {
            try
            {
                //var lang = Language.GetByID(language);
                //switch (lang.ID)
                //{
                //    case 7:
                //        id = 18;
                //        break;
                //    case 6:
                //        id = 11;
                //        break;
                //    default:
                //        id = 4;
                //        break;
                //}
                //lấy mẫu email
                var email = EmailTemplateTable.GetByID(id);
                var tt = email.EmailTitle;
                var ct = email.EmailContent;
                return Json(new { rs = true, tt, ct });
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Cashier", "LoadEmailDangkyhoc", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }
        protected static void AssignClassNew(tbl_Account teacher, tbl_Account aclog)
        {
            try
            {
                var classList = ClassTable.GetByLanguage(teacher.Language.Value);
                foreach (var i in classList)
                {
                    AssignTeacherClassTable.insertclass(teacher.ID, i.ID, aclog.UserName);
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Staff", "AssignClassNew", 0, e.Message + "||" + e.InnerException);
            }
        }
        [HttpPost]
        public ActionResult AssignAllClass(int UID)
        {
            if (Session["UID"] == null)
                return Json(new { rs = false, message = "Không thành công" }, JsonRequestBehavior.AllowGet);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var teacher = AccountTable.getbyID(UID);
            var classList = ClassTable.GetByLanguage(teacher.Language.Value);
            foreach (var i in classList)
            {
                AssignTeacherClassTable.insertclass(teacher.ID, i.ID, aclog.UserName);
            }
            return Json(new { rs = true });
        }
    }
    public class ChienDich
    {
        public string TenHocVien { get; set; }
        public string TVV { get; set; }
        public string TenKhoaHoc { get; set; }
        public double Gia { get; set; }
        public double DuocGiam { get; set; }
        public double DaThanhToan { get; set; }
        public double ConThieu { get; set; }
        public DateTime NgayTao { get; set; }
        public int SupportID { get; set; }
    }
}