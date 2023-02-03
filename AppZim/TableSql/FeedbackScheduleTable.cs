using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppZim.TableSql
{
    public class FeedbackScheduleTable
    {
        public static tbl_FeedbackSchedule Insert(int studentID, int courseScheduleID, int courseID, string feedbackContent,
            int scheduleStatus, int rating, string createBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedbackSchedule ck = new tbl_FeedbackSchedule();
                ck.StudentID = studentID;
                ck.CourseScheduleID = courseScheduleID;
                ck.ScheduleStatus = scheduleStatus;
                ck.CourseID = courseID;
                switch (scheduleStatus)
                {
                    case 2:
                        ck.ScheduleStatusName = "Giáo viên vắng mặt";
                        break;
                    case 3:
                        ck.ScheduleStatusName = "Mất mạng/ mất điện";
                        break;
                    default:
                        ck.ScheduleStatusName = "Tốt";
                        break;
                }

                ck.FeedbackContent = feedbackContent;
                ck.Rating = rating;
                ck.CreatedBy = createBy;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_FeedbackSchedule.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_FeedbackSchedule Update(int id, string feedbackContent,
            int scheduleStatus, int rating, string createBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedbackSchedule ck = db.tbl_FeedbackSchedule.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.ScheduleStatus = scheduleStatus;
                    switch (scheduleStatus)
                    {
                        case 2:
                            ck.ScheduleStatusName = "Giáo viên vắng mặt";
                            break;
                        case 3:
                            ck.ScheduleStatusName = "Mất mạng/ mất điện";
                            break;
                        default:
                            ck.ScheduleStatusName = "Tốt";
                            break;
                    }

                    ck.FeedbackContent = feedbackContent;
                    ck.Rating = rating;
                    ck.ModifiedBy = createBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_FeedbackSchedule getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedbackSchedule.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_FeedbackSchedule> GetByCourseScheduleID(int courseScheduleID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedbackSchedule.Where(n => n.CourseScheduleID == courseScheduleID).ToList();
                return l;
            }
        }
        //lấy feedback theo lịch học và học viên
        public static tbl_FeedbackSchedule GetByCourseScheduleIDAndStudentID(int courseScheduleID, int studentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedbackSchedule.Where(n => n.CourseScheduleID == courseScheduleID && n.StudentID == studentID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_FeedbackSchedule> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedbackSchedule.ToList();
                return l;
            }
        }
        public static int GetRating(int courseID, int rate, string sdate, string edate)
        {
            using (var db = new ZimEntities())
            {
                var l = db.API_GetListRating(courseID, rate, sdate, edate).FirstOrDefault();
                return l.Value;
            }
        }
        //tính rate trung bình để xuất excel
        public static double CalcRating(int courseID)
        {
            using(var db = new ZimEntities())
            {
                double rate = 0.0;
                var sumRate = 0.0;
                var lRate = db.tbl_FeedbackSchedule.Where(x => x.CourseID == courseID).ToList();
                foreach (var item in lRate)
                {
                    sumRate += (double)item.Rating;
                }
                if(lRate.Count > 0)
                {
                    rate = Math.Round((sumRate / lRate.Count), 1);
                }
                return rate;
            }
        }
    }
}