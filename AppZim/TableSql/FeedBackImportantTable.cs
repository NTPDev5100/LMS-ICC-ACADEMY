using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class FeedBackImportantTable
    {
        public static tbl_FeedBackImportant insert(int UID, int FeedBackID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackImportant.Where(n => n.UID == UID && n.FeedBackID == FeedBackID).FirstOrDefault();
                if (l != null)
                {
                    l.IsHidden = !l.IsHidden;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return l;
                }
                else
                {
                    tbl_FeedBackImportant ck = new tbl_FeedBackImportant();
                    ck.UID = UID;
                    ck.FeedBackID = FeedBackID;
                    ck.IsHidden = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_FeedBackImportant.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static List<tbl_FeedBackImportant> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackImportant.Where(n => n.UID == uid).ToList();
                return l;
            }
        }

        public static tbl_FeedBackImportant getbyuidvsfeedback(int uid, int feedback)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackImportant.Where(n => n.UID == uid && n.FeedBackID == feedback).FirstOrDefault();
                return l;
            }
        }
    }
}