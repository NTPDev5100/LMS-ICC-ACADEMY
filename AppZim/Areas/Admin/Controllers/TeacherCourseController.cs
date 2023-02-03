using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using PagedList;
using System;
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
    public class TeacherCourseController : BaseController
    {
        // GET: Admin/TeacherCourse
        public ActionResult CourseList(int page = 1)
        {
            int pageSize = 9;
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            //Lấy tất cả khóa học giáo viên đó tham gia
            IEnumerable<tbl_TeacherCourse> l = TeacherCourseTable.getbyteacherid(aclog.ID).Where(n => n.isHide != true).OrderByDescending(n => n.OpenDate).ToList();
            //lấy tất cả khóa học
            IEnumerable<tbl_Courses> course = CoursesTable.getall().Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.OpeningDate).ToList();
            //join bản để lấy ra khóa gv dạy           
            course = from c in course
                     join t in l on c.ID equals t.CourseID
                     select c;
            //key serch, bắt theo tên khóa, tên trung tâm và lớp
            if (!string.IsNullOrEmpty(Request.QueryString["search"]))
            {
                string q = Request.QueryString["search"].ToString();
                ViewBag.Search = q;
                l = from t in l
                    join c in course on t.CourseID equals c.ID
                    where AssetCRM.RemoveUnicode(c.CourseName.ToLower()).Contains(AssetCRM.RemoveUnicode(q).ToLower())
                   || AssetCRM.RemoveUnicode(c.SchoolName.ToLower()).Contains(AssetCRM.RemoveUnicode(q).ToLower())
                   || AssetCRM.RemoveUnicode(c.ClassName.ToLower()).Contains(AssetCRM.RemoveUnicode(q).ToLower())
                    select t;
            }
            // trung tâm
            if (!string.IsNullOrEmpty(Request.QueryString["center"]))
            {
                int center = Request.QueryString["center"].ToInt(0);
                ViewBag.Center = center;
                if (center > 0)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        where c.SchoolID == center
                        select t;
                }
            }
            // lớp
            if (!string.IsNullOrEmpty(Request.QueryString["classid"]))
            {
                int classid = Request.QueryString["classid"].ToInt(0);
                ViewBag.ClassID = classid;
                if (classid > 0)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        where c.ClassID == classid
                        select t;
                }
            }
            //trạng thái
            if (!string.IsNullOrEmpty(Request.QueryString["status"]))
            {
                int status = Request.QueryString["status"].ToInt(0);
                ViewBag.StarusID = status;
                if (status > 0)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        where c.Status == status
                        select t;
                }
            }
            // thời gian mở bắt đầu từ
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                string from = Request.QueryString["from"].ToString();
                try
                {
                    string f = Request.QueryString["from"].ToString().Trim();
                    DateTime d = DateTime.ParseExact(f, "dd/MM/yyyy", null);
                    ViewBag.From = d.ToString("dd/MM/yyyy");
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        where c.OpeningDate >= d.Date
                        select t;
                }
                catch { }
            }
            // thời gian bở kết thúc
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                string from = Request.QueryString["to"].ToString();
                try
                {
                    string f = Request.QueryString["to"].ToString().Trim();
                    DateTime d = DateTime.ParseExact(f, "dd/MM/yyyy", null);
                    ViewBag.To = d.ToString("dd/MM/yyyy");
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        where c.OpeningDate <= d.Date
                        select t;
                }
                catch { }
            }
            int sort = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["sort"]))
            {
                sort = Request.QueryString["sort"].ToInt(0);
                if (sort == 1)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.CourseName ascending
                        select t;
                }
                if (sort == 2)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.CourseName descending
                        select t;
                }
                if (sort == 3)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.OpeningDate ascending
                        select t;
                }
                if (sort == 4)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.OpeningDate descending
                        select t;
                }
                if (sort == 5)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.SchoolName ascending
                        select t;
                }
                if (sort == 6)
                {
                    l = from t in l
                        join c in course on t.CourseID equals c.ID
                        orderby c.SchoolName descending
                        select t;
                }
            }
            ViewBag.SortBy = sort;
            return View(l.ToPagedList(page, pageSize));
        }

        #region load lịch dạy
        public ActionResult Schedule()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID == 5)// học viên bị đá ra
                return RedirectToAction("Signin", "Login", new { area = "" });
            ViewBag.m = GetDateTime.Now.ToString("dd/MM/yyyy");
            return View();
        }

        [HttpPost]
        public ActionResult LoadScheduleTeacher(string month, int type)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            DateTime date = DateTime.ParseExact(month, "dd/MM/yyyy", null);
            if (type == 1)// trừ
                date = date.AddMonths(-1);
            if (type == 2)// cộng
                date = date.AddMonths(1);

            var calen = CoursesScheduleTable.getbyteacherid(aclog.ID);
            //var calen = CoursesScheduleTable.GetAll();
            calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();

            List<NewSchedule> l = new List<NewSchedule>();
            foreach (var item in calen)
            {
                var c = CoursesTable.getbyid(item.CourseID.Value);
                if (c == null)
                    continue;
                var school = SchoolTable.getbyID(c.SchoolID.Value);
                var sty = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                if (sty == null)
                    continue;
                int subid = 0;
                if (item.SubjectID != null)
                    subid = item.SubjectID.Value;
                else
                {
                    var cudt = CurriculumDetailTable.getbyID(item.CurriculumsDetailsID.Value);
                    if (cudt != null)
                        subid = cudt.SubjectID.Value;
                }
                var sub = SubjectTable.getbyID(subid);
                if (sub == null)
                {
                    sub = new tbl_Subject();
                    sub.SubjectName = "";
                }

                string[] stu = sty.sTime.Split(':');
                string[] etu = sty.eTime.Split(':');
                TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);

                NewSchedule t = new NewSchedule();
                t.course = c.ID;
                t.title = c.CourseName;
                t.start = item.Date.Value.Add(s).ToString("yyyy-MM-dd HH:mm");
                t.end = item.Date.Value.Add(e).ToString("yyyy-MM-dd HH:mm");
                t.allDay = false;
                t.subject = sub.SubjectName;
                if (c.CourseSelfStudy != true)
                    t.url = "/Admin/CourseDetail/ScheduleCourse/" + c.ID + "";
                else
                    t.url = "/Admin/Course/CourseSelfStudyScheduleDetail/" + c.ID + "";
                t.borderColor = "#3cd629";
                if (!string.IsNullOrEmpty(school.CodeColor))
                    t.borderColor = "#" + school.CodeColor;

                t.school = "";
                t.room = "";
                //t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" data-toggle=\"modal\" data-target=\"#div-edit-schedule\" onclick=\"getScheduleID(" + item.ID + ")\"" +
                //                        " style=\"font-size:10px; padding: 5px;background-color:cadetblue;border-color:cadetblue\">" +
                //                        "<i class=\"fas fa-stop-circle\" style=\"vertical-align:middle\"></i>  Kết thúc</a>";
                //if (item.Date.Value == DateTime.Now.Date)
                //{
                //    if (aclog.RoleID == 4)
                //    {
                //        if (aclog.ID == item.TeacherID.Value) //chỉ cho GV có lịch đó được tạo phòng
                //        {
                //            if (string.IsNullOrEmpty(item.RoomID)) //chỉ hiện nút tạo phòng khi chưa tạo
                //            {
                //                t.school = "<a href=\"/Admin/ZoomMeeting/CreateRoom?CoursesScheduleID=" + item.ID + "\" class=\"btn btn-primary btn-sm\" style=\"font-size:10px; padding: 5px\"><i class=\"fas fa-video\" style=\"vertical-align:middle\"></i>  Tạo phòng mới</a>";
                //            }
                //            else if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass))
                //            {
                var attendance = CoursesAttendanceStudentTable.GetBySchedule(item.CourseID.Value, item.Date.Value.Date, item.StudyTimeID.Value);
                if (attendance == null)
                    attendance = new tbl_CoursesAttendanceStudent() { AttendanceID = 0, LearningID = 0, Note = "" };
                var video = CoursesScheduleFileTable.getbyScheduleID(item.ID);
                if (DateTime.Now >= item.Date.Value.Add(s))
                {
                    t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" data-toggle=\"modal\" data-target=\"#div-edit-schedule\" onclick=\"getScheduleID(" + item.ID + ", " + attendance.AttendanceID + ", " + attendance.LearningID + ", '" + attendance.Note + "', " + item.FisnishStatus + ", " + video.Count + ")\"" +
                                            " style=\"font-size:10px; padding: 5px;background-color:cadetblue;border-color:cadetblue\">" +
                                            "<i class=\"fas fa-stop-circle\" style=\"vertical-align:middle\"></i>  Kết thúc</a>";
                }

                //                else
                //                {
                //                    t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Bắt đầu</a>";
                //                }
                //            }
                //        }
                //    }
                //    else
                //    {
                //        if (!string.IsNullOrEmpty(item.RoomID) && !string.IsNullOrEmpty(item.RoomPass) && item.isRoomStart == true)
                //        {
                //            t.room = "<a href=\"javascript:;\" class=\"btn btn-success btn-sm\" onclick=\"CheckJoinRoom(" + item.ID + ")\" style=\"font-size:10px; padding: 5px\"><i class=\"far fa-play-circle\" style=\"vertical-align:middle\"></i>  Vào phòng học</a>";
                //        }
                //    }
                //}

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
            return Json(new { rs = true, l = l, m = date.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
        }
        #endregion
        public ActionResult TotalLesson(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 4)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            var courseId = -1; //GV truyền -1 để lấy hết, k bị conflict logic vs admin
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                courseId = Request.QueryString["slCourse"].ToInt(0);
            }
            ViewBag.Course = courseId;
            ViewBag.Teacher = aclog.ID;

            //bool? isClosing = null;
            //if (!string.IsNullOrEmpty(Request.QueryString["slisClosing"]))
            //{
            //    isClosing = Request.QueryString["slisClosing"].ToBool();
            //}

            string from = DateTime.Now.FirstDayOfMonth().ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(Request.QueryString["from"]))
            {
                from = DateTime.ParseExact(Request.QueryString["from"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            }
            ViewBag.From = DateTime.ParseExact(from, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            string to = DateTime.Now.LastDayOfMonth().ToString("yyyy-MM-dd");
            if (!string.IsNullOrEmpty(Request.QueryString["to"]))
            {
                to = DateTime.ParseExact(Request.QueryString["to"], "dd/MM/yyyy", null).ToString("yyyy-MM-dd");
            }
            ViewBag.To = DateTime.ParseExact(to, "yyyy-MM-dd", null).ToString("dd/MM/yyyy");

            List<Admin_TeacherSalaryDetail_GetList_Result> l = new List<Admin_TeacherSalaryDetail_GetList_Result>();
            List<GetCourseByTeacherAndSchedule_Result> courses = new List<GetCourseByTeacherAndSchedule_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Admin_TeacherSalaryDetail_GetList(from, to, aclog.ID, courseId, (page - 1) * pageSize, pageSize).ToList();
                courses = db.GetCourseByTeacherAndSchedule(aclog.ID, "", from, to, 0, int.MaxValue).ToList();
            }

            ViewBag.Courses = courses;
            ViewBag.ClosingFrom = DateTime.Now.AddMonths(-1).FirstDayOfMonth().ToString("dd-MM-yyyy");
            ViewBag.ClosingTo = DateTime.Now.AddMonths(-1).LastDayOfMonth().ToString("dd-MM-yyyy");

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
        [HttpPost]
        public string MultiUpload(string id, string fileName)
        {
            var chunkNumber = id;
            //var file = Request.Files;
            //var chunks = file[0].InputStream;
            var chunks = Request.InputStream;
            string path = Server.MapPath("/Upload/VideoSchedule/");
            string newpath = Path.Combine(path, fileName + chunkNumber);
            var chunkThread = chunks;
            using (FileStream fs = System.IO.File.Create(newpath))
            {
                byte[] bytes = new byte[52428800];
                int bytesRead;
                while ((bytesRead = chunkThread.Read(bytes, 0, bytes.Length)) > 0)
                {
                    fs.Write(bytes, 0, bytesRead);
                }
            }
            return "done";
        }
        [HttpPost]
        public string UploadComplete(string fileName, string domain, int scheduleid, string complete)
        {
            string tempPath = Server.MapPath("/Upload/VideoSchedule/");
            string videoPath = Server.MapPath("/Upload/VideoSchedule/");
            string newPath = Path.Combine(tempPath, fileName);
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            var schedule = CoursesScheduleTable.getbyid(scheduleid);
            var studyTime = StudyTimeTable.getbyid(schedule.StudyTimeID ?? 0);
            var link = domain + "/Upload/VideoSchedule/" + fileName;
            Thread upload = new Thread(() =>
            {
                if (complete.Contains("true"))
                {

                    string[] filePaths = Directory.GetFiles(tempPath).Where(p => p.Contains(fileName)).OrderBy(p => Int32.Parse(p.Replace(fileName, "$").Split('$')[1])).ToArray();
                    foreach (string filePath in filePaths)
                    {
                        MergeFiles(newPath, filePath);
                    }
                    CoursesScheduleFileTable.insert(new tbl_CoursesScheduleFile() { CoursesScheduleID = scheduleid, FileName = link, TeacherAttendanceID = 0 }, aclog.UserName);
                    string history = aclog.FullName + " đã upload <a href=\"" + link + "\" target=\"_blank\">Video</a> cho ngày học " + schedule.Date.Value.ToString("dd/MM/yyyy") + " " + studyTime.sTime + " - " + studyTime.eTime;
                    HistoryUploadVideoTable.insert(scheduleid, history, aclog.ID);


                }
                System.IO.File.Move(Path.Combine(tempPath, fileName), Path.Combine(videoPath, fileName));
            });
            upload.Start();
            return "success";
        }

        private static void MergeFiles(string file1, string file2)
        {
            FileStream fs1 = null;
            FileStream fs2 = null;
            try
            {
                fs1 = System.IO.File.Open(file1, FileMode.Append);
                fs2 = System.IO.File.Open(file2, FileMode.Open);
                byte[] fs2Content = new byte[fs2.Length];
                fs2.Read(fs2Content, 0, (int)fs2.Length);
                fs1.Write(fs2Content, 0, (int)fs2.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " : " + ex.StackTrace);
            }
            finally
            {
                if (fs1 != null) fs1.Close();
                if (fs2 != null) fs2.Close();
                System.IO.File.Delete(file2);
            }
        }

        [HttpPost]
        public void NotificationUploadFail(int aclogId, int scheduleid)
        {
            var schedule = CoursesScheduleTable.getbyid(scheduleid);
            var studyTime = StudyTimeTable.getbyid(schedule.StudyTimeID ?? 0);
            NotificationTable.insertsingle("", "Upload file thất bại", $"Upload file thất bại. Ca {studyTime.StudyTimeName + " " + schedule.Date.Value.ToString("dd-MM-yyyy")}", aclogId, "admin", 0, 0);
        }
        //public JsonResult UploadFileChunks()
        //{
        //    var files = Request.Files;
        //    if (files.Count > 0)
        //    {
        //        try
        //        {
        //            Task.Factory.StartNew(() =>
        //            {
        //                for (var i = 0; i < files.Count; i++)
        //                {
        //                    string filePath = Path.Combine(GetUploadPath(), files[i].FileName);

        //                    using (FileStream fs = new FileStream(filePath, FileMode.Append))
        //                    {
        //                        var bytes = GetBytes(files[i].InputStream);
        //                        fs.Write(bytes, 0, bytes.Length);
        //                    }
        //                }
        //            });

        //            return Json(new { status = true });
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { status = false, message = ex.Message });
        //        }
        //    }

        //    return Json(new { status = false });
        //}

        //private byte[] GetBytes(Stream input)
        //{
        //    byte[] buffer = new byte[input.Length];
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        int read;
        //        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        //        {
        //            ms.Write(buffer, 0, read);
        //        }

        //        return ms.ToArray();
        //    }
        //}

        //private string GetUploadPath()
        //{
        //    var rootPath = System.Web.Hosting.HostingEnvironment.MapPath("/Upload/VideoSchedule/");

        //    if (!Directory.Exists(rootPath))
        //    {
        //        Directory.CreateDirectory(rootPath);
        //    }

        //    return rootPath;
        //}

        [HttpPost]
        public ActionResult AttendanceAllStudent(int status, int attendance, int learning, string note, int scheduleid)
        {
            try
            {
                if (Session["UID"] == null)
                    return RedirectToAction("Signin", "Login", new { area = "" });
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                if (aclog.RoleID != 4)
                    return RedirectToAction("Signin", "Login", new { area = "" });

                //var status = 1;
                //if (!string.IsNullOrEmpty(f.Get("dll-language")))
                //{
                //    status = f.Get("dll-language").ToInt(1);
                //}

                //var attendance = 1;
                //if (!string.IsNullOrEmpty(f.Get("slAttendance")))
                //{
                //    attendance = f.Get("slAttendance").ToInt(1);
                //}

                //var learning = 1;
                //if (!string.IsNullOrEmpty(f.Get("slLearning")))
                //{
                //    learning = f.Get("slLearning").ToInt(1);
                //}

                //var note = "";
                //if (!string.IsNullOrEmpty(f.Get("txt-note")))
                //{
                //    note = f.Get("txt-note").ToString();
                //}

                //var scheduleid = f.Get("hdscheduleid").ToInt(0);
                Thread tattendance = new Thread(() => { AttendanceAllStudent(scheduleid, attendance, learning, note, aclog); });
                tattendance.Start();
                var schedule = CoursesScheduleTable.getbyid(scheduleid);
                var studyTime = StudyTimeTable.getbyid(schedule.StudyTimeID ?? 0);
                //SaveVideo(aclog, scheduleid, schedule, studyTime, fileuploadaudio);
                var scheduleBefore = CoursesScheduleTable.getbyid(scheduleid);
                schedule = CoursesScheduleTable.updateteacherattendance(scheduleid, aclog.ID, status, aclog.UserName);
                ///cập nhật lương chấm bài
                if (scheduleBefore.TeacherAttendanceID == 0 && scheduleBefore.FisnishStatus == 0 && status != 3)
                {
                    using (var db = new ZimEntities())
                    {
                        double salary = 0;
                        int type = 1;
                        var teacher = AccountTable.getbyID(schedule.TeacherID ?? 0);
                        if (teacher != null)
                        {
                            var countschedule = db.tbl_CoursesSchedule
                                .Where(x => x.CourseID == schedule.CourseID && x.IsHide == false && x.ID <= schedule.ID)
                                .ToList();
                            int totalLesson = countschedule.Count();
                            var salaryList = CourseSalaryTable.getbycourse(schedule.CourseID ?? 0);

                            foreach (var i in salaryList)
                            {
                                if (totalLesson <= 0)
                                {
                                    break;
                                }

                                salary = (i.SalaryOfLesson ?? 0);
                                totalLesson -= (i.NumberOfLesson ?? 0);
                            }

                            if (salary == 0)
                            {
                                salary = salaryList.FirstOrDefault().SalaryOfLesson ?? 0;
                            }

                            if (status == 4)
                            {
                                type = 2;
                                salary = salary / 2;
                            }

                            var insertTeacherSalaryDetail = TeacherSalaryDetailTable.insert(new tbl_TeacherSalaryDetail
                            {
                                CourseID = schedule.CourseID,
                                CoursesScheduleID = schedule.ID,
                                TeacherID = teacher.ID,
                                Salary = salary,
                                Date = schedule.Date,
                                Type = type,//1 lương bình thường, 2 lương học viên off = 1/2 lương thường
                            }, aclog.UserName);
                        }
                    }
                }
                return Json(new { rs = true }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { rs = false }, JsonRequestBehavior.AllowGet);
            }
        }
        #region ngày nghỉ giáo viên
        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        public ActionResult DayOffTeacher()
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 4)// khác giáo viên là bị đá
                return RedirectToAction("Signin", "Login", new { area = "" });
            ViewBag.m = GetDateTime.Now.ToString("dd/MM/yyyy");

            DateTime monday = StartOfWeek(GetDateTime.Now.Date, DayOfWeek.Monday);
            var time = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slTime"]))
                time = Request.QueryString["slTime"].ToInt(0);
            ViewBag.Time = time;
            var lstudy = StudyTimeTable.getall().Where(x => x.isHide == false && (x.TimeOfDay == time || time == 0)).OrderBy(n => n.sTime).ToList();
            // kiem tra xem nghi full buoi trong tuan khong
            string sang = KiemTraFullBuoi(aclog.ID, 1, monday) == true ? "checked" : "";
            string chieu = KiemTraFullBuoi(aclog.ID, 2, monday) == true ? "checked" : "";
            string toi = KiemTraFullBuoi(aclog.ID, 3, monday) == true ? "checked" : "";
            StringBuilder boxx = new StringBuilder();
            boxx.Append("<div class=\"d-flex align-items-baseline\">" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + sang + " data-timeofday=\"1\"/> :Sáng&nbsp;" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + chieu + " data-timeofday=\"2\"/> :Chiều&nbsp;" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + toi + " data-timeofday=\"3\"/> :Tối&nbsp;" +
                        "</div>");
            StringBuilder html = new StringBuilder();
            html.Append("<tr>");
            html.Append("   <th>Ca" + boxx + "</th>");
            for (int d = 0; d < 7; d++)
            {
                DateTime dt = monday.AddDays(d);
                html.Append("   <th>" + dt.ToString("dddd") + " " + dt.ToString("dd/MM") + "</th>");
            }
            html.Append("</tr>");

            foreach (var item in lstudy)
            {
                html.Append("   <td>" + item.sTime + " " + item.eTime + "</td>");
                for (int d = 0; d < 7; d++)
                {
                    DateTime day = monday.AddDays(d);
                    string[] stu = item.sTime.Split(':');
                    string[] etu = item.eTime.Split(':');
                    TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                    TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);
                    CheckSchedule t = new CheckSchedule();
                    var check = DayOffTeacherTable.getbyuiddate(aclog.ID, day.Date).Where(n => n.StudyTimeID == item.ID && n.isHide != true).FirstOrDefault();
                    if (check == null)
                        t.checkday = false;
                    else
                        t.checkday = true;

                    var daydisable = CoursesScheduleTable.checkteacher(aclog.ID, day.Date, item.ID, 0);

                    if (!daydisable.checktrue)// đã có lịch
                    {
                        if (check != null)
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked \" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                        else
                            html.Append("   <td></td>");
                    }
                    else
                    {
                        if (check == null)
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                        else
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                    }
                }
                html.Append("</tr>");
            }
            ViewBag.html = html.ToString();
            ViewBag.Monday = monday.ToString("dd/MM/yyyy");
            return View();
        }

        public bool KiemTraFullBuoi(int uid, int buoi, DateTime monday)
        {
            var check = false;
            using (var db = new ZimEntities())
            {
                var ltimestudy = db.tbl_StudyTime.Where(x => x.TimeOfDay == buoi && x.isHide != true).ToList();
                int TongSoBuoi = ltimestudy.Count * 7;// tong so buoi phai check trong tuan
                var l = (from d in DayOffTeacherTable.GetAllInWeek(uid, monday, monday.AddDays(6))
                         join li in ltimestudy on d.StudyTimeID equals li.ID
                         where li.TimeOfDay == buoi && d.isHide != true
                         select d).ToList().Count;
                if (TongSoBuoi == l)
                {
                    check = true;
                }
            }
            return check;
        }

        public bool CheckMondayToSunday(int uid, int studytimeid, DateTime monday)
        {
            var check = false;
            int dem = 0;
            using (var db = new ZimEntities())
            {
                for (var d = 0; d < 7; d++)
                {
                    var dt = monday.AddDays(d).Date;
                    var buoi = db.tbl_DayOffTeacher.Where(x => x.isHide != true && x.UID == uid && x.StudyTimeID == studytimeid && x.DayOff == dt).FirstOrDefault();
                    if (buoi != null)// co lich nghi
                    {
                        dem++;
                    }
                }
            }
            if (dem == 0 || dem == 7)
            {
                check = true;
            }
            return check;
        }

        [HttpPost]
        public ActionResult LoadDayOff(string month, int type)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            DateTime date = DateTime.ParseExact(month, "dd/MM/yyyy", null);
            if (type == 1)// trừ
                date = date.AddDays(-7);
            if (type == 2)// cộng
                date = date.AddDays(7);

            var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            var studys = StudyTimeTable.getall().OrderBy(n => n.eTime).ToList();

            DateTime monday = StartOfWeek(date.Date, DayOfWeek.Monday);
            var lstudy = StudyTimeTable.getall().OrderBy(n => n.sTime).ToList();
            // kiem tra xem nghi full buoi trong tuan khong
            string sang = KiemTraFullBuoi(aclog.ID, 1, monday) == true ? "checked" : "";
            string chieu = KiemTraFullBuoi(aclog.ID, 2, monday) == true ? "checked" : "";
            string toi = KiemTraFullBuoi(aclog.ID, 3, monday) == true ? "checked" : "";
            StringBuilder boxx = new StringBuilder();
            boxx.Append("<div class=\"d-flex align-items-baseline\">" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + sang + " data-timeofday=\"1\"/> :Sáng&nbsp;" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + chieu + " data-timeofday=\"2\"/> :Chiều&nbsp;" +
                        "<input class=\"timeofday\" type=\"checkbox\" data-monday=\"" + monday.ToString("dd/MM/yyyy") + "\" " + toi + " data-timeofday=\"3\"/> :Tối&nbsp;" +
                        "</div>");
            StringBuilder html = new StringBuilder();
            html.Append("<tr>");
            html.Append("   <th>Ca" + boxx + "</th>");
            for (int d = 0; d < 7; d++)
            {
                DateTime dt = monday.AddDays(d);
                html.Append("   <th>" + dt.ToString("dddd") + " " + dt.ToString("dd/MM") + "</th>");
            }
            html.Append("</tr>");

            foreach (var item in lstudy)
            {
                html.Append("   <td>" + item.sTime + " " + item.eTime + "</td>");
                for (int d = 0; d < 7; d++)
                {
                    DateTime day = monday.AddDays(d);
                    string[] stu = item.sTime.Split(':');
                    string[] etu = item.eTime.Split(':');
                    TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                    TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);
                    CheckSchedule t = new CheckSchedule();
                    var check = DayOffTeacherTable.getbyuiddate(aclog.ID, day.Date).Where(n => n.StudyTimeID == item.ID && n.isHide != true).FirstOrDefault();
                    if (check == null)
                        t.checkday = false;
                    else
                        t.checkday = true;

                    var daydisable = CoursesScheduleTable.checkteacher(aclog.ID, day.Date, item.ID, 0);

                    //if (!daydisable.checktrue)// đã có lịch
                    //{
                    //    if (check != null)
                    //        html.Append("   <td><label><input type=\"checkbox\" class=\"filled-in dayoff-checked\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span></span></label></td>");
                    //    else
                    //        html.Append("   <td></td>");
                    //}
                    //else
                    //{
                    //    if (check == null)
                    //        html.Append("   <td><label><input type=\"checkbox\" class=\"filled-in dayoff-checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span></span></label></td>");
                    //    else
                    //        html.Append("   <td><label><input type=\"checkbox\" class=\"filled-in dayoff-checked\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span></span></label></td>");
                    //}
                    if (!daydisable.checktrue)// đã có lịch
                    {
                        if (check != null)
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                        else
                            html.Append("   <td></td>");
                    }
                    else
                    {
                        if (check == null)
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                        else
                            html.Append("   <td><label class='lb-contrainer'><input type=\"checkbox\" class=\"filled-in dayoff-checked\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/><span class='checkmark'></span></label></td>");
                    }
                }
                html.Append("</tr>");
            }
            return Json(new { rs = true, l = html.ToString(), m = date.ToString("dd/MM/yyyy") }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult TeacherOffInTimeOfDay(int timeofday, int checkoff, string strMonday)
        {
            try
            {
                bool hide = checkoff == 1 ? true : false;
                var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
                using (var db = new ZimEntities())
                {
                    var studyTimeList = StudyTimeTable.getByTimeOfDay(timeofday);
                    var monday = StartOfWeek(DateTime.ParseExact(strMonday, "dd/MM/yyyy", null), DayOfWeek.Monday);
                    for (var i = 0; i < 7; i++)
                    {
                        var tempDate = monday.AddDays(i);
                        Parallel.ForEach(studyTimeList, j =>
                        {
                            var _db = new ZimEntities();
                            var temp = _db.tbl_DayOffTeacher.Where(x => x.DayOff == tempDate && x.StudyTimeID == j.ID && x.UID == aclog.ID).FirstOrDefault();
                            if (temp != null)
                            {
                                temp.isHide = !temp.isHide;
                                _db.SaveChanges();
                            }
                            else
                            {
                                var data = new tbl_DayOffTeacher()
                                {
                                    DayOff = tempDate,
                                    isHide = false,
                                    StudyTimeID = j.ID,
                                    UID = aclog.ID,
                                    CreatedBy = aclog.UserName,
                                    CreatedDate = DateTime.Now
                                };
                                _db.tbl_DayOffTeacher.Add(data);
                                _db.SaveChanges();
                            }
                        });
                    }
                }
                return Json(new { rs = 1 }, JsonRequestBehavior.AllowGet);// ok
            }
            catch (Exception e)
            {
                return Json(new { rs = 0 }, JsonRequestBehavior.AllowGet);// loi
            }
        }
        [HttpPost]
        public ActionResult TeacherOff(int studyid, string dateoff)
        {
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            DateTime date = DateTime.ParseExact(dateoff, "dd/MM/yyyy", null);
            DateTime monday = StartOfWeek(date, DayOfWeek.Monday);
            int reload = 0;
            date = date.Date;
            var checkday = CoursesScheduleTable.getbyteacherid(aclog.ID).Where(n => n.Date == date && n.IsHide != true && n.StudyTimeID == studyid).FirstOrDefault();
            var daydisable = CoursesScheduleTable.checkteacher(aclog.ID, date.Date, studyid, 0);
            if (checkday == null && daydisable.checktrue)
            {
                var ck = DayOffTeacherTable.getbyuiddate(aclog.ID, date.Date).Where(n => n.StudyTimeID == studyid).FirstOrDefault();
                if (ck == null)
                {
                    DayOffTeacherTable.insert(aclog.ID, date.Date, studyid, aclog.UserName);
                    if (CheckMondayToSunday(aclog.ID, studyid, monday))
                    {
                        reload = 1;
                    }
                    return Json(new { rs = 1, reload = reload }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rs = DayOffTeacherTable.update(ck.ID, !ck.isHide.Value, aclog.UserName);
                    if (rs.isHide.Value)
                    {
                        if (CheckMondayToSunday(aclog.ID, studyid, monday))
                        {
                            reload = 1;
                        }
                        return Json(new { rs = 3, reload = reload }, JsonRequestBehavior.AllowGet);
                    }
                    if (CheckMondayToSunday(aclog.ID, studyid, monday))
                    {
                        reload = 1;
                    }
                    return Json(new { rs = 1, reload = reload }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (!daydisable.checktrue)
                {
                    var ck = DayOffTeacherTable.getbyuiddate(aclog.ID, date.Date).Where(n => n.StudyTimeID == studyid).FirstOrDefault();
                    if (ck != null)
                    {
                        var rs = DayOffTeacherTable.update(ck.ID, !ck.isHide.Value, aclog.UserName);
                        if (rs.isHide.Value)
                        {
                            if (CheckMondayToSunday(aclog.ID, studyid, monday))
                            {
                                reload = 1;
                            }
                            return Json(new { rs = 3, reload = reload }, JsonRequestBehavior.AllowGet);
                        }
                        if (CheckMondayToSunday(aclog.ID, studyid, monday))
                        {
                            reload = 1;
                        }
                        return Json(new { rs = 1, reload = reload }, JsonRequestBehavior.AllowGet);
                    }
                }
                if (CheckMondayToSunday(aclog.ID, studyid, monday))
                {
                    reload = 1;
                }
                return Json(new { rs = 2, reload = reload }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult CopyDayOff(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));

            DateTime monday = StartOfWeek(DateTime.ParseExact(f.Get("txt-date").ToString(), "d/M/yyyy", null).Date, DayOfWeek.Monday);
            DateTime sunday = monday.AddDays(7);

            var l = DayOffTeacherTable.getbyuid(aclog.ID).Where(n => n.isHide != true && monday.Date <= n.DayOff.Value.Date && n.DayOff.Value.Date <= sunday.Date).OrderBy(n => n.DayOff).ToList();
            int week = f.Get("txt-week-copy").ToInt(0);
            foreach (var item in l)
            {
                for (int i = 1; i <= week; i++)
                {
                    try
                    {
                        DateTime day = item.DayOff.Value.AddDays(i * 7);
                        var daydisable = CoursesScheduleTable.checkteacher(aclog.ID, day.Date, item.StudyTimeID.Value, 0);
                        if (daydisable.checktrue)
                        {
                            DayOffTeacherTable.insert(aclog.ID, day.Date, item.StudyTimeID.Value, aclog.UserName);
                        }
                    }
                    catch { continue; }
                }
            }
            SetAlert("Thành công", "s");
            return RedirectToAction("DayOffTeacher", "TeacherCourse");
        }
        #endregion
        //public static void UpdateSalary(tbl_CoursesSchedule model)
        //{
        //    var course = CoursesTable.getbyid(model.CourseID.Value);
        //    var scheduleList = CoursesScheduleTable.getbycourseid(model.CourseID.Value).OrderBy(x => x.Date).ToList();
        //    var salaryFirst = scheduleList.Take(course.NumberOfLessonFirst.Value).Select(x => x.ID).ToArray();//các buổi học tính lương 1
        //    var salarySecond = scheduleList.Skip(course.NumberOfLessonFirst.Value).Select(x => x.ID).ToArray();//các buổi học tính lương 2
        //    if (Array.IndexOf(salaryFirst, Convert.ToInt32(model.ID)) != -1)//Kiểm tra xem buổi học có nằm trong loại lương 1 k 
        //    {
        //        var teacherSalary = TeacherSalaryTable.GetByCourseID(model.CourseID.Value, DateTime.Now.Month, DateTime.Now.Year);
        //        if (teacherSalary == null)
        //        {
        //            var teacher = TeacherCourseTable.GetByCourseID(course.ID);
        //            teacherSalary = TeacherSalaryTable.insert(new tbl_TeacherSalary()
        //            {
        //                CourseID = course.ID,
        //                TeacherID = teacher.TeacherUID,
        //                Bonus = 0,
        //                NumberOfFirstSalary = 0,
        //                NumberOfSecondSalary = 0,
        //                Status = 0,
        //                CreatedDate = DateTime.Now,
        //                ApprovedBy = "",
        //                ApprovedDate = null
        //            });
        //        }
        //        TeacherSalaryTable.update(new tbl_TeacherSalary()
        //        {
        //            ID = teacherSalary.ID,
        //            NumberOfFirstSalary = teacherSalary.NumberOfFirstSalary + 1,
        //            NumberOfSecondSalary = teacherSalary.NumberOfSecondSalary,
        //            Bonus = teacherSalary.Bonus,
        //            Status = teacherSalary.Status,
        //            ApprovedBy = teacherSalary.ApprovedBy,
        //            ApprovedDate = teacherSalary.ApprovedDate
        //        });
        //    }
        //    else
        //    {
        //        var teacherSalary = TeacherSalaryTable.GetByCourseID(model.CourseID.Value, DateTime.Now.Month, DateTime.Now.Year);
        //        TeacherSalaryTable.update(new tbl_TeacherSalary()
        //        {
        //            ID = teacherSalary.ID,
        //            NumberOfFirstSalary = teacherSalary.NumberOfFirstSalary,
        //            NumberOfSecondSalary = teacherSalary.NumberOfSecondSalary + 1,
        //            Bonus = teacherSalary.Bonus,
        //            Status = teacherSalary.Status,
        //            ApprovedBy = teacherSalary.ApprovedBy,
        //            ApprovedDate = teacherSalary.ApprovedDate
        //        });
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult> UpdateReasonSalary(FormCollection f)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });

            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 1 && aclog.RoleID != 2 && aclog.RoleID != 4)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int id = 0;
            if (!string.IsNullOrEmpty(f.Get("hdid")))
            {
                id = f.Get("hdid").ToInt(0);
            }

            var teacherSalary = TeacherSalaryDetailTable.getbyid(id);
            if (teacherSalary == null)
            {
                SetAlert("Không tìm thấy thông tin", "e");
                return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
            }

            int status = 0;
            if (!string.IsNullOrEmpty(f.Get("slStatus")))
            {
                status = f.Get("slStatus").ToInt(0);
            }

            string reason = "";
            if (!string.IsNullOrEmpty(f.Get("txt-reason")))
            {
                reason = f.Get("txt-reason").ToString();
            }

            double salary = teacherSalary.Salary ?? 0;
            if (!string.IsNullOrEmpty(f.Get("txt-salary")))
            {
                salary = Convert.ToDouble(f.Get("txt-salary").ToString());
            }

            var rs = await TeacherSalaryDetailTable.UpdateReason(id, reason, aclog.UserName, status, salary);
            if (rs != null)
            {
                SetAlert("Thành công", "s");
            }
            else
            {
                SetAlert("Không tìm thấy thông tin", "e");
            }

            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);
        }

        public static void AttendanceAllStudent(int id, int attendance, int learning, string note, tbl_Account aclog)
        {
            string attname = "";
            if (attendance == 1)
                attname = "Có";
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
            var l = CoursesScheduleTable.getbyid(id);
            var cou = CoursesTable.getbyid(l.CourseID.Value);
            var studentcourse = CoursesStudentTable.getbycourseid(cou.ID);
            foreach (var s in studentcourse)
            {
                var cus = AccountTable.getbyID(s.StudentUID ?? 0);
                if (cus != null)
                {
                    var ck = CoursesAttendanceStudentTable.checkadd(l.CourseID.Value, cus.ID, l.Date.Value.Date, l.StudyTimeID.Value);
                    if (ck == null)
                    {

                        var school = SchoolTable.getbyID(cou.SchoolID.Value);
                        tbl_Account lead = new tbl_Account();
                        lead.ID = 0;
                        lead.FullName = "";
                        if (cou.TeacherLeaderUID != null)
                        {
                            lead.ID = cou.TeacherLeaderUID ?? 0;
                            lead.FullName = cou.TeacherLeaderName;
                        }
                        CoursesAttendanceStudentTable.insert(school, cou, cus, l.StudyTimeID.Value, l.Date.Value.Date, attendance, attname, learning, learingname, note, aclog.UserName, war, aclog.FullName, lead);
                        string history = "Học viên " + cus.FullName + " được điểm danh: " + attname + ", đánh giá: " + learingname + ", nhận xét: " + note + " bởi " + aclog.RoleName + " - " + aclog.FullName;
                        HistoryAttendanceTable.insert(cus.ID, l.ID, history, aclog.ID);
                        NotificationTable.insertsingle("", "Điểm danh thành công", "Bạn vừa được điểm danh: " + attname + ", đánh giá: " + learingname + ", Nhận xét: \"" + note, cus.ID, aclog.UserName, 2, 0);
                    }
                    else
                    {
                        CoursesAttendanceStudentTable.update(ck.ID, attendance, attname, learning, learingname, note, war, aclog.UserName);
                        string history = "Học viên " + cus.FullName + " được cập nhật điểm danh: " + attname + ", đánh giá: " + learingname + ", nhận xét: " + note + " bởi " + aclog.RoleName + " - " + aclog.FullName;
                        HistoryAttendanceTable.insert(cus.ID, l.ID, history, aclog.ID);
                    }
                }
            }
        }

        public ActionResult FeedbackList(int page = 1)
        {
            if (Session["UID"] == null)
                return RedirectToAction("Signin", "Login", new { area = "" });
            var aclog = AccountTable.getbyID(Session["UID"].ToString().ToInt(0));
            if (aclog.RoleID != 4)
                return RedirectToAction("Signin", "Login", new { area = "" });

            int pageSize = 20;
            var courseId = 0;
            if (!string.IsNullOrEmpty(Request.QueryString["slCourse"]))
            {
                courseId = Request.QueryString["slCourse"].ToInt(0);
            }
            ViewBag.Course = courseId;

            List<Teacher_GetListFeedback_Result> l = new List<Teacher_GetListFeedback_Result>();
            using (var db = new ZimEntities())
            {
                l = db.Teacher_GetListFeedback(aclog.ID, (page - 1) * pageSize, pageSize, courseId).ToList();
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
}
