using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppZim.TableSql
{
    public class HistoryAttendanceTable
    {
        public static tbl_HistoryAttendance insert(int studentId, int courseScheduleId, string historyContent, int createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_HistoryAttendance ck = new tbl_HistoryAttendance();
                ck.StudentId = studentId;
                ck.CourseScheduleId = courseScheduleId;
                ck.HistoryContent = historyContent;
                ck.CreatedBy = createby;
                ck.CreatedDate = DateTime.Now;
                db.tbl_HistoryAttendance.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_HistoryAttendance> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_HistoryAttendance.ToList();
                return l;
            }
        }

        public static tbl_HistoryAttendance getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_HistoryAttendance.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}