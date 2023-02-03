using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class NotificationCourseTable
    {
        //thêm dữ liệu
        public static tbl_NotificationCourse insert(string NotificationTitle, string NotificationContent, int courseid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_NotificationCourse n = new tbl_NotificationCourse();
                n.NotificationTitle = NotificationTitle;// tiêu đề
                n.NotificationContent = NotificationContent;// nội dung
                n.CourseID = courseid;               
                n.isHide = false;
                n.CreatedBy = n.ModifiedBy = createby;
                n.CreatedDate = n.ModifiedDate = GetDateTime.Now;
                db.tbl_NotificationCourse.Add(n);
                db.SaveChanges();
                return n;
            }
        }

        public static List<tbl_NotificationCourse> getbycourseid(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_NotificationCourse.Where(n => n.CourseID == courseid).OrderByDescending(x => x.ID).ToList();
                return l;
            }
        }
    }
}