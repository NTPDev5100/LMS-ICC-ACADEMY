using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesNoteTable
    {
        //thêm dữ liệu
        public static tbl_CoursesNote insert(int courseid, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesNote ck = new tbl_CoursesNote();
                ck.CourseID = courseid;
                ck.Note = note;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesNote.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_CoursesNote> getbycourseid(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesNote.Where(n => n.CourseID == courseid).OrderByDescending(n => n.CreatedDate).ToList();
                return l;
            }
        }
        public static tbl_CoursesNote insertByAPI(int courseid, string note, DateTime date, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesNote ck = new tbl_CoursesNote();
                ck.CourseID = courseid;
                ck.Note = note;
                ck.Date = date;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesNote.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_CoursesNote updateByAPI(int id, string note, DateTime date, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CoursesNote ck = db.tbl_CoursesNote.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Note = note;
                    ck.Date = date;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }
    }
}