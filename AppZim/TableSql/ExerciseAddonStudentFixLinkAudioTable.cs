using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonStudentFixLinkAudioTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonStudentFixLinkAudio insert(int exestudentid, int cmtid, string linkfile, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonStudentFixLinkAudio ck = new tbl_ExerciseAddonStudentFixLinkAudio();
                ck.ExerciseAddonStudentID = exestudentid;
                ck.CmtID = cmtid;
                ck.LinkFile = linkfile;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonStudentFixLinkAudio.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonStudentFixLinkAudio> getbycmtid(int cmtid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonStudentFixLinkAudio.Where(n => n.CmtID == cmtid).ToList();
                return l;
            }
        }
    }
}