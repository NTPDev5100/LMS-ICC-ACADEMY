using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    /// <summary>
    /// Bảng này lưu thông tin check random bài chấm của giáo viên do quản lý duyệt
    /// chỉ hện thị trên dashboard admin xem có bao nhiêu bài chấm ẩu
    /// </summary>
    public class ExerciseAddonStudentCheckFixTable
    {
        public static tbl_ExerciseAddonStudentCheckFix insert(int ExerciseAddonStudentID, int indexcount, int ExerciseAddonStudentID100END, int uidcheckfix,
            string checkfixname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentCheckFix ck = new tbl_ExerciseAddonStudentCheckFix();
                ck.ExerciseAddonStudentID = ExerciseAddonStudentID;
                ck.indexCount = indexcount;
                ck.indexName = "Bài random lần thứ: " + indexcount;
                ck.StatusAcceptReject = 1; // 1 chưa đánh giá, 2 accept, 3 reject
                ck.isHide = false;
                ck.ExerciseAddonStudentID100END = ExerciseAddonStudentID100END;
                ck.UIDCheckFix = uidcheckfix;
                ck.UIDCheckFixName = checkfixname;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentCheckFix.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_ExerciseAddonStudentCheckFix> getnew()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.StatusAcceptReject == 1).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }
        public static List<tbl_ExerciseAddonStudentCheckFix> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }

        public static List<tbl_ExerciseAddonStudentCheckFix> getbyindexcount(int index, int idcheck)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.indexCount == index && n.UIDCheckFix == idcheck).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudentCheckFix getbyExerciseAddonStudentID(int ExerciseAddonStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudentCheckFix getbyexerciseaddonstudentid(int ExerciseAddonStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudentCheckFix getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ExerciseAddonStudentCheckFix updateaccept(int ExerciseAddonStudentID, int StatusAcceptReject, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_ExerciseAddonStudentCheckFix.Where(n => n.ExerciseAddonStudentID == ExerciseAddonStudentID).FirstOrDefault();
                if (ck != null)
                {
                    ck.StatusAcceptReject = StatusAcceptReject;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}