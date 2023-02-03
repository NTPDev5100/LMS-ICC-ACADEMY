using AppZim.api;
using AppZim.Models;
using AppZim.TableSql;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AppZim.Areas.api.schedule
{
    [Route("api/schedule/{action}", Name = "apischedule")]
    public class scheduleController : ApiController
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
        public static DateTime StartOfWeek(DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
            return dt.AddDays(-1 * diff).Date;
        }
        [HttpGet]
        public HttpResponseMessage getStudentSchedule(string subject, int courseid)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
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
                        string[] arrsubj = subject.Split(',');
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
                    t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                    t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                    t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                    t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid && n.CourseID == item.CourseID).ToList().Select(n => n.Date).Distinct().Count() + 1);
                    t.school = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
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
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage getOffSchedule(string date, int type)
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID == 5)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                DateTime dateSchedule = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                if (type == 1)// trừ
                    dateSchedule = dateSchedule.AddDays(-7);
                if (type == 2)// cộng
                    dateSchedule = dateSchedule.AddDays(7);

                var firstDayOfMonth = new DateTime(dateSchedule.Year, dateSchedule.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                var studys = StudyTimeTable.getall().OrderBy(n => n.eTime).ToList();

                DateTime monday = StartOfWeek(dateSchedule.Date, DayOfWeek.Monday);
                var lstudy = StudyTimeTable.getall().OrderBy(n => n.sTime).ToList();
                //StringBuilder html = new StringBuilder();
                //html.Append("<tr>");
                //html.Append("   <th>Ca</th>");
                List<string> listDate = new List<string>();
                for (int d = 0; d < 7; d++)
                {
                    DateTime dt = monday.AddDays(d);
                    listDate.Add(dt.ToString("ddd") + " " + dt.ToString("dd/MM"));
                    //html.Append("   <th>" + dt.ToString("dddd") + " " + dt.ToString("dd/MM") + "</th>");
                }
                //html.Append("</tr>");
                List<SlotSchedule> off = new List<SlotSchedule>();
                List<StudyTime> timeName = new List<StudyTime>();
                foreach (var item in lstudy)
                {
                    //html.Append("   <td>" + item.sTime + " " + item.eTime + "</td>");
                    var time = new StudyTime();
                    time.StudyTimeName = item.sTime + " " + item.eTime;
                    time.StudyTimeID = item.ID;
                    timeName.Add(time);
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
                        var slot = new SlotSchedule();
                        if (!daydisable.checktrue)// đã có lịch
                        {
                            if (check != null)
                            {
                                //html.Append("   <td><label><input type=\"checkbox\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/></label></td>");
                                slot.StudyTimeID = item.ID;
                                slot.StudyTimeName = item.StudyTimeName;
                                slot.StudyDate = day.ToString("dd/MM/yyyy");
                                slot.CheckBox = true;
                                slot.Checked = true;
                                off.Add(slot);
                            }
                            else
                            {
                                //html.Append("   <td></td>");
                                slot.StudyTimeID = item.ID;
                                slot.StudyTimeName = item.StudyTimeName;
                                slot.StudyDate = day.ToString("dd/MM/yyyy");
                                slot.CheckBox = false;
                                slot.Checked = false;
                                off.Add(slot);
                            }
                        }
                        else
                        {
                            if (check == null)
                            {
                                //html.Append("   <td><label><input type=\"checkbox\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/></label></td>");
                                slot.StudyTimeID = item.ID;
                                slot.StudyTimeName = item.StudyTimeName;
                                slot.StudyDate = day.ToString("dd/MM/yyyy");
                                slot.CheckBox = true;
                                slot.Checked = false;
                                off.Add(slot);
                            }
                            else
                            {
                                //html.Append("   <td><label><input type=\"checkbox\" checked=\"checked\" data-studyid=\"" + item.ID + "\" data-date=\"" + day.ToString("dd/MM/yyyy") + "\"/></label></td>");
                                slot.StudyTimeID = item.ID;
                                slot.StudyTimeName = item.StudyTimeName;
                                slot.StudyDate = day.ToString("dd/MM/yyyy");
                                slot.CheckBox = true;
                                slot.Checked = true;
                                off.Add(slot);
                            }
                        }
                    }
                    //html.Append("</tr>");
                }
                return Request.CreateResponse(HttpStatusCode.OK, new { data = off, date = dateSchedule, listdate = listDate, listtime = timeName, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpPost]
        public HttpResponseMessage saveOrCancelOffSchedule()
        {
            try
            {
                var aclog = AccountTable.getByToken(getToken());
                if (aclog == null)
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
                if (aclog.ExpiredToken < DateTime.Now)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });
                if (aclog.RoleID != 4)
                    return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.Forbidden });

                string date = HttpContext.Current.Request.Form.Get("date");
                int studyTimeID = HttpContext.Current.Request.Form.Get("studyTimeID").ToInt(0);
                int timeOfDay = HttpContext.Current.Request.Form.Get("timeOfDay").ToInt(0);

                if (string.IsNullOrEmpty(date))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.RequiredInformation });
                }
                if (studyTimeID == 0)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ApiMessage.RequiredInformation });
                }
                if (timeOfDay != 0)
                {
                    var studyTimeList = StudyTimeTable.getByTimeOfDay(timeOfDay);
                    var monday = StartOfWeek(DateTime.ParseExact(date, "dd/MM/yyyy", null), DayOfWeek.Monday);
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
                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.Success });
                }
                else
                {
                    var dateSchedule = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                    var checkday = CoursesScheduleTable.getbyteacherid(aclog.ID).Where(n => n.Date == dateSchedule && n.IsHide != true && n.StudyTimeID == studyTimeID).FirstOrDefault();
                    var daydisable = CoursesScheduleTable.checkteacher(aclog.ID, dateSchedule.Date, studyTimeID, 0);
                    if (checkday == null && daydisable.checktrue)
                    {
                        var ck = DayOffTeacherTable.getbyuiddate(aclog.ID, dateSchedule.Date).Where(n => n.StudyTimeID == studyTimeID).FirstOrDefault();
                        if (ck == null)
                        {
                            DayOffTeacherTable.insert(aclog.ID, dateSchedule.Date, studyTimeID, aclog.UserName);
                            return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.SaveDayOffSuccess });
                        }
                        else
                        {
                            var rs = DayOffTeacherTable.update(ck.ID, !ck.isHide.Value, aclog.UserName);
                            if (rs.isHide.Value)
                                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.CancelDayOffSuccess });
                            return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.SaveDayOffSuccess });
                        }
                    }
                    else
                    {
                        if (!daydisable.checktrue)
                        {
                            var ck = DayOffTeacherTable.getbyuiddate(aclog.ID, dateSchedule.Date).Where(n => n.StudyTimeID == studyTimeID).FirstOrDefault();
                            if (ck != null)
                            {
                                var rs = DayOffTeacherTable.update(ck.ID, !ck.isHide.Value, aclog.UserName);
                                if (rs.isHide.Value)
                                    return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.CancelDayOffSuccess });
                                return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.SaveDayOffSuccess });
                            }
                        }
                        return Request.CreateResponse(HttpStatusCode.OK, new { message = ApiMessage.SaveFail });
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getTeacherSchedule(string dateSchedule, int type)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                DateTime date = DateTime.ParseExact(dateSchedule, "dd/MM/yyyy", null);
                if (type == 1)// trừ
                    date = date.AddMonths(-1);
                if (type == 2)// cộng
                    date = date.AddMonths(1);

                var calen = CoursesScheduleTable.getbyteacherid(aclog.ID);
                calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();

                List<CheckSchedule> l = new List<CheckSchedule>();
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
                    var attendance = CoursesAttendanceStudentTable.GetBySchedule(item.CourseID.Value, item.Date.Value.Date, item.StudyTimeID.Value);
                    if (attendance == null)
                        attendance = new tbl_CoursesAttendanceStudent() { AttendanceID = 0, AttendanceName = "", LearningID = 0, LearningName = "", Note = "" };

                    string[] stu = sty.sTime.Split(':');
                    string[] etu = sty.eTime.Split(':');
                    TimeSpan s = new TimeSpan(int.Parse(stu[0]), int.Parse(stu[1]), 0);
                    TimeSpan e = new TimeSpan(int.Parse(etu[0]), int.Parse(etu[1]), 0);

                    CheckSchedule t = new CheckSchedule();
                    t.id = item.ID;
                    t.course = c.ID;
                    t.title = c.CourseName;
                    t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                    t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                    t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                    t.allDay = false;
                    t.subject = sub.SubjectName;
                    t.videos = CoursesScheduleFileTable.getbyScheduleID(item.ID).Count();
                    t.status = c.Status ?? 0;
                    t.statusName = c.StatusName;
                    t.LearningID = attendance.LearningID ?? 0;
                    t.LearningName = attendance.LearningName;
                    t.attendanceID = attendance.AttendanceID ?? 0;
                    t.AttendanceName = attendance.AttendanceName;
                    t.Note = attendance.Note;
                    t.FisnishStatus = item.FisnishStatus ?? 0;
                    //if (c.CourseSelfStudy != true)
                    //    t.url = "/Admin/CourseDetail/ScheduleCourse/" + c.ID + "";
                    //else
                    //    t.url = "/Admin/Course/CourseSelfStudyScheduleDetail/" + c.ID + "";
                    //t.borderColor = "#3cd629";
                    //if (!string.IsNullOrEmpty(school.CodeColor))
                    //    t.borderColor = "#" + school.CodeColor;

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
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, date, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getTeacherScheduleInCourse(int courseID)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                var calen = CoursesScheduleTable.getbycourseid(courseID);
                List<CheckSchedule> l = new List<CheckSchedule>();
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

                    var attendance = CoursesAttendanceStudentTable.GetBySchedule(item.CourseID.Value, item.Date.Value.Date, item.StudyTimeID.Value);
                    if (attendance == null)
                        attendance = new tbl_CoursesAttendanceStudent() { AttendanceID = 0, AttendanceName = "", LearningID = 0, LearningName = "", Note = "" };

                    CheckSchedule t = new CheckSchedule();
                    t.course = c.ID;
                    t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                    t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                    t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                    t.allDay = false;
                    t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid).ToList().Select(n => n.Date).Distinct().Count() + 1);
                    //t.url = "/Admin/CourseDetail/ScheduleCourse/" + c.ID + "";
                    t.url = url;
                    t.borderColor = colorcalenda;
                    t.teacher = te.FullName;
                    t.school = c.SchoolName;
                    t.status = c.Status ?? 0;
                    t.statusName = c.StatusName;
                    t.LearningID = attendance.LearningID ?? 0;
                    t.LearningName = attendance.LearningName;
                    t.attendanceID = attendance.AttendanceID ?? 0;
                    t.AttendanceName = attendance.AttendanceName;
                    t.Note = attendance.Note;
                    t.FisnishStatus = item.FisnishStatus ?? 0;
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
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage getSchedule(string dateSchedule, int type)
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                if (aclog.RoleID == 5)
                {
                    DateTime date = DateTime.ParseExact(dateSchedule, "dd/MM/yyyy", null);
                    if (type == 1)// trừ
                        date = date.AddMonths(-1);
                    if (type == 2)// cộng
                        date = date.AddMonths(1);

                    var assign = CoursesStudentTable.getbystudentid(aclog.ID);
                    List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
                    foreach (var item in assign)
                    {
                        var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                        List<tbl_CoursesSchedule> p = new List<tbl_CoursesSchedule>();
                        p = t;
                        calen.AddRange(p);
                    }
                    calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();
                    //Cái Schedule mới em tạo ở bên dưới thêm một số chi tiết để đổ data ra cho hợp lý
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
                        t.id = item.ID;
                        t.couserID = c.ID;
                        t.className = c.ClassName;
                        t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                        t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                        t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                        t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid && n.CourseID == item.CourseID).ToList().Select(n => n.Date).Distinct().Count() + 1);
                        t.school = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
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
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, date, message = ApiMessage.Success });
                }
                else
                {
                    DateTime date = DateTime.ParseExact(dateSchedule, "dd/MM/yyyy", null);
                    if (type == 1)// trừ
                        date = date.AddMonths(-1);
                    if (type == 2)// cộng
                        date = date.AddMonths(1);

                    var calen = CoursesScheduleTable.getbyteacherid(aclog.ID);
                    calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();

                    List<CheckSchedule> l = new List<CheckSchedule>();
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

                        CheckSchedule t = new CheckSchedule();
                        t.course = c.ID;
                        t.title = c.CourseName;
                        t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                        t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                        t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                        t.allDay = false;
                        t.subject = sub.SubjectName;
                        //if (c.CourseSelfStudy != true)
                        //    t.url = "/Admin/CourseDetail/ScheduleCourse/" + c.ID + "";
                        //else
                        //    t.url = "/Admin/Course/CourseSelfStudyScheduleDetail/" + c.ID + "";
                        //t.borderColor = "#3cd629";
                        //if (!string.IsNullOrEmpty(school.CodeColor))
                        //    t.borderColor = "#" + school.CodeColor;

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
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, date, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }

        [HttpGet]
        public HttpResponseMessage getUpcomingLesson()
        {
            var aclog = AccountTable.getByToken(getToken());
            if (aclog == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, new { message = ApiMessage.Unauthorized });
            if (aclog.ExpiredToken < DateTime.Now)
                return Request.CreateResponse(HttpStatusCode.Forbidden, new { message = ApiMessage.ExpiredToken });

            try
            {
                if (aclog.RoleID == 5)
                {
                    DateTime date = DateTime.Now;
                    var assign = CoursesStudentTable.getbystudentid(aclog.ID);
                    List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
                    foreach (var item in assign)
                    {
                        var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                        calen.AddRange(t);
                    }
                    calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();
                    //Cái Schedule mới em tạo ở bên dưới thêm một số chi tiết để đổ data ra cho hợp lý
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
                        t.id = item.ID;
                        t.couserID = c.ID;
                        t.className = c.ClassName;
                        t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                        t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                        t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                        t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid && n.CourseID == item.CourseID).ToList().Select(n => n.Date).Distinct().Count() + 1);
                        t.school = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
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
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
                else
                {
                    DateTime date = DateTime.Now;
                    var calen = CoursesScheduleTable.getbyteacherid(aclog.ID);
                    calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();

                    List<CheckSchedule> l = new List<CheckSchedule>();
                    foreach (var item in calen)
                    {
                        var c = CoursesTable.getbyid(item.CourseID.Value);
                        if (c == null)
                            continue;
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

                        CheckSchedule t = new CheckSchedule();
                        t.id = item.ID;
                        t.course = c.ID;
                        t.title = c.CourseName;
                        t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                        t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                        t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                        t.allDay = false;
                        t.subject = sub.SubjectName;
                        t.school = c.SchoolName;
                        t.teacher = AccountTable.getbyID(item.TeacherID.Value).FullName;

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
                    return Request.CreateResponse(HttpStatusCode.OK, new { data = l, message = ApiMessage.Success });
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        [HttpGet]
        public HttpResponseMessage parentLoadSchedule(int studentID, string dateSchedule, int type)
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
                DateTime date = DateTime.ParseExact(dateSchedule, "dd/MM/yyyy", null);
                if (type == 1)// trừ
                    date = date.AddMonths(-1);
                if (type == 2)// cộng
                    date = date.AddMonths(1);

                var assign = CoursesStudentTable.getbystudentid(studentID);
                List<tbl_CoursesSchedule> calen = new List<tbl_CoursesSchedule>();
                foreach (var item in assign)
                {
                    var t = CoursesScheduleTable.getbycourseid(item.CourseID.Value);
                    calen.AddRange(t);
                }
                calen = calen.Where(n => n.Date.Value.Month == date.Month && n.Date.Value.Year == date.Year).ToList();
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
                    t.id = item.ID;
                    t.couserID = c.ID;
                    t.className = c.ClassName;
                    t.start = item.Date.Value.Add(s).ToString("dd/MM/yyyy HH:mm:ss");
                    t.end = item.Date.Value.Add(e).ToString("dd/MM/yyyy HH:mm:ss");
                    t.day = item.Date.Value.Add(s).ToString("dd/MM/yyyy");
                    t.subject = sub.SubjectName + " - " + (calen.Where(n => n.Date < item.Date && n.SubjectID == subid && n.CourseID == item.CourseID).ToList().Select(n => n.Date).Distinct().Count() + 1);
                    t.school = SchoolTable.getbyID(item.SchoolID.Value).SchoolName;
                    t.teacher = te.FullName;
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
                return Request.CreateResponse(HttpStatusCode.OK, new { data = l, date, message = ApiMessage.Success });
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, new { message = ex.Message });
            }
        }
        public class Schedule
        {
            public int id { get; set; }
            public int couserID { get; set; }
            public string className { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string day { get; set; }
            public string url { get; set; }
            public string borderColor { get; set; }
            public string room { get; set; }
            public string subject { get; set; }
            public string school { get; set; }
            public string teacher { get; set; }
            public bool allDay { get; set; }
        }
        public class CheckSchedule
        {
            public int id { get; set; }
            public int course { get; set; }
            public string title { get; set; }
            public string start { get; set; }
            public string end { get; set; }
            public string day { get; set; }
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
            public int videos { get; set; }
            public int status { get; set; }
            public string statusName { get; set; }
            public int attendanceID { get; set; }
            public string AttendanceName { get; set; }
            public int LearningID { get; set; }
            public string LearningName { get; set; }
            public string Note { get; set; }

            public int FisnishStatus { get; set; }//0. Chưa học, 1. Hoàn thành, 2. Giáo viên vắng mặt, 3. mất mạng/ mất điện,4 Học viên vắng mặt
            public string FisnishStatusName
            {
                get
                {
                    return FisnishStatus == 0 ? "Chưa học" :
                        FisnishStatus == 1 ? "Hoàn thành" :
                        FisnishStatus == 2 ? "Giáo viên vắng mặt" :
                        FisnishStatus == 3 ? "Mất mạng/ mât điện" :
                        FisnishStatus == 4 ? "Học viên vắng mặt" : "";
                }
            }
        }
    }
}
