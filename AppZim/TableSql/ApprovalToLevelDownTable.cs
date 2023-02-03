using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ApprovalToLevelDownTable
    {
        public static tbl_ApprovalToLevelDown insert(int uid, double revenueInMonth, int positionCurrent, int positionChange, int campaignId)
        {
            using (var db = new ZimEntities())
            {
                tbl_ApprovalToLevelDown ck = new tbl_ApprovalToLevelDown();
                ck.UID = uid;
                ck.Revenue = revenueInMonth;
                ck.CurrentRank = positionCurrent;
                ck.NewRank = positionChange;
                ck.campaignId = campaignId;
                ck.Status = 0;
                ck.Note = "";
                ck.CreatedBy = "Auto";
                ck.CreatedDate = DateTime.Now;
                ck.IsSendMail = false;
                db.tbl_ApprovalToLevelDown.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_ApprovalToLevelDown update(int id, int status, string note, bool? isSendMail, string modifiedby, int level)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_ApprovalToLevelDown.Where(x => x.ID == id && x.Status == 0).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;
                    ck.Note = note;
                    ck.IsSendMail = isSendMail == null ? ck.IsSendMail : isSendMail;
                    ck.ModifiedBy = modifiedby;
                    ck.ModifiedDate = DateTime.Now;
                    ck.NewRank = level;
                    db.SaveChanges();
                    if (ck.Status == 1)
                        AccountTable.UpdatePosition(ck.UID.Value, level);
                    return ck;
                }
                return null;
            }
        }
    }
}