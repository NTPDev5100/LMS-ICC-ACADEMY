using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.UI.WebControls;
using static AppZim.ZIM.Zoom;

namespace AppZim.Areas.Admin.Controllers
{
    public class ReportController : BaseController
    {
        public ActionResult CourseList(int teacherID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = TeacherCourseTable.getbyteacherid(teacherID);
            var lThongke = new List<ThongKe>();
            Parallel.ForEach(l, item =>
            {
                var TeacherCourse = item;
                int reocord = 0;

                using (var db = new ZimEntities())
                {
                    var lx = db.tbl_CoursesSchedule.Where(x => x.CourseID == item.CourseID && x.RoomID != null && x.RoomPass != null).ToList().Count;
                    reocord = lx;
                }
                lThongke.Add(new ThongKe { l = TeacherCourse, RecordZoom = reocord });
            });
            ViewBag.TeacherName = AccountTable.getbyID(teacherID).FullName;
            ViewBag.TeacherID = teacherID;
            return View(lThongke);
        }
        public ActionResult ExportExcel(int teacherID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = TeacherCourseTable.getbyteacherid(teacherID);

            //xuat file excel
            var dataTable = new System.Data.DataTable();
            dataTable.Columns.Add("Khóa học", typeof(string));
            dataTable.Columns.Add("Ngày tạo khóa học", typeof(string));
            dataTable.Columns.Add("Giáo viên", typeof(string));
            dataTable.Columns.Add("Tổng số buổi", typeof(string));
            dataTable.Columns.Add("Số buổi hoàn thành", typeof(string));
            dataTable.Columns.Add("Số buổi đã dạy", typeof(string));
            dataTable.Columns.Add("Tổng thời gian dạy", typeof(string));
            dataTable.Columns.Add("Điểm đánh giá", typeof(string));
            dataTable.Columns.Add("RecordZoom", typeof(string));
            dataTable.Columns.Add("Lương", typeof(string));

            Parallel.ForEach(l, item =>
            {
                int reocord = 0;

                var lx = 0;
                double? salary = null;
                using (var db = new ZimEntities())
                {
                    lx = db.tbl_CoursesSchedule.Where(x => x.CourseID == item.CourseID && x.RoomID != null && x.RoomPass != null).ToList().Count;
                    reocord = lx;
                    salary = CourseSalaryTable.getbycourse(item.CourseID.Value).Where(i => i.SalaryOfLesson != null).Select(x => x.SalaryOfLesson).Sum();
                }

                var d = dataTable.Rows.Add(
                                    item.CourseName,
                                    item.CreatedDate.Value.ToString("dd-MM-yyyy"),
                                    AccountTable.GetFullName(item.TeacherUID.Value),
                                    CoursesScheduleTable.getbyTeacherIDandcouseID(item.CourseID.Value, item.TeacherUID.Value).Count.ToString(),
                                    CoursesScheduleTable.GetByCourseAndStatus(item.CourseID.Value, 1).Count.ToString(),
                                    CoursesScheduleTable.SoBuoiDaDay(item.CourseID.Value, 1, item.TeacherUID.Value).Count.ToString(),
                                    CoursesScheduleTable.TongThoiGianDay(item.CourseID.Value, item.TeacherUID.Value),
                                    FeedbackScheduleTable.CalcRating(item.CourseID.Value),
                                    reocord,
                                    salary
                );
            });
            CultureInfo cult = new CultureInfo("vi-VN");
            var grid = new GridView();
            grid.DataSource = dataTable;

            grid.DataBind();
            Response.ClearContent();
            Response.Buffer = true;

            Response.AddHeader("content-disposition", "attachment; filename=ThongKe" + AccountTable.GetFullName(teacherID) + ".xls");
            Response.ContentType = "application/ms-excel";
            Response.ContentEncoding = System.Text.Encoding.Unicode;
            Response.BinaryWrite(System.Text.Encoding.Unicode.GetPreamble());
            Response.Charset = "";
            StringWriter sw = new StringWriter();
            HtmlTextWriter htw = new HtmlTextWriter(sw);
            grid.HeaderStyle.Font.Bold = true;
            grid.ShowFooter = true;
            grid.BorderStyle = BorderStyle.Solid;
            grid.RenderControl(htw);
            Response.Output.Write(sw.ToString());
            Response.Flush();
            Response.End();

            return Redirect(Request.UrlReferrer.AbsoluteUri);
        }

        public JsonResult GetDetalSchedule(int CourseID, int teacherid)
        {
            List<ScheduleDetails> l = new List<ScheduleDetails>();
            var lSchedule = CoursesScheduleTable.SoBuoiDaDay(CourseID, 1, teacherid);
            Parallel.ForEach(lSchedule, item =>
            {
                var i = new ScheduleDetails();
                i.Ngay = item.Date.Value.ToString("dd/MM/yyyy");
                i.KhungGio = StudyTimeTable.getbyid(item.StudyTimeID.Value).StudyTimeName;
                l.Add(i);
            });
            return Json(l, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ListCollectionHistory(int receiverid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            ViewBag.TeacherName = AccountTable.GetFullName(receiverid);
            var l = new List<tbl_StudentAppointment>();
            using (var db = new ZimEntities())
            {
                l = db.tbl_StudentAppointment.Where(x => x.PaymentTeacherReceiveID == receiverid && x.CloseAppointment != 1).ToList();
            }
            return View(l);
        }
        public ActionResult RevenueOfSaler(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //page size
            int pageSize = 10;
            //key search
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            
            // lấy ngày đầu và ngày cuối tháng
            string fromdate = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000");
            ViewBag.From = DateTime.Now.FirstDayOfMonth().ToString("dd/MM/yyyy");

            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 00:00:00.000");
                ViewBag.From = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }
            string todate = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd 23:59:59.998");
            ViewBag.To = DateTime.Now.LastDayOfMonth().ToString("dd/MM/yyyy");

            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                todate = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 23:59:59.998");
                ViewBag.To = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }

            var export = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["hdexport"]))
                export = Request.QueryString["hdexport"].ToInt(0);
            if(export == 1)
            {
                page = 1;
                pageSize = int.MaxValue;
            }   
            List<Admin_Statistical_SalerRevenue_Result> l = new List<Admin_Statistical_SalerRevenue_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_Statistical_SalerRevenue(search, fromdate, todate, aclog.ID, aclog.RoleID, (page - 1) * pageSize, pageSize).ToList();
            }
            ViewBag.StartIndex = (page - 1) * pageSize;
            int total = 0;
            if (l.Count > 0)
            {
                total = l[0].TotalRow;
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
        public ActionResult CollectToDelivery(int page = 1)
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
            List<Admin_COD_GetList_Result> l = new List<Admin_COD_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_COD_GetList(search, (page - 1) * pageSize, pageSize).ToList();
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
        public ActionResult RevenueDetail(int page = 1)
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
            int tvv = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["tvv"]))
            {
                tvv = Request.QueryString["tvv"].ToInt(0);
            }
            ViewBag.TVV = tvv;
            List<Admin_GetStudentRevenueByTVV_Result> l = new List<Admin_GetStudentRevenueByTVV_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_GetStudentRevenueByTVV(search, tvv, (page - 1) * pageSize, pageSize).ToList();
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
    }
    public class ThongKe
    {
        public tbl_TeacherCourse l { get; set; }
        public int RecordZoom { get; set; }
    }
    public class ScheduleDetails
    {
        public string Ngay { get; set; }
        public string KhungGio { get; set; }
    }
}