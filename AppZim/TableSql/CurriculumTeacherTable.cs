using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CurriculumTeacherTable
    {
        //thêm dữ liệu
        public static tbl_CurriculumTeacher insert(int courseid, DateTime date, int studytimeid, string linkdoc, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CurriculumTeacher ck = new tbl_CurriculumTeacher();
                ck.CourseID = courseid;
                ck.StudyTimeID = studytimeid;
                ck.Date = date.Date;
                ck.LinkDocument = linkdoc;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CurriculumTeacher.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_CurriculumTeacher getcurritecher(int courseid, DateTime date, int studytimeid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumTeacher.Where(n => n.CourseID == courseid && n.Date == date.Date && n.StudyTimeID == studytimeid && n.isHide != true).FirstOrDefault();
                return l;
            }
        }
    }
}