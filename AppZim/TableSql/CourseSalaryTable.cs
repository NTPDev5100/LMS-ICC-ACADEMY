using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CourseSalaryTable
    {
        public static tbl_CourseSalary insert(int course, int numberoflesson, double salary, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CourseSalary ck = new tbl_CourseSalary();
                ck.CourseID = course;
                ck.NumberOfLesson = numberoflesson;
                ck.SalaryOfLesson = salary;
                ck.IsHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = DateTime.Now;
                db.tbl_CourseSalary.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CourseSalary update(int id, int numberoflesson, double salary, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CourseSalary ck = db.tbl_CourseSalary.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    
                    ck.NumberOfLesson = numberoflesson;
                    ck.SalaryOfLesson = salary;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        
        public static void DeleteByCourseId(int courseId, string createby)
        {
            using (var db = new ZimEntities())
            {
                List<tbl_CourseSalary> ck = db.tbl_CourseSalary.Where(n => n.CourseID == courseId).ToList();
                foreach(var i in ck)
                {
                    i.IsHide = true;
                    i.ModifiedBy = createby;
                    i.ModifiedDate = DateTime.Now;
                }
                db.SaveChanges();
            }
        }

        public static tbl_CourseSalary getbyId(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CourseSalary.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CourseSalary> getbycourse(int course)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CourseSalary.Where(n => n.CourseID == course && n.IsHide == false).OrderBy(x => x.ID).ToList();
                return l;
            }
        }
    }
}