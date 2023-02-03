using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class FeedBackDetailTable
    {
        //thêm dữ liệu
        public static tbl_FeedBackDetail insert(int FeedbackID, string ContentFeedback, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedBackDetail ck = new tbl_FeedBackDetail();
                ck.FeedBackID = FeedbackID;
                ck.ContentFeedback = ContentFeedback;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_FeedBackDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_FeedBackDetail> getbyfeedbackid(int FeedbackID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackDetail.Where(n => n.FeedBackID == FeedbackID).ToList();
                return l;
            }
        }
    }
}