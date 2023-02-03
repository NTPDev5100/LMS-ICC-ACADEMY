using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class NotificationOptionShcoolTable
    {
       /// <summary>
       /// Thêm dữ liệu
       /// </summary>
       /// <param name="notificationid">ID thông báo</param>
       /// <param name="schoolid">trung tâm nhận được</param>
       /// <param name="createby">người tạo</param>
       /// <returns></returns>
        public static tbl_NotificationOptionShcool insert(int notificationid, int schoolid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_NotificationOptionShcool n = new tbl_NotificationOptionShcool();
                n.NotificationID = notificationid;
                n.SchoolID = schoolid;
                n.isHide = false;               
                n.CreatedBy = n.ModifiedBy = createby;
                n.CreatedDate = n.ModifiedDate = GetDateTime.Now;
                db.tbl_NotificationOptionShcool.Add(n);
                db.SaveChanges();
                return n;
            }
        }
        public static tbl_NotificationOptionShcool getbynotivsschoolid(int notificationid, int schoolid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_NotificationOptionShcool l = db.tbl_NotificationOptionShcool.Where(n => n.NotificationID == notificationid && n.SchoolID == schoolid).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_NotificationOptionShcool> getbynotificationid(int notificationid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationOptionShcool.Where(n => n.NotificationID == notificationid).ToList();
                return l;
            }
        }
    }
}