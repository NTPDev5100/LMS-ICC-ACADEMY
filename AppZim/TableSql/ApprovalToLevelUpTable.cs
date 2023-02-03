using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ApprovalToLevelUpTable
    {
        public static tbl_ApprovalToLevelUp insert(int uid, double revenue, int positionOld, int positionNew)
        {
            using (var db = new ZimEntities())
            {
                tbl_ApprovalToLevelUp ck = new tbl_ApprovalToLevelUp();
                ck.UID = uid;
                ck.Revenue = revenue;
                ck.CurrentRank = positionNew;
                ck.OldRank = positionOld;
                ck.Status = 0;
                ck.Note = "";
                ck.CreatedBy = "Auto";
                ck.CreatedDate = DateTime.Now;
                ck.IsSendMail = false;
                db.tbl_ApprovalToLevelUp.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_ApprovalToLevelUp update(int id, int status, string note, bool? isSendMail, string createby, int level)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_ApprovalToLevelUp.Where(x => x.ID == id && x.Status == 0).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;
                    ck.Note = note;
                    ck.IsSendMail = isSendMail == null ? ck.IsSendMail : isSendMail;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = DateTime.Now;
                    ck.CurrentRank = level;
                    db.SaveChanges();
                    if (ck.Status == 1)
                        AccountTable.UpdatePosition(ck.UID.Value, level);
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_ApprovalToLevelUp> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ApprovalToLevelUp.ToList();
                return l;
            }
        }

        public static tbl_ApprovalToLevelUp getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ApprovalToLevelUp.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_ApprovalToLevelUp CheckExists(int UID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_ApprovalToLevelUp.Where(n => n.UID == UID && n.Status == 0).FirstOrDefault();
                return a;
            }
        }
    }
}