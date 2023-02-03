using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppZim.TableSql
{
    public class HistoryUploadVideoTable
    {
        public static tbl_HistoryUploadVideo insert(int courseScheduleId, string historyContent, int createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_HistoryUploadVideo ck = new tbl_HistoryUploadVideo();
                ck.CourseScheduleId = courseScheduleId;
                ck.HistoryContent = historyContent;
                ck.CreatedBy = createby;
                ck.CreatedDate = DateTime.Now;
                db.tbl_HistoryUploadVideo.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_HistoryUploadVideo> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_HistoryUploadVideo.ToList();
                return l;
            }
        }

        public static tbl_HistoryUploadVideo getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_HistoryUploadVideo.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}