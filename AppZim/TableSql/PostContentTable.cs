using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class PostContentTable
    {
        public static tbl_PostContent insert(string title, string imgpost, string imgpostthumbnail, string content, string createby, bool poststatus, bool background, int groupID,int fileType)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = new tbl_PostContent();
                ck.TitlePost = title;
                ck.PostIMG = imgpost;
                ck.PostIMGThumbnail = imgpostthumbnail;
                ck.ContentPost = content;
                ck.isHideStaff = false;
                ck.isHideStudent = false;
                ck.NumberView = 0;
                ck.GroupID = groupID;
                ck.PostType = fileType;
                ck.Status = 1;// 1 chờ duyệt 2, không duyệt, 3 duyệt, 4 xóa
                ck.StatusPost = poststatus;// true là post ngắn, false or null post blog
                ck.BackgroundPost = background;//true thì chữ trên hình, false hình riêng chữ riêng
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_PostContent.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_PostContent update(int id, string title, string imgpost, string imgpostthumbnail, string content, string createby, bool poststatus, bool background)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        ck.TitlePost = title;
                    if (!string.IsNullOrEmpty(imgpost))
                    {
                        ck.PostIMG = imgpost;
                        ck.PostIMGThumbnail = imgpostthumbnail;
                    }
                    if (!string.IsNullOrEmpty(content))
                        ck.ContentPost = content;
                    //ck.StatusPost = poststatus;// true là post ngắn, false or null post blog
                    //ck.BackgroundPost = background;//true thì chữ trên hình, false hình riêng chữ riêng
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Lấy bài post theo id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static tbl_PostContent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        /// <summary>
        /// Lấy tất cả dữ liệu
        /// </summary>
        /// <returns></returns>
        public static List<tbl_PostContent> getbyall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostContent.ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy bài post của group
        /// </summary>
        /// <param name="id"></param>
        /// <param name="title"></param>
        /// <param name="imgpost"></param>
        /// <param name="content"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        /// 

        public static tbl_PostContent update(int id, string title, string imgpost, string imgpostthub, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (!string.IsNullOrEmpty(title))
                        ck.TitlePost = title;
                    if (!string.IsNullOrEmpty(imgpost))
                    {
                        ck.PostIMG = imgpost;
                        ck.PostIMGThumbnail = imgpostthub;
                    }
                    ck.ContentPost = content;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Cập nhât trạng thái duyệt
        /// </summary>
        /// <param name="id"></param>
        /// <param name="status"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_PostContent updatestatus(int id, int status, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Ẩn hiện bài trên trang chính
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hide"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_PostContent updatehideshowstaff(int id, bool hide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHideStaff = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Ẩn hiện bài trên trang chính
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hide"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_PostContent updatehideshowstudent(int id, bool hide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHideStudent = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// số lượng lượt xem bài
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static tbl_PostContent updatenumberview(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.NumberView = ck.NumberView.Value + 1;
                    db.SaveChanges();
                }
                return ck;
            }
        }/// <summary>
         /// Xóa status
         /// </summary>
         /// <param name="id"></param>
         /// <returns></returns>
        public static tbl_PostContent deletestatus(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id && n.StatusPost == true).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = 4;//xóa
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Lấy bài theo group
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public static List<tbl_PostContent> GetByGroupID(int groupID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_PostContent.Where(n => n.GroupID == groupID && n.Status != 4).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy số lượng bài mới trong 2 ngày
        /// </summary>
        /// <param name="groupID"></param>
        /// <returns></returns>
        public static int GetPostNewByGroupID(int groupID)
        {
            using (var db = new ZimEntities())
            {
                DateTime dt = GetDateTime.Now.AddDays(-1);
                var l = db.tbl_PostContent.Where(n => n.GroupID == groupID && n.Status != 4 && n.CreatedDate > dt).ToList();
                return l.Count;
            }
        }
        //dùng 1 lần rồi xóa
        public static tbl_PostContent updatethumbnail(int id, string thumbnail)
        {
            using (var db = new ZimEntities())
            {
                tbl_PostContent ck = db.tbl_PostContent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.PostIMGThumbnail = thumbnail;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}