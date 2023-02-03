using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentCommentRedoTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonStudentCommentRedo insert(int StuExeID, string comment, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentCommentRedo ck = new tbl_ExerciseAddonStudentCommentRedo();
                ck.ExerciseAddonStudentID = StuExeID;
                ck.Comment = comment;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentCommentRedo.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonStudentCommentRedo> getbyexstudentid(int StuExeID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCommentRedo.Where(n => n.ExerciseAddonStudentID == StuExeID).ToList();
                return l;
            }
        }
    }
}