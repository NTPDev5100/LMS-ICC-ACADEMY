using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AppZim.Controllers
{
    public class HomeController : Controller
    {
        // GET: Admin/Home
        public ActionResult Dashboard()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// khác học viên
                return RedirectToAction("Signin", "Login", new { area = "" });
            var assign = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.isHide != true).ToList();
            List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
            foreach (var item in assign)
            {
                var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                calen.AddRange(t);
            }
            var subs = calen.Select(n => n.SubjectID).Distinct();
            StringBuilder aps = new StringBuilder();
            foreach (var item in subs)
            {
                if (item == null)
                    continue;
                var sub = SubjectTable.getbyID(item.Value);
                if (sub != null)
                    aps.Append("<option value=\"" + sub.ID + "\">" + sub.SubjectName + " - " + sub.ClassName + "</option>");
            }
            ViewBag.Sub = aps.ToString();
            return View();
        }

        [HttpPost]
        public ActionResult LoadScheduleStudent(int courseid, string subject)
        {
            string[] arrsubj = subject.Split(',');
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var assign = CoursesStudentTable.getbystudentid(aclog.ID);
            if (courseid != 0)
                assign = assign.Where(n => n.CourseID == courseid).ToList();
            List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
            foreach (var item in assign)
            {
                var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                List<tbl_CoursesSchedule> p = new List<tbl_CoursesSchedule>();

                if (!string.IsNullOrEmpty(subject))
                {
                    foreach (var i in arrsubj)
                    {
                        int subid = i.ToInt(0);
                        p.AddRange(t.Where(n => n.SubjectID == subid).ToList());
                    }
                }
                else
                    p = t;
                calen.AddRange(p);
            }

            //Cái Schedule mới em tạo ở bên dưới thêm một số chi tiết để đổ data ra cho hợp lý
            ConcurrentBag<NewSchedule> l = new ConcurrentBag<NewSchedule>();
            Parallel.ForEach(calen, item =>
            {
                var _db = new ZimEntities();
                var te = _db.tbl_Account.Where(x => x.ID == item.TeacherID.Value).FirstOrDefault();
                if (te == null)
                    return;
                var c = _db.tbl_Courses.Where(x => x.ID == item.CourseID.Value).FirstOrDefault();
                if (c == null)
                    return;
                var sty = _db.tbl_StudyTime.Where(x => x.ID == item.StudyTimeID.Value).FirstOrDefault();
                if (sty == null)
                    return;

                string[] stu = sty.sTime.Split(':');
                string[] etu = sty.eTime.Split(':');
                TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);
                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                var sub = _db.tbl_Subject.Where(x => x.ID == subid).FirstOrDefault();
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }
                NewSchedule t = new NewSchedule();
                t.couserID = c.ID;
                t.className = c.ClassName;
                t.start = item.Date.Value.Add(s).ToString("yyyy-MM-dd HH:mm");
                t.end = item.Date.Value.Add(e).ToString("yyyy-MM-dd HH:mm");
                t.room = "";
                //by TriS copy từ bên giáo viên qua
                //if (item.Date.Value == DateTime.Now.Date)
                //{
                //    if (aclog.RoleID == 5) // là học viên
                //    {
                //        if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass) && item.isRoomStart == true)
                //        {
                //            t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Vào phòng học</a>";
                //        }
                //    }
                //}

                t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid && n.CourseID == item.CourseID).ToList().Select(n => n.Date).Distinct().Count() + 1);
                t.branch = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.url = "/StudentCourse/CourseDetail/" + c.ID + "";
                t.borderColor = "#3cd629";
                t.teacher = te.FullName;
                t.allDay = false;
                bool ck = true;
                var lc = l.Where(n => n.couserID == c.ID).ToList();
                foreach (var jtem in lc)
                {
                    if (jtem.couserID == t.couserID && jtem.start == t.start)
                    {
                        ck = false;
                        break;
                    }
                }
                if (ck)
                    l.Add(t);
            });
            return Json(new { rs = true, l = l }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult PartialMenu()
        {
            return View();
        }
        public ActionResult PartialMobileMenu()
        {
            return View();
        }
        public ActionResult TermOfService()
        {
            return View();
        }
        public class Schedule
        {
            public int id { get; set; }
            public int couserID { get; set; }
            public string className { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public string url { get; set; }
            public string borderColor { get; set; }
            public string room { get; set; }
            public string subject { get; set; }
            public string branch { get; set; }
            public string teacher { get; set; }
            public bool allDay { get; set; }

        }
        /// <summary>
        /// không ảnh hưởng bởi timezone
        /// </summary>
        public class NewSchedule
        {
            public int id { get; set; }
            public int couserID { get; set; }
            public string className { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string url { get; set; }
            public string borderColor { get; set; }
            public string room { get; set; }
            public string subject { get; set; }
            public string branch { get; set; }
            public string teacher { get; set; }
            public bool allDay { get; set; }

        }
        public ActionResult TeacherDashBoard()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 4)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var l = TeacherCourseTable.getbyteacherid(aclog.ID);
            var lThongke = new ConcurrentBag<ThongKe>();
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
            ViewBag.TeacherName = AccountTable.getbyID(aclog.ID).FullName;
            ViewBag.TeacherID = aclog.ID;
            return View(lThongke);
        }
        public ActionResult AppointmentTest(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            
            int pageSize = 20;
            List<get_list_of_AppointmentTestForStudent_Result> l = new List<get_list_of_AppointmentTestForStudent_Result>();
            using (var db = new ZimEntities())
            {
                l = db.get_list_of_AppointmentTestForStudent(aclog.ID, page - 1, pageSize).ToList();
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
    }
}