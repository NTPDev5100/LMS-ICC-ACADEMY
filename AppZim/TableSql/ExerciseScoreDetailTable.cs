using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseScoreDetailTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseScoreDetail insert(int exstudentid, string scorename, double score, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseScoreDetail ck = new tbl_ExerciseScoreDetail();
                ck.ExerciseStudentID = exstudentid;
                ck.Score = Math.Round(score, 2);
                ck.ScoreName = scorename;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseScoreDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseScoreDetail> getbyexeid(int ExerciseStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseScoreDetail.Where(n => n.ExerciseStudentID == ExerciseStudentID && n.isHide != true).ToList();
                return l;
            }
        }

        public static void updatehide(int ExerciseStudentID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseScoreDetail.Where(n => n.ExerciseStudentID == ExerciseStudentID && n.isHide != true).ToList();
                foreach(var item in l)
                {
                    item.isHide = true;
                    item.ModifiedBy = createby;
                    item.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
    }
}