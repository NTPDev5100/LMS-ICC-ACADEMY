using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ExerciseAddonDraftFixTable
    {
        //thêm dữ liệu
        public static tbl_ExerciseAddonDraftFix insert(int ExerciseAddonDraftID, string classcmt, string notefix, string link, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ExerciseAddonDraftFix ck = new tbl_ExerciseAddonDraftFix();
                ck.ExerciseAddonDraftID = ExerciseAddonDraftID;
                ck.DataCMT = classcmt;
                ck.NumberaCMT = classcmt.Replace("cmt-", "").ToInt(0);
                ck.FixNote = notefix;
                ck.LinkAudio = link;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ExerciseAddonDraftFix.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ExerciseAddonDraftFix> getbyexedraftid(int exedraftid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraftFix.Where(n => n.ExerciseAddonDraftID == exedraftid && n.isHide != true).ToList();
                return l;
            }
        }
        public static void deletedata(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ExerciseAddonDraftFix.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    db.tbl_ExerciseAddonDraftFix.Remove(l);
                    db.SaveChanges();
                }
            }
        }
    }
}