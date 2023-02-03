using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class FeedBackCategoryTable
    {
        //thêm dữ liệu
        public static tbl_FeedBackCategory insert(int role, string rolename, string CategoryFeedback, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedBackCategory ck = new tbl_FeedBackCategory();
                ck.Role = role;
                ck.RoleName = rolename;
                ck.CategoryFeedback = CategoryFeedback;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_FeedBackCategory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_FeedBackCategory update(int ID, int role, string rolename, string CategoryFeedback, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedBackCategory ck = db.tbl_FeedBackCategory.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Role = role;
                    ck.RoleName = rolename;
                    ck.CategoryFeedback = CategoryFeedback;                   
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_FeedBackCategory getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCategory.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_FeedBackCategory> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBackCategory.ToList();
                return l;
            }
        }
    }
}