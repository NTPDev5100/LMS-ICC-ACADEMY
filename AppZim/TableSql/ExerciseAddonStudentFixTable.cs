using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentFixTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonStudentFix insert(int ExerciseAddonStudentID, string classcmt, string notefix, string link, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentFix ck = new tbl_ExerciseAddonStudentFix();
                ck.ExerciseAddonStudentID = ExerciseAddonStudentID;
                ck.DataCMT = classcmt;
                ck.FixNote = notefix;
                ck.LinkAudio = link;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentFix.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonStudentFix> getbyexestudent(int ExerciseAddonStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFix.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID && n.isHide != true).ToList();
                return l;
            }
        }

        public static void updatehide(int ExerciseAddonStudentID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFix.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID && n.isHide != true).ToList();
                foreach(var item in l)
                {
                    item.isHide = true;
                    item.ModifiedBy = createby;
                    item.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
        public static void Update(int commentID, string comment, string userName)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFix.Where(n => n.ID == commentID && n.isHide != true).FirstOrDefault();
                if (l != null)
                {
                    l.FixNote = comment;
                    l.ModifiedBy = userName;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
        public static void Delete(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFix.Where(n => n.ID == id && n.isHide != true).FirstOrDefault();
                if (l != null)
                {
                    l.isHide = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
    }
}