using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentFileAudioTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonStudentFileAudio insert(int exestudentid, string linkfile, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentFileAudio ck = new tbl_ExerciseAddonStudentFileAudio();
                ck.ExerciseAddonStudentID = exestudentid;
                ck.LinkFile = linkfile;
                ck.ExerciseAddonStudentOnSetPackageID = 0;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentFileAudio.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ExerciseAddonStudentFileAudio insertOnSetPackage(int exestudentid,int onSetPackageID, string linkfile, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentFileAudio ck = new tbl_ExerciseAddonStudentFileAudio();
                ck.ExerciseAddonStudentID = exestudentid;
                ck.LinkFile = linkfile;
                ck.ExerciseAddonStudentOnSetPackageID = onSetPackageID;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentFileAudio.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonStudentFileAudio> getbyexerciseaddonstudentID(int exestudentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFileAudio.Where(n => n.ExerciseAddonStudentID == exestudentid).ToList();
                return l;
            }
        }
    }
}