using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class PostCommentTable
    {
        /// <summary>
        /// Chèn bình luận bài viết
        /// </summary>
        /// <param name="postid"></param>
        /// <param name="uid"></param>
        /// <param name="content"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_PostComment insert(int postid, int postcmtid, int uid, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostComment ck = new tbl_PostComment();
                ck.PostContentID = postid;
                ck.PostCommentID = postcmtid;
                ck.UID = uid;
                ck.CMT = content;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_PostComment.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// Lấy tất cả cmt theo bài viết
        /// </summary>
        /// <param name="postid"></param>
        /// <returns></returns>
        public static List<tbl_PostComment> getbypostid(int postid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostComment.Where(n => n.PostContentID == postid).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy cmt con của cmt
        /// </summary>
        /// <param name="postcmtid"></param>
        /// <returns></returns>
        public static List<tbl_PostComment> getbypostcmtid(int postcmtid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostComment.Where(n => n.PostCommentID == postcmtid).ToList();
                return l;
            }
        }
        public static tbl_PostComment getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostComment.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Lấy rra những cmt mới nhất
        /// </summary>
        /// <returns></returns>
        public static List<tbl_PostComment> gettopcmt(int role)
        {
            using (var db = new ZimEntities())
            {
                IEnumerable<tbl_PostComment> l = db.tbl_PostComment.ToList();
                IEnumerable<tbl_PostContent> p = db.tbl_PostContent.Where(n => n.Status == 3).ToList();
                if (role == 5)
                    p = db.tbl_PostContent.Where(n => n.isHideStudent != true && n.StatusPost != true).ToList();
                else
                    p = db.tbl_PostContent.Where(n => n.isHideStaff != true && n.StatusPost != true).ToList();

                l = from x in l
                    join o in p on x.PostContentID equals o.ID
                    select x;
                List<tbl_PostComment> ol = l.OrderByDescending(n => n.CreatedDate).Skip(0).Take(10).ToList();
                return ol;
            }
        }
        public static List<tbl_PostComment> gettopcmt_group(int groupID,int page)
        {
            using (var db = new ZimEntities())
            {
                IEnumerable<tbl_PostComment> l = db.tbl_PostComment.ToList();
                IEnumerable<tbl_PostContent> p = db.tbl_PostContent.Where(n=>n.GroupID==groupID).OrderByDescending(x => x.CreatedDate).Skip((page-1)*12).Take(12).ToList();
                l = from x in l
                    join o in p on x.PostContentID equals o.ID
                    select x;
                List<tbl_PostComment> ol = l.OrderByDescending(n => n.CreatedDate).Skip(0).Take(10).ToList();
                return ol;
            }
        }
    }
}