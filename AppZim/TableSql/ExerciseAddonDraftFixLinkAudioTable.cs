using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonDraftFixLinkAudioTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonDraftFixLinkAudio insert(int exdraftid, int cmtdraftid, string linkfile, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonDraftFixLinkAudio ck = new tbl_ExerciseAddonDraftFixLinkAudio();
                ck.ExerciseAddonDraftID = exdraftid;
                ck.CmtDraftID = cmtdraftid;
                ck.LinkFile = linkfile;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonDraftFixLinkAudio.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonDraftFixLinkAudio> getbycmtid(int CmtDraftID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraftFixLinkAudio.Where(n => n.CmtDraftID == CmtDraftID).ToList();
                return l;
            }
        }

        public static List<tbl_ExerciseAddonDraftFixLinkAudio> getbyexedraftid(int exedraftid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraftFixLinkAudio.Where(n => n.ExerciseAddonDraftID == exedraftid).ToList();
                return l;
            }
        }
        public static void deletedata(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraftFixLinkAudio.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    db.tbl_ExerciseAddonDraftFixLinkAudio.Remove(l);
                    db.SaveChanges();
                }
            }
        }
    }
}