using AppZim.api;
using AppZim.Areas.Admin.Controllers;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.api.course
{
    public class courseController : ApiController
    {
        public string getToken()
        {
            try
            {
                IEnumerable<string> headerValues = Request.Headers.GetValues("token");
                return headerValues.FirstOrDefault();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [HttpGet]
        public HttpResponseMessage getListExerciseGroup(int courseID, int page = 1)
        {
            //
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID == 14)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var course = CoursesTable.getbyid(courseID);
                var curriculum = 0;
                int pageSize = 10;
                if (course != null)
                    curriculum = course.CurriculumsID.Value;

                if (aclog.RoleID == 5)
                {
                    var l = new List<API_ExerciseGroup_getlist_Result>();
                    using (var db = new ZimEntities())
                    {
                        l = db.API_ExerciseGroup_getlist(aclog.ID, courseID, curriculum, (page - 1) * pageSize, pageSize).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
                else
                {
                    var l = new List<API_ExerciseGroup_getListForTeacher_Result>();
                    using (var db = new ZimEntities())
                    {
                        l = db.API_ExerciseGroup_getListForTeacher(aclog.ID, courseID, curriculum, (page - 1) * pageSize, pageSize).ToList();
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListExercise(int groupID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var exerciseList = new List<ExerciseList>();
                var l = new List<API_Exercise_getlist_byGroupID_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Exercise_getlist_byGroupID(aclog.ID, groupID).ToList();
                }
                foreach (var i in l)
                {
                    var exercise = new ExerciseList();
                    exercise.ID = i.ID;
                    exercise.ExerciseGroupID = i.ExerciseGroupID.Value;
                    exercise.ExerciseTitle = i.ExerciseTitle;
                    exercise.FileName = i.FileName;
                    exercise.Type = i.Type.Value;
                    exercise.CreatedDate = i.CreatedDate.Value.ToString("dd/MM/yyyy");
                    exercise.CreatedBy = AccountTable.getbyusername(i.CreatedBy).FullName;
                    exercise.HaveDeadline = false;
                    exercise.LinkFile = i.LinkFile;
                    exercise.HaveDeadline = i.HaveDeadline == 0 ? false : true;
                    exercise.IsDone = i.IsDone == 0 ? false : true;
                    exercise.IsRemark = i.Remarked == 0 ? false : true;
                    exerciseList.Add(exercise);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = exerciseList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getExerciseDetail(int id, int type)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                if (type == 5 || type == 6 || type == 7)
                {
                    var exercise = ExerciseTable.getbyid(id);
                    DataExercise ex = new DataExercise();
                    ex.Question = ExerciseGroupTable.getbyid(exercise.ExerciseGroupID.Value).SetPackageQuestion;
                    ex.QuestionList = ExerciseTable.getbygroupid(exercise.ExerciseGroupID.Value);
                    List<API_answer_getListByGroupID_Result> l = new List<API_answer_getListByGroupID_Result>();
                    using (var db = new ZimEntities())
                    {
                        l = db.API_answer_getListByGroupID(exercise.ExerciseGroupID.Value).ToList();
                    }
                    ex.AnswerList = l;
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = ex, message = ApiMessage.Success });
                }
                else
                {
                    var temp = ExerciseTable.getbyid(id);
                    var exercise = new tbl_Exercise();
                    exercise.ID = temp.ID;
                    exercise.ExerciseGroupID = temp.ExerciseGroupID;
                    exercise.ExerciseTitle = temp.ExerciseTitle;
                    exercise.FileName = temp.FileName;
                    exercise.LinkFile = Media.Host + temp.LinkFile;
                    exercise.Type = temp.Type;
                    exercise.ExerciseAddonPrice = temp.ExerciseAddonPrice;
                    exercise.WordLimit = temp.WordLimit;
                    exercise.isHide = temp.isHide;
                    exercise.CreatedDate = temp.CreatedDate;
                    exercise.CreatedBy = temp.CreatedBy;
                    exercise.ModifiedDate = temp.ModifiedDate;
                    exercise.ModifiedBy = temp.ModifiedBy;
                    exercise.TopicID = temp.TopicID;
                    exercise.PartNumber = temp.PartNumber;
                    exercise.ParentQuestionID = temp.ParentQuestionID;
                    var answer = ExerciceMultiChoiseTable.getbyexeid(id);
                    return Request.CreateResponse(HttpStatusCode.OK, new { exercise, answer, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListClasses()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var classes = ClassTable.getall().Where(n => n.isHide != true).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = classes, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListCenters()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var schools = SchoolTable.getall().Where(n => n.CityID == aclog.LocationID).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = schools, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListStudentCourses()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var l = new List<API_Student_getListCourse_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Student_getListCourse(aclog.ID).ToList();
                }
                var list = new List<CourseInformation>();
                var studytime = StudyTimeTable.getall();
                foreach (var i in l)
                {
                    var c = new CourseInformation();
                    c.CourseID = i.CourseID.Value;
                    c.CourseStudentID = i.CourseStudentID.Value;
                    c.CourseName = i.CourseName;
                    c.Status = i.Status.Value;
                    c.StatusName = i.StatusName;
                    c.TeacherLeaderName = i.TeacherLeaderName;
                    c.Price = i.Price.Value;
                    c.TotalPaid = i.TotalPaid.Value;
                    c.PriceLeft = i.PriceLeft.Value;
                    c.OpeningDate = i.OpeningDate;
                    c.SchoolName = i.SchoolName;
                    var s = new API_Student_getStudyTimeByCourseID_Result();
                    using (var db = new ZimEntities())
                    {
                        s = db.API_Student_getStudyTimeByCourseID(i.CourseID.Value).FirstOrDefault();
                    }
                    if (s != null)
                    {
                        c.StudyTime = s.StudyTime;
                        c.TeacherName = s.TeacherName;
                    }
                    c.EndDate = CoursesScheduleTable.getbycourseid(i.CourseID.Value).OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefault().Value.ToString("dd/MM/yyyy");
                    c.OpeningDate = i.OpeningDate;
                    c.TotalStudent = i.TotalStudent.Value;
                    c.TotalStudyDay = i.TotalStudyDay.Value;
                    list.Add(c);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListTeacherCourses(string search, int centerID, int classID, int status, string fdate,
            string tdate, int sort, int page)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pagesize = 10;
                List<API_getListTeacherCourses_Result> l = new List<API_getListTeacherCourses_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_getListTeacherCourses(aclog.ID, search, centerID, classID, status, fdate, tdate, sort,
                        (page - 1) * pagesize, pagesize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getCourseDetail(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var course = new API_getCourseDetail_Result();
                using (var db = new ZimEntities())
                {
                    course = db.API_getCourseDetail(courseID).FirstOrDefault();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = course, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }


        [HttpGet]
        public HttpResponseMessage getExistsVideoSchedule(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            try
            {
                List<API_GetExistsVideoSchedule_Result> videoScheduleList = new List<API_GetExistsVideoSchedule_Result>();
                using (var db = new ZimEntities())
                {
                    videoScheduleList = db.API_GetExistsVideoSchedule(courseID).ToList();
                    //videoScheduleList = l.Select(x => new VideoSchedule()
                    //{
                    //    ID = x.ID,
                    //    CourseID = x.CourseID,
                    //    SubjectID = x.SubjectID,
                    //    CurriculumsDetailsID = x.CurriculumsDetailsID,
                    //    StudyTimeID = x.StudyTimeID,
                    //    SubjectName = x.SubjectName,
                    //    TimeName = x.TimeName,
                    //    Date = x.Date,
                    //    Files = x.Files
                    //}).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = videoScheduleList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        public HttpResponseMessage createTimeLine()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int courseid = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                var note = HttpContext.Current.Request.Form.Get("note");
                var date = HttpContext.Current.Request.Form.Get("date");
                if (courseid == 0 && string.IsNullOrEmpty(note) && string.IsNullOrEmpty(date))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var dateTimeLine = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:ss", null);
                var rs = CoursesNoteTable.insertByAPI(courseid, note, dateTimeLine, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = rs, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage editTimeLine()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int timeLineID = HttpContext.Current.Request.Form.Get("timeLineID").ToInt(0);
                var note = HttpContext.Current.Request.Form.Get("note");
                var date = HttpContext.Current.Request.Form.Get("date");
                if (timeLineID == 0 && string.IsNullOrEmpty(note) && string.IsNullOrEmpty(date))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var dateTimeLine = DateTime.ParseExact(date, "dd/MM/yyyy HH:mm:ss", null);
                var rs = CoursesNoteTable.updateByAPI(timeLineID, note, dateTimeLine, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = rs, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getTimeLine(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var rs = CoursesNoteTable.getbycourseid(courseID).OrderByDescending(x => x.ID).ToList();
                var list = new List<Timeline>();
                foreach (var i in rs)
                {
                    var t = new Timeline();
                    t.ID = i.ID;
                    t.CourseID = i.CourseID.Value;
                    t.Note = i.Note;
                    t.CreatedDate = i.CreatedDate;
                    var acc = AccountTable.getbyusername(i.CreatedBy);
                    if (acc != null)
                    {
                        t.CreatedBy = acc.FullName;
                        t.RoleName = acc.RoleName;
                    }
                    list.Add(t);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message + " - " + ex.InnerException });
            }
        }
        [HttpGet]
        public HttpResponseMessage getStudentList(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var list = new List<API_Teacher_getListStudent_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_Teacher_getListStudent(courseID).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage LoadLessonInCourse(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var c = CoursesTable.getbyid(courseID);
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                if (c.CourseSelfStudy == true)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
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
                List<Lesson> lessonList = new List<Lesson>();

                foreach (var item in sche)
                {
                    foreach (var jca in lca)
                    {
                        var lesson = new Lesson();
                        if (item.Value.Date == GetDateTime.Now.Date)
                        {
                            //l.Append("  <option selected data-date = \"" + item.Value.ToString("dd/MM/yyyy") + "\" data-study = \"" + jca.ID + "\"> " + item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]</option>");
                            lesson.StudyTimeID = jca.ID;
                            lesson.StudyDate = item.Value.ToString("dd/MM/yyyy");
                            lesson.OptionName = item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]";
                            lesson.Selected = true;
                            lessonList.Add(lesson);
                        }
                        else
                        {
                            //l.Append("  <option data-date = \"" + item.Value.ToString("dd/MM/yyyy") + "\" data-study = \"" + jca.ID + "\"> " + item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]</option>");
                            lesson.StudyTimeID = jca.ID;
                            lesson.StudyDate = item.Value.ToString("dd/MM/yyyy");
                            lesson.OptionName = item.Value.ToString("dd/MM/yyyy") + " [" + jca.sTime + "-" + jca.eTime + "]";
                            lesson.Selected = false;
                            lessonList.Add(lesson);
                        }
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = lessonList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage AttendanceStudent()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
            try
            {
                var attendanceStudent = HttpContext.Current.Request.Form.Get("attendanceStudent");
                List<tbl_CoursesAttendanceStudent> attendanceList = new List<tbl_CoursesAttendanceStudent>();
                if (!string.IsNullOrEmpty(attendanceStudent))
                {
                    try
                    {
                        attendanceList = JsonConvert.DeserializeObject<List<tbl_CoursesAttendanceStudent>>(attendanceStudent);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                }
                foreach (var item in attendanceList)
                {
                    int courseID = item.CourseID.Value;
                    int studentID = item.StudentID.Value;
                    string date = item.Date.Value.ToString("dd/MM/yyyy");
                    int statusID = item.AttendanceID.Value;
                    int learningID = item.LearningID.Value;
                    string note = item.Note;
                    int warning = item.Warning == true ? 1 : 0;
                    int caID = item.StudyTimeID.Value;

                    DateTime d = GetDateTime.Now.Date;
                    d = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                    if (d.Date != DateTime.Now.Date)
                        return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                    string attname = "";
                    switch (statusID)
                    {
                        case 1:
                            attname = "Có";
                            break;
                        case 2:
                            attname = "Vắng có phép";
                            break;
                        case 3:
                            attname = "Vắng không phép";
                            break;
                        case 4:
                            attname = "Đi muộn";
                            break;
                        case 5:
                            attname = "Về sớm";
                            break;
                        default:
                            attname = "Nghỉ lễ";
                            break;
                    }
                    string learingname = "";
                    switch (learningID)
                    {
                        case 1:
                            learingname = "Giỏi";
                            break;
                        case 2:
                            learingname = "Khá";
                            break;
                        case 3:
                            learingname = "Trung bình";
                            break;
                        case 4:
                            learingname = "Kém";
                            break;
                        case 5:
                            learingname = "Theo dõi đặc biệt";
                            break;
                        case 6:
                            learingname = "Có cố gắng";
                            break;
                        case 7:
                            learingname = "Không cố gắng";
                            break;
                        default:
                            learingname = "Không nhận xét";
                            break;
                    }
                    bool war = false;
                    if (warning == 1)
                        war = true;
                    var l = CoursesScheduleTable.getbycourseidbydatevsstudyid(courseID, d.Date, caID);
                    if (l == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                    else
                    {
                        var ck = CoursesAttendanceStudentTable.checkadd(courseID, studentID, d.Date, caID);
                        if (ck == null)
                        {
                            var cus = AccountTable.getbyID(studentID);
                            var cou = CoursesTable.getbyid(courseID);

                            if (warning == 1)//push noti cảnh báo cho tư vấn viên
                            {
                                war = true;
                                if (cus != null && cou != null)
                                {
                                    var nt = NotificationTable.insertsingle("", "[Cảnh báo học viên]", "Cảnh báo Học viên [" + cus.FullName + "] khóa [" + cou.CourseName + "]. Nhận xét GV: \"" + note, cus.SupportUID.Value, aclog.UserName, 2, 0);
                                    ////push notifition desktop                          
                                    //string strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
                                    //string strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                                    //string url = strUrl + "Admin/Customer/CustomerWarning";
                                    //AppZim.ZIM.AssetCRM.PushNotiDesktop(nt.UID.Value, nt.NotificationTitle, url);
                                }
                            }
                            var school = SchoolTable.getbyID(cou.SchoolID.Value);
                            tbl_Account lead = new tbl_Account();
                            lead.ID = 0;
                            lead.FullName = "";
                            if (cou.TeacherLeaderUID != null)
                            {
                                lead.ID = cou.TeacherLeaderUID.Value;
                                lead.FullName = cou.TeacherLeaderName;
                            }
                            CoursesAttendanceStudentTable.insert(school, cou, cus, l.StudyTimeID.Value, d.Date, statusID, attname, learningID, learingname, note, aclog.UserName, war, aclog.FullName, lead);
                            string history = "Học viên " + cus.FullName + " được điểm danh: " + attname + ", đánh giá: " + learingname + ", nhận xét: " + note + " bởi " + aclog.RoleName + " - " + aclog.FullName;
                            HistoryAttendanceTable.insert(cus.ID, l.ID, history, aclog.ID);
                            NotificationTable.insertsingle("", "Điểm danh thành công", "Giáo viên đã điểm danh cho bạn, bạn vào và đánh giá chất lượng buổi học nha", cus.ID, aclog.UserName, 18, 0);
                            var stu = DeviceMobileTable.getbyUID(cus.ID)?.IncludePlayerID;
                            PushNotifyMobile.SendNotifyMobile("Giáo viên đã điểm danh cho bạn, bạn vào và đánh giá chất lượng buổi học nha", new[] { stu });
                            // điểm danh giáo viên
                            var schedule = CoursesScheduleTable.getbycourseid(cou.ID).Where(n => n.Date.Value == d.Date && n.StudyTimeID == caID && n.IsHide != true && n.TeacherAttendanceID == 0).ToList();
                            foreach (var te in schedule)
                            {
                                CoursesScheduleTable.updateteacherattendance(te.ID, aclog.ID, 1, aclog.UserName);
                            }
                        }
                        else
                            CoursesAttendanceStudentTable.update(ck.ID, statusID, attname, learningID, learingname, note, war, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage loadAttendanceByDate(int courseID, string date, int caID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                DateTime d = GetDateTime.Now.Date;
                d = DateTime.ParseExact(date, "dd/MM/yyyy", null);

                var l = CoursesScheduleTable.getbycourseidbydatevsstudyid(courseID, d.Date, caID);
                if (l == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                else
                {
                    var stu = CoursesStudentTable.getbycourseid(courseID);
                    var attendanceList = new List<AttendanceRecord>();

                    foreach (var item in stu)
                    {
                        var acstu = AccountTable.getbyID(item.StudentUID.Value);
                        var ck = CoursesAttendanceStudentTable.checkshow(courseID, acstu.ID, d.Date, caID);
                        var attendance = new AttendanceRecord();
                        if (ck != null)
                        {
                            attendance.StudentID = ck.StudentID.Value;
                            attendance.StudentName = ck.StudentName;
                            attendance.Note = ck.Note;
                            attendance.AttendanceName = ck.AttendanceName;
                            attendance.AttendanceID = ck.AttendanceID.Value;
                            attendance.LearningName = ck.LearningName;
                            attendance.LearningID = ck.LearningID.Value;
                            attendance.Warning = ck.Warning.Value;
                            attendance.DateAttendance = ck.Date.Value;
                        }
                        if (d.Date != GetDateTime.Now.Date)
                        {
                            attendance.Disible = true;
                        }
                        else
                        {
                            attendance.Disible = false;
                        }
                        attendanceList.Add(attendance);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = attendanceList, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getDocumentList(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var course = CoursesTable.getbyid(courseID);
                var documentList = DocumentTable.getdocbycurrid(course.CurriculumsID.Value);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = documentList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getNotificationsCourse(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var l = NotificationCourseTable.getbycourseid(courseID).OrderByDescending(n => n.ID).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage createNotificationsCourse()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int courseid = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                var title = HttpContext.Current.Request.Form.Get("title");
                var content = HttpContext.Current.Request.Form.Get("content");
                var contentnoti = "Thông báo mới";
                var rs = NotificationCourseTable.insert(title, content, courseid, aclog.UserName);
                if (rs != null)
                {
                    var l = CoursesStudentTable.getbycourseid(courseid);
                    foreach (var item in l)
                    {
                        NotificationTable.insertsingle("", rs.NotificationTitle, contentnoti, item.StudentUID.Value, aclog.UserName, 1, courseid);
                    }
                    var te = TeacherCourseTable.getbycourseid(courseid);
                    foreach (var item in te)
                    {
                        NotificationTable.insertsingle("", rs.NotificationTitle, contentnoti, item.TeacherUID.Value, aclog.UserName, 1, courseid);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage createExerciseGroup()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int courseID = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                var type = HttpContext.Current.Request.Form.Get("type").ToInt(1);
                var groupName = HttpContext.Current.Request.Form.Get("groupName");
                if (type != 1 && type != 3)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                var c = CoursesTable.getbyid(courseID);
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var rs = ExerciseGroupTable.insert(0, c.ID, type, groupName, aclog.UserName);

                if (rs != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage editExerciseGroup()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int groupID = HttpContext.Current.Request.Form.Get("groupID").ToInt(0);
                var groupName = HttpContext.Current.Request.Form.Get("groupName");

                var g = ExerciseGroupTable.getbyid(groupID);
                if (g == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var rs = ExerciseGroupTable.update(g.ID, groupName, 0, aclog.UserName);

                if (rs != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListExerciseGroupByCourseID(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var g = ExerciseGroupTable.getbycourseid(courseID);

                if (g != null)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = g, message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage addExerciseCourse()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var c = CoursesTable.getbyid(HttpContext.Current.Request.Form.Get("courseID").ToInt(0));
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var nhombt = ExerciseGroupTable.getbyid(HttpContext.Current.Request.Form.Get("exerciseGroupID").ToInt(0));
                if (nhombt == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                string ques = HttpContext.Current.Request.Form.Get("question");
                var fileupload = HttpContext.Current.Request.Files.Get("file");

                string filename = "";
                string link = "";
                if (fileupload != null)
                {
                    string name = Path.GetFileNameWithoutExtension(fileupload.FileName);
                    string ext = Path.GetExtension(fileupload.FileName);
                    if (ext.Contains(".js") || ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1") || ext.Contains(".exe"))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                    }
                    //var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                    var fileName = Guid.NewGuid() + ext;
                    var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/exercise/"), fileName);
                    link = "/Upload/exercise/" + fileName;
                    fileupload.SaveAs(path);
                    filename = fileupload.FileName;
                }
                var q = ExerciseTable.insert(nhombt.ID, ques, filename, link, nhombt.TypeGroup.Value, 0, aclog.UserName, 0);
                if (q != null)
                {
                    // gửi thông báo cho học viên có bài tập mới
                    var lstudent = CoursesStudentTable.getbycourseid(c.ID);
                    var title = "Bài tập mới khóa [" + c.CourseName + "]";
                    var content = "Bài tập mới cho khóa [" + c.CourseName + "]";

                    foreach (var item in lstudent)
                    {
                        NotificationTable.insertsingle("", title, content, item.StudentUID.Value, aclog.UserName, 4, c.ID);
                    }
                    ///
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage updateDeadlineExercise()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var c = CoursesTable.getbyid(HttpContext.Current.Request.Form.Get("courseID").ToInt(0));
                var ex = ExerciseTable.getbyid(HttpContext.Current.Request.Form.Get("exerciseID").ToInt(0));
                var stime = HttpContext.Current.Request.Form.Get("stime");
                var etime = HttpContext.Current.Request.Form.Get("etime");
                if (ex == null || c == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                if (string.IsNullOrEmpty(stime) || string.IsNullOrEmpty(etime))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                DateTime sd = GetDateTime.Now.Date;
                DateTime ed = GetDateTime.Now.Date;
                sd = DateTime.ParseExact(stime, "dd/MM/yyyy", null);
                ed = DateTime.ParseExact(etime, "dd/MM/yyyy", null);

                if (ed < sd)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                var listUpdate = ExerciseTable.getbygroupid(ex.ExerciseGroupID.Value);
                Parallel.ForEach(listUpdate, i =>
                {
                    var ck = CoursesExerciseDeadlineTable.getbycourseid(c.ID, i.ID);
                    if (ck == null)
                        CoursesExerciseDeadlineTable.insert(c.ID, i.ID, sd.Date, ed.Date, aclog.UserName);
                    else
                        CoursesExerciseDeadlineTable.update(ck.ID, sd.Date, ed.Date, aclog.UserName);
                });
                // gửi thông báo cho học viên có bài tập mới
                var lstudent = CoursesStudentTable.getbycourseid(c.ID);
                var title = "Cập nhật bài tập khóa [" + c.CourseName + "]";
                var content = "Cập nhật bài tập cho khóa [" + c.CourseName + "]";

                foreach (var item in lstudent)
                {
                    NotificationTable.insertsingle("", title, content, item.StudentUID.Value, aclog.UserName, 4, c.ID);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage editExerciseCourse()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var ex = ExerciseTable.getbyid(HttpContext.Current.Request.Form.Get("exerciseID").ToInt(0));
                var nhom = ExerciseGroupTable.getbyid(ex.ExerciseGroupID.Value);
                if (nhom.CourseID == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                }
                if (nhom.CourseID == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });
                }
                var nhombt = ExerciseGroupTable.getbyid(HttpContext.Current.Request.Form.Get("groupID").ToInt(0));
                if (nhombt == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                if (nhombt.TypeGroup != 1 && nhombt.TypeGroup != 3)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                string ques = HttpContext.Current.Request.Form.Get("question");
                var q = ExerciseTable.update(ex.ID, nhombt.ID, ques, ex.isHide.Value, aclog.UserName, ex.ParentQuestionID.Value);
                if (q != null)
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage uploadCurriculum()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var c = CoursesTable.getbyid(HttpContext.Current.Request.Form.Get("courseID").ToInt(0));
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                DateTime d = DateTime.ParseExact(HttpContext.Current.Request.Form.Get("date"), "dd/MM/yyyy", null);
                var sty = StudyTimeTable.getbyid(HttpContext.Current.Request.Form.Get("caID").ToInt(0));

                if (ModelState.IsValid)
                {
                    var file = HttpContext.Current.Request.Files.Get("file");
                    //Checking file is available to save.  
                    if (file != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file.FileName);
                        string ext = Path.GetExtension(file.FileName);
                        if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                        }
                        var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
                        var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/curriculumteacher/"), fileName);
                        string link = "/Upload/curriculumteacher/" + fileName;
                        file.SaveAs(path);

                        CurriculumTeacherTable.insert(c.ID, d.Date, sty.ID, link, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        //[HttpPost]
        //public HttpResponseMessage saveScoresExam()
        //{
        //    var aclog = AccountTable.getByToken(getToken());
        //    if (aclog == null)
        //        return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
        //    if (aclog.ExpiredToken < DateTime.Now)
        //        return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
        //    if (aclog.RoleID != 4)
        //        return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

        //    try
        //    {
        //        var c = CoursesTable.getbyid(HttpContext.Current.Request.Form.Get("courseID").ToInt(0));
        //        if (c == null)
        //        {
        //            return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
        //        }

        //        DateTime d = DateTime.ParseExact(HttpContext.Current.Request.Form.Get("date"), "dd/MM/yyyy", null);
        //        var sty = StudyTimeTable.getbyid(HttpContext.Current.Request.Form.Get("caID").ToInt(0));

        //        if (ModelState.IsValid)
        //        {
        //            var file = HttpContext.Current.Request.Files.Get("file");
        //            Checking file is available to save.
        //            if (file != null)
        //            {
        //                string name = Path.GetFileNameWithoutExtension(file.FileName);
        //                string ext = Path.GetExtension(file.FileName);
        //                if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
        //                {
        //                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
        //                }
        //                var fileName = name + " " + GetDateTime.Now.ToString("dd-MM-yyyy-hh-mm-ss") + ext;
        //                var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/curriculumteacher/"), fileName);
        //                string link = "/Upload/curriculumteacher/" + fileName;
        //                file.SaveAs(path);

        //                CurriculumTeacherTable.insert(c.ID, d.Date, sty.ID, link, aclog.UserName);
        //            }
        //        }
        //        return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
        //    }
        //}
        [HttpPost]
        public HttpResponseMessage submitChoice()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int gid = HttpContext.Current.Request.Form.Get("groupID").ToInt(0);
                int courseid = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);

                var ckstu = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
                if (ckstu == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var c = CoursesTable.getbyid(courseid);
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var gex = ExerciseGroupTable.getbyid(gid);
                if (gex == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var q = ExerciseTable.getbyid(HttpContext.Current.Request.Form.Get("questionID").ToInt(0));
                var anwer = ExerciceMultiChoiseTable.getbyid(HttpContext.Current.Request.Form.Get("answerID").ToInt(0));
                ExerciseAddonStudentTable.insert(aclog, c, new tbl_SetPackage() { ID = 0, PackageName = "" }, q, "", anwer.AnswerTitle, anwer.ID, 0, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage submitMindMap()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var answerobject = HttpContext.Current.Request.Form.Get("answer");
                List<QandA> answerList = new List<QandA>();
                if (!string.IsNullOrEmpty(answerobject))
                {
                    try
                    {
                        answerList = JsonConvert.DeserializeObject<List<QandA>>(answerobject);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                }
                for (int i = 0; i < answerList.Count; i++)
                {
                    QandA qa = answerList[i];
                    var question = ExerciseTable.getbyid(qa.QuesID);
                    var group = ExerciseGroupTable.getbyid(question.ExerciseGroupID.Value);
                    var course = CoursesTable.getbyid(group.CourseID.Value);
                    if (course == null)
                    {
                        course = CoursesTable.getbycurriculumn(group.CurriculumID.Value);
                    }
                    List<string> listans = qa.AnsID.Split(',').ToList();
                    List<tbl_ExerciceMultiChoise> listanswer = new List<tbl_ExerciceMultiChoise>();
                    foreach (var item in listans)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var answers = ExerciceMultiChoiseTable.getbyid(item.ToInt(0));
                            if (answers != null)
                            {
                                listanswer.Add(answers);
                            }
                        }
                    }
                    tbl_ExerciceMultiChoise answer = listanswer.FirstOrDefault();//chọn đúng hay sai thì cũng quy về thằng này
                    tbl_ExerciceMultiChoise checkcorrect = listanswer.Where(n => n.ExerciseID == question.ID && n.isTrue == true).FirstOrDefault();
                    if (checkcorrect != null)
                    {
                        answer = checkcorrect;
                    }
                    if (answer == null)
                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };

                    if (question != null)
                    {
                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = course.ID, CourseName = course.CourseName, }, new tbl_SetPackage() { ID = 0, PackageName = "" }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // viết và nói
        [HttpPost]
        public HttpResponseMessage submitEssayAndSpeaking()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int courseid = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                int hdfexeid = HttpContext.Current.Request.Form.Get("exerciseID").ToInt(0);
                string answer = HttpContext.Current.Request.Form.Get("answer");

                string linkfile = "";
                if (!string.IsNullOrEmpty(HttpContext.Current.Request.Form.Get("link")))
                    linkfile = HttpContext.Current.Request.Form.Get("link");

                var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseid).FirstOrDefault();
                if (ck == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var c = CoursesTable.getbyid(courseid);
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var ex = ExerciseTable.getbyid(hdfexeid);
                if (ex == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var checkdate = CoursesExerciseDeadlineTable.getbycourseid(c.ID, ex.ID);
                if (checkdate == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = "Bài tập chưa mở" });
                }

                if (checkdate.eTime.Value < GetDateTime.Now.Date)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = "Đã hết hạn làm bài" });
                }

                var checkex = ExerciseAddonStudentTable.getbyuidvsexeid(aclog.ID, ex.ID);
                if (checkex != null)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = "Bạn đã nộp bài rồi" });
                }

                int idfix = 0;
                var creex = AccountTable.getbyusername(ex.CreatedBy);
                if (creex != null)
                {
                    if (creex.RoleID == 4)// giáo viên
                        idfix = creex.ID;
                }

                var file = HttpContext.Current.Request.Files.GetMultiple("file");
                if (string.IsNullOrEmpty(linkfile) && file == null && ex.Type == 3)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                }
                var rs = ExerciseAddonStudentTable.insert(aclog, c, new tbl_SetPackage() { ID = 0, PackageName = "" }, ex, "", answer, 0, idfix, aclog.UserName);

                if (ex.Type == 3)// audio mới kiểm tra file 
                {
                    foreach (var item in file)
                    {
                        if (item != null)
                        {
                            string name = Path.GetFileNameWithoutExtension(item.FileName);
                            string ext = Path.GetExtension(item.FileName);
                            if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                            {
                                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.InvalidFile });
                            }
                            var fileName = Guid.NewGuid() + ext;
                            var path = Path.Combine(HttpContext.Current.Server.MapPath("~/Upload/exercisestudent/"), fileName);
                            linkfile += "/Upload/exercisestudent/" + fileName + "|";
                            item.SaveAs(path);
                        }
                    }

                    if (!string.IsNullOrEmpty(linkfile))
                    {
                        string[] arrlink = linkfile.Split('|');
                        for (int p = 0; p < arrlink.Length; p++)
                        {
                            //lưu file
                            if (!string.IsNullOrEmpty(arrlink[p]))
                                ExerciseAddonStudentFileAudioTable.insert(rs.ID, arrlink[p], aclog.UserName);
                        }
                    }
                }

                if (rs != null)
                {
                    if (idfix != 0)// gửi thông báo cho giáo viên
                    {
                        var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Có bài tập mới từ khóa " + rs.CourseName, rs.UIDFix.Value, aclog.UserName, 6, 0);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // drag
        [HttpPost]
        public HttpResponseMessage submitDrag()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var answerobject = HttpContext.Current.Request.Form.Get("answer");
                List<QandA> answerList = new List<QandA>();
                if (!string.IsNullOrEmpty(answerobject))
                {
                    try
                    {
                        answerList = JsonConvert.DeserializeObject<List<QandA>>(answerobject);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                }
                for (int i = 0; i < answerList.Count; i++)
                {
                    var question = ExerciseTable.getbyid(answerList[i].QuesID);
                    var group = ExerciseGroupTable.getbyid(question.ExerciseGroupID.Value);
                    var course = CoursesTable.getbyid(group.CourseID.Value);
                    if (course == null)
                    {
                        var curriculum = CurriculumTable.getbyID(group.CurriculumID.Value);
                        course = CoursesTable.getbycurriculumn(curriculum.ID);
                    }
                    var answer = ExerciceMultiChoiseTable.getbyid(answerList[i].AnsID.ToInt(0));
                    if (answer == null)
                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };
                    if (question != null)
                    {
                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = course.ID, CourseName = course.CourseName, }, new tbl_SetPackage() { ID = 0, PackageName = "" }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // chọn nhiều
        [HttpPost]
        public HttpResponseMessage submitMultipleChoices()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var answerobject = HttpContext.Current.Request.Form.Get("answer");
                List<QandA> answerList = new List<QandA>();
                if (!string.IsNullOrEmpty(answerobject))
                {
                    try
                    {
                        answerList = JsonConvert.DeserializeObject<List<QandA>>(answerobject);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                }

                for (int i = 0; i < answerList.Count; i++)
                {
                    QandA qa = answerList[i];
                    var question = ExerciseTable.getbyid(qa.QuesID);
                    var group = ExerciseGroupTable.getbyid(question.ExerciseGroupID.Value);
                    var course = CoursesTable.getbyid(group.CourseID.Value);
                    if (course == null)
                    {
                        var curriculum = CurriculumTable.getbyID(group.CurriculumID.Value);
                        course = CoursesTable.getbycurriculumn(curriculum.ID);
                    }
                    var answer = ExerciceMultiChoiseTable.getbyid(qa.AnsID.ToInt(0));
                    if (answer == null)
                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = "", isTrue = false };
                    if (question != null)
                    {
                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = course.ID, CourseName = course.CourseName, }, new tbl_SetPackage() { ID = 0, PackageName = "" }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // điền từ
        [HttpPost]
        public HttpResponseMessage submitWrite()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var answerobject = HttpContext.Current.Request.Form.Get("answer");
                List<QandA> answerList = new List<QandA>();
                if (!string.IsNullOrEmpty(answerobject))
                {
                    try
                    {
                        answerList = JsonConvert.DeserializeObject<List<QandA>>(answerobject);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                }
                for (int i = 0; i < answerList.Count; i++)
                {
                    QandA qa = answerList[i];
                    var question = ExerciseTable.getbyid(qa.QuesID);
                    var group = ExerciseGroupTable.getbyid(question.ExerciseGroupID.Value);
                    var course = CoursesTable.getbyid(group.CourseID.Value);
                    if (course == null)
                    {
                        var curriculum = CurriculumTable.getbyID(group.CurriculumID.Value);
                        course = CoursesTable.getbycurriculumn(curriculum.ID);
                    }
                    var answer = ExerciceMultiChoiseTable.getbytitle(qa.AnsID.Trim().ToString(), question.ID);
                    if (answer == null)
                        answer = new tbl_ExerciceMultiChoise() { ID = 0, AnswerTitle = qa.AnsID.Trim(), isTrue = false };
                    if (question != null)
                    {
                        ExerciseAddonStudentTable.insert(aclog, new tbl_Courses() { ID = course.ID, CourseName = course.CourseName, }, new tbl_SetPackage() { ID = 0, PackageName = "" }, question, "", answer.AnswerTitle, answer.ID, 0, aclog.UserName);
                    }
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // danh sách bài tập của học viên mới nộp
        [HttpGet]
        public HttpResponseMessage getNewExerciseList(string search, int courseID, string fdate, string tdate, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(fdate))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(tdate))
                {
                    try
                    {
                        DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                    }
                    catch { }
                }
                var list = new List<API_newexercise_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_newexercise_getlist(aclog.ID, search, courseID, 1, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // danh sách bài tập chưa chấm
        [HttpGet]
        public HttpResponseMessage getProcessExerciseList(string search, int courseID, string fdate, string tdate, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(fdate))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(tdate))
                {
                    try
                    {
                        DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                    }
                    catch { }
                }
                var list = new List<API_processexercise_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_processexercise_getlist(aclog.ID, search, courseID, 2, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // danh sách bài tập chấm lại
        [HttpGet]
        public HttpResponseMessage getRedoFixExerciseList(string search, int courseID, string fdate, string tdate, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(fdate))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(tdate))
                {
                    try
                    {
                        DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                    }
                    catch { }
                }
                var list = new List<API_processexercise_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_processexercise_getlist(aclog.ID, search, courseID, 4, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // danh sách bài tập đã chấm
        [HttpGet]
        public HttpResponseMessage getDoneExerciseList(string search, int courseID, string fdate, string tdate, int page = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (search == null)
                    search = "";
                int pageSize = 10;
                // thời gian bắt đầu từ
                string fromdate = null;
                if (!string.IsNullOrEmpty(fdate))
                {
                    try
                    {
                        DateTime fd = DateTime.ParseExact(fdate, "dd/MM/yyyy", null);
                        fromdate = fd.ToString("yyyy-MM-dd 00:00:00.000");
                    }
                    catch { }
                }
                string todate = null;
                // đến
                if (!string.IsNullOrEmpty(tdate))
                {
                    try
                    {
                        DateTime td = DateTime.ParseExact(tdate, "dd/MM/yyyy", null);
                        todate = td.ToString("yyyy-MM-dd 23:59:59.998");
                    }
                    catch { }
                }
                var list = new List<API_doneexercise_getlist_Result>();
                using (var db = new ZimEntities())
                {
                    list = db.API_doneexercise_getlist(aclog.ID, search, courseID, fromdate, todate, (page - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        // xem chi tiết bài làm của học viên đã được chấm
        [HttpGet]
        public HttpResponseMessage getExerciseDoneDetail(int exerciseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var l = ExerciseAddonStudentTable.getbyid(exerciseID);
                if (l == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var lfix = ExerciseAddonStudentFixTable.getbyexestudent(l.ID);

                var cmtList = new List<Comment>();
                for (int i = 0; i < lfix.Count; i++)
                {
                    var cmt = new Comment();
                    cmt.CommentID = lfix[i].ID;
                    var position = lfix[i].DataCMT.Split('|');
                    cmt.Text = position[0];
                    cmt.Start = position[1];
                    cmt.End = position[2];
                    cmt.CommentContent = lfix[i].FixNote;
                    cmtList.Add(cmt);
                }
                var audio = ExerciseAddonStudentFileAudioTable.getbyexerciseaddonstudentID(l.ID);
                var lscore = ExerciseScoreDetailTable.getbyexeid(l.ID).FirstOrDefault();
                var course = CoursesTable.getbyid(l.CourseID.Value);
                var ex = ExerciseTable.getbyid(l.ExerciseID.Value);
                return Request.CreateResponse(HttpStatusCode.OK, new { exercise = l, comment = cmtList, score = lscore, course, audio, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage UploadFileAudio()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            try
            {
                string link = "";
                var httpContext = HttpContext.Current;
                var file = httpContext.Request.Files.Get("file");

                if (file != null)
                {
                    string name = Path.GetFileNameWithoutExtension(file.FileName);
                    string ext = ".webm";
                    if (ext.Contains(".php") || ext.Contains(".asp") || ext.Contains(".aspx") || ext.Contains(".ps1"))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.Forbidden });
                    }
                    var fileName = name + Guid.NewGuid() + ext;
                    var path = Path.Combine(httpContext.Server.MapPath("~/Upload/exercisestudent/"), fileName);
                    link = "/Upload/exercisestudent/" + fileName;
                    file.SaveAs(path);
                    return Request.CreateResponse(HttpStatusCode.OK, new { link, message = ApiMessage.Success });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage loadTeacherCourseForCombobox()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                IEnumerable<tbl_Courses> courses = CoursesTable.getall().Where(n => n.CourseSelfStudy != true).OrderByDescending(n => n.ID).ToList();
                IEnumerable<tbl_TeacherCourse> tecourses = TeacherCourseTable.getbyteacherid(aclog.ID).Where(n => n.isHide != true).OrderByDescending(n => n.ID).ToList();
                courses = from c in courses
                          join t in tecourses on c.ID equals t.CourseID
                          select c;

                return Request.CreateResponse(HttpStatusCode.OK, new { listcourse = courses, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage fixExercise()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                int exerciseID = HttpContext.Current.Request.Form.Get("exerciseID").ToInt(0);
                string fix = HttpContext.Current.Request.Form.Get("fixContent");
                double score = Convert.ToDouble(HttpContext.Current.Request.Form.Get("score"));
                var ex = ExerciseAddonStudentTable.getbyid(exerciseID);
                if (ex == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                if (ex.UIDFix == 0 || ex.UIDFix == null)
                {
                    ExerciseAddonStudentTable.updateUIDFix(ex.ID, aclog.ID);
                }
                //ẩn điểm cũ nếu có
                ExerciseScoreDetailTable.updatehide(ex.ID, aclog.UserName);
                ExerciseScoreDetailTable.insert(ex.ID, "", score, aclog.UserName);

                var rs = ExerciseAddonStudentTable.updatefix(ex.ID, fix, "", score, aclog.UserName);
                if (rs != null)
                {
                    var nt = NotificationTable.insertsingle("", "Thông báo sửa bài", "Bài tập của bạn đã được sửa " + rs.CourseName, rs.UIDStudent.Value, aclog.UserName, 7, rs.ID);
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage manyChoicesDetail()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int courseID = HttpContext.Current.Request.Form.Get("courseID").ToInt(0);
                int groupID = HttpContext.Current.Request.Form.Get("groupID").ToInt(0);

                var ck = CoursesStudentTable.getbystudentid(aclog.ID).Where(n => n.CourseID == courseID).FirstOrDefault();
                if (ck == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                var c = CoursesTable.getbyid(courseID);
                if (c == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }

                var gex = ExerciseGroupTable.getbyid(groupID);
                if (gex == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                }
                string score = "N/A";
                int i = 0;
                var ckwork = ExerciseAddonStudentTable.checkworkexercise(aclog.ID, groupID);
                if (ckwork)
                {
                    var le = ExerciseTable.getbygroupid(groupID);
                    foreach (var item in le)
                    {
                        var atrue = ExerciceMultiChoiseTable.getbyanwertrue(item.ID);
                        var astu = ExerciseAddonStudentTable.getbyuidvsexeid(aclog.ID, item.ID);
                        if (atrue.ID == astu.ExerciseMultipleChoiseID)
                            i += 1;
                    }
                    score = i + "/" + le.Count;
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { score, course = c, groupexercise = gex, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage commentExercise()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int exerciseID = HttpContext.Current.Request.Form.Get("exerciseID").ToInt(0);
                string data = HttpContext.Current.Request.Form.Get("comment");
                var ex = ExerciseAddonStudentTable.getbyid(exerciseID);
                string[] dt = data.Split(',');
                // thêm mới comment
                var cmt = ExerciseAddonStudentFixTable.insert(ex.ID, dt[0] + "|" + dt[1] + "|" + dt[2], dt[3], "", aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage editCommentExercise()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int commentID = HttpContext.Current.Request.Form.Get("commentID").ToInt(0);
                string comment = HttpContext.Current.Request.Form.Get("comment");
                ExerciseAddonStudentFixTable.Update(commentID, comment, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage deleteCommentExercise()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                int commentID = HttpContext.Current.Request.Form.Get("commentID").ToInt(0);
                ExerciseAddonStudentFixTable.Delete(commentID, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getExerciseResult(int groupID, int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var exercise = ExerciseTable.getbygroupid(groupID);
                var answerList = new List<ExerciseResult>();
                int correctquestion = 0;//số câu đúng
                var l = new List<API_Exercise_getResult_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Exercise_getResult(aclog.ID, groupID).ToList();
                }
                Parallel.ForEach(l, i =>
                {
                    var check = i.CorrectAnswer.SequenceEqual(i.Answer);
                    if (check == true)
                        correctquestion += 1;
                    var ans = new ExerciseResult();
                    ans.ID = i.ID;
                    ans.YourAnswer = i.Answer;
                    ans.KeyResult = i.CorrectAnswer;
                    answerList.Add(ans);
                });
                answerList = answerList.OrderBy(x => x.ID).ToList();
                string correct = correctquestion.ToString() + '/' + exercise.Count;
                double scorePerQuestion = (double)10 / exercise.Count;// tính số điểm mỗi câu
                double tempScore = correctquestion * scorePerQuestion;//số điểm tạm thời
                var score = AssetCRM.RoundScore(tempScore);
                var checkexist = GroupScoreTable.CheckExist(courseID, groupID, aclog.ID);
                if (checkexist == null)
                {
                    GroupScoreTable.insert(courseID, groupID, aclog.ID, score, correctquestion, exercise.Count);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { answerList, score, correct, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getDonEssayExcerciseForStudent(int groupID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var l = new List<API_Student_GetDoneExercise_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Student_GetDoneExercise(aclog.ID, groupID).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getListCoursesByStudentID(int StudentID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 14)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                if (StudentID == 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                var l = new List<API_Student_getListCourse_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.API_Student_getListCourse(StudentID).ToList();
                }
                var list = new List<CourseInformation>();
                var studytime = StudyTimeTable.getall();
                foreach (var i in l)
                {
                    var c = new CourseInformation();
                    c.CourseID = i.CourseID.Value;
                    c.CourseStudentID = i.CourseStudentID.Value;
                    c.CourseName = i.CourseName;
                    c.StatusName = i.StatusName;
                    c.TeacherLeaderName = i.TeacherLeaderName;
                    c.Price = i.Price.Value;
                    c.TotalPaid = i.TotalPaid.Value;
                    c.PriceLeft = i.PriceLeft.Value;
                    c.OpeningDate = i.OpeningDate;
                    c.SchoolName = i.SchoolName;
                    var s = new API_Student_getStudyTimeByCourseID_Result();
                    using (var db = new ZimEntities())
                    {
                        s = db.API_Student_getStudyTimeByCourseID(i.CourseID.Value).FirstOrDefault();
                    }
                    if (s != null)
                    {
                        c.StudyTime = s.StudyTime;
                        c.TeacherName = s.TeacherName;
                    }
                    c.EndDate = CoursesScheduleTable.getbycourseid(i.CourseID.Value).OrderByDescending(x => x.Date).Select(x => x.Date).FirstOrDefault().Value.ToString("dd/MM/yyyy");
                    c.OpeningDate = i.OpeningDate;
                    c.TotalStudent = i.TotalStudent.Value;
                    c.TotalStudyDay = i.TotalStudyDay.Value;
                    list.Add(c);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage loadAttendanceByStudentID(int courseID, int studentID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 14)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var attendanceList = new List<AttendanceRecord>();
                var schedule = CoursesScheduleTable.getbycourseid(courseID);
                var student = AccountTable.getbyID(studentID);
                foreach (var j in schedule)
                {
                    var ck = CoursesAttendanceStudentTable.checkshow(courseID, studentID, j.Date.Value, j.StudyTimeID.Value);
                    var attendance = new AttendanceRecord();
                    if (ck != null)
                    {
                        attendance.StudentID = ck.StudentID.Value;
                        attendance.StudentName = ck.StudentName;
                        attendance.Note = ck.Note;
                        attendance.AttendanceName = ck.AttendanceName;
                        attendance.AttendanceID = ck.AttendanceID.Value;
                        attendance.LearningName = ck.LearningName;
                        attendance.LearningID = ck.LearningID.Value;
                        attendance.Warning = ck.Warning.Value;
                        attendance.DateAttendance = ck.Date.Value;
                    }
                    else
                    {
                        attendance.StudentID = student.ID;
                        attendance.StudentName = student.FullName;
                    }
                    attendanceList.Add(attendance);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = attendanceList, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage loadScoreColumnForCombobox(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var curriculumn = CoursesTable.getbyid(courseID).CurriculumsID;
                if (curriculumn == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                var s = new List<API_Score_LoadScoreColumnForCombobox_Result>();
                using (var db = new ZimEntities())
                {
                    s = db.API_Score_LoadScoreColumnForCombobox(curriculumn).ToList();
                }
                var studentList = CoursesStudentTable.getbycourseid(courseID);
                var list = new List<SortAccount>();
                foreach (var i in studentList)
                {
                    var stu = new SortAccount();
                    stu.ID = i.StudentUID.Value;
                    stu.Avatar = "";
                    stu.RoleName = "Học viên";
                    stu.FullName = i.StudentName;
                    list.Add(stu);
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { scorelist = s, studentlist = list, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        public HttpResponseMessage saveScore()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                string score = HttpContext.Current.Request.Form.Get("score");
                List<Score> scoreJson = new List<Score>();
                if (!string.IsNullOrEmpty(score))
                {
                    try
                    {
                        scoreJson = JsonConvert.DeserializeObject<List<Score>>(score);
                    }
                    catch (Exception ex)
                    {
                        { throw ex; }
                    }
                    foreach (var i in scoreJson)
                    {
                        ScoreStudentDetailTable.Insert(i.ScoreID, i.StudentID, i.ScoreValue, i.Remark, aclog.UserName);
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getScorePerStudent(int courseID, int studentID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var curriculumn = CoursesTable.getbyid(courseID).CurriculumsID;
                if (curriculumn == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });

                var configscore = new List<API_ConfigScore_GetListByCurriculumn_Result>();
                using (var db = new ZimEntities())
                {
                    configscore = db.API_ConfigScore_GetListByCurriculumn(curriculumn).ToList();
                }
                var listScore = new List<ScoreDetail>();
                Parallel.ForEach(configscore, i =>
                {
                    var score = new ScoreDetail();
                    score.StudentID = studentID;
                    score.ConfigID = i.ConfigID;
                    score.ConfigName = i.ConfigName;
                    score.ScoreFactor = i.ScoreFactor.Value;
                    var scoredetail = new List<API_Score_GetListByStudentID_Result>();
                    using (var db = new ZimEntities())
                    {
                        scoredetail = db.API_Score_GetListByStudentID(i.ConfigID, studentID).ToList();
                    }
                    score.Score = scoredetail;
                    listScore.Add(score);
                });

                // tính rank
                var r = new List<API_Score_GetValue_Result>();
                using (var db = new ZimEntities())
                {
                    r = db.API_Score_GetValue(curriculumn).ToList();
                }
                int rank = 0;
                double diemTB = 0;
                for (var i = 0; i < r.Count; i++)
                {
                    if (r[i].StudentID == studentID)
                    {
                        rank = Convert.ToInt32(r[i].RankNumber.Value);
                        diemTB = r[i].DiemTB.Value;
                    }
                }
                listScore = listScore.OrderBy(x => x.ScoreFactor).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { data = listScore, rank, diemTB, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getScoreByCourseID(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var curriculumn = CoursesTable.getbyid(courseID).CurriculumsID;
                if (curriculumn == null)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.NotFoundInformation });
                // lấy danh sách học viên
                var studentList = CoursesStudentTable.getStudentIDByCourseID(courseID);
                var listScore = new List<ScoreDetail>();
                var configscore = new List<API_ConfigScore_GetListByCurriculumn_Result>();
                using (var db = new ZimEntities())
                {
                    configscore = db.API_ConfigScore_GetListByCurriculumn(curriculumn).ToList();
                }
                foreach (var s in studentList)
                {
                    Parallel.ForEach(configscore, i =>
                    {
                        var score = new ScoreDetail();
                        score.StudentID = s;
                        score.ConfigID = i.ConfigID;
                        score.ConfigName = i.ConfigName;
                        score.ScoreFactor = i.ScoreFactor.Value;
                        var scoredetail = new List<API_Score_GetListByStudentID_Result>();
                        using (var db = new ZimEntities())
                        {
                            scoredetail = db.API_Score_GetListByStudentID(i.ConfigID, s).ToList();
                        }
                        score.Score = scoredetail;
                        listScore.Add(score);
                    });
                }
                // tính rank
                var rank = new List<API_Score_GetValue_Result>();
                using (var db = new ZimEntities())
                {
                    rank = db.API_Score_GetValue(curriculumn).ToList();
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { studentlist = studentList, data = listScore, rank, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getExerciseDeadline(int exerciseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var deadline = CoursesExerciseDeadlineTable.GetByExerciseID(exerciseID);
                return Request.CreateResponse(HttpStatusCode.OK, new { data = deadline, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        public HttpResponseMessage attendanceAllStudent()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var scheduleId = HttpContext.Current.Request.Form.Get("scheduleId").ToInt(0);
                var statusId = HttpContext.Current.Request.Form.Get("statusId").ToInt(0);
                var attendanceId = HttpContext.Current.Request.Form.Get("attendanceId").ToInt(0);
                var learningId = HttpContext.Current.Request.Form.Get("learningId").ToInt(0);
                var note = HttpContext.Current.Request.Form.Get("note");
                var file = HttpContext.Current.Request.Files.Get("file");
                string ext = "";
                if (file != null)
                {
                    ext = Path.GetExtension(file.FileName).ToLower();
                    var result = AssetCRM.isValidImageAndVideo(ext); // Validate Header
                    if (!result)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "File không đúng định dạng" });
                    }
                }

                Thread tattendance = new Thread(() => { TeacherCourseController.AttendanceAllStudent(scheduleId, attendanceId, learningId, note, aclog); });
                tattendance.Start();
                var link = "";
                var schedule = CoursesScheduleTable.getbyid(scheduleId);
                var studyTime = StudyTimeTable.getbyid(schedule.StudyTimeID ?? 0);
                if (file != null)
                {
                    var fileName = DateTime.Now.ToFileTime().ToString() + ext;
                    var path = Path.Combine(HttpContext.Current.Server.MapPath("/Upload/VideoSchedule/"), fileName);
                    string strPathAndQuery = HttpContext.Current.Request.Url.PathAndQuery;
                    string strUrl = HttpContext.Current.Request.Url.AbsoluteUri.Replace(strPathAndQuery, "/");
                    link = strUrl + "Upload/VideoSchedule/" + fileName;
                    file.SaveAs(path);
                    CoursesScheduleFileTable.insert(new tbl_CoursesScheduleFile() { CoursesScheduleID = scheduleId, FileName = link, TeacherAttendanceID = 0 }, aclog.UserName);
                    string history = aclog.FullName + " đã upload <a href=\"" + link + "\" target=\"_blank\">Video</a> cho ngày học " + schedule.Date.Value.ToString("dd/MM/yyyy") + " " + studyTime.sTime + " - " + studyTime.eTime;
                    HistoryUploadVideoTable.insert(scheduleId, history, aclog.ID);
                }

                var scheduleBefore = CoursesScheduleTable.getbyid(scheduleId);
                schedule = CoursesScheduleTable.updateteacherattendance(scheduleId, aclog.ID, statusId, aclog.UserName);
                ///cập nhật lương chấm bài
                if (scheduleBefore.TeacherAttendanceID == 0 && scheduleBefore.FisnishStatus == 0 && statusId != 3)
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

                            if (statusId == 4)
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
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data = link });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        /// <summary>
        /// format ngày kiểu yyyy-MM-dd
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage teachingHistory(int courseId = 0, string fromDate = null, string toDate = null, int pageSize = 10, int pageIndex = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                List<Admin_TeacherSalaryDetail_GetList_Result> l = new List<Admin_TeacherSalaryDetail_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.Admin_TeacherSalaryDetail_GetList(fromDate, toDate, aclog.ID, courseId, (pageIndex - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data = l });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        /// <summary>
        /// format ngày kiểu yyyy-MM-dd
        /// </summary>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        [HttpGet]
        public HttpResponseMessage loadCoursesForTeachingHistory(string fromDate, string toDate)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                fromDate = fromDate + " 00:00:00.000";
                toDate = toDate + " 23:59:59.998";
                List<GetCourseByTeacherAndSchedule_Result> courses = new List<GetCourseByTeacherAndSchedule_Result>();
                using (var db = new ZimEntities())
                {
                    courses = db.GetCourseByTeacherAndSchedule(aclog.ID, "", fromDate, toDate, 0, int.MaxValue).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data = courses });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        public async Task<HttpResponseMessage> updateReasonSalary([FromBody] UpdateReasonSalary model)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var teacherSalary = await TeacherSalaryDetailTable.getbyidAsync(model.id);
                var rs = await TeacherSalaryDetailTable.UpdateReason(model.id, model.reason, aclog.UserName, model.status, teacherSalary.Salary ?? 0);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPut]
        public HttpResponseMessage PayrollForTeacher()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                TeacherSalaryDetailTable.PayrollForTeacher(aclog.ID, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage SalaryReviewTeacher(int month, int year, int pageSize = 10, int pageIndex = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 4)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                List<Admin_TeacherSalary_GetList_Result> l = new List<Admin_TeacherSalary_GetList_Result>();
                using (var db = new ZimEntities())
                {
                    l = db.Admin_TeacherSalary_GetList(0, aclog.ID, month, year, (pageIndex - 1) * pageSize, pageSize).ToList();
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data = l });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage faq(string search, int pageSize = 10, int pageIndex = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                IEnumerable<tbl_FAQ> l = FAQTable.GetAll().Where(x => x.RoleID == aclog.RoleID || x.RoleID == 0).OrderByDescending(n => n.ID).ToList();
                //key search
                l = from ex in l
                    where AssetCRM.RemoveUnicode(ex.TitleQuestion).ToUpper().Contains(AssetCRM.RemoveUnicode(search).ToUpper())
                    select ex;
                l = l.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data = l });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<HttpResponseMessage> SignContract(int pageSize = 10, int pageIndex = 1)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var data = await ContractCustomerTable.GetByStudentId(aclog.ID);
                data = data.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();

                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<HttpResponseMessage> StudentSignContract()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
            if (aclog.RoleID != 5)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

            try
            {
                var id = HttpContext.Current.Request.Form.Get("id").ToInt(0);
                var sign = HttpContext.Current.Request.Form.Get("sign").ToString();
                var data = ContractCustomerTable.getbyid(id);
                data.ContractContent = data.ContractContent.Replace("[signature]", "<img src=\"" + sign + "\" />");
                await ContractCustomerTable.updateContent(id, data.ContractContent, aclog.UserName);
                await SignatureTable.Insert(aclog.ID, sign, aclog.UserName);
                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success, data });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
    }
}
