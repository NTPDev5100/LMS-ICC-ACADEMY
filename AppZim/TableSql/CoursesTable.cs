using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesTable
    {
        //thêm dữ liệu
        public static tbl_Courses insert(string coursename, int gradeid, string gradename, int classid,
            string classname, DateTime opendate, int caid, string caname, int sotiet, int currid,
            string curriname, int price, int schoolid, string schoolname, int roomid, string roomname, string createby,
            bool selfstudy, tbl_Account academicuid, int realLesson)
        {
            using (var db = new ZimEntities())
            {
                tbl_Courses ck = new tbl_Courses();
                ck.Lesson = realLesson;
                ck.GradeID = gradeid;
                ck.GradeName = gradename;
                ck.CourseName = coursename;
                ck.ClassID = classid;
                ck.ClassName = classname;
                ck.OpeningDate = opendate.Date;
                ck.StudyTimeID = caid;
                ck.StudyTimeName = caname;
                ck.TietHoc = sotiet;
                ck.CurriculumsID = currid;
                ck.CurriculumsName = curriname;
                ck.Price = price;
                ck.SchoolID = schoolid;
                ck.SchoolName = schoolname;
                ck.SchoolRoomID = roomid;
                ck.SchoolRoomName = roomname;
                ck.Mail3Lesson = false;
                ck.Status = 1;//1 sắp mở, 2 đang diễn ra, 3 đã xong
                ck.StatusName = "Sắp mở";
                ck.IsDelete = false;
                ck.CountStudent = 0;
                ck.DifferenceSchedule = 0;
                ck.AcademicUID = academicuid.ID;
                ck.AcademicName = academicuid.FullName;
                ck.CourseSelfStudy = selfstudy;//true khóa trực lớp
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Courses.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_Courses InsertCourseOnline(string coursename, int gradeid, string gradename, int classid, string classname,
            DateTime opendate, DateTime enddate, int currid, string curriname, int price, int schoolid, string schoolname,
            string createby, bool selfstudy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Courses ck = new tbl_Courses();
                ck.GradeID = gradeid;
                ck.GradeName = gradename;
                ck.CourseName = coursename;
                ck.ClassID = classid;
                ck.ClassName = classname;
                ck.OpeningDate = opendate.Date;
                ck.EndingDate = enddate;
                ck.StudyTimeID = 0;
                ck.StudyTimeName = "";
                ck.TietHoc = 0;
                ck.CurriculumsID = currid;
                ck.CurriculumsName = curriname;
                ck.Price = price;
                ck.SchoolID = schoolid;
                ck.SchoolName = schoolname;
                ck.SchoolRoomID = 0;
                ck.SchoolRoomName = "";
                ck.Status = 1;//1 sắp mở, 2 đang diễn ra, 3 đã xong
                ck.StatusName = "Sắp mở";
                ck.IsDelete = false;
                ck.CountStudent = 0;
                ck.CourseSelfStudy = selfstudy;//true khóa trực lớp
                ck.CreatedBy = createby;
                ck.ModifiedBy = "";
                ck.ModifiedDate = null;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_Courses.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// cập nhật trạng thái
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="status"></param>
        /// <param name="statusname"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Courses updatestatus(int ID, int status, string statusname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Courses ck = db.tbl_Courses.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;//1 sắp mở, 2 đang diễn ra, 3 đã xong
                    ck.StatusName = statusname;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Courses UpdateName(int ID, string courseName, string createby, int status, string statusName)
        {
            using (var db = new ZimEntities())
            {
                tbl_Courses ck = db.tbl_Courses.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.CourseName = courseName;
                    ck.Status = status;
                    ck.StatusName = statusName;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Courses Delete(int ID, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Courses ck = db.tbl_Courses.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = IsDelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Lấy tất cả các kháo
        /// </summary>
        /// <returns></returns>
        public static List<tbl_Courses> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(x => x.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy tất cả các khóa do gv đó chủ nhiện
        /// </summary>
        /// <returns></returns>
        public static List<tbl_Courses> getbylead(int lead)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.TeacherLeaderUID == lead && n.IsDelete == false).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }
        public static List<tbl_Courses> GetByCurriculumsID(int CurriculumsID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.CurriculumsID == CurriculumsID && n.IsDelete == false).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy theo id của khoas
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static tbl_Courses getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Cập nhật số lượng học viên
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Courses updatecountstudent(int id, int count, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.CountStudent = count;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static void updateEndDate(int id, DateTime endDate)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.EndingDate = endDate;
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// Cập nhật giáo viên chủ nhiện
        /// </summary>
        /// <param name="id"></param>
        /// <param name="teacher"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Courses updateteacherlead(int id, int lesson, tbl_Account acdemic, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.DifferenceSchedule = lesson;
                    if (acdemic != null)
                    {
                        l.AcademicUID = acdemic.ID;
                        l.AcademicName = acdemic.FullName;
                    }
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static string GetByStudent(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var courseList = db.tbl_CoursesStudent.Where(x => x.StudentUID == studentid && x.isHide == false).Select(x => new { x.CourseID, x.CourseName }).Distinct().ToList();
                StringBuilder html = new StringBuilder();
                foreach (var i in courseList)
                {
                    html.Append("<option value=\"" + i.CourseID + "\">" + i.CourseName + "</option>");
                }
                return html.ToString();
            }
        }
        public static tbl_Courses getbycurriculumn(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.CurriculumsID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_Courses update3mail(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Mail3Lesson = true;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public async static Task<List<tbl_Courses>> GetCourseByTeacherId(int teacherId)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses
                        .AsQueryable()
                        .Join(db.tbl_TeacherCourse,
                                C => C.ID,
                                TC => TC.CourseID,
                                (C, TC) => new { Course = C, TeacherCourse = TC })
                        .Where(x => x.TeacherCourse.TeacherUID == teacherId).Select(x => x.Course).ToList();

                return l;
            }
        }

        public async static Task UpdateLesson(int courseID, int lesson)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Courses.Where(n => n.ID == courseID).FirstOrDefault();
                if (l != null)
                {
                    l.Lesson = lesson;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}