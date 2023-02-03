using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesScheduleTable
    {
        public static tbl_CoursesSchedule insert(int CourseID, bool CourseSelfStudy, int CurriculumsDetailsID, DateTime Date, int StudyTimeID, int SchoolID, int RoomID,
            int TeacherID, string CreateBy, int subjectid)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesSchedule co = new tbl_CoursesSchedule();
                co.CourseID = CourseID;
                co.CourseSelfStudy = CourseSelfStudy;
                co.CurriculumsDetailsID = CurriculumsDetailsID;
                co.SubjectID = subjectid;
                co.Date = Date.Date;
                co.IsHide = false;
                co.StudyTimeID = StudyTimeID;
                co.SchoolID = SchoolID;
                co.SchoolRoomID = RoomID;
                co.TeacherID = TeacherID;
                co.TeacherAttendanceID = 0;
                co.IsSendMail = false;
                co.Notify = false;
                co.FisnishStatus = 0;//0. Chưa học, 1. Hoàn thành, 2. Giáo viên vắng mặt, 3. mất mạng/ mất điện,4 Học viên vắng mặt
                co.CreatedDate = co.ModifiedDate = GetDateTime.Now;
                co.CreatedBy = co.ModifiedBy = CreateBy;
                db.tbl_CoursesSchedule.Add(co);
                db.SaveChanges();
                return co;
            }
        }

        public static tbl_CoursesSchedule update(int id, DateTime Date, int TeacherID, int studyid, int roomid, string CreateBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesSchedule co = db.tbl_CoursesSchedule.Where(n => n.ID == id && (n.FisnishStatus == 0 || n.FisnishStatus == 2)).FirstOrDefault();
                if (co == null)
                    return null;
                string history = "";

                if (co.Date != Date.Date)
                {
                    history += "Ngày học: " + co.Date.Value.ToString("dd/MM/yyyy") + " => " + Date.Date.ToString("dd/MM/yyyy") + " </br>";
                }
                co.Date = Date.Date;

                co.IsHide = false;

                if (co.TeacherID != TeacherID)
                {
                    var oldTeacher = AccountTable.getbyID(co.TeacherID.Value);
                    var newTeacher = AccountTable.getbyID(TeacherID);
                    history += " Giáo viên: " + oldTeacher.FullName + " => " + newTeacher.FullName + " </br>";
                }
                co.TeacherID = TeacherID;

                if (co.StudyTimeID != studyid)
                {
                    var oldStudy = StudyTimeTable.getbyid(co.StudyTimeID.Value);
                    var newStudy = StudyTimeTable.getbyid(studyid);
                    history += " Ca học: " + oldStudy.sTime + " - " + oldStudy.eTime + " => " + newStudy.sTime + " - " + newStudy.eTime + " </br>";
                }
                co.StudyTimeID = studyid;

                co.SchoolRoomID = roomid;
                co.ModifiedDate = DateTime.Now;
                co.ModifiedBy = CreateBy;
                db.SaveChanges();
                if (history != "")
                {
                    var h = new tbl_CoursesScheduleHistory();
                    h.CourseID = co.CourseID.Value;
                    h.History = history;
                    h.CreatedDate = DateTime.Now;
                    h.CreatedBy = CreateBy;
                    db.tbl_CoursesScheduleHistory.Add(h);
                    db.SaveChanges();
                }
                return co;
            }
        }

        public static tbl_CoursesSchedule updateTeacher(int id, int TeacherID, string CreateBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesSchedule co = db.tbl_CoursesSchedule.Where(n => n.ID == id).FirstOrDefault();
                if (co == null)
                    return null;

                co.IsHide = false;
                co.TeacherID = TeacherID;
                co.ModifiedDate = DateTime.Now;
                co.ModifiedBy = CreateBy;
                db.SaveChanges();
                return co;
            }
        }

        public static bool checkroom(int schoolid, int studyid, int roomid, DateTime Date, int courseid)// true được
        {
            using (var db = new ZimEntities())
            {
                var study = StudyTimeTable.getbyid(studyid);
                if (study == null)
                    return false;

                string[] stime = study.sTime.Split(':');
                TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                DateTime st = Date.Date.Add(sth);
                DateTime et = st.AddHours(study.StyleLesson.Value);


                bool check = true;
                var l = db.tbl_CoursesSchedule.Where(n => n.SchoolID == schoolid && n.SchoolRoomID == roomid && n.Date == Date && n.IsHide != true).ToList();
                foreach (var item in l)
                {
                    var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                    if (stu == null)
                        continue;

                    if (courseid != 0)
                    {
                        if (item.CourseID == courseid)
                            continue;
                    }

                    string[] istime = stu.sTime.Split(':');
                    TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                    DateTime ist = Date.Date.Add(isth);
                    DateTime iet = ist.AddHours(stu.StyleLesson.Value);

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

        public static int checkroomreturncourseid(int schoolid, int studyid, DateTime Date, int courseid)// true được
        {
            using (var db = new ZimEntities())
            {
                int id = 0;
                var study = StudyTimeTable.getbyid(studyid);
                if (study == null)
                    return id;

                string[] stime = study.sTime.Split(':');
                TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                DateTime st = Date.Date.Add(sth);
                DateTime et = st.AddHours(study.StyleLesson.Value);


                var l = db.tbl_CoursesSchedule.Where(n => n.SchoolID == schoolid && n.Date == Date && n.IsHide != true).ToList();
                foreach (var item in l)
                {
                    var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                    if (stu == null)
                        continue;

                    if (courseid != 0)
                    {
                        if (item.CourseID == courseid)
                            continue;
                    }

                    string[] istime = stu.sTime.Split(':');
                    TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                    DateTime ist = Date.Date.Add(isth);
                    DateTime iet = ist.AddHours(stu.StyleLesson.Value);

                    if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                    {
                        id = item.CourseID.Value;
                        break;
                    }

                    if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                    {
                        id = item.CourseID.Value;
                        break;
                    }

                    if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                    {
                        id = item.CourseID.Value;
                        break;
                    }
                }
                return id;
            }
        }

        public static tbl_CoursesSchedule getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.ID == id && n.IsHide != true).FirstOrDefault();
                return l;
            }
        }

        /// <summary>
        /// Chỉ lấy ID lịch học theo RoomID
        /// </summary>
        /// <param name="RoomID"></param>
        /// <returns></returns>
        public static int GetByRoomID(string RoomID)
        {
            int id = 0;
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.RoomID == RoomID && n.IsHide != true).FirstOrDefault();
                if (l != null)
                {
                    id = l.ID;
                }
                return id;
            }
        }

        public static tbl_CoursesSchedule UpdateRoom(int ID, string RoomID, string RoomPass, string starturl)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesSchedule.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.RoomID = RoomID;
                    ck.RoomPass = RoomPass;
                    ck.isRoomStart = false;
                    ck.ZoomStartURL = starturl;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_CoursesSchedule UpdateisStartRoom(int ID, bool isStart)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesSchedule.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isRoomStart = isStart;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_CoursesSchedule> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.IsHide != true).ToList();
                return l;
            }
        }
        public class classcheckteacher
        {
            public bool checktrue { get; set; }
            public int courseid { get; set; }
        }
        public static classcheckteacher checkteacher(int UID, DateTime Date, int studyid, int courseid)//kiểm tra giáo viên có đụng ko
        {
            using (var db = new ZimEntities())
            {
                var study = StudyTimeTable.getbyid(studyid);
                if (study == null)
                    return null;

                classcheckteacher rs = new classcheckteacher();
                rs.checktrue = true;
                rs.courseid = 0;

                //check ca, ngày nghỉ
                var loff = db.tbl_DayOffTeacher.Where(n => n.UID == UID && n.DayOff == Date && n.isHide != true && n.StudyTimeID == studyid).FirstOrDefault();
                if (loff != null)
                {
                    rs.checktrue = false;
                    rs.courseid = 0;
                }

                string[] stime = study.sTime.Split(':');
                TimeSpan sth = new TimeSpan(stime[0].ToInt(), stime[1].ToInt(0), 0);
                DateTime st = GetDateTime.Now.Date.Add(sth);
                //DateTime et = st.AddHours(study.StyleLesson.Value);
                string[] etime = study.eTime.Split(':');
                TimeSpan eth = new TimeSpan(etime[0].ToInt(), etime[1].ToInt(0), 0);
                DateTime et = GetDateTime.Now.Date.Add(eth);

                //bool check = true;
                var l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == UID && n.Date == Date && n.IsHide != true).ToList();
                foreach (var item in l)
                {
                    var stu = StudyTimeTable.getbyid(item.StudyTimeID.Value);
                    if (stu == null)
                        continue;

                    if (courseid != 0)
                    {
                        if (item.CourseID == courseid)
                            continue;
                    }

                    string[] istime = stu.sTime.Split(':');
                    TimeSpan isth = new TimeSpan(istime[0].ToInt(), istime[1].ToInt(0), 0);
                    DateTime ist = GetDateTime.Now.Date.Add(isth);
                    //DateTime iet = ist.AddHours(stu.StyleLesson.Value);
                    string[] ietime = stu.eTime.Split(':');
                    TimeSpan ieth = new TimeSpan(ietime[0].ToInt(), ietime[1].ToInt(0), 0);
                    DateTime iet = GetDateTime.Now.Date.Add(ieth);

                    if (st == ist || et == iet)// thời gian bắt đầu, kết thúc cùng nhau => ko được
                    {
                        //check = false;
                        rs.checktrue = false;
                        rs.courseid = item.CourseID.Value;
                        break;
                    }

                    if ((st <= ist && ist < et) || (st < iet && iet <= et))// thời gian bắt đầu, kết thúc nằm trong ca khác => ko được
                    {
                        //check = false;
                        rs.checktrue = false;
                        rs.courseid = item.CourseID.Value;
                        break;
                    }

                    if ((st < ist && iet < et) || (st > ist && iet > et))// ca bị bao hoặc bao 1 ca khác => ko được
                    {
                        //check = false;
                        rs.checktrue = false;
                        rs.courseid = item.CourseID.Value;
                        break;
                    }
                }
                if (!rs.checktrue)
                    return rs;


                return rs;
            }
        }

        public static List<tbl_CoursesSchedule> getbycourseid(int CourseID)// lấy danh sách ngày học của khóa học
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == CourseID && n.IsHide != true).ToList();
                return l;
            }
        }
        public static List<tbl_CoursesSchedule> GetByCourseIDHasTeacher(int CourseID)// lấy danh sách ngày học của khóa học - có giáo viên
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == CourseID && n.IsHide != true && n.TeacherAttendanceID != 0).ToList();
                return l;
            }
        }
        public static List<tbl_CoursesSchedule> getbydate(DateTime date)// lấy tất cả trong ngày
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.Date == date.Date && n.IsHide != true).ToList();
                return l;
            }
        }

        public static tbl_CoursesSchedule getbycourseidbydatevsstudyid(int CourseID, DateTime date, int studyid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == CourseID && n.Date == date.Date && n.StudyTimeID == studyid && n.IsHide != true).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> getbycourseidbydateall(int CourseID, DateTime date)// lấy danh sách ngày học của khóa học
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == CourseID && n.Date == date.Date && n.IsHide != true).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> getbyteacherid(int teacherid)// lấy danh sách ngày học của khóa học
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == teacherid && n.IsHide != true).ToList();
                return l;
            }
        }
        /// <summary>
        /// Tính tổng giờ dạy
        /// </summary>
        /// <param name="TeacherID"></param>
        /// <param name="Year"></param>
        /// <param name="Month"></param>
        /// <returns></returns>
        public static int GetTotalHourByTeacherIDYearMonth(int TeacherID, int Year, int Month)
        {
            using (var db = new ZimEntities())
            {
                int l = db.tbl_CoursesSchedule.Where(n => n.TeacherID == TeacherID && n.IsHide != true && n.Date.Value.Year == Year && n.Date.Value.Month == Month).Count();
                return l;
            }
        }


        public static List<tbl_CoursesSchedule> getbyschoolid(int schoolid)// lấy danh sách ngày học của trung tâm
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.SchoolID == schoolid && n.IsHide != true).ToList();
                return l;
            }
        }
        //cập nhật giáo viên dạy
        public static tbl_CoursesSchedule updateteacherattendance(int id, int teacherid, int status, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.TeacherAttendanceID = teacherid;
                    l.FisnishStatus = status;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        // lấy theo khóa học và trạng thái hoàn thành
        public static List<tbl_CoursesSchedule> GetByCourseAndStatus(int courseid, int finishid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseid && n.IsHide == false && n.FisnishStatus == finishid).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> SoBuoiDaDay(int courseid, int finishid, int teacherid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseid && n.IsHide == false && n.FisnishStatus == finishid && n.TeacherAttendanceID == teacherid).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> getbyTeacherIDandcouseID(int courseid, int teacherID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseid && n.IsHide == false && n.TeacherID == teacherID).ToList();
                return l;
            }
        }

        public static int TongThoiGianDay(int courseid, int teacherid)
        {
            using (var db = new ZimEntities())
            {
                int tong = 0;
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseid
                                                        && n.IsHide == false
                                                        && n.TeacherID == teacherid
                                                        && n.FisnishStatus == 1
                                                        && n.TeacherAttendanceID != 0).ToList();
                foreach (var item in l)
                {
                    tong += StudyTimeTable.getbyid(item.StudyTimeID.Value).StyleLesson.Value;
                }
                return tong;
            }
        }

        public static void UpdateSentMail(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsSendMail = true;
                    db.SaveChanges();
                }
            }
        }
        public static void UpdateIsHide(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsHide = true;
                    db.SaveChanges();
                }
            }
        }
        public static tbl_CoursesSchedule GetLastLesson(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == id).OrderByDescending(x => x.Date).FirstOrDefault();
                return l;
            }
        }
        public static tbl_CoursesSchedule GetFirstLesson(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == id).OrderBy(x => x.Date).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> GetDelaySchedule(DateTime date, int courseId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.CourseID == courseId && n.Date >= date && (n.FisnishStatus == 0 || n.FisnishStatus == 2)).OrderBy(x => x.Date).ToList();
                return l;
            }
        }
        
        public static List<tbl_CoursesScheduleHistory> GetHistoryByCourseID(int courseId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesScheduleHistory.Where(n => n.CourseID == courseId).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesSchedule> GetMultiple(int[] id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.IsHide == false && id.Contains(n.ID)).ToList();
                return l;
            }
        }

        public static async void UpdateSalaryForSchedule(int courseId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.IsHide == false && n.CourseID == courseId).OrderBy(x => x.ID).ToList();
                var salaryList = db.tbl_CourseSalary.Where(x => x.IsHide == false && x.CourseID == courseId).ToList();
                int total = l.Count;
                int I_TEMP = 0;
                int INDEX = 0;

                for(var i = 0; i < total; i++)
                {
                    I_TEMP++;
                    if (I_TEMP > salaryList[INDEX].NumberOfLesson)
                    {
                        INDEX++;
                        I_TEMP = 1;
                    } 

                    if(INDEX == salaryList.Count)
                    {
                        INDEX--;
                    }

                    l[i].Salary = salaryList[INDEX].SalaryOfLesson;
                }

                var temp_id = l.Select(x => x.ID).ToList();
                var salaryUpdate = db.tbl_TeacherSalaryDetail.Where(x => temp_id.Contains(x.CoursesScheduleID.Value) && x.isHide == false && x.isClosing == false).ToList();

                int totalUpdate = salaryUpdate.Count;
                for(var i = 0; i < totalUpdate; i++)
                {
                    salaryUpdate[i].Salary = l.Where(x => x.ID == salaryUpdate[i].CoursesScheduleID).FirstOrDefault()?.Salary ?? 0;
                }

                await db.SaveChangesAsync();
            }
        }

        public static void UpdateNotify(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesSchedule.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Notify = true;
                    db.SaveChanges();
                }
            }
        }
    }
}