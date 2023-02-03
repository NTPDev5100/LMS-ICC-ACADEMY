using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    /// <summary>
    /// Bảng này lưu thông tin check phản hồi của tư vấn viên do quản lý duyệt   
    /// </summary>
    public class FeedBackCheckSolutionTable
    {
        public static tbl_FeedBackCheckSolution insert(int FeedBackID, int indexcount, int FeedBackID100END, int uidcheckfix,
           string checkfixname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedBackCheckSolution ck = new tbl_FeedBackCheckSolution();
                ck.FeedBackID = FeedBackID;
                ck.indexCount = indexcount;
                ck.indexName = "Bài random lần thứ: " + indexcount;
                ck.StatusAcceptReject = 1; // 1 chưa đánh giá, 2 accept, 3 reject
                ck.isHide = false;
                ck.FeedBackID100END = FeedBackID100END;
                ck.UIDCheckFix = uidcheckfix;
                ck.UIDCheckFixName = checkfixname;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_FeedBackCheckSolution.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_FeedBackCheckSolution> getnew()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCheckSolution.Where(n => n.StatusAcceptReject == 1).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }
        public static List<tbl_FeedBackCheckSolution> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCheckSolution.OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }

        public static List<tbl_FeedBackCheckSolution> getbyindexcount(int index, int idcheck)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCheckSolution.Where(n => n.indexCount == index && n.UIDCheckFix == idcheck).OrderByDescending(n => n.ID).ToList();
                return l;
            }
        }

        public static tbl_FeedBackCheckSolution getbyFeedBackID(int FeedBackID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCheckSolution.Where(n => n.FeedBackID == FeedBackID).FirstOrDefault();
                return l;
            }
        }        

        public static tbl_FeedBackCheckSolution getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCheckSolution.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_FeedBackCheckSolution updateaccept(int FeedBackID, int StatusAcceptReject, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_FeedBackCheckSolution.Where(n => n.FeedBackID == FeedBackID).FirstOrDefault();
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