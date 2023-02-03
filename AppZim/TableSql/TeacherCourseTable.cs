using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MB.Extensions;

namespace AppZim.TableSql
{
    public class TeacherCourseTable
    {
        //thêm dữ liệu
        public static tbl_TeacherCourse insert(int teacherid, tbl_Courses course, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_TeacherCourse ck = db.tbl_TeacherCourse.Where(n => n.TeacherUID == teacherid && n.CourseID == course.ID && n.isHide != true).FirstOrDefault();
                if (ck != null)
                    return null;

                ck = new tbl_TeacherCourse();
                ck.TeacherUID = teacherid;
                ck.CourseID = course.ID;
                ck.CourseName = course.CourseName;
                ck.OpenDate = course.OpeningDate;
                ck.isHide = false;
                ck.ChairmanTeacher = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_TeacherCourse.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_TeacherCourse> getbycourseid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TeacherCourse.Where(n => n.CourseID == id).ToList();
                return l;
            }
        }
        public static List<tbl_TeacherCourse> getAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TeacherCourse.Where(x=>x.isHide == false).ToList();
                return l;
            }
        }
        public static List<tbl_TeacherCourse> GetAllByTeacherUID(int TeacherUID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TeacherCourse.Where(x => x.TeacherUID == TeacherUID).ToList();
                return l;
            }
        }

        public static tbl_TeacherCourse GetByCourseID(int CourseID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TeacherCourse.Where(n => n.CourseID == CourseID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_TeacherCourse> getbyteacherid(int teacherid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TeacherCourse.Where(n => n.TeacherUID == teacherid).ToList();
                return l;
            }
        }

        public static List<tbl_TeacherCourse> GetCourseByTeacherID_ClassID(int teacherid, int classid)
        {
            using (var db = new ZimEntities())
            {
                var r1 = db.tbl_Courses.Where(x => x.ClassID == 7);
                var ids = new int[1, 2, 3, 4];
                var listID = new List<int>();
                foreach (var p in r1)
                {
                    listID.Add(p.ID);
                }
                var r2 = db.tbl_TeacherCourse.Where(y => y.TeacherUID == 20);

                r2 = r2.Where(pp => listID.Contains(pp.CourseID.ToString().ToInt(0)));

                return r2.ToList();
            }
        }
    }
}