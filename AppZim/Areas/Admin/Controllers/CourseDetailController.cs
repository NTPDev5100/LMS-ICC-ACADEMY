using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Areas.Admin.Controllers
{
    public class CourseDetailController : BaseController
    {
        #region Lịch học chi tiết
        public ActionResult ScheduleCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.Center = c.SchoolID.Value;
            ViewBag.Course = c.ID;
            return View(c);
        }
        [HttpPost]
        public ActionResult LoadScheduleCourse(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var calen = CoursesScheduleTable.getbycourseid(id);
            List<NewSchedule> l = new List<NewSchedule>();
            foreach (var item in calen)
            {
                var te = AccountTable.getbyID(item.TeacherID.Value);
                if (te == null)
                    continue;

                var c = CoursesTable.getbyid(item.CourseID.Value);
                if (c == null)
                    continue;

                var sty = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                if (sty == null)
                    continue;

                string[] stu = sty.sTime.Split(':');
                string[] etu = sty.eTime.Split(':');
                TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);

                //kiểm tra xem giáo viên up giáo trình buổi dạy ngày hôm đó chưa
                string colorcalenda = "#f42929";
                string url = "javascript:;";
                var ckcurri = CurriculumTeacherTable.getcurritecher(c.ID, item.Date.Value.Date, item.StudyTimeID.Value);
                if (ckcurri != null)
                {
                    colorcalenda = "#3cd629";
                    url = ckcurri.LinkDocument;
                }

                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                else
                {
                    var currdetail = CurriculumDetailTable.getbyID(item.CurriculumsDetailsID.Value);
                    if (currdetail != null)
                        subid = currdetail.SubjectID.Value;
                }
                var sub = SubjectTable.getbyID(subid);
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }

                NewSchedule t = new NewSchedule();
                t.id = item.ID;
                t.course = c.ID;
                t.title = sty.sTime + "-" + sty.eTime + ", GV: " + te.FullName;
                t.start = item.Date.Value.Add(s).ToString("yyyy-MM-dd HH:mm");
                t.end = item.Date.Value.Add(e).ToString("yyyy-MM-dd HH:mm");
                t.allDay = false;
                t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid).ToList().Select(n => n.Date).Distinct().Count() + 1);
                t.url = url;
                t.borderColor = colorcalenda;
                t.teacher = te.FullName;
                t.school = c.SchoolName;


                if (DateTime.Now.Date == item.Date.Value.Date) //chỉ hiện nút bằng ngày hiện tại và theo GV đó
                {
                    if (aclog.RoleID == 4)
                    {
                        if (aclog.ID == item.TeacherID.Value) //chỉ cho GV có lịch đó được tạo phòng
                        {

                            if (string.IsNullOrEmpty(item.RoomID)) //chỉ hiện nút tạo phòng khi chưa tạo
                            {
                                t.school = "<a href=\"/Admin/ZoomMeeting/CreateRoom?CoursesScheduleID=" + item.ID + "\" class=\"btn btn-primary btn-sm\" style=\"font-size:10px; padding: 5px\"><i class=\"fas fa-video\" style=\"vertical-align:middle\"></i>  Tạo phòng mới</a>";
                            }
                            else if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass))
                            {
                                t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Bắt đầu</a>";
                            }
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass) && item.isRoomStart == true)
                        {
                            t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Vào phòng học</a>";
                        }
                    }
                }
                else
                {
                    t.school = c.SchoolName;
                }

                bool ck = true;
                var lc = l.Where(n => n.course == c.ID).ToList();
                foreach (var jtem in lc)
                {
                    if (jtem.course == t.course && jtem.start == t.start)
                    {
                        ck = false;
                        break;
                    }
                }
                if (ck)
                    l.Add(t);
            }
            return Json(new { rs = true, l = l }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult LoadStudyTimeByCourseDate(int courseid, string date)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var c = CoursesTable.getbyid(courseid);
            if (c == null || string.IsNullOrEmpty(date))
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            DateTime d = DateTime.ParseExact(date, "dd/MM/yyyy", null);
            var sche = CoursesScheduleTable.getbycourseidbydateall(c.ID, d.Date).Select(n => n.StudyTimeID).Distinct();
            StringBuilder op = new StringBuilder();
            op.Append("<option value=\"\">------</option>");
            foreach (var item in sche)
            {
                var sty = StudyTimeTable.getbyid(item.Value);
                if (sty != null)
                    op.Append("<option value=\"" + sty.ID + "\">" + sty.sTime + " - " + sty.eTime + "</option>");
            }
            return Json(new { rs = true, l = op.ToString() }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Danh sách học viên
        public ActionResult ListStudent(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            return View(c);
        }
        #endregion

        #region kì thi
        public ActionResult CourseExam(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            var exam = CoursesExamTable.getbycourse(id);
            StringBuilder l = new StringBuilder();
            l.Append("  <option value = \"0\">---</option>");
            foreach (var item in exam)
            {
                l.Append("  <option value = \"" + item.ID + "\">" + item.ExamName + "</option>");
            }
            ViewBag.Date = l.ToString();
            return View(c);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult AddExam(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var course = CoursesTable.getbyid(f.Get("hdfcourse").ToInt(0));
            var refer = f.Get("ddl-view-history").ToInt(0);
            if (refer != 0)
            {
                SetAlert("Fail", "w");
                return RedirectToAction("CourseExam", "CourseDetail", new { id = course.ID });
            }
            var name = f.Get("txt-exam-name").ToString();
            DateTime d = GetDateTime.Now.Date;
            var rs = CoursesExamTable.insert(course.ID, name, d.Date, aclog.UserName);
            if (rs != null)
            {
                string lis = "";
                string spe = "";
                string red = "";
                string wri = "";
                string note = "";
                string overall = "";
                int target = 1;
                var l = CoursesStudentTable.getbycourseid(rs.CourseID.Value);
                foreach (var item in l)
                {
                    string op = "txt-lis-" + item.ID;
                    lis = f.Get(op).ToString();
                    spe = f.Get("txt-spe-" + item.ID + "").ToString().TrimEnd('.');
                    red = f.Get("txt-red-" + item.ID + "").ToString().TrimEnd('.');
                    wri = f.Get("txt-wri-" + item.ID + "").ToString().TrimEnd('.');
                    note = f.Get("txt-note-" + item.ID + "").ToString().TrimEnd('.');
                    overall = f.Get("txt-overall-" + item.ID + "").ToString().TrimEnd('.');
                    target = f.Get("ddl-target-" + item.ID + "").ToInt(1);
                    CoursesExamDetailTable.insert(rs.ID, course.ID, item.StudentUID.Value, lis, spe, red, wri, note, overall, target, aclog.UserName);
                }
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("CourseExam", "CourseDetail", new { id = course.ID });
        }
        [HttpPost]
        public ActionResult LoadDetailExam(int examid, int courseid)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            StringBuilder shtml = new StringBuilder();
            var ex = CoursesExamTable.getbyid(examid);
            if (ex != null)
            {
                var l = CoursesExamDetailTable.getbyexamid(ex.ID);
                foreach (var item in l)
                {
                    var st = AccountTable.getbyID(item.UID.Value);
                    if (st == null)
                        continue;

                    shtml.Append("<tr>");
                    shtml.Append("  <td class=\"no -wrap\">" + st.FullName + "</td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" disabled class=\"form-control\" value=\"" + item.Listening + "\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" disabled class=\"form-control\" value=\"" + item.Speaking + "\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" disabled class=\"form-control\" value=\"" + item.Reading + "\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" disabled class=\"form-control\" value=\"" + item.Writing + "\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" disabled class=\"form-control\" value=\"" + item.Overall + "\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td>");
                    shtml.Append("      <textarea disabled class=\"form-control\" rows = \"1\">" + item.Note + "</textarea>");
                    if (item.Target != null)
                    {
                        if (item.Target == 1)
                            shtml.Append("      <span class=\"badge badge-light mg-t-5\">---</span>");
                        if (item.Target == 2)
                            shtml.Append("      <span class=\"badge badge-success mg-t-5\">Achieved</span>");
                        if (item.Target == 3)
                            shtml.Append("      <span class=\"badge badge-danger mg-t-5\">Not Achieved</span>");
                    }
                    shtml.Append("  </td>");
                    shtml.Append("</tr>");
                }
            }
            else
            {
                var c = CoursesTable.getbyid(courseid);
                var l = CoursesStudentTable.getbycourseid(c.ID);

                foreach (var item in l)
                {
                    shtml.Append("<tr>");
                    shtml.Append("  <td class=\"no -wrap\">" + item.StudentName + "</td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" name=\"txt-lis-" + item.ID + "\" class=\"form-control\" placeholder=\"0.0\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" name=\"txt-spe-" + item.ID + "\" class=\"form-control\" placeholder=\"0.0\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" name=\"txt-red-" + item.ID + "\" class=\"form-control\" placeholder=\"0.0\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td class=\"wd-80\">");
                    shtml.Append("      <input type = \"text\" name=\"txt-wri-" + item.ID + "\" class=\"form-control\" placeholder=\"0.0\" />");
                    shtml.Append("  </td>");
                    shtml.Append("  <td>");
                    shtml.Append("      <textarea class=\"form-control\" name=\"txt-note-" + item.ID + "\" rows = \"1\" placeholder=\"...\"></textarea>");
                    shtml.Append("      <select name =\"ddl-target-" + item.ID + "\" class=\"form-control mg-t-10\">");
                    shtml.Append("          <option value=\"1\">---</option>");
                    shtml.Append("          <option value=\"2\">Achieved</ option >");
                    shtml.Append("          <option value=\"3\">Not Achieved</option>");
                    shtml.Append("      </select>");
                    shtml.Append("  </td>");
                    shtml.Append("</tr>");
                }
            }
            return Json(new { rs = true, l = shtml.ToString() }, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region thông báo khóa
        public ActionResult NotificationCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.Course = c;
            return View(c);
        }
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult AddNotification(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int courseid = f.Get("hdfcourse").ToInt(0);
            var course = CoursesTable.getbyid(courseid);
            var title = f.Get("txt-title").ToString();
            var content = "<a href=\"/Admin/CourseDetail/NotificationCourse/" + courseid + "\">Thông báo mới</a>";
            var rs = NotificationCourseTable.insert(title, f.Get("txt-content").ToString(), courseid, aclog.UserName);
            if (rs != null)
            {
                var list = new List<int>();
                var l = CoursesStudentTable.getbycourseid(courseid);
                foreach (var item in l)
                {
                    NotificationTable.insertsingle("", rs.NotificationTitle, content, item.StudentUID.Value, aclog.UserName, 1, courseid);
                }
                var te = TeacherCourseTable.getbycourseid(courseid);
                foreach (var item in te)
                {
                    NotificationTable.insertsingle("", rs.NotificationTitle, content, item.TeacherUID.Value, aclog.UserName, 1, courseid);
                }
                list.AddRange(l.Select(x => x.StudentUID.Value).ToList());
                list.AddRange(te.Select(x => x.TeacherUID.Value).ToList());
                SetAlert("Thành công", "s");
                Thread tnotify = new Thread(() => SendMobile(list, course.CourseName));
                tnotify.Start();
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("NotificationCourse", "CourseDetail", new { id = courseid });
        }
        #endregion

        #region Điểm danh
        public ActionResult AttendanceCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            var sche = CoursesScheduleTable.getbycourseid(c.ID).OrderBy(n => n.Date).Select(n => n.Date).Distinct();
            var cadis = CoursesScheduleTable.getbycourseid(c.ID).Select(n => n.StudyTimeID).Distinct().ToList();
            List<tbl_StudyTime> lca = new List<tbl_StudyTime>();
            for (int i = 0; i < cadis.Count; i++)
            {
                var ca = StudyTimeTable.getbyid(cadis[i].Value);
                if (ca != null)
                    lca.Add(ca);
            }
            lca = lca.OrderBy(n => n.sTime).ToList();

            StringBuilder l = new StringBuilder();
            foreach (var item in sche)
            {
                foreach (var jca in lca)
                {
                    if (item.Value.Date == GetDateTime.Now.Date)
                        l.Append("  <option selected data-date = \"" + item.Value.ToString("dd/MM/yyyy") + "\" data-study = \"" + jca.ID + "\"> " + item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]</option>");
                    else
                        l.Append("  <option data-date = \"" + item.Value.ToString("dd/MM/yyyy") + "\" data-study = \"" + jca.ID + "\"> " + item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]</option>");
                }
            }
            ViewBag.Date = l.ToString();
            return View(c);
        }

        [HttpPost]
        public ActionResult LoadAttendanceByDate(int courseid, string date, int caid)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            StringBuilder shtml = new StringBuilder();
            DateTime d = GetDateTime.Now.Date;
            try
            {
                d = DateTime.ParseExact(date, "dd/MM/yyyy", null);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }

            var l = CoursesScheduleTable.getbycourseidbydatevsstudyid(courseid, d.Date, caid);
            if (l == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
            {
                var stu = CoursesStudentTable.getbycourseid(courseid);
                foreach (var item in stu)
                {
                    var acstu = AccountTable.getbyID(item.StudentUID.Value);
                    var ck = CoursesAttendanceStudentTable.checkshow(courseid, acstu.ID, d.Date, caid);

                    string note = "";
                    string attname = "";
                    int attid = 0;
                    string learning = "";
                    int leaid = 0;
                    string warning = "";
                    if (ck != null)
                    {
                        note = ck.Note;
                        attname = ck.AttendanceName;
                        attid = ck.AttendanceID.Value;
                        learning = ck.LearningName;
                        leaid = ck.LearningID.Value;
                        if (ck.Warning == true)
                            warning = "Cảnh báo";
                    }
                    string disible = "";
                    if (d.Date != GetDateTime.Now.Date)
                        disible = "disabled";

                    shtml.Append("<tr>");
                    shtml.Append("  <td>");
                    shtml.Append("      <span>" + acstu.FullName + "</span>");
                    shtml.Append("  </td>");
                    shtml.Append("  <td>");
                    shtml.Append("      <select " + disible + " class=\"form-control\" id=\"ddl-diem-danh-" + acstu.ID + "\">");
                    shtml.Append("          <option value = \"0\">---</option>");
                    if (attid == 1)
                        shtml.Append("          <option selected value = \"1\">Có</option>");
                    else
                        shtml.Append("          <option value = \"1\">Có</option>");

                    if (attid == 2)
                        shtml.Append("          <option selected value = \"2\">Vắng có phép</option>");
                    else
                        shtml.Append("          <option value = \"2\">Vắng có phép</option>");

                    if (attid == 3)
                        shtml.Append("          <option selected value = \"3\">Vắng không phép</option>");
                    else
                        shtml.Append("          <option value = \"3\">Vắng không phép</option>");

                    if (attid == 4)
                        shtml.Append("          <option selected value = \"4\">Đi muộn</option>");
                    else
                        shtml.Append("          <option value = \"4\">Đi muộn</option>");

                    if (attid == 5)
                        shtml.Append("          <option selected value = \"5\">Về sớm</option>");
                    else
                        shtml.Append("          <option value = \"5\">Về sớm</option>");

                    if (attid == 6)
                        shtml.Append("          <option selected value = \"6\">Nghỉ lễ</option>");
                    else
                        shtml.Append("          <option value = \"6\">Nghỉ lễ</option>");

                    shtml.Append("      </select>");
                    shtml.Append("  </td>");
                    shtml.Append("  <td>");
                    shtml.Append("      <select " + disible + " class=\"form-control\" id = \"ddl-hoc-luc-" + acstu.ID + "\">");
                    shtml.Append("          <option value = \"0\">---</option>");

                    if (leaid == 1)
                        shtml.Append("          <option selected value = \"1\">Giỏi</option>");
                    else
                        shtml.Append("          <option value = \"1\">Giỏi</option>");

                    if (leaid == 2)
                        shtml.Append("          <option selected value = \"2\">Khá</option>");
                    else
                        shtml.Append("          <option value = \"2\">Khá</option>");

                    if (leaid == 3)
                        shtml.Append("          <option selected value = \"3\">Trung bình</option>");
                    else
                        shtml.Append("          <option value = \"3\">Trung bình</option>");

                    if (leaid == 4)
                        shtml.Append("          <option selected value = \"4\">Kém</option>");
                    else
                        shtml.Append("          <option value = \"4\">Kém</option>");

                    if (leaid == 5)
                        shtml.Append("          <option selected value = \"5\">Theo dõi đặc biệt</option>");
                    else
                        shtml.Append("          <option value = \"5\">Theo dõi đặc biệt</option>");

                    if (leaid == 6)
                        shtml.Append("          <option selected value = \"6\">Có cố gắng</option>");
                    else
                        shtml.Append("          <option value = \"6\">Có cố gắng</option>");

                    if (leaid == 7)
                        shtml.Append("          <option selected value = \"7\">Không cố gắng</option>");
                    else
                        shtml.Append("          <option value = \"7\">Không cố gắng</option>");

                    if (leaid == 8)
                        shtml.Append("          <option selected value = \"8\" >Không nhận xét</option>");
                    else
                        shtml.Append("          <option value = \"8\" >Không nhận xét</option>");

                    shtml.Append("      </select>");
                    shtml.Append("  </td>");
                    shtml.Append("  <td>");
                    shtml.Append("      <textarea " + disible + " class=\"form-control\" rows=\"1\" id = \"txt-note-" + acstu.ID + "\" class=\"browser-default\">" + note + "</textarea>");
                    shtml.Append("  </td>");

                    ////thời gian HV Join Meeting
                    //double timeJoinMeeting = 0;
                    //int TotalJoinMeeting = 0;
                    //if (ck != null)
                    //{
                    //    if (!string.IsNullOrEmpty(ck.sTimeJoinMeeting.ToString()) && !string.IsNullOrEmpty(ck.eTimeJoinMeeting.ToString()))
                    //    {
                    //        timeJoinMeeting = (ck.eTimeJoinMeeting.Value - ck.sTimeJoinMeeting.Value).TotalMinutes;
                    //    }
                    //    if (!string.IsNullOrEmpty(ck.TotalJoinMeeting.ToString()))
                    //    {
                    //        TotalJoinMeeting = ck.TotalJoinMeeting.Value;
                    //    }
                    //}
                    //shtml.Append("  <td>");
                    //shtml.Append("      <span>" + timeJoinMeeting + " phút</span>");
                    //shtml.Append("  </td>");

                    ////Số lần HV Join Meeting
                    //shtml.Append("  <td class=\"text-center\">");
                    //shtml.Append("      <span>" + TotalJoinMeeting + "</span>");
                    //shtml.Append("  </td>");

                    //Cảnh báo
                    shtml.Append("  <td class=\"text-center\">");
                    shtml.Append("      <div class=\"custom-control custom-checkbox\">");
                    if (string.IsNullOrEmpty(warning))
                        shtml.Append("      <input " + disible + " type = \"checkbox\" class=\"custom-control-input\" id=\"chk-warning-" + acstu.ID + "\">");
                    else
                        shtml.Append("      <input " + disible + " checked type = \"checkbox\" class=\"custom-control-input\" id=\"chk-warning-" + acstu.ID + "\">");
                    shtml.Append("          <label class=\"custom-control-label\" for=\"chk-warning-" + acstu.ID + "\"></label>");
                    shtml.Append("      </div>");

                    shtml.Append("  </td>");

                    //Nút cập nhật
                    //shtml.Append("  <td>");
                    //if (d.Date == GetDateTime.Now.Date && aclog.RoleID == 4)
                    //    shtml.Append("      <a href = \"javascript:;\" data-id = \"" + acstu.ID + "\" data-date = \"" + GetDateTime.Now.ToString("dd/MM/yyyy") + "\" class=\"btn btn-secondary btn-icon btn-sm update-attendance\">Cập nhật</a>");
                    //shtml.Append("  </td>");
                    shtml.Append("</tr>");
                }
            }
            return Json(new { rs = true, l = shtml.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AttendanceStudent(int id, int student, int attendance, int learning, string note, int warning)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            string attname = "";
            if (attendance == 1)
                attname = "Có mặt";
            else if (attendance == 2)
                attname = "Vắng có phép";
            else if (attendance == 3)
                attname = "Vắng không phép";
            else if (attendance == 4)
                attname = "Đi muộn";
            else if (attendance == 5)
                attname = "Về sớm";

            string learingname = "";
            if (learning == 1)
                learingname = "Giỏi";
            else if (learning == 2)
                learingname = "Khá";
            else if (learning == 3)
                learingname = "Trung bình";
            else if (learning == 4)
                learingname = "Kém";
            else if (learning == 5)
                learingname = "Theo dõi đặc biệt";
            else if (learning == 6)
                learingname = "Có cố gắng";
            else if (learning == 7)
                learingname = "Không cố gắng";
            else if (learning == 8)
                learingname = "Không nhận xét";

            bool war = false;
            if (warning == 1)
                war = true;

            var l = CoursesScheduleTable.getbyid(id);
            var cus = AccountTable.getbyID(student);
            if (l == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
            {
                var ck = CoursesAttendanceStudentTable.checkadd(l.CourseID.Value, student, l.Date.Value.Date, l.StudyTimeID.Value);
                if (ck == null)
                {
                    
                    var cou = CoursesTable.getbyid(l.CourseID.Value);

                    var school = SchoolTable.getbyID(cou.SchoolID.Value);
                    tbl_Account lead = new tbl_Account();
                    lead.ID = 0;
                    lead.FullName = "";
                    if (cou.TeacherLeaderUID != null)
                    {
                        lead.ID = cou.TeacherLeaderUID.Value;
                        lead.FullName = cou.TeacherLeaderName;
                    }
                    CoursesAttendanceStudentTable.insert(school, cou, cus, l.StudyTimeID.Value, l.Date.Value.Date, attendance, attname, learning, learingname, note, aclog.UserName, war, aclog.FullName, lead);
                    string history = "Học viên " + cus.FullName + " được điểm danh: " + attname + ", đánh giá: " + learingname + ", nhận xét: " + note + " bởi " + aclog.RoleName + " - " + aclog.FullName;
                    HistoryAttendanceTable.insert(cus.ID, l.ID, history, aclog.ID);
                    NotificationTable.insertsingle("", "Điểm danh thành công", "Giáo viên đã điểm danh cho bạn, bạn vào và đánh giá chất lượng buổi học nha", student, aclog.UserName, 18, 0);
                    var te = DeviceMobileTable.getbyUID(student)?.IncludePlayerID;
                    PushNotifyMobile.SendNotifyMobile("Giáo viên đã điểm danh cho bạn, bạn vào và đánh giá chất lượng buổi học nha", new[] { te });
                    if (war == true)//push noti cảnh báo cho tư vấn viên
                    {
                        if (cus != null && cou != null)
                        {
                            NotificationTable.insertsingle("", "[Cảnh báo học viên]", "Cảnh báo Học viên [" + cus.FullName + "] khóa [" + cou.CourseName + "]. Nhận xét GV: \"" + note, cus.SupportUID ?? 0, aclog.UserName, 2, 0);
                        }
                    }
                }
                else
                {
                    CoursesAttendanceStudentTable.update(ck.ID, attendance, attname, learning, learingname, note, war, aclog.UserName);
                    string history = "Học viên " + cus.FullName + " được cập nhật điểm danh: " + attname + ", đánh giá: " + learingname + ", nhận xét: " + note + " bởi " + aclog.RoleName + " - " + aclog.FullName;
                    HistoryAttendanceTable.insert(cus.ID, l.ID, history, aclog.ID);
                }
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region chỉnh sửa khóa học
        public ActionResult EditCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }

            //var cacourse = CoursesScheduleTable.getbycourseid(c.ID).Select(n => n.StudyTimeID).Distinct().ToList();
            var cas = StudyTimeTable.getall().OrderBy(n => n.sTime).ToList();
            StringBuilder sca = new StringBuilder();
            for (int i = 0; i < cas.Count; i++)
            {
                //bool check = false;
                //for (int j = 0; j < cacourse.Count; j++)
                //{
                //    if (cas[i].ID == cacourse[j])
                //    {
                //        check = true;
                //        break;
                //    }
                // }
                //if (check)
                sca.Append("<option  value=\"" + cas[i].ID + "\" selected>Ca " + cas[i].sTime + "-" + cas[i].eTime + "</option>");
                //else
                //sca.Append("<option value=\"" + cas[i].ID + "\">Ca " + cas[i].sTime + "-" + cas[i].eTime + "</option>");
            }
            var teacher = CoursesScheduleTable.getbycourseid(c.ID).OrderBy(x => x.ID).Select(n => n.TeacherID).FirstOrDefault();
            ViewBag.Teacher = teacher;
            ViewBag.StudyID = sca.ToString();
            ViewBag.DateNow = GetDateTime.Now.ToString("dd/MM/yyyy");
            List<tbl_StudyTime> studyTimeList = StudyTimeTable.getall().OrderBy(x => x.sTime).ToList();
            var studyString = "";
            foreach (var s in studyTimeList)
            {
                studyString += "{id:" + s.ID + ",name:\"" + s.sTime + " - " + s.eTime + "\",select:false},";
            }
            ViewBag.StudyList = studyString.Remove(studyString.Length - 1, 1);
            return View(c);
        }

        [HttpPost]
        public ActionResult LoadTietHoc(int CourseID, int addday, int totalday)
        {
            try
            {
                //totalday = 1 là thêm buổi, không phải sửa
                var course = CoursesTable.getbyid(CourseID);
                var lDate = CoursesScheduleTable.getbycourseid(course.ID);
                DateTime sDate = DateTime.Now.AddDays(-1);
                lDate = lDate.Where(x => x.FisnishStatus != 1 && x.Date >= sDate).ToList();
                List<NgayHoc> l = new List<NgayHoc>();
                if (addday == 0)//sửa
                {
                    foreach (var item in lDate)
                    {
                        var ca = StudyTimeTable.getbyid(item.StudyTimeID ?? 0);
                        if (ca == null)
                            continue;

                        var te = AccountTable.getbyID(item.TeacherID ?? 0);
                        if (te == null)
                            continue;

                        NgayHoc lt = new NgayHoc();
                        lt.ID = item.ID;
                        lt.eventName = "" + item.Date.Value.ToString("dd/MM") + " - " + te.FullName + " [" + ca.sTime + "-" + ca.eTime + "]";
                        lt.Color = "orange";
                        lt.calendar = "Đã chọn ngày học";
                        lt.date = item.Date.Value.ToString("yyyy-MM-dd");
                        lt.TeacherID = te.ID;
                        lt.TeacherName = te.FullName + "[" + ca.sTime + "-" + ca.eTime + "]";
                        lt.CaID = ca.ID;
                        lt.CaName = "Ca: " + ca.sTime + ":" + ca.eTime + "";
                        l.Add(lt);
                    }
                    l = l.OrderBy(n => n.ID).ToList();
                }
                else// cộng thêm ngày
                {
                    for (int i = 0; i < totalday; i++)
                    {
                        NgayHoc lt = new NgayHoc();
                        List<TietHoc> t = new List<TietHoc>();

                        lt.ID = i + 1;
                        lt.eventName = "Ngày học thêm " + i + "";
                        lt.Color = "orange";
                        lt.calendar = "Đã chọn ngày học";
                        l.Add(lt);
                    }
                    l = l.OrderBy(n => n.ID).ToList();
                }
                return Json(new { d = l, limit = 5 }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { d = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult TaiNgayHoc()//lấy ra những ngày học được
        {
            ConcurrentBag<Ngay> l = new ConcurrentBag<Ngay>();
            DateTime sDate = DateTime.Now.Date;
            var dayOff = DayOffTable.getall().Where(x => x.DayOff >= sDate && x.isHide == false).ToList();
            Parallel.For(0, 720, i =>
            {
                var _db = new ZimEntities();
                Ngay t = new Ngay();
                DateTime Day = sDate.AddDays(i);

                var checkdayoff = dayOff.Where(n => n.DayOff.Value == Day).FirstOrDefault();
                if (checkdayoff != null)
                    return;

                t.Day = "{\"id\":\"d-" + i + "\",\"limit\":5, \"eventName\":\"Blank Event\", \"calendar\":\"blank\", \"color\":\"blank\", \"date\":\"" + Day.ToString("yyyy-MM-dd") + "\"}";
                l.Add(t);
            });
            return Json(new { rs = true, d = l.ToList() }, JsonRequestBehavior.AllowGet);
        }

        public async Task<int> TinhCa(DateTime Day, string caid, int cid)
        {
            string[] arrca = caid.Split(',');
            int limit = arrca.Length;
            foreach (var c in arrca)
            {
                var check = checkca(c.ToInt(0), Day, cid);
                if (check == false)
                    limit -= limit;
            }
            return limit;
        }
        // check thử khóa học này, ngày này, ca này có trống hay không
        public static bool checkca(int studyid, DateTime Date, int courseid)// true được
        {
            using (var db = new ZimEntities())
            {
                var study = StudyTimeTable.getbyid(studyid);
                if (study == null)
                    return false;

                string[] stime = study.sTime.Split(':');
                TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                DateTime st = Date.Date.Add(sth);
                DateTime et = st.AddMinutes(study.StyleLesson.Value);

                bool check = true;
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseid && n.Date == Date && n.IsHide != true).ToList();
                foreach (var item in l)
                {
                    var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                    if (stu == null)
                        continue;

                    string[] istime = stu.sTime.Split(':');
                    TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                    DateTime ist = Date.Date.Add(isth);
                    DateTime iet = ist.AddMinutes(stu.StyleLesson.Value);

                    if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                    {
                        check = false;
                        break;
                    }

                    if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                    {
                        check = false;
                        break;
                    }

                    if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                    {
                        check = false;
                        break;
                    }
                }
                return check;
            }
        }
        [HttpPost]
        public ActionResult TaiGiaoVien(int courseid, string date, int studytime, int program, int teacher)
        {
            //2018-04-01
            //[{ id: 1, name: 'tutor 1' }, { id: 2, name: 'tutor 2' }, { id: 3, name: 'tutor 3' }]           
            var l = new List<Admin_Teacher_GetAvailable_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_Teacher_GetAvailable(teacher, courseid, program, studytime, date).ToList();
            }
            if (l.Count == 0)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            return Json(new { rs = true, l = l.ToArray(), CaID = studytime }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]//lấy danh sách ca học đã được chọn trước đó để show ra
        public ActionResult TaiCaHoc()
        {
            var cas = StudyTimeTable.getall().ToList();
            List<DsGiaoVien> studys = new List<DsGiaoVien>();
            DsGiaoVien studyone = new DsGiaoVien();
            studyone.id = 0;
            studyone.name = "---Chọn ca---";
            studys.Add(studyone);

            foreach (var c in cas)
            {
                DsGiaoVien study = new DsGiaoVien();
                study.id = c.ID;
                study.name = "Ca: " + c.sTime + " - " + c.eTime;
                studys.Add(study);
            }
            DsGiaoVien[] aRsStudy = studys.ToArray();
            return Json(new { rs = true, study = aRsStudy }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateCourse(FormCollection f)
        {
            // cờ notifi xem có thay đổi lịch hay ko
            bool notificationchangerschedule = false;

            //2018-03-01,4,1398.1399|2018-03-02,4,1400.1401| Ngày, ID Giáo viên, list ID tiet học
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var c = CoursesTable.getbyid(f.Get("hdfcourseid").ToInt(0));

            if (string.IsNullOrEmpty(f.Get("hdflistCalender").ToString()))
            {
                return Redirect(Request.UrlReferrer.AbsoluteUri);
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            string[] sCalender = f.Get("hdflistCalender").ToString().Split('|');
            for (int i = 0; i < sCalender.Length; i++)
            {
                string[] sdata = sCalender[i].Split(',');

                DateTime date = DateTime.ParseExact(sdata[1], "yyyy-MM-dd", null);
                var teacher = AccountTable.getbyID(sdata[2].ToInt(0));
                if (teacher == null)
                    continue;

                if (f.Get("hdfaddday").ToInt(0) == 0)// sửa khóa
                {
                    var kt = CoursesScheduleTable.getbyid(sdata[0].ToInt(0));
                    if (kt.Date.Value.Date != date.Date)
                        notificationchangerschedule = true;
                    CoursesScheduleTable.update(sdata[0].ToInt(0), date, teacher.ID, sdata[3].ToInt(0), 0, aclog.UserName);
                }
                else// thêm ngày học
                {
                    CoursesScheduleTable.insert(c.ID, false, 0, date.Date, sdata[3].ToInt(0),
                        c.SchoolID.Value, 0, teacher.ID, aclog.UserName, 0);
                    notificationchangerschedule = true;
                }
            }
            var last = CoursesScheduleTable.GetLastLesson(c.ID);
            CoursesTable.updateEndDate(c.ID, last.Date.Value);
            //gửi thông báo cho hv, gv, học vụ
            if (notificationchangerschedule)
            {
                var title = "Thay đổi lịch khóa [" + c.CourseName + "]";
                var content = "Vui lòng kiểm tra lịch để biết thêm chi tiết";
                var rs = NotificationCourseTable.insert(title, content, c.ID, aclog.UserName);
                if (rs != null)
                {
                    var l = CoursesStudentTable.getbycourseid(c.ID);
                    foreach (var item in l)
                    {
                        NotificationTable.insertsingle("", rs.NotificationTitle, rs.NotificationContent, item.StudentUID.Value, aclog.UserName, 3, c.ID);
                    }
                    var te = TeacherCourseTable.getbycourseid(c.ID);
                    foreach (var item in te)
                    {
                        NotificationTable.insertsingle("", rs.NotificationTitle, rs.NotificationContent, item.TeacherUID.Value, aclog.UserName, 3, c.ID);
                    }
                    if (c.AcademicUID != null)// gửi cho học vụ
                        NotificationTable.insertsingle("", rs.NotificationTitle, rs.NotificationContent, c.AcademicUID.Value, aclog.UserName, 3, c.ID);
                    SetAlert("Thành công", "s");
                }
            }
            //================================
            SetAlert("Cập nhật thành công", "s");
            return RedirectToAction("EditCourse", "CourseDetail", new { id = c.ID });
        }
        [HttpPost]
        public ActionResult DeleteSchedule(int coursesScheduleID)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 1 && aclog.RoleID != 7)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                CoursesScheduleTable.UpdateIsHide(coursesScheduleID);
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region tài liệu khóa học
        public ActionResult DocumentCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.CourseID = c.ID;
            ViewBag.CurriculumsID = c.CurriculumsID.Value;
            return View(c);
        }
        #endregion

        #region bài tập khóa học
        public ActionResult ExerciseCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.CourseID = c.ID;
            return View(c);
        }
        [HttpPost]
        public ActionResult UpdateDeadlineExercise(int courseid, int exerciseid, string stime, string etime)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var ex = ExerciseTable.getbyid(exerciseid);
            var c = CoursesTable.getbyid(courseid);
            if (ex == null || c == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            if (string.IsNullOrEmpty(stime) || string.IsNullOrEmpty(etime))
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

            DateTime sd = GetDateTime.Now.Date;
            DateTime ed = GetDateTime.Now.Date;

            try
            {
                sd = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
                ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
            if (ed < sd)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            var ck = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (ck == null)
                CoursesExerciseDeadlineTable.insert(c.ID, ex.ID, sd.Date, ed.Date, aclog.UserName);
            else
                CoursesExerciseDeadlineTable.update(ck.ID, sd.Date, ed.Date, aclog.UserName);
            // gửi thông báo cho học viên có bài tập mới
            var lstudent = CoursesStudentTable.getbycourseid(c.ID);
            var title = "Cập nhật bài tập khóa [" + c.CourseName + "]";
            var content = "<a href=\"/Admin/CourseDetail/ExerciseCourse/" + c.ID + "\">Cập nhật bài tập cho khóa [" + c.CourseName + "]</a>";

            foreach (var item in lstudent)
            {
                NotificationTable.insertsingle("", title, content, item.StudentUID.Value, aclog.UserName, 4, c.ID);
                //Push noti desktop
            }
            ///
            return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExerciseGroup(int courseid, int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.gid = id;
            ViewBag.Course = c;
            return View();
        }
        #endregion

        #region ghi chú khóa
        public ActionResult NoteCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            ViewBag.CourseID = c.ID;
            return View(c);
        }
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddNote(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            int courseid = f.Get("hdfcourse").ToInt(0);
            var note = f.Get("txt-note").ToString();

            var rs = CoursesNoteTable.insert(courseid, note, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("NoteCourse", "CourseDetail", new { id = courseid });
        }
        #endregion

        //giáo viên khóa học
        public ActionResult TeacherCourse(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            if (c.CourseSelfStudy == true)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "Course");
            }
            return View(c);
        }

        [HttpPost]
        public ActionResult UpFileCurruTeacher(FormCollection f, HttpPostedFileBase[] fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var c = CoursesTable.getbyid(f.Get("hdfCourseID").ToInt(0));
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "Course");
            }

            DateTime d = DateTime.ParseExact(f.Get("ddl-date").ToString(), "dd/MM/yyyy", null);
            var sty = StudyTimeTable.getbyid(f.Get("ddl-study").ToInt(0));

            if (ModelState.IsValid)
            {   //iterating through multiple file collection   
                foreach (HttpPostedFileBase file in fileupload)
                {
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                            return RedirectToAction("CourseList", "Course");
                        }
                        var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                        var path = Path.Combine(Server.MapPath("~/Upload/curriculumteacher/"), fileName);
                        string link = "/Upload/curriculumteacher/" + fileName;
                        file.SaveAs(path);

                        CurriculumTeacherTable.insert(c.ID, d.Date, sty.ID, link, aclog.UserName);
                    }
                }
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("ScheduleCourse", "CourseDetail", new { id = c.ID });
        }

        // tạo nhóm câu hỏi của khóa
        [HttpPost]
        public ActionResult AddGroupExerciseCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(f.Get("hdfCourseID").ToInt(0));
            if (c == null)
            {
                SetAlert("Không tìm khóa học", "e");
                return RedirectToAction("CourseList", "TeacherCourse");
            }
            int type = f.Get("ddl-type").ToInt(1);
            string groupname = f.Get("txt-group").ToString();
            var rs = ExerciseGroupTable.insert(0, c.ID, type, groupname, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = c.ID });
        }
        // cập nhật nhóm câu hỏi của khóa
        [HttpPost]
        public ActionResult UpdateGroupExerciseCourse(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var g = ExerciseGroupTable.getbyid(f.Get("hdfGroupID").ToInt(0));
            if (g == null)
            {
                SetAlert("Không tìm thấy dữ liệu", "e");
                return RedirectToAction("CourseList", "TeacherCourse");
            }
            string groupname = f.Get("txt-group-edit-name").ToString();
            var rs = ExerciseGroupTable.update(g.ID, groupname, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = g.CourseID.Value });
        }
        // tạo câu hỏi cho khóa
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddExerciseCourse(FormCollection f, HttpPostedFileBase fileupload)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var c = CoursesTable.getbyid(f.Get("hdfcID").ToInt(0));
            if (c == null)
            {
                SetAlert("Không tìm khóa học", "e");
                return RedirectToAction("CourseList", "TeacherCourse");
            }
            var nhombt = ExerciseGroupTable.getbyid(f.Get("ddl-group").ToInt(0));
            if (nhombt == null)
            {
                SetAlert("Không tìm thấy nhóm bài tập", "e");
                return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = c.ID });
            }
            string ques = f.Get("hdfQuestion").ToString();

            string filename = "";
            string link = "";
            if (fileupload != null)
            {
                string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                string ext = Path.GetExtension(fileupload.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                {
                    SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                    return RedirectToAction("OrderDetail", "OrderList");
                }
                //var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);
                link = "/Upload/exercise/" + fileName;
                fileupload.SaveAs(path);
                filename = fileupload.FileName;
            }
            var q = ExerciseTable.insert(nhombt.ID, ques, filename, link, nhombt.TypeGroup.Value, 0, aclog.UserName, 0, "");
            if (q != null)
            {
                // gửi thông báo cho học viên có bài tập mới
                var lstudent = CoursesStudentTable.getbycourseid(c.ID);
                var title = "Bài tập mới khóa [" + c.CourseName + "]";
                var content = "<a href=\"/Admin/CourseDetail/ExerciseCourse/" + c.ID + "\">Bài tập mới cho khóa [" + c.CourseName + "]</a>";

                foreach (var item in lstudent)
                {
                    NotificationTable.insertsingle("", title, content, item.StudentUID.Value, aclog.UserName, 4, c.ID);
                }
                ///
                SetAlert("Thành công", "s");
            }
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = c.ID });
        }

        [HttpPost]
        public ActionResult GetExerciseID(int exerciseid)
        {
            var ck = ExerciseTable.getbyid(exerciseid);
            if (ck == null)
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            else
                return Json(new { rs = true, ex = ck }, JsonRequestBehavior.AllowGet);
        }
        // cập nhật câu hỏi khóa
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult UpdateExerciseCourse(FormCollection f, HttpPostedFileBase fileuploadedit)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ex = ExerciseTable.getbyid(f.Get("hdfexID").ToInt(0));
            var nhom = ExerciseGroupTable.getbyid(ex.ExerciseGroupID.Value);
            if (nhom.CourseID == null && aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
            {
                SetAlert("Bài tập của giáo trình, bạn không thể sửa", "e");
                return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = nhom.CourseID.Value });
            }
            if (nhom.CourseID == 0 && aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 7)
            {
                SetAlert("Bài tập của giáo trình, bạn không thể sửa", "e");
                return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = nhom.CourseID.Value });
            }
            var nhombt = ExerciseGroupTable.getbyid(f.Get("ddl-group-edit").ToInt(0));
            if (nhombt == null)
            {
                SetAlert("Không tìm thấy nhóm bài tập", "e");
                return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = nhom.CourseID.Value });
            }
            string ques = f.Get("hdfQuestionEdit").ToString();

            string filename = "";
            string link = "";
            if (fileuploadedit != null)
            {
                string name = Path.GetFileNameWithoutExtension(fileuploadedit.FileName);
                string ext = Path.GetExtension(fileuploadedit.FileName);
                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                {
                    SetAlert("File của bạn đã bị chặn, vui lòng không tải lên file .php, .asp, .aspx, .ps1", "e");
                    return RedirectToAction("OrderDetail", "OrderList");
                }
                //var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Server.MapPath("~/Upload/exercise/"), fileName);
                link = "/Upload/exercise/" + fileName;
                fileuploadedit.SaveAs(path);
                filename = fileuploadedit.FileName;
            }
            var q = ExerciseTable.update(ex.ID, nhombt.ID, ques, ex.isHide.Value, aclog.UserName, ex.ParentQuestionID.Value, ex.DescribeAnswer);
            if (q != null)
                SetAlert("Thành công", "s");
            else
                SetAlert("Không thành công", "e");
            return RedirectToAction("ExerciseCourse", "CourseDetail", new { id = nhom.CourseID.Value });
        }
        #region feedback theo từng buổi học
        //danh sách các buổi học cần feedback
        public ActionResult FeedbackSchedule(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var course = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                course = Request.QueryString["slCourse"].ToString().ToInt(0);
            }
            ViewBag.Course = course;
            var pageSize = 20;
            var l = new List<Admin_FeedbackSchedule_GetList_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_FeedbackSchedule_GetList(aclog.ID, course, (page - 1) * pageSize, pageSize).ToList();
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
        public ActionResult FeedbackedList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var course = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                course = Request.QueryString["slCourse"].ToString().ToInt(0);
            }
            ViewBag.Course = course;
            var pageSize = 20;
            var l = new List<Admin_FeedbackSchedule_GetListFeedbacked_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_FeedbackSchedule_GetListFeedbacked(aclog.RoleID, aclog.ID, course, (page - 1) * pageSize, pageSize).ToList();
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
        public ActionResult InsertFeedbackSchedule(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var courseScheduleID = 0;
            if (!string.IsNullOrEmpty(f.Get("courseSchedule")))
            {
                courseScheduleID = f.Get("courseSchedule").ToInt(0);
            }
            var courseSchedule = CoursesScheduleTable.getbyid(courseScheduleID);

            var courseID = 0;
            if (!string.IsNullOrEmpty(f.Get("courseid")))
            {
                courseID = f.Get("courseid").ToInt(0);
            }
            var feedbackContent = "";
            if (!string.IsNullOrEmpty(f.Get("txt-content")))
            {
                feedbackContent = f.Get("txt-content");
            }
            var scheduleStatus = 0;
            if (!string.IsNullOrEmpty(f.Get("slStatus")))
            {
                scheduleStatus = f.Get("slStatus").ToInt(0);
            }
            var rating = 0;
            if (!string.IsNullOrEmpty(f.Get("rating")))
            {
                rating = f.Get("rating").ToInt(0);
            }
            var rs = FeedbackScheduleTable.Insert(aclog.ID, courseScheduleID, courseID, feedbackContent, scheduleStatus, rating, aclog.UserName);
            if (rs != null)
            {
                var account = DeviceMobileTable.getbyUID(courseSchedule.TeacherID ?? 0)?.IncludePlayerID;
                PushNotifyMobile.SendNotifyMobile("Học viên vừa đánh giá buổi dạy của bạn, bạn vào xem nha", new[] { account });
                NotificationTable.insertsingle("", "Có đánh giá mới", "Học viên vừa đánh giá buổi dạy của bạn, bạn vào xem nha", courseSchedule.TeacherID ?? 0, "admin", 19, 0);
                SetAlert("Thành công", "s");
            }
            else SetAlert("Thất bại", "e");

            return RedirectToAction("FeedbackSchedule", "CourseDetail", new { area = "Admin" });
        }
        public ActionResult UpdateFeedbackSchedule(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var feedbackid = 0;
            if (!string.IsNullOrEmpty(f.Get("feedbackid")))
            {
                feedbackid = f.Get("feedbackid").ToInt(0);
            }
            var feedbackContent = "";
            if (!string.IsNullOrEmpty(f.Get("txt-content")))
            {
                feedbackContent = f.Get("txt-content");
            }
            var scheduleStatus = 0;
            if (!string.IsNullOrEmpty(f.Get("slStatus")))
            {
                scheduleStatus = f.Get("slStatus").ToInt(0);
            }
            var rating = 5;
            if (!string.IsNullOrEmpty(f.Get("rating")))
            {
                rating = f.Get("rating").ToInt(5);
            }
            var rs = FeedbackScheduleTable.Update(feedbackid, feedbackContent, scheduleStatus, rating, aclog.UserName);
            if (rs != null)
                SetAlert("Thành công", "s");
            else SetAlert("Thất bại", "e");

            return RedirectToAction("FeedbackedList", "CourseDetail", new { area = "Admin" });
        }
        #endregion
        public void SendMobile(List<int> list, string coursename)
        {
            foreach (var i in list)
            {
                try
                {
                    var includeID = DeviceMobileTable.getbyUID(i)?.IncludePlayerID;
                    PushNotifyMobile.SendNotifyMobile("Có một thông báo mới từ khóa học" + coursename, new[] { includeID });
                }
                catch
                {
                    continue;
                }
            }
        }
        #region video khóa học
        public ActionResult VideoOfSchedule(int id)
        {
            var data = new List<Admin_VideoSchedule_GetList_Result>();
            using (var db = new ZimEntities())
            {
                data = db.Admin_VideoSchedule_GetList(id).ToList();
            }
            ViewBag.CourseID = id;
            return View(data);
        }
        #endregion

        [HttpPost]
        public ActionResult LoadAttendance(int courseid, int studentid)
        {
            var schedules = new List<GetAttendanceByCourseAndStudent_Result>();
            using (var db = new ZimEntities())
            {
                schedules = db.GetAttendanceByCourseAndStudent(studentid, courseid).ToList();
            }
            return Json(new { data = schedules });
        }

        [HttpPost]
        public ActionResult LoadAttendanceByID(int id, int studentid)
        {
            try
            {
                var schedule = CoursesScheduleTable.getbyid(id);
                var attendace = CoursesAttendanceStudentTable.checkadd(schedule.CourseID.Value, studentid, schedule.Date.Value, schedule.StudyTimeID.Value);
                return Json(new { rs = true, data = attendace });
            }
            catch
            {
                return Json(new { rs = false });
            }
        }

        [HttpPost]
        public ActionResult UpdateSchedule(int id, string date, int studytime, int teacher)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var datetime = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                var schedule = CoursesScheduleTable.update(id, datetime, teacher, studytime, 0, aclog.UserName);
                return Json(new { rs = true });
            }
            catch
            {
                return Json(new { rs = false });
            }
        }

        [HttpPost]
        public ActionResult LoadTeacher(int course, string date, int studytime)
        {
            try
            {
                var c = CoursesTable.getbyid(course);
                var l = new List<Admin_Teacher_GetAvailable_Result>();
                var formatDate = string.Join("-", date.Split('/').Reverse());
                using (var db = new ZimEntities())
                {
                    l = db.Admin_Teacher_GetAvailable(0, course, c.ClassID.Value, studytime, formatDate).ToList();
                }
                if (l.Count == 0)
                    return Json(new { rs = false }, JsonRequestBehavior.AllowGet);

                return Json(new { rs = true, data = l.ToArray() }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false });
            }
        }

        [HttpPost]
        public ActionResult DelayedClassTime(int id, int course, int teacherId, string dayofweek, string studytime, string startdate)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var c = CoursesTable.getbyid(course);
                var schedule = CoursesScheduleTable.getbyid(id);
                var delaySchedule = CoursesScheduleTable.GetDelaySchedule(schedule.Date.Value, c.ID);
                var offs = DayOffTable.getall().Where(x => x.DayOff >= schedule.Date.Value).Select(x => x.DayOff).ToArray();
                int[] days = dayofweek.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                int[] cahocs = studytime.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                DateTime date = DateTime.ParseExact(startdate, "dd/MM/yyyy", null); //ngày bắt đầu
                
                do
                {
                    // kiểm tra xem có phải ngày nghỉ hay không
                    if (Array.IndexOf(offs, date) != -1)
                        continue;

                    for (int i = 0; i < days.Length; i++)
                    {
                        if(days[i] != ((int)date.DayOfWeek))
                            continue;

                        var study = StudyTimeTable.getbyid(cahocs[i]);
                        if (study == null)
                            continue;

                        string[] stime = study.sTime.Split(':');
                        TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                        DateTime st = DateTime.Now.Date.Add(sth);
                        string[] etime = study.eTime.Split(':');
                        TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                        DateTime et = DateTime.Now.Date.Add(eth);
                        var check = checkteacher(teacherId, date, st, et, cahocs[i]);
                        if (!check)
                        {
                            var rs = CoursesScheduleTable.update(delaySchedule[0]?.ID ?? 0, date, teacherId, cahocs[i], 0, aclog.UserName);
                            if(rs != null)
                            {
                                delaySchedule.RemoveAt(0);
                            }
                        }
                    }
                    date = date.AddDays(1);
                }
                while (delaySchedule.Count != 0);

                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false });
            }
        }
        
        [HttpPost]
        public ActionResult DelayedMultipleClass(string id, int course, int teacherId, string dayofweek, string studytime, string startdate)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID == 5)// học viên bị đá ra
                    return RedirectToAction("Signin", "Login", new { area = "" });

                var c = CoursesTable.getbyid(course);
                int[] ids = id.Split(',').Select(x => Int32.Parse(x)).ToArray();
                var delaySchedule = CoursesScheduleTable.GetMultiple(ids);
                var offs = DayOffTable.getall().Where(x => x.DayOff >= delaySchedule[0].Date.Value).Select(x => x.DayOff).ToArray();
                int[] days = dayofweek.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                int[] cahocs = studytime.Split(',').Select(n => Convert.ToInt32(n)).ToArray();
                DateTime date = DateTime.ParseExact(startdate, "dd/MM/yyyy", null); //ngày bắt đầu
                
                do
                {
                    // kiểm tra xem có phải ngày nghỉ hay không
                    if (Array.IndexOf(offs, date) != -1)
                        continue;

                    for (int i = 0; i < days.Length; i++)
                    {
                        if(days[i] != ((int)date.DayOfWeek))
                            continue;

                        var study = StudyTimeTable.getbyid(cahocs[i]);
                        if (study == null)
                            continue;

                        string[] stime = study.sTime.Split(':');
                        TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                        DateTime st = DateTime.Now.Date.Add(sth);
                        string[] etime = study.eTime.Split(':');
                        TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                        DateTime et = DateTime.Now.Date.Add(eth);
                        var check = checkteacher(teacherId, date, st, et, cahocs[i]);
                        if (!check)
                        {
                            var rs = CoursesScheduleTable.update(delaySchedule[0]?.ID ?? 0, date, teacherId, cahocs[i], 0, aclog.UserName);
                            if(rs != null)
                            {
                                delaySchedule.RemoveAt(0);
                            }
                        }
                    }
                    date = date.AddDays(1);
                }
                while (delaySchedule.Count != 0);

                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false });
            }
        }
        public static bool checkteacher(int UID, DateTime Date, DateTime st, DateTime et, int studytime)//kiểm tra giáo viên có đụng ko
        {
            using (var db = new ZimEntities())
            {
                var off = db.tbl_DayOffTeacher.Where(x => x.isHide == false && x.UID == UID && x.DayOff == Date && x.StudyTimeID == studytime).FirstOrDefault();
                if (off != null)
                    return true;
                var l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == UID && n.IsHide != true && (n.Date.Value.Day == Date.Day && n.Date.Value.Month == Date.Month && n.Date.Value.Year == Date.Year)).ToList();
                if (l.Count() != 0)
                {
                    foreach (var item in l)
                    {
                        var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                        if (stu == null)
                            continue;

                        string[] istime = stu.sTime.Split(':');
                        TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                        DateTime ist = DateTime.Now.Date.Add(isth);

                        string[] ietime = stu.eTime.Split(':');
                        TimeSpan ieth = new TimeSpan(ietime[0].ToInt(), ietime[1].ToInt(0), 0);
                        DateTime iet = DateTime.Now.Date.Add(ieth);

                        if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                        {
                            return true;
                        }

                        if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                        {
                            return true;
                        }

                        if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }
    }
}