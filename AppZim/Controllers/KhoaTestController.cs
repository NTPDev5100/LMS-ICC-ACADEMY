using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Web.Mvc;
using WebUI.Business;
using static AppZim.Areas.Admin.Controllers.CourseController;

namespace AppZim.Controllers
{

    public class KhoaTestController : BaseController
    {
        // GET: KhoaTest
      

        public ActionResult Index()

        {
            //if (Session["UID"] == null)
            //    return RedirectToAction("Signin", "Login", new { area = "" });
            //var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            //List<TVVData> l;
            //var rs = SqlHelper.ExecuteDataTable("lay_doanhthu_hoadon_hocvien_cua_tvv");
            //l = (from rw in rs.AsEnumerable()
            //     select new TVVData()
            //     {
            //         ID = Convert.ToInt32(rw["ID"]),
            //         FullName = rw["FullName"].ToString(),
            //         TotalInvoices = rw["TotalInvoices"] == DBNull.Value ? 0 : Convert.ToDouble(rw["TotalInvoices"]),
            //         StudentCount = rw["studentCount"] == DBNull.Value ? 0 : Convert.ToDouble(rw["studentCount"]),
            //         InvoicesCount = rw["countRow"] == DBNull.Value ? 0 : Convert.ToDouble(rw["countRow"]),
            //     }).ToList();
            return View();
        }

        [HttpPost]
        public ActionResult LoadCampaignRevenueTVV(int tvvID,string sdate, string edate)
        {
            var tvv = AccountTable.getbyID(tvvID);
            if (tvv == null)
                return Json(new { rs = false, message="Khong tim thay tvv"}, JsonRequestBehavior.AllowGet);

            DateTime st = new DateTime();
            DateTime et = GetDateTime.Now;
            if (!string.IsNullOrEmpty(sdate))
            {
                st = DateTime.ParseExact(sdate, "dd/MM/yyyy", null);
            }
            if (!string.IsNullOrEmpty(edate))
            {
                et = DateTime.ParseExact(edate, "dd/MM/yyyy", null);
            }

            SqlParameter[] parameters =
              {
                    new SqlParameter("@startDate", SqlDbType.Date) { Value = st },
                    new SqlParameter("@endDate", SqlDbType.Date) { Value =et },
                    new SqlParameter("@TVVName", SqlDbType.NVarChar) { Value = tvv.UserName },
                };
            List<CampaignRevenue> l;
            var rs = SqlHelper.ExecuteDataTable("getCampaignRevenueTVV",parameters);
            l = (from rw in rs.AsEnumerable()
                 select new CampaignRevenue()
                 {
                     ID = Convert.ToInt32(rw["ID"]),
                     SDate = Convert.ToDateTime(rw["sTime"]),
                     EDate = Convert.ToDateTime(rw["eTime"]),
                     Total = rw["Total"] == DBNull.Value ? 0 : Convert.ToDouble(rw["Total"]),
                     Date = Convert.ToDateTime(rw["sTime"]).ToString("dd/MM")+" - " + Convert.ToDateTime(rw["eTime"]).ToString("dd/MM")
                 }).ToList();

            return Json(new { rs = true, data = l }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult LoadDistrictStudentCount(int schoolID)
        {
            SqlParameter[] parameters =
              {
                    new SqlParameter("@schoolID", SqlDbType.Int) { Value = schoolID },                
              };
            List<DistrictStudent> l;
            var rs = SqlHelper.ExecuteDataTable("DistrictStudentCount", parameters);
            l = (from rw in rs.AsEnumerable()
                 select new DistrictStudent()
                 {
                     DistrictID = Convert.ToInt32(rw["dis"]),
                     DistrictName = rw["DistrictName"] == DBNull.Value ? "Chưa có" : rw["DistrictName"].ToString(),
                     StudentCount= rw["sl"] == DBNull.Value ? 0 : Convert.ToInt32(rw["sl"])
                 }).ToList();
            List<DistrictStudent> final = new List<DistrictStudent>();
            int get5 = 5;
            int conlai = 0;
            for (int i = 0; i < l.Count; i++)
            {
                DistrictStudent item = l[i];
                if (item.DistrictID == 0)
                {
                    conlai += item.StudentCount;
                }
                else
                {
                    if (get5 != 0)
                    {
                        get5--;
                        final.Add(item);
                    }
                    else
                    {
                        conlai += item.StudentCount;
                    }
                }                 
            }
            final.Add(new DistrictStudent() { DistrictID = 0, DistrictName = "Còn lại", StudentCount = conlai });
            return Json(new { rs = true, data = final }, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult LoadScheduleStudent()
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            var assign = CoursesStudentTable.getbystudentid(aclog.ID);
            List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
            foreach (var item in assign)
            {
                var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                calen.AddRange(t);
            }

            //Cái Schedule mới em tạo ở bên dưới thêm một số chi tiết để đổ data ra cho hợp lý
            List<Schedule> l = new List<Schedule>();
            foreach (var item in calen)
            {
                var te = AccountTable.getbyID(item.TeacherID.Value);
                if (te == null)
                    continue;
                var room = SchoolRoomTable.getbyID(item.SchoolRoomID.Value);
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
                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                var sub = SubjectTable.getbyID(subid);
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }
                Schedule t = new Schedule();
                t.couserID = c.ID;
                t.className = c.ClassName;
                t.start = item.Date.Value.Add(s);
                t.end = item.Date.Value.Add(e);
                t.room = room.RoomName;
                t.subject = sub.SubjectName;
                t.branch = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.url = "/KhoaTest/StudentCourseDetail/" + c.ID + "";
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
            }
            return Json(new { rs = true, l = l }, JsonRequestBehavior.AllowGet);
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
        [HttpPost]
        public ActionResult LoadScheduleCourse(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var calen = CoursesScheduleTable.getbycourseid(id);
            List<Schedule> l = new List<Schedule>();
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

                var room = SchoolRoomTable.getbyID(item.SchoolRoomID.Value);
                Schedule t = new Schedule();
                t.couserID = c.ID;
                t.className = c.ClassName;
                t.teacher = te.FullName;
                t.room = room.RoomName;         
                t.start = item.Date.Value.Add(s);
                t.end = item.Date.Value.Add(e);
                t.allDay = false;
                t.subject = sub.SubjectName;
                t.branch = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.url = url;
                t.borderColor = colorcalenda;

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
               
            }
            var top6 = l.Where(n=>n.start >= GetDateTime.Now).OrderBy(n => n.start).Take(6).ToList();
            return Json(new { rs = true, l = l ,top6 = top6 }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult StudentCourseDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            ViewBag.Course = c;
            return View(c);
        }
        public ActionResult CourseList()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = CoursesStudentTable.getbystudentid(aclog.ID);
            return View(l);
        }
        public ActionResult PartialNextLession(int id)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var calen = CoursesScheduleTable.getbycourseid(id);
            List<Schedule> l = new List<Schedule>();
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

                var room = SchoolRoomTable.getbyID(item.SchoolRoomID.Value);
                Schedule t = new Schedule();
                t.couserID = c.ID;
                t.className = c.ClassName;
                t.teacher = te.FullName;
                t.room = room.RoomName;
                t.start = item.Date.Value.Add(s);
                t.end = item.Date.Value.Add(e);
                t.allDay = false;
                t.subject = sub.SubjectName;
                t.branch = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                t.url = url;
                t.borderColor = colorcalenda;
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

            }
            var top6 = l.Where(n => n.start >= GetDateTime.Now).OrderBy(n => n.start).Take(6).ToList();
            ViewBag.Top6 = top6;
            return View();           
        }
        public ActionResult CourseDocument(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            ViewBag.Course = c;
            return View(c);
        }
        public ActionResult StudentExercise(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            var c = CoursesTable.getbyid(id);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            ViewBag.Course = c;

            return View(c);
        }
        public ActionResult ExrciseDetailEssayMulti(int courseid, int gid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var gex = ExerciseGroupTable.getbyid(gid);
            if (gex == null)
            {
                SetAlert("Không tìm thấy bài tập", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            string score = "N/A";
            int i = 0;
            var ckwork = ExerciseAddonStudentTable.checkworkexercise(aclog.ID, gid);
           var count = 0;
            if (ckwork)
            {
                var le = ExerciseTable.getbygroupid(gid);
                foreach (var item in le)
                {
                    var atrue = ExerciceMultiChoiseTable.getbyanwertrue(item.ID);
                    var astu = ExerciseAddonStudentTable.getbyuidvsexeid(aclog.ID, item.ID);
                    if (atrue.ID == astu.ExerciseMultipleChoiseID)
                        i += 1;
                }
                score = i + "/" + le.Count;
               count += le.Count;
            }
            ViewBag.Score = score;
            ViewBag.Course = c;
            ViewBag.GroupExercise = gex;
            ViewBag.FullName = aclog.FullName;
      
            return View();
        }
        public ActionResult SubmitExrciseDetailEssayMulti(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int gid = f.Get("hdfgroupid").ToInt(0);
            int courseid = f.Get("hdfcourseid").ToInt(0);

            var ckstu = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ckstu == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "KhoaTest");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "KhoaTest");
            }

            var gex = ExerciseGroupTable.getbyid(gid);
            if (gex == null)
            {
                SetAlert("Không tìm thấy bài tập", "e");
                return RedirectToAction("CourseExercise", "KhoaTest", new { id = c.ID });
            }
            string[] data = f.Get("hdfdata").ToString().Split('|');

            bool ck = false;
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] qa = data[i].Split('-');
                var q = ExerciseTable.getbyid(qa[0].ToInt(0));
                var a = ExerciceMultiChoiseTable.getbyid(qa[1].ToInt(0));
                if (q == null || a == null)
                {
                    ck = true;
                    break;
                }
            }
            if (ck)
            {
                SetAlert("Dữ liệu không hợp lệ. Vui lòng thử lại", "e");
                return RedirectToAction("ExrciseDetailEssayMulti", "KhoaTest", new { courseid = c.ID, gid = gex.ID });
            }
            for (int i = 0; i < data.Length - 1; i++)
            {
                string[] qa = data[i].Split('-');
                var q = ExerciseTable.getbyid(qa[0].ToInt(0));
                var anwer = ExerciceMultiChoiseTable.getbyid(qa[1].ToInt(0));
                ExerciseAddonStudentTable.insert(aclog, c, new tbl_SetPackage() { ID = 0, PackageName = "" }, q, "", anwer.AnswerTitle, anwer.ID, 0, aclog.UserName);
            }

            SetAlert("Thành công", "s");
            return RedirectToAction("ExrciseDetailEssayMulti", "KhoaTest", new { courseid = c.ID, gid = gex.ID });
        }
        public ActionResult Notification(int page = 0)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            int pageSize = 50;
            var l = NotificationTable.GetByUID(aclog.ID).OrderBy(n => n.Status).ThenByDescending(n => n.CreatedDate).ToList();
            if (page == 0)
                page = 1;
            return View(l.ToPagedList(page, pageSize));
        }
        public ActionResult ExrciseDetailEssayFix(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài làm", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            if (l.UIDStudent != aclog.ID)
            {
                SetAlert("Bạn không xem được bài sửa", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }
            var lfix = ExerciseAddonStudentFixTable.getbyexestudent(l.ID);
            ViewBag.Fix = lfix;

            var lscore = ExerciseScoreDetailTable.getbyexeid(l.ID);
            ViewBag.Score = lscore;

            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;
            return View(l);
        }

        public ActionResult ExrciseDetailEssay(int courseid, int eid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var ex = ExerciseTable.getbyid(eid);
            if (ex == null)
            {
                SetAlert("Không tìm thấy câu hỏi", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (checkdate == null)
            {
                SetAlert("Bài tập chưa được mở", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }

            ViewBag.Course = c;
            ViewBag.Exercise = ex;
            ViewBag.FullName = aclog.FullName;
            ViewBag.Deadline = checkdate.eTime.Value.ToString("dd/MM/yyyy");
            return View();
        }

        public ActionResult ExerciseDetail(int id)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var l = ExerciseAddonStudentTable.getbyid(id);
            if (l == null)
            {
                SetAlert("Không tìm thấy bài", "e");
                return RedirectToAction("ExerciseList", "TeacherExercise");
            }
            //if (l.StatusDone == 1)
            //    ExerciseAddonStudentTable.updatestatusdone(id, aclog.ID, 2, 0, aclog.UserName);

            var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
            ViewBag.Exercise = ex;
            ViewBag.ExerciseStudent = l;

            return View(l);
        }

        public ActionResult ExrciseDetailAudio(int courseid, int eid)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });

            var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
            if (ck == null)
            {
                SetAlert("Bạn không đăng kí học khóa này", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var c = CoursesTable.getbyid(courseid);
            if (c == null)
            {
                SetAlert("Không tìm thấy khóa học", "e");
                return RedirectToAction("CourseList", "StudentCourse");
            }

            var ex = ExerciseTable.getbyid(eid);
            if (ex == null)
            {
                SetAlert("Không tìm thấy câu hỏi", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }
            var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
            if (checkdate == null)
            {
                SetAlert("Bài tập chưa được mở", "e");
                return RedirectToAction("CourseExrcise", "StudentCourse", new { id = c.ID });
            }

            ViewBag.Course = c;
            ViewBag.Exercise = ex;
            ViewBag.FullName = aclog.FullName;
            ViewBag.Deadline = checkdate.eTime.Value.ToString("dd/MM/yyyy");
            return View();
        }
        //[HttpPost]
        //public ActionResult TestIMG(HttpPostedFileBase txtIMG)
        //{
        //    //string linkIMG = "";
        //    //string linkIMGThumbnails;
        //    //if (txtIMG != null)
        //    //{
        //    //    string name = Path.GetFileNameWithoutExtension(txtIMG.FileName);
        //    //    string ext = Path.GetExtension(txtIMG.FileName);
        //    //    var fileName = name + GetDateTime.Now.ToString("ddmmyy_hhmmss") + ext;
        //    //    var path = Path.Combine(Server.MapPath("~/Upload/TestIMG/"), fileName);
        //    //    linkIMG = "/Upload/TestIMG/" + fileName;
        //    //    txtIMG.SaveAs(path);
        //    //    AssetCRM.GenerateThumbNailNew("~" + linkIMG, "~/Upload/thumbnails/TestIMG/" + fileName, 150, false);
        //    //    linkIMGThumbnails = "/Upload/thumbnails/SetIMG/" + fileName;
        //    //    ModelState.Clear();
        //    //}
        //    //return RedirectToAction("Index", "KhoaTest");
        //}
    }
}