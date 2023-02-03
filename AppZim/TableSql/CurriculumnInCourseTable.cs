using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CurriculumnInCourseTable
    {
        //thêm dữ liệu
        public static tbl_CurriculumnInCourse insert(int courseId, int curriculumnId, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CurriculumnInCourse ck = db.tbl_CurriculumnInCourse.Where(n => n.CourseID == courseId && n.CurriculumnID == curriculumnId).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    ck = new tbl_CurriculumnInCourse();
                    ck.CourseID = courseId;
                    ck.CurriculumnID = curriculumnId;
                    ck.isHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = DateTime.Now;
                    db.tbl_CurriculumnInCourse.Add(ck);
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static List<tbl_CurriculumnInCourse> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumnInCourse.Where(x => x.isHide == false).ToList();
                return l;
            }
        }
        
        public static IEnumerable<tbl_Curriculum> getByCourseId(int courseId)
        {
            using (var db = new ZimEntities())
            {
                IEnumerable<tbl_Curriculum> l = db.tbl_CurriculumnInCourse.Where(x => x.isHide == false && x.CourseID == courseId)
                    .Join(db.tbl_Curriculum, 
                    CI => CI.CurriculumnID, 
                    C => C.ID, 
                    (CI, C) => new { C.ID, C.CurriculumsName, C.LinkCurriculumn }).ToList()
                    .Select(x => new tbl_Curriculum { ID = x.ID, CurriculumsName = x.CurriculumsName, LinkCurriculumn = x.LinkCurriculumn});
                return l;
            }
        }

        public static tbl_CurriculumnInCourse getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CurriculumnInCourse.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_CurriculumnInCourse Delete(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CurriculumnInCourse ck = db.tbl_CurriculumnInCourse.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = true;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}