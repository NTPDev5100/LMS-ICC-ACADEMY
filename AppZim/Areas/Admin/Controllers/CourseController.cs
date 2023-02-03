using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Org.BouncyCastle.Asn1.Ocsp;
using PagedList;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class CourseController : BaseController
    {
        #region khóa học bình thường
        //tạo khóa học
        [HttpGet]
        public ActionResult CreateCourse()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            return View();
        }

        //danh sách khóa học
        [HttpGet]
        public async Task<ActionResult> CourseList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;

            //page size
            int pageSize = 9;
            //key search
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            // trung tâm
            int sch = 0;
            if (Request.QueryString["school"].ToInt(0) != 0)
            {
                ViewBag.School = Request.QueryString["school"].ToInt(0);
                sch = Request.QueryString["school"].ToInt(0);
            }
            // lớp
            int lop = 0;
            if (Request.QueryString["lop"].ToInt(0) != 0)
            {
                ViewBag.Lop = Request.QueryString["lop"].ToInt(0);
                lop = Request.QueryString["lop"].ToInt(0);
            }
            //sắp xếp
            //int sort = 0;
            //if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            //{
            //    sort = Request.QueryString["sort"].ToInt(0);
            //}

            // lấy ngày đầu và ngày cuối tháng

            string fromdate = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd 00:00:00.000");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                fromdate = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 00:00:00.000");
                ViewBag.From = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }
            string todate = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd 23:59:59.998");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                todate = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("yyyy-MM-dd 23:59:59.998");
                ViewBag.To = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }
            var teacher = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTeacher"]))
                teacher = Request.QueryString["slTeacher"].ToInt(0);
            ViewBag.Teacher = teacher;
            List<get_list_of_course_Result> l = new List<get_list_of_course_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_course(search, sch, lop, teacher, fromdate, todate, pageSize, (page - 1) * pageSize).ToList();
            }
            //ViewBag.Sort = sort;
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

        /// <summary>
        /// Danh sách khóa học online
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult> CourseListOnline(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var df = DateTime.ParseExact("01/01/1001", "dd/MM/yyyy", null).Date;

            //page size
            int pageSize = 9;
            //key search
            string search = "";
            ViewBag.Search = Request.QueryString["search"];
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
                search = Request.QueryString["search"].ToString().Trim();
            // trung tâm
            int sch = 0;
            if (Request.QueryString["school"].ToInt(0) != 0)
            {
                ViewBag.School = Request.QueryString["school"].ToInt(0);
                sch = Request.QueryString["school"].ToInt(0);
            }
            //sắp xếp
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
            }
            string fromdate = string.IsNullOrEmpty(Request.QueryString["from"]) != true ? DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString() : null;
            string todate = string.IsNullOrEmpty(Request.QueryString["to"]) != true ? DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString() : null;
            if (fromdate != null)
            {
                ViewBag.From = DateTime.ParseExact(Request.QueryString["from"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy");
            }
            if (todate != null)
            {
                ViewBag.To = DateTime.ParseExact(Request.QueryString["to"].ToString().Trim(), "dd/MM/yyyy", null).ToString("dd/MM/yyyy"); ;
            }

            List<ASW_CourseList_Online_Result> l = new List<ASW_CourseList_Online_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CourseList_Online(search, sch, fromdate, todate, sort, pageSize, page - 1).ToList();
            }
            ViewBag.Sort = sort;
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

        #region kiểm tra lịch phòng
        //[HttpGet]
        //public ActionResult ScheduleZoom()
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    return View();
        //}
        [HttpGet]
        public ActionResult ScheduleStudyTimeSingle()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            return View();
        }

        public class CheckSchedule
        {
            public int id { get; set; }
            public int course { get; set; }
            public string title { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public bool allDay { get; set; }
            public string url { get; set; }
            public string borderColor { get; set; }
            public bool checkday { get; set; }
            public bool daydisable { get; set; }
            public string subject { get; set; }
            public int studyid { get; set; }
            public string room { get; set; }
            public string teacher { get; set; }
            public string school { get; set; }
        }
        /// <summary>
        /// không bị ảnh hưởng bởi timezone
        /// </summary>
        public class NewSchedule
        {
            public int id { get; set; }
            public int course { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public bool allDay { get; set; }
            public string url { get; set; }
            public string borderColor { get; set; }
            public bool checkday { get; set; }
            public bool daydisable { get; set; }
            public string subject { get; set; }
            public int studyid { get; set; }
            public string room { get; set; }
            public string teacher { get; set; }
            public string school { get; set; }
        }

        /// <summary>
        /// Kiểm tra phòng
        /// </summary>
        /// <param name="schoolid"></param>
        /// <param name="roomid"></param>
        /// <param name="studyid"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadSchedule(int schoolid, string roomid, string studyid, string stime, string etime)
        {
            var st = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
            var ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);

            int tt = 60;
            if (!string.IsNullOrEmpty(etime))
            {
                string check = "";
                do
                {
                    check = ed.ToString("dddd");
                    if (!check.Contains("Monday"))
                        ed = ed.AddDays(1);
                }
                while (!check.Contains("Monday"));
                tt = (ed - st).Days;
            }


            StringBuilder html = new StringBuilder();
            DateTime monday = StartOfWeek(st, DayOfWeek.Monday);

            string[] rooms = roomid.Split(',');

            //var roomz = SchoolRoomTable.getbyID(roomid.ToInt(0));
            var sty = StudyTimeTable.getall();
            for (int day = 0; day <= tt; day += 7)
            {

                html.Append("<tr class=\"hd\">");
                html.Append("   <th style=\"text-align: center; vertical-align: middle;\">CA</th>");
                for (int d = 0; d < 7; d++)
                {
                    html.Append("<th>" + monday.AddDays(d).ToString("dddd") + " <br/> " + monday.AddDays(d).ToString("dd/MM") + "</th>");
                }
                html.Append("</tr>");

                List<tbl_StudyTime> lca = new List<tbl_StudyTime>();
                if (studyid.ToInt(-1) != 0)
                {
                    string[] arrca = studyid.Split(',');
                    for (int i = 0; i < arrca.Length; i++)
                    {
                        var t = StudyTimeTable.getbyid(arrca[i].ToInt(0));
                        if (t != null)
                            lca.Add(t);
                    }
                }
                else
                    lca = StudyTimeTable.getall();
                lca = lca.OrderBy(n => n.sTime).ToList();

                foreach (var item in lca)
                {
                    html.Append("<tr>");
                    html.Append("   <th><p class=\"from-time\">" + item.sTime + "</p><span class=\"to-time\">" + item.eTime + "</span></th>");
                    for (int d = 0; d < 7; d++)
                    {
                        //html.Append("   <td style=\"background: aqua;\"></td>");     
                        var check = CoursesScheduleTable.checkroomreturncourseid(schoolid, item.ID, monday.AddDays(d).Date, 0);

                        if (check == 0)//được // còn lịch
                            html.Append("   <td></td>");
                        else
                        {
                            var c = CoursesTable.getbyid(check);
                            html.Append("   <td style=\"text-align: left; background: #fbfff1;\">" + c.CourseName + "</td>");
                        }
                    }
                    html.Append("</tr>");
                }
                monday = monday.AddDays(7);
            }
            return Json(new { rs = true, l = html.ToString() }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy kiểm tra lịch ca học đơn
        /// </summary>
        /// <param name="schoolid"></param>
        /// <param name="roomid"></param>
        /// <param name="studyid"></param>
        /// <param name="stime"></param>
        /// <param name="etime"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadScheduleCheckStudyTimeSignle(int schoolid, string roomid, string studyid, string stime, string etime)
        {
            var st = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
            var ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);

            int tt = 60;
            if (!string.IsNullOrEmpty(etime))
            {
                string check = "";
                do
                {
                    check = ed.ToString("dddd");
                    if (!check.Contains("Monday"))
                        ed = ed.AddDays(1);
                }
                while (!check.Contains("Monday"));
                tt = (ed - st).Days;
            }
            StringBuilder html = new StringBuilder();
            DateTime monday = StartOfWeek(st, DayOfWeek.Monday);
            //var roomz = SchoolRoomTable.getbyID(roomid.ToInt(0));
            var caz = StudyTimeTable.getbyid(studyid.ToInt(0));
            if (caz == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            for (int day = 0; day <= tt; day += 7)
            {
                html.Append("<tr class=\"hd\">");
                html.Append("   <th style=\"text-align: center; vertical-align: middle;\">CA</th>");
                for (int d = 0; d < 7; d++)
                {
                    html.Append("<th>" + monday.AddDays(d).ToString("dddd") + " <br/> " + monday.AddDays(d).ToString("dd/MM") + "</th>");
                }
                html.Append("</tr>");

                //List<tbl_SchoolRoom> lroom = new List<tbl_SchoolRoom>();
                //if (roomid.ToInt(-1) != 0)
                //{
                //    string[] arrroom = roomid.Split(',');
                //    for (int i = 0; i < arrroom.Length; i++)
                //    {
                //        var t = SchoolRoomTable.getbyID(arrroom[i].ToInt(0));
                //        if (t != null)
                //            lroom.Add(t);
                //    }
                //}
                //else
                //    lroom = SchoolRoomTable.getbyschoolid(schoolid);

                //foreach (var item in lroom)
                //{
                //    html.Append("<tr>");
                //    html.Append("   <th><p class=\"from-time\">" + item.RoomName + "</p></th>");
                //    for (int d = 0; d < 7; d++)
                //    {
                //        //html.Append("   <td style=\"background: aqua;\"></td>");     
                //        var check = CoursesScheduleTable.checkroomreturncourseid(schoolid, caz.ID, item.ID, monday.AddDays(d).Date, 0);
                //        if (check == 0)//được // còn lịch
                //            html.Append("   <td></td>");
                //        else
                //        {
                //            var c = CoursesTable.getbyid(check);
                //            html.Append("   <td style=\"text-align: left; background: #fbfff1;\">" + c.CourseName + "</td>");
                //        }
                //    }
                //    html.Append("</tr>");
                //}
                monday = monday.AddDays(7);
            }
            return Json(new { rs = true, l = html.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region kiểm tra lịch giáo viên
        [HttpGet]
        public ActionResult ScheduleTeacher()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
            return View();
        }
        /// <summary>
        /// Lấy danh sách giáo viên theo thành phố
        /// </summary>
        /// <param name="id">id thành phố</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadTeacherByCity(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var t = AccountTable.getbyrole(4).Where(n => n.LocationID == id && n.Status == 2).OrderBy(n => n.FullName).ToList();
            StringBuilder shtml = new StringBuilder();
            t = t.OrderBy(n => n.FullName).ToList();
            foreach (var item in t)
            {
                shtml.Append(" <option value=\"" + item.ID + "\">" + item.FullName + "</option>");
            }
            return Json(new { rs = shtml.ToString() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Lấy danh sách giáo viên theo thành phố
        /// </summary>
        /// <param name="id">id thành phố</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadTeacherByCenter(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var t = AccountTable.getbyrole(4).Where(n => n.SchoolID == id && n.Status == 2).OrderBy(n => n.FullName).ToList();
            StringBuilder shtml = new StringBuilder();
            t = t.OrderBy(n => n.FullName).ToList();
            shtml.Append(" <option value=\"0\">Tất cả giáo viên</option>");
            foreach (var item in t)
            {
                shtml.Append(" <option value=\"" + item.ID + "\">" + item.FullName + "</option>");
            }
            return Json(new { rs = shtml.ToString() }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy danh sách giáo viên theo thành phố nhưng theo khóa học, xet giáo viên chủ nhiệm
        /// </summary>
        /// <param name="id">id khóa học</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadTeacherByCourse(int courseid)
        {
            try
            {
                StringBuilder shtmlAcdemic = new StringBuilder();
                StringBuilder htmlSalary = new StringBuilder();
                StringBuilder shtmlTeacher = new StringBuilder();
                var course = CoursesTable.getbyid(courseid);
                var acdemics = AccountTable.getbyrole(7);
                var salarys = CourseSalaryTable.getbycourse(courseid);

                foreach (var item in acdemics)
                {
                    if (course.AcademicUID != null)
                    {
                        if (item.ID == course.AcademicUID.Value)
                            shtmlAcdemic.Append(" <option selected value=\"" + item.ID + "\">" + item.FullName + "</option>");
                        else
                            shtmlAcdemic.Append(" <option value=\"" + item.ID + "\">" + item.FullName + "</option>");
                    }
                    else
                    {
                        shtmlAcdemic.Append(" <option value=\"" + item.ID + "\">" + item.FullName + "</option>");
                    }
                }
                var teacherList = new List<Admin_GetTeacherByClass_Result>();
                using (var db = new ZimEntities())
                {
                    teacherList = db.Admin_GetTeacherByClass(course.ClassID ?? 0).ToList();
                }
                foreach (var item in teacherList)
                {
                    shtmlTeacher.Append("<option value=\"" + item.UID + "\">" + item.FullName + "</option>");
                }

                foreach (var i in salarys)
                {
                    htmlSalary.Append("<div class=\"form-row\" id=\"numberSalary-" + i.ID + "\" >");
                    htmlSalary.Append("   <div class=\"form-group col-sm-6\">");
                    htmlSalary.Append("       <input name=\"txt-numberoflessontemp\" data-type=\"number\" class=\"form-control\" placeholder=\"Số buổi áp dụng\" value=\"" + i.NumberOfLesson + "\" />");
                    htmlSalary.Append("   </div>");
                    htmlSalary.Append("   <div class=\"col-sm-5 form-group\">");
                    htmlSalary.Append("       <input name=\"txt-salarytemp\" data-type=\"currency\" class=\"form-control\" placeholder=\"Lương/ buổi\" value=\"" + String.Format("{0:0,0}", i.SalaryOfLesson) + "\" />");
                    htmlSalary.Append("   </div>");
                    htmlSalary.Append("   <div class=\"col-sm-1 form-group\" style=\"padding: 4px\" ");
                    htmlSalary.Append("       <a href=\"javascript:;\" onclick=\"remove_salary('" + i.ID + "')\" style=\"float:left; font-size:20px; color: red\"><i class=\"far fa-times-circle\"></i></a>");
                    htmlSalary.Append("   </div>");
                    htmlSalary.Append("</div>");
                }

                return Json(new { rs = htmlSalary.ToString(), rs2 = shtmlAcdemic.ToString(), rs3 = shtmlTeacher.ToString(), lesson = course.DifferenceSchedule, status = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { status = false }, JsonRequestBehavior.AllowGet);
            }

        }
        /// <summary>
        /// Cập nhật giáo viên chủ nhiệm
        /// </summary>
        /// <param name="f"></param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult UpdateTeacherByCourse(FormCollection f)
        {
            var url = f.Get("hdf-url").ToString();
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6 && aclog.RoleID != 7 && aclog.RoleID != 8)// admin, manager, quản lý
                return RedirectToAction("Signin", "Login", new { area = "" });

            bool changeAcdemic = false;
            tbl_Courses c = CoursesTable.getbyid(f.Get("hdfID").ToInt(0));
            if (c == null)
            {
                SetAlert("Không có dữ liệu", "e");
                return Redirect(url);
            }

            string coursename = c.CourseName;
            if (!string.IsNullOrEmpty(f.Get("txt-coursename")))
            {
                coursename = f.Get("txt-coursename").ToString();
            }

            int status = 1;
            if (!string.IsNullOrEmpty(f.Get("slStatus-field")))
            {
                status = f.Get("slStatus-field").ToString().ToInt(0);
            }

            string statusName = "";
            switch (status)
            {
                case 1:
                    statusName = "Sắp mở";
                    break;
                case 2:
                    statusName = "Đang mở";
                    break;
                case 3:
                    statusName = "Đã đóng";
                    break;
            }

            CoursesTable.UpdateName(c.ID, coursename, aclog.UserName, status, statusName);

            tbl_Account acdemic = new tbl_Account();
            acdemic = AccountTable.getbyID(f.Get("ddl-acdemic-officer").ToInt(0));

            if (aclog.RoleID == 7)
                acdemic = AccountTable.getbyID(c.AcademicUID.Value);

            if (c.AcademicUID != null)
            {
                if (acdemic != null)
                {
                    if (acdemic.ID != c.AcademicUID)
                        changeAcdemic = true;
                }

            }
            else
                changeAcdemic = true;

            int lesson = 0;
            if (!string.IsNullOrEmpty(f.Get("txt-lesson")))
                lesson = f.Get("txt-lesson").ToInt(0);

            int teacher = 0;
            if (!string.IsNullOrEmpty(f.Get("slTeacher")))
                teacher = f.Get("slTeacher").ToInt(0);

            string salary = "";
            if (!string.IsNullOrEmpty(f.Get("hd-salary")))
            {
                salary = f.Get("hd-salary").ToString();
            }

            string numberOfLesson = "";
            if (!string.IsNullOrEmpty(f.Get("txt-numberoflessontemp")))
            {
                numberOfLesson = f.Get("txt-numberoflessontemp").ToString();
            }

            if (!string.IsNullOrEmpty(salary) && !string.IsNullOrEmpty(numberOfLesson))
            {
                CourseSalaryTable.DeleteByCourseId(c.ID, aclog.UserName);

                List<double> salarys = salary.Split(',').Select(Double.Parse).ToList();
                List<int> lessons = numberOfLesson.Split(',').Select(Int32.Parse).ToList();

                for (var i = 0; i < lessons.Count; i++)
                {
                    CourseSalaryTable.insert(c.ID, lessons[i], salarys[i], aclog.UserName);
                }

                Thread tschedule = new Thread(() =>
                {
                    CoursesScheduleTable.UpdateSalaryForSchedule(c.ID);
                });
                tschedule.Start();
            }

            if (changeAcdemic)
            {
                CoursesTable.updateteacherlead(c.ID, lesson, acdemic, aclog.UserName);
                //gửi thông báo cho admin và giáo viên đó
                List<tbl_Account> ad = AccountTable.getall().Where(n => (n.RoleID == 1) && n.Status == 2).ToList();
                ad.Remove(aclog);
                //Gửi trước cho admin và manager
                foreach (var itemac in ad)
                {
                    tbl_NotificationSingle nt = new tbl_NotificationSingle();

                    if (changeAcdemic)//Thông báo thay đổi Học vụ
                    {
                        nt = NotificationTable.insertsingle("", "Chỉ định quản lý học vụ lớp", "Học vụ [ " + acdemic.FullName + " ] được chỉ định quản lý lớp [ " + c.CourseName + " ].", itemac.ID, aclog.UserName, 16, 0);
                    }
                }

                if (changeAcdemic)//Gửi cho học vụ đó
                {
                    tbl_NotificationSingle nt = new tbl_NotificationSingle();
                    nt = NotificationTable.insertsingle("", "Chỉ định quản lý học vụ lớp", "Học vụ [ " + acdemic.FullName + " ] được chỉ định quản lý lớp [ " + c.CourseName + " ].", acdemic.ID, aclog.UserName, 16, 0);
                }
            }
            if (teacher != 0)
            {
                var schedules = CoursesScheduleTable.getbycourseid(c.ID).Where(x => x.FisnishStatus == 0 || x.FisnishStatus == 2).ToList();
                for (var i = 0; i < schedules.Count(); i++)
                {
                    CoursesScheduleTable.updateTeacher(schedules[i].ID, teacher, aclog.UserName);
                }
                TeacherCourseTable.insert(teacher, c, aclog.UserName);
            }
            return Redirect(url);
        }

        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        [HttpPost]
        public async Task<ActionResult> LoadScheduleTeacher(string teacherid, string studyid, string stime, string etime)
        {
            var sd = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
            var ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);

            int tt = 60;
            if (!string.IsNullOrEmpty(etime))
            {
                string check = "";
                do
                {
                    check = ed.ToString("dddd");
                    if (!check.Contains("Monday"))
                        ed = ed.AddDays(1);
                }
                while (!check.Contains("Monday"));
                tt = (ed - sd).Days;
            }
            StringBuilder html = new StringBuilder();
            DateTime monday = StartOfWeek(sd, DayOfWeek.Monday);
            var te = AccountTable.getbyID(teacherid.ToInt(0));
            for (int day = 0; day <= tt; day += 7)
            {

                html.Append("<tr class=\"hd\">");
                html.Append("   <th style=\"text-align: center;\">CA</th>");
                for (int d = 0; d < 7; d++)
                {
                    html.Append("<th>" + monday.AddDays(d).ToString("dddd") + " <br/> " + monday.AddDays(d).ToString("dd/MM") + "</th>");
                }
                html.Append("</tr>");

                string[] arrca = studyid.Split(',');
                List<tbl_StudyTime> lca = new List<tbl_StudyTime>();
                if (studyid == "0")
                {
                    lca = StudyTimeTable.getall().OrderBy(n => n.sTime).ToList();
                }
                else
                {
                    for (int i = 0; i < arrca.Length; i++)
                    {
                        await Task.Run(() =>
                        {
                            var t = StudyTimeTable.getbyid(arrca[i].ToInt(0));
                            if (t != null)
                                lca.Add(t);
                        });

                    }
                }
                lca = lca.OrderBy(n => n.sTime).ToList();
                foreach (var item in lca)
                {
                    html.Append("<tr>");
                    html.Append("   <th><p class=\"from-time\">" + item.sTime + "</p><span class=\"to-time\">" + item.eTime + "</span></th>");
                    for (int d = 0; d < 7; d++)
                    {
                        var checkteacher = CoursesScheduleTable.checkteacher(te.ID, monday.AddDays(d).Date, item.ID, 0);
                        if (checkteacher.checktrue)//được
                            html.Append("   <td></td>");
                        else
                        {
                            //kiểm tra xem có đăng ký nghỉ ko
                            var checkoff = DayOffTeacherTable.getbyuiddate(te.ID, monday.AddDays(d).Date).Where(n => n.StudyTimeID == item.ID && n.isHide != true).FirstOrDefault();
                            if (checkoff != null)
                            {
                                html.Append("   <td class=\"off-day\"><span class=\"img-off\"><img src=\"/app-assets/zimv2/assets/img/day-off.png\" alt=\"icon\"></span></td>");
                            }
                            else
                            {
                                var c = CoursesTable.getbyid(checkteacher.courseid);
                                if (c != null)
                                {
                                    string sub = "";

                                    if (CoursesScheduleTable.getbyteacherid(te.ID).Where(n => n.CourseID == c.ID && n.Date == monday.AddDays(d).Date).FirstOrDefault().SubjectID != null)
                                    {
                                        try
                                        {
                                            sub = SubjectTable.getbyID(CoursesScheduleTable.getbyteacherid(te.ID).Where(n => n.CourseID == c.ID && n.Date == monday.AddDays(d).Date).FirstOrDefault().SubjectID.Value).SubjectName;

                                        }
                                        catch
                                        {
                                            continue;
                                        }
                                    }
                                    if (string.IsNullOrEmpty(sub))
                                        html.Append("   <td style=\"background: #fbfff1;\">" + c.CourseName + "</td>");
                                    else
                                        html.Append("   <td style=\"background: #fbfff1;\">" + c.CourseName + "<br><span class=\"cl-sub\">" + sub + "</span></td>");
                                }
                                else
                                    html.Append("   <td style=\"background: #fbfff1;\"></td>");
                            }
                        }
                    }
                    html.Append("</tr>");
                }
                monday = monday.AddDays(7);
            }
            return Json(new { rs = true, l = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        //Kiểm tra lịch tất cả giáo viên
        [HttpGet]
        public ActionResult ScheduleTeacherAll()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5 || aclog.RoleID == 4)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            // lấy ngày đầu và ngày cuối tháng
            DateTime date = DateTime.Now;
            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);
            ViewBag.From = firstDayOfMonth.ToString("dd/MM/yyyy");
            ViewBag.To = lastDayOfMonth.ToString("dd/MM/yyyy");
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> LoadScheduleTeacherAll(string teacherid, string studyid, string stime, string etime)
        {
            var sd = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
            var ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);

            int tt = 60;
            if (!string.IsNullOrEmpty(etime))
            {
                string check = "";
                do
                {
                    check = ed.ToString("dddd");
                    if (!check.Contains("Monday"))
                        ed = ed.AddDays(1);
                }
                while (!check.Contains("Monday"));
                tt = (ed - sd).Days;
            }
            StringBuilder html = new StringBuilder();
            DateTime monday = StartOfWeek(sd, DayOfWeek.Monday);
            for (int day = 0; day <= tt; day += 7)
            {
                html.Append("<tr class=\"hd\">");
                html.Append("   <th style=\"text-align: center;\">CA</th>");
                for (int d = 0; d < 7; d++)
                {
                    html.Append("<th>" + monday.AddDays(d).ToString("dddd") + " <br/> " + monday.AddDays(d).ToString("dd/MM") + "</th>");
                }
                html.Append("</tr>");

                string[] arrca = studyid.Split(',');
                string[] arrgv = teacherid.Split(',');
                List<tbl_StudyTime> lca = new List<tbl_StudyTime>();
                List<tbl_Account> lt = new List<tbl_Account>();
                if (studyid == "0")
                {
                    lca = StudyTimeTable.getall().OrderBy(n => n.sTime).ToList();
                }
                else
                {
                    for (int i = 0; i < arrca.Length; i++)
                    {
                        await Task.Run(() =>
                        {
                            var t = StudyTimeTable.getbyid(arrca[i].ToInt(0));
                            if (t != null)
                                lca.Add(t);
                        });
                    }
                }
                lca = lca.OrderBy(n => n.sTime).ToList();


                if (teacherid == "0")
                {
                    lt = AccountTable.getbyrole(4).Where(n => n.Status == 2).OrderBy(n => n.FullName).ToList();
                }
                else
                {
                    for (int i = 0; i < arrgv.Length; i++)
                    {
                        await Task.Run(() =>
                        {
                            var t = AccountTable.getbyID(arrgv[i].ToInt(0));
                            if (t != null)
                                lt.Add(t);
                        });
                    }
                }

                foreach (var item in lca)
                {
                    html.Append("<tr>");
                    html.Append("   <th><p class=\"from-time\">" + item.sTime + "</p><span class=\"to-time\">" + item.eTime + "</span></th>");
                    for (int d = 0; d < 7; d++)
                    {
                        StringBuilder dayHtml = new StringBuilder();
                        foreach (var te in lt)
                        {
                            var checkteacher = CoursesScheduleTable.checkteacher(te.ID, monday.AddDays(d).Date, item.ID, 0);
                            if (checkteacher.checktrue)//được
                                dayHtml.Append("<div style=\"background:#3d9e0ebf;padding: 5px;color: #fff;line-height: 1.7;\">" + te.FullName + "</div><br>");
                        }
                        html.Append("   <td>" + dayHtml.ToString() + "</td>");
                    }
                    html.Append("</tr>");
                }
                monday = monday.AddDays(7);
            }
            return Json(new { rs = true, l = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region tạo khóa học
        [HttpPost]// kiểm tra các ca xem có cùng số tiết ko, có bị đụng giờ ko
        public ActionResult KiemTraSoTietCa(string listafter, string listbefore)
        {
            string[] arrlistafter = listafter.Split(',');
            string[] arrlistbefore = listbefore.Split(',');
            int id = 0;

            if (arrlistbefore.Length > 0)
            {
                //kiếm id mới
                for (int i = 1; i < arrlistafter.Length; i++)
                {
                    bool ck = false;
                    for (int j = 1; j < arrlistbefore.Length; j++)
                    {
                        if (arrlistbefore[j].ToInt(0) == arrlistafter[i].ToInt(0))
                        {
                            ck = true;
                            break;
                        }
                    }
                    if (!ck)
                    {
                        id = arrlistafter[i].ToInt(0);
                        break;
                    }
                }

                //nếu có id mới
                bool bl = true;
                if (id != 0)
                {
                    var cuoi = StudyTimeTable.getbyid(id);
                    var dau = StudyTimeTable.getbyid(arrlistbefore[0].ToInt(0));
                    if (cuoi.StyleLesson != dau.StyleLesson)
                        bl = false;
                    if (bl)
                    {
                        //kiểm tra xem có đụng giờ ko giờ
                        string[] iscuoi = cuoi.sTime.Split(':');
                        TimeSpan isth = new TimeSpan(iscuoi[0].ToInt(), iscuoi[1].ToInt(0), 0);
                        DateTime st = GetDateTime.Now.Date.Add(isth);
                        DateTime et = st.AddMinutes(cuoi.StyleLesson.Value);

                        for (int i = 0; i < arrlistbefore.Length; i++)
                        {
                            var cacheck = StudyTimeTable.getbyid(arrlistbefore[i].ToInt(0));
                            string[] ca = cacheck.sTime.Split(':');
                            TimeSpan cath = new TimeSpan(ca[0].ToInt(), ca[1].ToInt(0), 0);
                            DateTime ist = GetDateTime.Now.Date.Add(cath);
                            DateTime iet = ist.AddMinutes(cacheck.StyleLesson.Value);

                            if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                            {
                                bl = false;
                                break;
                            }

                            if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                            {
                                bl = false;
                                break;
                            }

                            if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                            {
                                bl = false;
                                break;
                            }
                        }
                    }
                }
                if (!bl)
                    return Json(new { rs = false, id = id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// kiểm tra các học viên có cùng ngôn ngữ không
        public ActionResult KiemTraHocVien(string listafter, string listbefore)
        {
            string[] arrlistafter = listafter.Split(',');
            string[] arrlistbefore = listbefore.Split(',').Where(x => x.Length > 0).ToArray();
            int id = 0;

            if (arrlistbefore.Length > 0)
            {
                //kiếm id mới
                for (int i = 1; i < arrlistafter.Length; i++)
                {
                    bool ck = false;
                    for (int j = 1; j < arrlistbefore.Length; j++)
                    {
                        if (arrlistbefore[j].ToInt(0) == arrlistafter[i].ToInt(0))
                        {
                            ck = true;
                            break;
                        }
                    }
                    if (!ck)
                    {
                        id = arrlistafter[i].ToInt(0);
                        break;
                    }
                }

                //nếu có id mới
                bool bl = true;
                if (id != 0)
                {
                    var cuoi = StudentAppointmentTable.getbyid(id);
                    var dau = StudentAppointmentTable.getbyid(arrlistbefore[0].ToInt(0));
                    var stuc = AccountTable.getbyID(cuoi.StudentUID.Value);
                    var stud = AccountTable.getbyID(dau?.StudentUID ?? 0);
                    if (stuc.Language != stud.Language)
                        bl = false;
                }
                if (!bl)
                    return Json(new { rs = false, id = id }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp của khối
        public ActionResult LoadGrade(int student)
        {
            var hentest = StudentAppointmentTable.getbyid(student);
            var stu = AccountTable.getbyID(hentest.StudentUID.Value);
            var khois = GradeTable.getall().Where(n => n.Language == stu.Language.Value).ToList();
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"0\">---</option>");
            foreach (var item in khois)
            {
                html.Append("<option value=\"" + item.ID + "\">" + item.GradeName + "</option>");
            }
            return Json(new { rs = html.ToString() }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]// tải lớp của khối
        public ActionResult LoadClass(int gradeid, int student)
        {
            var hentest = StudentAppointmentTable.getbyid(student);
            var stu = AccountTable.getbyID(hentest.StudentUID.Value);
            var lops = ClassTable.getgradeid(gradeid).Where(n => n.Language == stu.Language.Value && (n.Coefficient == hentest.CoefficientID || hentest.CoefficientID == 0)).ToList();
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"0\">---</option>");
            foreach (var item in lops)
            {
                html.Append("<option value=\"" + item.ID + "\">" + item.ClassName + "</option>");
            }
            return Json(new { rs = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp online của khối
        public ActionResult LoadClassOnline(int gradeid)
        {
            var lops = ClassTable.getgradeid(gradeid).Where(n => n.isHide != true && n.TypeClass == 2).ToList();
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">---</option>");
            foreach (var item in lops)
            {
                html.Append("<option value=\"" + item.ID + "\">" + item.ClassName + "</option>");
            }
            return Json(new { rs = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp của khối
        public ActionResult LoadGiaoTrinh(int classid, string studyid)
        {
            string[] arrca = studyid.Split(',');
            var ca = StudyTimeTable.getbyid(arrca[0].ToInt(0));
            if (ca == null)
                return Json(new { rs = "" }, JsonRequestBehavior.AllowGet);

            var giaotrinhs = new List<Admin_Curriculumn_LoadForCreateCourse_Result>();
            using (var db = new ZimEntities())
            {
                giaotrinhs = db.Admin_Curriculumn_LoadForCreateCourse(classid, ca.StyleLesson).ToList();
            }
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"0\">---</option>");
            foreach (var item in giaotrinhs)
            {
                html.Append("<option data-total=\"" + item.TotalLesson + "\" value=\"" + item.ID + "\">" + item.CurriculumsName + " - " + item.TotalLesson + " buổi </option>");
            }
            return Json(new { rs = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]// tải lớp của khối
        public ActionResult LoadGiaoTrinhOnline(int classid)
        {
            var giaotrinhs = CurriculumTable.getbyclass(classid).ToList();
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">---</option>");
            foreach (var item in giaotrinhs)
            {
                var check = ClassTable.getbyID(item.ClassID.Value);
                if (check.TypeClass == 2) //chỉ append giáo trình offline
                {
                    html.Append("<option value=\"" + item.ID + "\">" + item.CurriculumsName + "</option>");
                }
            }
            return Json(new { rs = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        public class TietHoc
        {
            public int TietID { get; set; }// ID tiết học
            public int TenTiet { get; set; }// tên tiết học
            public string TenTietvsMon { get; set; }// tên tiết học
        }
        public class NgayHoc
        {
            public int ID { get; set; }
            public string eventName { get; set; }
            public string Color { get; set; }
            public string calendar { get; set; }
            public string date { get; set; }
            public int TeacherID { get; set; }
            public string TeacherName { get; set; }
            public int CaID { get; set; }
            public string CaName { get; set; }
        }
        [HttpPost]// tải tiết học
        public ActionResult LoadTietHoc(int GiaoTrinhID)
        {
            var giaotrinh = CurriculumTable.getbyID(GiaoTrinhID);
            var chitietGT = CurriculumDetailTable.getbycurriculumid(giaotrinh.ID);

            List<NgayHoc> l = new List<NgayHoc>();
            int ID = 1;

            if (chitietGT != null)
            {
                for (int i = 0; i < chitietGT.Count; i += 1)
                {
                    NgayHoc lt = new NgayHoc();
                    int SoTiet = i + 1;

                    lt.ID = ID;
                    lt.eventName = "Ngày học " + ID + "";
                    lt.Color = "orange";
                    lt.calendar = "Đã chọn ngày học";
                    l.Add(lt);
                    ID += 1;
                }
                l = l.OrderBy(n => n.ID).ToList();
            }
            return Json(new { d = l }, JsonRequestBehavior.AllowGet);
        }


        public class Ngay
        {
            public DateTime SortDay { get; set; }
            public string Day { get; set; }
        }
        [HttpPost]
        public ActionResult TaiNgayHoc(string CaID, string Date)//lấy ra những ngày học được
        {
            string[] arrca = CaID.Split(',');
            // chuỗi json {"id": 17,limit: 1, eventName: 'Blank Event', calendar: 'Blank', color: 'blank', date: '2018-02-09'}
            List<Ngay> l = new List<Ngay>();
            DateTime sDate = DateTime.ParseExact(Date, "dd/MM/yyyy", null);
            //DateTime sDate = Convert.ToDateTime(Date);

            DateTime Day = sDate.Date;


            for (int i = 1; i <= 180; i++)
            {
                Ngay t = new Ngay();
                if (i != 1)
                    Day = Day.AddDays(1);

                var checkdayoff = DayOffTable.getbydate(Day);
                if (checkdayoff != null)
                    continue;

                Task<int> rs = TinhCa(Day, CaID, 0);
                int limit = rs.Result;

                t.Day = "{\"id\":\"d-" + i + "\",\"limit\":" + limit + ", \"eventName\":\"Blank Event\", \"calendar\":\"blank\", \"color\":\"blank\", \"date\":\"" + Day.ToString("yyyy-MM-dd") + "\"}";
                if (limit > 0)
                    l.Add(t);
            }
            l = l.ToList();
            return Json(new { d = l }, JsonRequestBehavior.AllowGet);
        }

        public async Task<int> TinhCa(DateTime Day, string caid, int cid)
        {
            string[] arrca = caid.Split(',');
            int limit = arrca.Length;
            return limit;
        }

        public class DsGiaoVien
        {
            public int id { get; set; }
            public string name { get; set; }
            public bool select { get; set; }
        }
        [HttpPost]//lấy ra những giáo viên dạy được
        public ActionResult TaiGiaoVien(int SchoolID, int MonID, int CaID, string Date, int RoomID, int teacher)
        {
            try
            {
                //2018-04-01
                //[{ id: 1, name: 'tutor 1' }, { id: 2, name: 'tutor 2' }, { id: 3, name: 'tutor 3' }]           
                DateTime sDate = DateTime.ParseExact(Date, "yyyy-MM-dd", null);
                var caduocchon = StudyTimeTable.getbyid(CaID);
                List<DsGiaoVien> l = new List<DsGiaoVien>();
                var mon = SubjectTable.getbyID(MonID);

                var lAcc = AccountTable.getbyrole(4).Where(n => n.ID == teacher).ToList();//lấy giáo viên roleID = 4
                if (lAcc != null)
                {
                    //lấy ra giáo viên dạy được lớp đó và môn đó
                    List<tbl_Account> lgv = new List<tbl_Account>();
                    foreach (var item in lAcc)
                    {
                        //kiểm tra xem giáo viên có đăng ký nghỉ hay ko
                        var checkdayoffteacher = DayOffTeacherTable.getbyuiddate(item.ID, sDate.Date).Where(n => n.StudyTimeID == caduocchon.ID && n.isHide != true).FirstOrDefault();
                        if (checkdayoffteacher != null)
                            continue;

                        //xem thằng đó cáo dạy được ko
                        if (mon.SpecialSubject != true)//môn đặc biệt bỏ qua
                        {
                            var check = AssignTeacherClassTable.getbyuidclasssub(item.ID, mon.ClassID.Value, mon.ID);
                            if (check != null)
                            {
                                if (!check.isHide.Value)
                                {
                                    //xem thằng đó có rảnh ko
                                    var ckteacher = CoursesScheduleTable.checkteacher(item.ID, sDate.Date, caduocchon.ID, 0);
                                    if (ckteacher.checktrue)
                                    {
                                        item.FullName = item.FullName + " [Ca: " + caduocchon.sTime + "-" + caduocchon.eTime + "]";
                                        lgv.Add(item);
                                    }
                                }
                            }
                        }
                        else
                        {
                            //xem thằng đó có rảnh ko
                            var ckteacher = CoursesScheduleTable.checkteacher(item.ID, sDate.Date, caduocchon.ID, 0);
                            if (ckteacher.checktrue)
                            {
                                item.FullName = item.FullName + " [Ca: " + caduocchon.sTime + "-" + caduocchon.eTime + "]";
                                lgv.Add(item);
                            }
                        }
                    }
                    //
                    if (lgv.Count > 0)
                    {
                        tbl_AssignTeacherClass lvGV = new tbl_AssignTeacherClass();
                        if (mon.SpecialSubject != true)//môn đặc biệt bỏ qua
                        {
                            for (int i = 0; i < lgv.Count; i++)
                            {
                                var checklv = AssignTeacherClassTable.getbyuidclasssub(lgv[i].ID, mon.ClassID.Value, mon.ID);
                                if (i == 0)
                                    lvGV = checklv;
                                else
                                {
                                    if (checklv.SubjectLevel > lvGV.SubjectLevel)// thằng nào có lv môn cao hơn thì được ưu tiên dạy trước
                                        lvGV = checklv;
                                }
                                DsGiaoVien t = new DsGiaoVien();
                                t.id = lgv[i].ID;
                                t.name = lgv[i].FullName;
                                l.Add(t);
                            }
                        }
                        else
                        {
                            lvGV.UID = lgv[0].ID;
                            foreach (var item in lgv)
                            {
                                DsGiaoVien t = new DsGiaoVien();
                                t.id = item.ID;
                                t.name = item.FullName;
                                l.Add(t);
                            }
                        }
                        foreach (var jtem in l)
                        {
                            if (jtem.id == lvGV.UID)
                            {
                                jtem.select = true;
                                break;
                            }
                        }
                        l = l.OrderByDescending(n => n.select).ToList();
                    }
                }
                if (l.Count == 0)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                DsGiaoVien[] aRsGV = l.ToArray();
                return Json(new { rs = true, l = aRsGV, CaID = caduocchon.ID }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]//lấy danh sách ca học đã được chọn trước đó để show ra
        public ActionResult TaiCaHoc(string CaID, string CaDaChon)
        {
            string[] arrcadachon = CaDaChon.Split(',');
            string[] arrca = CaID.Split(',');

            List<tbl_StudyTime> cas = new List<tbl_StudyTime>();
            for (int i = 0; i < arrca.Length; i++)
            {
                var cai = StudyTimeTable.getbyid(arrca[i].ToInt(0));
                var check = true;
                if (arrcadachon.Length > 0)
                {
                    for (int j = 0; j < arrcadachon.Length - 1; j++)
                    {
                        var caj = StudyTimeTable.getbyid(arrcadachon[j].ToInt(0));
                        if (cai.ID == caj.ID)
                        {
                            check = false;
                            break;
                        }
                    }
                }
                if (check)
                    cas.Add(cai);
            }
            cas = cas.OrderBy(n => n.sTime).ToList();
            List<DsGiaoVien> studys = new List<DsGiaoVien>();
            DsGiaoVien studyone = new DsGiaoVien();
            studyone.id = 0;
            studyone.name = "---Chọn ca---";
            studys.Add(studyone);

            foreach (var cai in cas)
            {
                //add ca cho chọn 29/07
                DsGiaoVien study = new DsGiaoVien();
                study.id = cai.ID;
                study.name = "Ca: " + cai.sTime + ":" + cai.eTime;
                studys.Add(study);
            }
            DsGiaoVien[] aRsStudy = studys.ToArray();
            return Json(new { rs = true, study = aRsStudy }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //2018-03-01,4,1398.1399|2018-03-02,4,1400.1401| Ngày, ID Giáo viên, list ID tiet học, CaID
            if (string.IsNullOrEmpty(f.Get("hdflistCalender").ToString()))
            {
                SetAlert("Không có dữ liệu", "e");
                return RedirectToAction("CreateCourse", "Course");
            }

            DateTime Opencourse = DateTime.ParseExact(f.Get("txt-start-date").ToString(), "dd/MM/yyyy", null);

            var ct = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            if (ct == null)
            {
                SetAlert("Không tìm thấy trung tâm này", "e");
                return RedirectToAction("CreateCourse", "Course");
            }

            var hocvu = AccountTable.getbyID(f.Get("ddl-hocvu").ToInt(0));
            if (hocvu == null)
            {
                SetAlert("Vui lòng chọn học vụ", "e");
                return RedirectToAction("CreateCourse", "Course");
            }

            var lop = ClassTable.getbyID(f.Get("ddl-class").ToInt(0));
            if (lop == null)
            {
                SetAlert("Không tìm thấy chương trình học này", "e");
                return RedirectToAction("CreateCourse", "Course");
            }

            var gt = new tbl_Curriculum() { ID = 0, StyleLensson = 0, CurriculumsName = "" };
            string coursename = f.Get("txt-course-name").ToString();

            string salary = "";
            if (!string.IsNullOrEmpty(f.Get("txt-salary")))
            {
                salary = f.Get("txt-salary").ToString();
            }

            string lesson = "";
            if (!string.IsNullOrEmpty(f.Get("txt-numberoflesson")))
            {
                lesson = f.Get("txt-numberoflesson").ToString();
            }

            int realLesson = 0;
            if (!string.IsNullOrEmpty(f.Get("total-lesson")))
            {
                realLesson = f.Get("total-lesson").ToInt(0);
            }

            var c = CoursesTable.insert(coursename, lop.GradeID.Value, lop.GradeName, lop.ID, lop.ClassName, Opencourse.Date,
                0, "", gt.StyleLensson.Value, gt.ID, gt.CurriculumsName, 0,
                ct.ID, ct.SchoolName, 0, "", aclog.UserName, false, hocvu, realLesson);
            //
            tbl_Account teacher = null;
            if (c != null)//thành công
            {
                string[] sCalender = f.Get("hdflistCalender").ToString().Split('|');
                for (int i = 0; i < sCalender.Length; i++)
                {
                    string[] sdata = sCalender[i].Split(',');

                    DateTime date = DateTime.ParseExact(sdata[0], "yyyy-MM-dd", null);

                    if (teacher == null)
                        teacher = AccountTable.getbyID(sdata[1].ToInt(0));

                    CoursesScheduleTable.insert(c.ID, c.CourseSelfStudy.Value, 0, date.Date, sdata[2].ToInt(0), ct.ID, 0, teacher.ID, aclog.UserName, 0);
                    if (i == (sCalender.Length - 1))
                    {
                        CoursesTable.updateEndDate(c.ID, date);
                    }
                }

                Thread tcurriculumn = new Thread(() =>
                {
                    List<double> salarys = salary.Split(',').Select(Double.Parse).ToList();
                    List<int> lessons = lesson.Split(',').Select(Int32.Parse).ToList();

                    for (var i = 0; i < lessons.Count; i++)
                    {
                        CourseSalaryTable.insert(c.ID, lessons[i], salarys[i], aclog.UserName);
                    }

                    var inte = TeacherCourseTable.insert(teacher.ID, c, aclog.UserName);
                    string contentMail = "";
                    string tt = "";

                    // tạo thông báo cho giáo viên

                    using (var db = new ZimEntities())
                    {
                        int idTemplate = 1;
                        switch (teacher?.Language ?? 1)
                        {
                            case 2:
                                idTemplate = 36;
                                break;
                            case 6:
                                idTemplate = 37;
                                break;
                            default:
                                idTemplate = 38;
                                break;
                        }
                        var template = db.tbl_EmailTemplate.Where(x => x.ID == idTemplate).FirstOrDefault();                                             
                        //tiêu đề mail
                        tt = template.EmailTitle;
                        //nội dung mail                      
                        contentMail = template.EmailContent;
                        contentMail = contentMail.Replace("[ten-khoa-hoc]", coursename);
                        contentMail = contentMail.Replace("[chuong-trinh-hoc]", lop.ClassName);
                        contentMail = contentMail.Replace("[ngay-khai-giang]", Opencourse.Date.ToString());
                    }

                    if (inte != null)
                    {
                        var nt = NotificationTable.insertsingle("", "Nhận khóa học mới", "Bạn vừa nhận được lớp mới, bạn vào xem thông tin lớp của mình nhé", teacher.ID, aclog.UserName, 3, c.ID);
                        var teacherDevice = DeviceMobileTable.getbyUID(teacher.ID);
                        if (teacherDevice != null) // không null mới gửi được chứ
                        {
                            string[] teacherIncludeID = new[] { teacherDevice.IncludePlayerID };
                            PushNotifyMobile.SendNotifyMobile("Bạn vừa nhận được lớp mới, bạn vào xem thông tin lớp của mình nhé", teacherIncludeID);
                        }

                        if (!string.IsNullOrEmpty(teacher.Email))
                            AssetCRM.SendMail(teacher.Email, tt, contentMail);
                    }
                    var giaotrinhid = f.Get("ddl-cru").Split(',').Select(x => Convert.ToInt32(x)).ToList();
                    foreach (var g in giaotrinhid)
                    {
                        CurriculumnInCourseTable.insert(c.ID, g, aclog.UserName);
                    }
                });
                tcurriculumn.Start();

                SetAlert("Tạo khóa \"" + coursename + "\" thành công", "s");
                string title = f.Get("txt-title").ToString();
                string content = f.Get("txt-content").ToString();
                // them hoc vien vao CourseOfStudent (neu co)
                var listStudentAppoint = f.Get("liststudentappoint").Split(',').ToList();
                foreach (var item in listStudentAppoint)
                {
                    var app = StudentAppointmentTable.getbyid(item.ToInt(0));
                    if (app != null)
                    {
                        //Send mail cho học viên
                        var check = AssignCusAppointmentToCourse(app, c, title, content);
                    }
                }

                Thread tschedule = new Thread(() =>
                {
                    CoursesScheduleTable.UpdateSalaryForSchedule(c.ID);
                });
                tschedule.Start();
            }
            else
                SetAlert("Không tạo được khóa học", "e");
            return RedirectToAction("ScheduleCourse", "CourseDetail", new { area = "Admin", id = c.ID });
        }
        #endregion

        #endregion


        #region Khóa trực lớp tự học
        [HttpGet]
        public ActionResult CreatecourseSelfStudy()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            return View();
        }
        //danh sách khóa học
        [HttpGet]
        public ActionResult CourseListSelfStudy(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            int pageSize = 20;
            IEnumerable<tbl_Courses> l = CoursesTable.getall().Where(n => n.CourseSelfStudy == true).OrderByDescending(n => n.CreatedDate).ToList();
            //key search
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                ViewBag.Search = Request.QueryString["search"];
                string search = Request.QueryString["search"].ToString();

                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.CourseName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.SchoolName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    || AssetCRM.RemoveUnicode(ex.SchoolRoomName).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
            }
            //sort
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                    l = l.OrderBy(n => n.SchoolName).ToList();
                if (sort == 2)
                    l = l.OrderByDescending(n => n.SchoolName).ToList();
                if (sort == 3)
                    l = l.OrderBy(n => n.CourseName).ToList();
                if (sort == 4)
                    l = l.OrderByDescending(n => n.CourseName).ToList();
                if (sort == 5)
                    l = l.OrderBy(n => n.CreatedDate).ToList();
                if (sort == 6)
                    l = l.OrderByDescending(n => n.CreatedDate).ToList();
            }
            ViewBag.Sort = sort;
            ViewBag.Total = l.Count();
            return View(l.ToPagedList(page, pageSize));
        }
        //danh sách khóa học
        [HttpGet]
        public ActionResult CourseSelfStudyScheduleDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var l = CoursesTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy khóa này", "e");
                return RedirectToAction("CreatecourseSelfStudy", "Course");
            }
            if (l.CourseSelfStudy != true)
            {
                SetAlert("Không tìm thấy khóa này", "e");
                return RedirectToAction("CreatecourseSelfStudy", "Course");
            }
            return View(l);
        }

        //[HttpPost]// tải tiết học
        //public ActionResult SelfStudyLoadTietHoc(int studyid)
        //{
        //    var study = StudyTimeTable.getbyid(studyid);
        //    List<NgayHoc> l = new List<NgayHoc>();
        //    int ID = 1;
        //    var tday = 0;
        //    tday = 7 * 1;
        //    for (int i = 0; i < tday; i += 1)
        //    {
        //        NgayHoc lt = new NgayHoc();
        //        List<TietHoc> t = new List<TietHoc>();
        //        int SoTiet = i + 1;

        //        lt.ID = ID;
        //        lt.Type = 1;
        //        lt.eventName = "Ngày học " + ID + "";
        //        lt.Color = "orange";
        //        lt.calendar = "Đã chọn ngày học";
        //        for (int j = i; j < SoTiet; j++)
        //        {
        //            TietHoc th = new TietHoc();
        //            if (j < tday)
        //            {
        //                th.TietID = 0;
        //                th.TenTiet = 1;
        //                th.TenTietvsMon = "Trực lớp";
        //                t.Add(th);
        //            }
        //            else
        //                break;
        //        }
        //        lt.Tiet = t;
        //        lt.MonID = 0;
        //        l.Add(lt);
        //        ID += 1;
        //    }
        //    l = l.OrderBy(n => n.ID).ToList();

        //    return Json(new { d = l }, JsonRequestBehavior.AllowGet);
        //}
        [HttpPost]//lấy ra những giáo viên dạy được
        public ActionResult SelfStudyTaiGiaoVien(int SchoolID, int CaID, string Date)
        {
            //2018-04-01
            //[{ id: 1, name: 'tutor 1' }, { id: 2, name: 'tutor 2' }, { id: 3, name: 'tutor 3' }]           
            DateTime sDate = DateTime.ParseExact(Date, "yyyy-MM-dd", null);

            List<DsGiaoVien> l = new List<DsGiaoVien>();
            var scho = SchoolTable.getbyID(SchoolID);

            var lAcc = AccountTable.getbyrole(4).Where(n => n.Status == 2).ToList();//lấy giáo viên roleID = 4
            if (lAcc != null)
            {
                //lấy ra giáo viên dạy được lớp đó và môn đó
                List<tbl_Account> lgv = new List<tbl_Account>();
                foreach (var item in lAcc)
                {
                    //kiểm tra xem giáo viên có đăng ký nghỉ hay ko
                    var checkdayoffteacher = DayOffTeacherTable.getbyuiddate(item.ID, sDate.Date).Where(n => n.StudyTimeID == CaID && n.isHide != true).FirstOrDefault();
                    if (checkdayoffteacher != null)
                        continue;

                    //xem thằng đó có rảnh ko
                    var ckteacher = CoursesScheduleTable.checkteacher(item.ID, sDate.Date, CaID, 0);
                    if (ckteacher.checktrue)
                    {
                        DsGiaoVien t = new DsGiaoVien();
                        t.id = item.ID;
                        t.name = item.FullName;
                        l.Add(t);
                    }
                }
                l = l.OrderBy(n => n.name).ToList();
            }
            if (l.Count == 0)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            DsGiaoVien[] aRsGV = l.ToArray();
            return Json(new { rs = true, l = aRsGV }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public ActionResult SelfStudyAddCourse(FormCollection f)
        //{
        //    if (Session["UID"] == null)
        //        return RedirectToAction("Signin", "Login", new { area = "" });
        //    var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
        //    if (aclog.RoleID == 5)// học viên bị đá ra
        //        return RedirectToAction("Signin", "Login", new { area = "" });

        //    //2018-03-01,4,1398.1399|2018-03-02,4,1400.1401| Ngày, ID Giáo viên, list ID tiet học
        //    if (string.IsNullOrEmpty(f.Get("hdflistCalender").ToString()))
        //    {
        //        SetAlert("Không có dữ liệu", "e");
        //        return RedirectToAction("CreatecourseSelfStudy", "Course");
        //    }

        //    DateTime Opencourse = DateTime.ParseExact(f.Get("txt-start-date").ToString(), "dd/MM/yyyy", null);

        //    var ct = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
        //    if (ct == null)
        //    {
        //        SetAlert("Không tìm thấy trung tâm này", "e");
        //        return RedirectToAction("CreatecourseSelfStudy", "Course");
        //    }

        //    var cahoc = StudyTimeTable.getbyid(f.Get("ddl-studytime").ToInt(0));
        //    if (cahoc == null)
        //    {
        //        SetAlert("Không tìm thấy ca học này", "e");
        //        return RedirectToAction("CreatecourseSelfStudy", "Course");
        //    }

        //    string coursename = f.Get("txt-course-name").ToString();

        //    tbl_Account hocvu = new tbl_Account();
        //    hocvu.ID = 0;
        //    hocvu.FullName = "";

        //    var c = CoursesTable.insert(coursename, 0, "", 0, "", Opencourse.Date, cahoc.ID, cahoc.StudyTimeName, cahoc.StyleLesson.Value, 0, "", 0,
        //        ct.ID, ct.SchoolName, 0, "", aclog.UserName, true, hocvu, 0, 0, 0, 0);
        //    //
        //    if (c != null)//thành công
        //    {
        //        string[] sCalender = f.Get("hdflistCalender").ToString().Split('|');
        //        for (int i = 0; i < sCalender.Length; i++)
        //        {
        //            string[] sdata = sCalender[i].Split(',');

        //            DateTime date = DateTime.ParseExact(sdata[0], "yyyy-MM-dd", null);
        //            var teacher = AccountTable.getbyID(sdata[1].ToInt(0));
        //            if (teacher == null)
        //                continue;

        //            string[] listTietID = sdata[2].Split('.');

        //            for (int j = 0; j < listTietID.Length; j++)
        //            {
        //                CoursesScheduleTable.insert(c.ID, c.CourseSelfStudy.Value, listTietID[j].ToInt(0), date.Date, cahoc.ID, ct.ID, 0, teacher.ID, aclog.UserName, 0);
        //            }
        //        }
        //        var lteacher = CoursesScheduleTable.getbycourseid(c.ID).Select(n => n.TeacherID).Distinct();
        //        foreach (var item in lteacher)
        //        {
        //            var te = AccountTable.getbyID(item.Value);
        //            if (te != null)
        //            {
        //                var inte = TeacherCourseTable.insert(item.Value, c, aclog.UserName);
        //                // tạo thông báo cho giáo viên và push notidesktop
        //                if (inte != null)
        //                {
        //                    var nt = NotificationTable.insertsingle("", "Nhận lịch trực lớp", "Bạn được xếp lịch trực lớp" + c.CourseName, te.ID, aclog.UserName, 3, c.ID);
        //                }
        //            }
        //        }
        //        SetAlert("Tạo khóa \"" + coursename + "\" thành công", "s");
        //    }
        //    else
        //        SetAlert("Không tạo được khóa học", "e");
        //    return RedirectToAction("CreatecourseSelfStudy", "Course");
        //}
        #endregion
        [HttpGet]
        public ActionResult ScheduleStudyTime()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            return View();
        }

        /// <summary>
        /// Xuất Excel danh sách khóa học
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <param name="schoolID"></param>
        /// <param name="classID"></param>
        /// <param name="fdate"></param>
        /// <param name="tdate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelCourseList(string search, int sort, int schoolID, int classID, string fdate, string tdate)
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
            if (fdate == "")
                fdate = null;
            if (tdate == "")
                tdate = null;
            List<ASW_CourseList_ExportExcel_Result> l = new List<ASW_CourseList_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CourseList_ExportExcel(search, schoolID, classID, fdate, tdate, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Khoá học</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Chủ nhiệm</th>");
            strExport.Append("                        <th scope=\"col\">Giáo viên</th>");
            strExport.Append("                        <th scope=\"col\">Học phí</th>");
            strExport.Append("                        <th scope=\"col\">Hình thức</th>");
            strExport.Append("                        <th scope=\"col\">Bắt đầu</th>");
            strExport.Append("                        <th scope=\"col\">Kết thúc</th>");
            strExport.Append("                        <th scope=\"col\">Học viên</th>");
            strExport.Append("                        <th scope=\"col\">Đã thanh toán</th>");
            strExport.Append("                        <th scope=\"col\">Còn thiếu</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                string trangthai = "Mới tạo";
                if (i.Status == 2)
                {
                    trangthai = "Đang mở";
                }
                else if (i.Status == 3)
                {
                    trangthai = "Đã đóng";
                }

                var gv = TeacherCourseTable.getbycourseid(i.ID).Where(n => n.isHide != true).ToList();
                string gvs = "";
                for (int j = 0; j < gv.Count; j++)
                {
                    var te = AccountTable.getbyID(gv[j].TeacherUID.Value);
                    if (j != gv.Count - 1)
                    {
                        gvs += te.FullName + ", ";
                    }
                    else
                    {
                        gvs += te.FullName;
                    }
                }

                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + trangthai + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.TeacherLeaderName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + gvs + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0đ}", i.Price) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.TypeCourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.OpeningDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + CoursesScheduleTable.getbycourseid(i.ID).OrderByDescending(n => n.Date).FirstOrDefault().Date.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + CoursesStudentTable.getbycourseid(i.ID).Count + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", CoursesStudentTable.getbycourseid(i.ID).Sum(x => Convert.ToDouble(x.TotalPaid.Value))) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", CoursesStudentTable.getbycourseid(i.ID).Sum(x => Convert.ToDouble(x.PriceLeft.Value))) + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Khoa_Hoc_Offline" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult ExportExcelCourseListOnline(string search, int sort, int schoolID, string fdate, string tdate)
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
            if (fdate == "")
                fdate = null;
            if (tdate == "")
                tdate = null;
            List<ASW_CourseList_Online_ExportExcel_Result> l = new List<ASW_CourseList_Online_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_CourseList_Online_ExportExcel(search, schoolID, fdate, tdate, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Khoá học</th>");
            strExport.Append("                        <th scope=\"col\">Trạng thái</th>");
            strExport.Append("                        <th scope=\"col\">Giáo viên dạy</th>");
            strExport.Append("                        <th scope=\"col\">Học phí</th>");
            strExport.Append("                        <th scope=\"col\">Hình thức</th>");
            strExport.Append("                        <th scope=\"col\">Bắt đầu</th>");
            strExport.Append("                        <th scope=\"col\">Kết thúc</th>");
            strExport.Append("                        <th scope=\"col\">Học viên</th>");
            strExport.Append("                        <th scope=\"col\">Đã thanh toán</th>");
            strExport.Append("                        <th scope=\"col\">Còn thiếu</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                string trangthai = "Mới tạo";
                if (i.Status == 2)
                {
                    trangthai = "Đang mở";
                }
                else if (i.Status == 3)
                {
                    trangthai = "Đã đóng";
                }
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + trangthai + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.TeacherName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0đ}", i.Price) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.TypeCourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.OpeningDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.EndingDate.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + CoursesStudentTable.getbycourseid(i.ID).Count + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", CoursesStudentTable.getbycourseid(i.ID).Sum(x => Convert.ToDouble(x.TotalPaid.Value))) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", CoursesStudentTable.getbycourseid(i.ID).Sum(x => Convert.ToDouble(x.PriceLeft.Value))) + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Khoa_Hoc_Online" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Xuất Excel danh sách học viên chuyển khóa
        /// </summary>
        /// <param name="search"></param>
        /// <param name="sort"></param>
        /// <param name="schoolID"></param>
        /// <param name="fdate"></param>
        /// <param name="tdate"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ExportExcelChangeCourse(string search, int sort, int schoolID, string fdate, string tdate)
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
            if (fdate == "")
                fdate = null;
            if (tdate == "")
                tdate = null;
            List<ASW_ChangeCourse_ExportExcel_Result> l = new List<ASW_ChangeCourse_ExportExcel_Result>();
            using (var db = new ZimEntities())
            {
                l = db.ASW_ChangeCourse_ExportExcel(search, schoolID, fdate, tdate, sort).ToList();
            }
            if (l.Count() == 0)
            {
                return Json(new { rs = false, noti = "Dữ liệu trống!" }, JsonRequestBehavior.AllowGet);
            }

            StringBuilder strExport = new StringBuilder();
            strExport.Append("       <table border=\"1\">");
            strExport.Append("                     <tr>");
            strExport.Append("                        <th scope=\"col\">STT</th>");
            strExport.Append("                        <th scope=\"col\">Trung tâm</th>");
            strExport.Append("                        <th scope=\"col\">Học viên</th>");
            strExport.Append("                        <th scope=\"col\">Khoá học</th>");
            strExport.Append("                        <th scope=\"col\">Giá tiền</th>");
            strExport.Append("                        <th scope=\"col\">Đã đóng</th>");
            strExport.Append("                        <th scope=\"col\">Giảm giá</th>");
            strExport.Append("                        <th scope=\"col\">Còn lại</th>");
            strExport.Append("                        <th scope=\"col\">Ghi chú</th>");
            strExport.Append("                        <th scope=\"col\">Cam kết</th>");
            strExport.Append("                        <th scope=\"col\">Nhập ngày</th>");
            strExport.Append("                    </tr>");

            var index = 0;
            foreach (var i in l)
            {
                index++;
                strExport.Append("                    <tr>");
                strExport.Append("                        <td style=\"text-align:center\">" + index + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.SchoolName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.StudentName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + i.CourseName + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.PriceCourse) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", i.TotalPaid) + "</td>");
                strExport.Append("                        <td style=\"text-align:left\">" + String.Format("{0:0,0}", i.PriceDiscout) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + String.Format("{0:0,0}", i.PriceLeft) + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Note + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.Commitment + "</td>");
                strExport.Append("                        <td style=\"text-align:center\">" + i.CreatedDate.Value.ToString("dd/MM/yyyy") + "</td>");
                strExport.Append("                    </tr>");
            }
            strExport.Append("</table>");
            var filename = "DS_Hoc_Vien_Chuyen_Khoa" + GetDateTime.Now.ToString("dd/MM/yyyy");
            return Json(new { rs = true, data = strExport.ToString(), f = filename }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteCourse(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 1)
            {
                var c = CoursesTable.getbyid(id);
                if (c.Status == 2 || c.Status == 3 || c.OpeningDate <= DateTime.Now)
                {
                    return Json(new { rs = false, message = "Không thể xóa khóa học đã hoạt động" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    CoursesTable.Delete(id, true, aclog.UserName);
                    return Json(new { rs = true, message = "Thành công" }, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult AddCourseOnline(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            DateTime Opencourse = DateTime.ParseExact(f.Get("txt-start-date").ToString(), "dd/MM/yyyy", null);
            DateTime Endcourse = DateTime.ParseExact(f.Get("txt-end-date").ToString(), "dd/MM/yyyy", null);
            var ct = SchoolTable.getbyID(f.Get("ddl-school").ToInt(0));
            if (ct == null)
            {
                SetAlert("Không tìm thấy trung tâm này", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            var gt = CurriculumTable.getbyID(f.Get("ddl-giaotrinh").ToInt(0));
            if (gt == null)
            {
                SetAlert("Không tìm thấy giáo trình này", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            var lop = ClassTable.getbyID(gt.ClassID.Value);
            if (lop == null)
            {
                SetAlert("Không tìm thấy lớp học này", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            string coursename = f.Get("txt-course-name").ToString();
            if (lop.PriceClass == null)
            {
                SetAlert("Lớp chưa cấu hình giá tiền", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            var gv = AccountTable.getbyID(f.Get("ddl-teacher").ToInt(0));
            if (gv == null)
            {
                SetAlert("Không tìm thấy giáo viên này", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            var sl = StaffSalaryTable.GetByUIDType(gv.ID, 1);
            if (sl == null || sl.Persent == 0)
            {
                SetAlert("Vui lòng cấu hình lương Online cho giáo viên trước!", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            if (Opencourse.Date < DateTime.Now.Date)
            {
                SetAlert("Thời gian mở không thể nhỏ hơn hiện tại", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            if (Opencourse.Date >= Endcourse.Date)
            {
                SetAlert("Thời gian đóng không thể nhỏ hơn hoặc bằng thời gian mở", "e");
                return RedirectToAction("CourseListOnline", "Course");
            }
            var rs = CoursesTable.InsertCourseOnline(coursename, lop.GradeID.Value, lop.GradeName, lop.ID, lop.ClassName, Opencourse.Date, Endcourse.Date,
                gt.ID, gt.CurriculumsName, lop.PriceClass.Value, ct.ID, ct.SchoolName, aclog.UserName, false);

            //Insert vào bảng lương online
            int yearOpen = Opencourse.Date.Year; //năm mở khóa
            int monthOpen = Opencourse.Date.Month; //tháng mở khóa
            int yearEnd = Endcourse.Date.Year; //năm đóng khóa
            int monthEnd = Endcourse.Date.Month; //tháng đóng khóa
            if (monthOpen == monthEnd && yearOpen == yearEnd) //nếu là khóa 1 tháng và ngược lại
            {
                SalaryTeacherOnlineTable.Insert(gv.ID, rs.ID, rs.CourseName, sl.Persent.Value, yearOpen, monthOpen, true, aclog.UserName);
            }
            else
            {
                SalaryTeacherOnlineTable.Insert(gv.ID, rs.ID, rs.CourseName, sl.Persent.Value, yearOpen, monthOpen, false, aclog.UserName);
                SalaryTeacherOnlineTable.Insert(gv.ID, rs.ID, rs.CourseName, sl.Persent.Value, yearEnd, monthEnd, false, aclog.UserName);
            }

            if (rs != null)
            {
                var inte = TeacherCourseTable.insert(gv.ID, rs, aclog.UserName);
                // tạo thông báo cho giáo viên và push notidesktop
                //if (inte != null)
                //{
                //    var nt = NotificationTable.insertsingle("", "Nhận khóa học mới", "Bạn được xếp lịch dạy khóa mới, <a href=\"/Admin/CourseDetail/ScheduleCourse/" + c.ID + "\">" + c.CourseName + "</a>", te.ID, aclog.UserName);

                //    string strPathAndQuery = Request.Url.PathAndQuery;
                //    string strUrl = Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                //    string datalink = "" + strUrl + "Admin/CourseDetail/ScheduleCourse/" + rs.ID;
                //    AppZim.ZIM.AssetCRM.PushNotiDesktop(gv.ID, nt.NotificationTitle, datalink);
                //}
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");

            return RedirectToAction("CourseListOnline", "Course");
        }

        /// <summary>
        /// Tạo khóa học auto xếp lịch
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult CreateCourseNew(int appoint = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var appoitment = StudentAppointmentTable.getbyid(appoint);
                if (appoitment != null)
                {
                    ViewBag.StudyTime = appoitment.StudyID;
                    ViewBag.SchoolID = appoitment.SchoolID;
                    var desireDates = DesiredStudyTimeTable.getbyStudentAppointmentID(appoitment.ID);
                    ViewBag.NumberTimeDesrire = desireDates.Count();
                }
            }
            catch { }
            ViewBag.Appoint = appoint;
            return View();
        }
        /// <summary>
        /// Lấy danh sách chuyên môn theo 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult GetGrade(int studentAppointmentID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var grades = GradeTable.getbyStudentAppointmentID(studentAppointmentID);
            return Json(new { data = grades }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Lấy danh sách chương trình
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetProgram(int studentAppointmentID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var programs = new List<Admin_Program_GetByAppoitment_Result>();
            using (var db = new ZimEntities())
            {
                programs = db.Admin_Program_GetByAppoitment(studentAppointmentID).ToList();
            }
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">---</option>");
            foreach (var i in programs)
            {
                html.Append("<option value=\"" + i.ID + "\">" + i.ClassName + "</option>");
            }
            return Json(new { data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Lấy danh sách giáo trình
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCurriculumn(int studentAppointmentID)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var appointment = StudentAppointmentTable.getbyid(studentAppointmentID);
            var student = AccountTable.getbyID(appointment.StudentUID.Value);
            var curriculumns = CurriculumTable.getByLanguage(student.Language ?? 0);
            StringBuilder html = new StringBuilder();
            html.Append("<option value=\"\">---</option>");
            foreach (var i in curriculumns)
            {
                html.Append("<option value=\"" + i.ID + "\">" + i.CurriculumsName + "</option>");
            }
            return Json(new { data = html.ToString() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Tự động tạo tên khóa học dựa trên mã khối học
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GenerateCourseName(int programid)
        {
            var program = ClassTable.getbyID(programid);
            string code = "";
            var language = Language.GetByID(program.Language ?? 0);
            if (language != null)
            {
                var lastCourse = new tbl_Courses();
                using (var db = new ZimEntities())
                {
                    lastCourse = db.tbl_Courses.Where(x => x.CourseName.StartsWith(language.LanguageCode)).OrderByDescending(x => x.ID).FirstOrDefault();
                }
                var tempCode = language.LanguageCode + "0";
                if (lastCourse != null)
                {
                    tempCode = lastCourse.CourseName;
                }
                var codeString = Regex.Replace(tempCode, "[^0-9]+", string.Empty);
                int intValue = codeString.ToInt(0);
                intValue++;
                code = language.LanguageCode + intValue.ToString();
            }
            return Json(new { rs = code }, JsonRequestBehavior.AllowGet);
        }
        //đẩy khách đăng ký dữ chỗ vào khóa -  dung cho tao khoa hoc moi
        public bool AssignCusAppointmentToCourse(tbl_StudentAppointment app, tbl_Courses course, string tt, string ct)
        {
            try
            {
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

                var commitmentpackage = ClassTable.getbyID(app.CommitmentID ?? 0);
                if (commitmentpackage == null)
                {
                    commitmentpackage = new tbl_Class() { ID = 0, ClassName = "", PriceClass = 0 };
                }

                var stu = AccountTable.getbyID(app.StudentUID.Value);

                tbl_CoursesStudent r = CoursesStudentTable.insert(app.RewardTeachers ?? 0, stu, course, app.CustomPrice ?? 0,
                        app.ID, app.CustomPrice ?? 0, app.Note, app.PaymentMethodID ?? 0, app.PaymentMethodName,
                        aclog.UserName, 0, true, app.DateAppointment == null ? "" : app.DateAppointment.Value.ToString("dd/MM/yyyy"),
                        "", 0, app.ComboID.Value, commitmentpackage, app.DiscountID ?? 0, app.DiscountPrice ?? 0);

                if (r != null)
                {
                    if (app.ComboID > 0)// combo
                    {
                        ComboDetailTable.UpdateDone(app.ID, app.ComboID ?? 0, course.ClassID ?? 0, aclog.UserName, 0);
                    }
                    else
                    {
                        StudentAppointmentTable.updatedone(app.ID, aclog.UserName);
                    }

                    string codex = DeviceMobileTable.getbyUID(r.StudentUID.Value)?.IncludePlayerID;
                    string[] includeID = new[] { codex };
                    PushNotifyMobile.SendNotifyMobile("Bạn vừa được xếp vào lớp " + r.CourseName + ". Cùng vào xem thông tin và giáo viên của mình nhé", includeID);
                    NotificationTable.insertsingle("", "Bạn vừa được xếp vào lớp " + r.CourseName, "Cùng vào xem thông tin và giáo viên của mình nhé", r.StudentUID.Value, aclog.UserName, 3, course.ID);

                    int count = CoursesStudentTable.getbycourseid(r.CourseID.Value).Where(n => n.isHide != true).ToList().Count;
                    CoursesTable.updatecountstudent(course.ID, count, aclog.UserName);
                    var first = CoursesScheduleTable.GetFirstLesson(course.ID);
                    var time = StudyTimeTable.getbyid(first.StudyTimeID.Value);

                    // gửi mail theo form
                    if (!string.IsNullOrEmpty(stu.Email))
                    {
                        ct = ct.Replace("[ten-hoc-vien]", stu.FullName);
                        ct = ct.Replace("[thoi-gian-hoc]", time.sTime + " - " + time.eTime);
                        ct = ct.Replace("[tai-khoan]", stu.UserName);
                        ct = ct.Replace("[mat-khau]", "iccacademy");
                        AssetCRM.SendMail(stu.Email, tt, ct);
                    }
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                AssetCRM.Writelog("CourseController", "AssignCusAppointmentToCourse", 0, ex.Message + "||" + ex.InnerException);
                return false;
            }
        }
        /// <summary>
        /// lấy danh sách giáo viên dạy được giáo trình, ca học, ngày học, ngôn ngữ tương ứng
        /// </summary>
        /// <param name="classid"></param>
        /// <param name="studyid"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LoadGiaoVien(int programid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 6 && aclog.RoleID != 7)
                return RedirectToAction("Signin", "Login", new { area = "" });
            try
            {
                var teacherList = new List<Admin_GetTeacherByClass_Result>();
                using (var db = new ZimEntities())
                {
                    teacherList = db.Admin_GetTeacherByClass(programid).ToList();
                }
                StringBuilder html = new StringBuilder();
                foreach (var item in teacherList)
                {
                    html.Append("<option value=\"" + item.UID + "\">" + item.FullName + "</option>");
                }
                return Json(new { rs = true, teacher = html.ToString() }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public ActionResult LoadEmailnhanlop(int studentid, int teacherid, string ngaykhaigiang, string tenkhoa,
            int schoolid, int classid, string curriculumnid)
        {
            try
            {
                var idstudent = StudentAppointmentTable.getbyid(studentid)?.StudentUID ?? 0;
                var student = AccountTable.getbyID(idstudent);
                using (var db = new ZimEntities())
                {
                    int id = 1;
                    switch (student?.Language ?? 1)
                    {
                        case 7:
                            id = 15;
                            break;
                        case 6:
                            id = 8;
                            break;
                        default:
                            id = 1;
                            break;
                    }
                    var template = db.tbl_EmailTemplate.Where(x => x.ID == id).FirstOrDefault();
                    var teacher = AccountTable.getbyID(teacherid);
                    string ct = "";
                    string tt = "";
                    var chuongtrinhhoc = ClassTable.getbyID(classid);
                    var giaotrinh = "";
                    List<int> giaotrinhs = curriculumnid.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                    foreach (var g in giaotrinhs)
                    {
                        var curr = CurriculumTable.getbyID(g);
                        giaotrinh += "[" + curr?.CurriculumsName + " - " + curr?.LinkCurriculumn + "]  ";
                    }
                    //tiêu đề mail
                    tt = template.EmailTitle;
                    //nội dung mail
                    ct = template.EmailContent;
                    ct = ct.Replace("[ten-hoc-vien]", student.FullName);
                    ct = ct.Replace("[tai-khoan]", student.UserName);
                    ct = ct.Replace("[mat-khau]", Encryptor.Decrypt(student.Password));
                    ct = ct.Replace("[ten-khoa-hoc]", tenkhoa);
                    ct = ct.Replace("[ten-giang-vien]", teacher.FullName);
                    ct = ct.Replace("[id-pass-zoom]", teacher.ZoomID + " - " + teacher.ZoomPassword);
                    ct = ct.Replace("[ngay-khai-giang]", ngaykhaigiang);
                    ct = ct.Replace("[chuong-trinh-hoc]", chuongtrinhhoc.ClassName);
                    ct = ct.Replace("[link-giao-trinh]", giaotrinh);
                    return Json(new { rs = true, tt, ct });
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Course", "LoadEmailnhanlop", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }

        [HttpPost]
        public ActionResult LoadEmailClassNewForTeacher(int teacherid, string ngaykhaigiang, string tenkhoa,
            int schoolid, int classid, string curriculumnid)
        {
            try
            {
                var teacher = AccountTable.getbyID(teacherid);

                using (var db = new ZimEntities())
                {
                    int idTemplate = 1;
                    switch (teacher?.Language ?? 1)
                    {
                        case 7:
                            idTemplate = 15;
                            break;
                        case 6:
                            idTemplate = 8;
                            break;
                        default:
                            idTemplate = 1;
                            break;
                    }
                    var template = db.tbl_EmailTemplate.Where(x => x.ID == idTemplate).FirstOrDefault();
                    string ct = "";
                    string tt = "";
                    var chuongtrinhhoc = ClassTable.getbyID(classid);
                    var giaotrinh = "";
                    List<int> giaotrinhs = curriculumnid.Split(',').Select(x => Convert.ToInt32(x)).ToList();
                    foreach (var g in giaotrinhs)
                    {
                        var curr = CurriculumTable.getbyID(g);
                        giaotrinh += "[" + curr?.CurriculumsName + " - " + curr?.LinkCurriculumn + "]  ";
                    }
                    //tiêu đề mail
                    tt = template.EmailTitle;
                    //nội dung mail
                    ct = template.EmailContent;
                    ct = ct.Replace("[ten-khoa-hoc]", tenkhoa);
                    ct = ct.Replace("[ten-giang-vien]", teacher.FullName);
                    ct = ct.Replace("[id-pass-zoom]", teacher.ZoomID + " - " + teacher.ZoomPassword);
                    ct = ct.Replace("[ngay-khai-giang]", ngaykhaigiang);
                    ct = ct.Replace("[chuong-trinh-hoc]", chuongtrinhhoc.ClassName);
                    ct = ct.Replace("[link-giao-trinh]", giaotrinh);
                    return Json(new { rs = true, tt, ct });
                }
            }
            catch (Exception e)
            {
                AssetCRM.Writelog("Course", "LoadEmailClassNewForTeacher", 0, e.Message + "||" + e.InnerException);
                return Json(new { rs = false });
            }
        }

        [HttpPost]
        public ActionResult UpdateStatusCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int courseId = f.Get("hdCourseId").ToInt();
            int status = f.Get("slStatus").ToInt();
            string statusName = "";
            switch (status)
            {
                case 1:
                    statusName = "Sắp mở";
                    break;
                case 2:
                    statusName = "Đang mở";
                    break;
                case 3:
                    statusName = "Đã đóng";
                    break;
            }

            CoursesTable.updatestatus(courseId, status, statusName, aclog.UserName);

            SetAlert("Thành công", "s");
            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }
    }
}
