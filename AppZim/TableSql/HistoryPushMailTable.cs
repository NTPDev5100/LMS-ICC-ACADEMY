using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class HistoryPushMailTable
    {
        public static tbl_HistoryPushMail Insert(int courseId, int templateId, string typeName, DateTime ngaykhaigiang, string title, string content, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_HistoryPushMail ck = new tbl_HistoryPushMail();
                ck.CourseId = courseId;
                ck.EmailTemplateID = templateId;
                ck.TypeName = typeName;
                ck.NgayKhaiGiang = ngaykhaigiang;
                ck.EmailTitle = title;
                ck.EmailContent = content;
                ck.CreatedBy = CreatedBy;
                ck.CreatedDate = DateTime.Now;
                db.tbl_HistoryPushMail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_HistoryPushMail> GetAll()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_HistoryPushMail> ck = db.tbl_HistoryPushMail.ToList();
                return ck;
            }
        }
    }
}