using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesAttendanceStudentTable
    {
        //thêm dữ liệu
        public static tbl_CoursesAttendanceStudent insert(tbl_School school, tbl_Courses courses, tbl_Account account, int studytimeid, DateTime date, int attendaceid, string attendacename,
            int leaningid, string leaningname, string note, string createby, bool warning, string teachername, tbl_Account lead)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = new tbl_CoursesAttendanceStudent();
                try
                {
                    ck.SchoolID = school.ID;
                    ck.SchoolName = school.SchoolName;
                    ck.StudentID = account.ID;
                }
                catch
                { }
                ck.StudentName = account.FullName;
                ck.StudentPhone = account.Phone;
                try
                {
                    ck.CourseID = courses.ID;
                    ck.CourseName = "<a href=\"/Admin/CourseDetail/AttendanceCourse/" + courses.ID + "\">" + courses.CourseName + "</a>";
                }
                catch
                { }
                ck.StudyTimeID = studytimeid;
                ck.Date = date.Date;
                ck.Note = note;
                ck.AttendanceID = attendaceid;
                ck.AttendanceName = attendacename;
                ck.LearningID = leaningid;
                ck.LearningName = leaningname;
                ck.isHide = false;
                ck.Warning = warning;
                ck.TeacherName = teachername;
                ck.TeacherLeadID = lead.ID;
                ck.TeacherLeadName = lead.FullName;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesAttendanceStudent.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CoursesAttendanceStudent update(int id, int attendaceid, string attendacename, int leaningid, string leaningname, string note, bool warning, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.AttendanceID = attendaceid;
                    ck.AttendanceName = attendacename;
                    ck.LearningID = leaningid;
                    ck.LearningName = leaningname;
                    ck.Note = note;
                    ck.Warning = warning;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_CoursesAttendanceStudent UpdateDiemDanh(int id, int attendaceid, string attendacename)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.AttendanceID = attendaceid;
                    ck.AttendanceName = attendacename;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_CoursesAttendanceStudent UpdateTimeEndMeeting(int id, DateTime eTimeJoinMeeting, int TotalJoinMeeting)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.eTimeJoinMeeting = eTimeJoinMeeting;
                    ck.TotalJoinMeeting = TotalJoinMeeting;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_CoursesAttendanceStudent UpdateTimeStartMeeting(int id, DateTime sTimeJoinMeeting)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.sTimeJoinMeeting = sTimeJoinMeeting;
                    ck.TotalJoinMeeting = 1;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_CoursesAttendanceStudent> getbyuidcourseid(int uid, int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.StudentID == uid && n.CourseID == courseid).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesAttendanceStudent> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.StudentID == uid && n.CourseID != 0).ToList();
                return l;
            }
        }

        public static tbl_CoursesAttendanceStudent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        //check show thì xem thằng nào acctive mưới hienr thị
        public static tbl_CoursesAttendanceStudent checkshow(int courseid, int studentid, DateTime date, int caid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.CourseID == courseid && n.StudentID == studentid && n.Date == date.Date && n.StudyTimeID == caid && n.isHide == false).FirstOrDefault();
                return l;
            }
        }

        // thêm dữ liệu thì kiểm tra xem có ko
        public static tbl_CoursesAttendanceStudent checkadd(int courseid, int studentid, DateTime date, int studyid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.CourseID == courseid && n.StudentID == studentid && n.Date == date && n.StudyTimeID == studyid).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Note: Thêm bình luận mới
        /// </summary>
        /// <param name="id"></param>
        /// <param name="contentnew"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_CoursesAttendanceStudent updatecmtnew(int id, string contentnew, string createby,string ehem)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesAttendanceStudent ck = db.tbl_CoursesAttendanceStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.CmtNew = contentnew;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }
        /// <summary>
        /// Lấy học sinh bị cảnh báo
        /// </summary>
        /// <returns></returns>
        public static List<tbl_CoursesAttendanceStudent> getcuswarning()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.Warning == true).ToList();
                return l;
            }
        }
        public static List<tbl_CoursesAttendanceStudent> getcuswarningPriorityTeacher(int teacherID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.Warning == true && n.TeacherLeadID == teacherID).OrderByDescending(n => n.ModifiedDate).ToList();
                var l2 = db.tbl_CoursesAttendanceStudent.Where(n => n.Warning == true && n.TeacherLeadID != teacherID).OrderByDescending(n => n.ModifiedDate).ToList();
                l.AddRange(l2);
                return l;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid">Student ID</param>
        /// <returns></returns>
        public static tbl_CoursesAttendanceStudent getInCustomerDetail(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(n => n.StudentID == uid && n.CourseID == 0).FirstOrDefault();
                return l;
            }
        }
        
        public static tbl_CoursesAttendanceStudent GetBySchedule(int courseId, DateTime date, int studyTime)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesAttendanceStudent.Where(x => x.Date == date && x.StudyTimeID == studyTime && x.CourseID == courseId && x.isHide == false).FirstOrDefault();
                return l;
            }
        }
    }
}