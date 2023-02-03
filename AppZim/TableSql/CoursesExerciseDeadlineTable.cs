using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesExerciseDeadlineTable
    {
        //thêm dữ liệu
        public static tbl_CoursesExerciseDeadline insert(int courseid, int exerciseid, DateTime sd, DateTime ed, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExerciseDeadline ck = new tbl_CoursesExerciseDeadline();
                ck.CourseID = courseid;
                ck.ExerciseID = exerciseid;
                ck.sTime = sd.Date;
                ck.eTime = ed.Date;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesExerciseDeadline.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CoursesExerciseDeadline update(int id, DateTime sd, DateTime ed, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesExerciseDeadline ck = db.tbl_CoursesExerciseDeadline.Where(n => n.ID == id).FirstOrDefault();
                ck.sTime = sd.Date;
                ck.eTime = ed.Date;
                ck.isHide = false;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;            
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CoursesExerciseDeadline getbycourseid(int courseid, int exerciseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesExerciseDeadline.Where(n => n.CourseID == courseid && n.ExerciseID == exerciseid).FirstOrDefault();
                return l;
            }
        }
        public static tbl_CoursesExerciseDeadline GetByExerciseID(int exerciseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesExerciseDeadline.Where(n => n.ExerciseID == exerciseid && n.isHide == false).FirstOrDefault();
                return l;
            }
        }
    }
}