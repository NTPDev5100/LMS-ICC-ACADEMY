using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesExamTable
    {

        //thêm dữ liệu
        public static tbl_CoursesExam insert(int courseid, string examname, DateTime examdate, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExam ck = new tbl_CoursesExam();
                ck.CourseID = courseid;
                ck.ExamName = examname;
                ck.DateExam = examdate.Date;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesExam.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CoursesExam getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExam ck = db.tbl_CoursesExam.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }

        public static List<tbl_CoursesExam> getbycourse(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesExam.Where(n => n.CourseID == courseid).ToList();
                return ck;
            }
        }      
    }
}