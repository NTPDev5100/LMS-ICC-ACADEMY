using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AccountTotalHourTeachingTable
    {
        //thêm dữ liệu
        public static tbl_AccountTotalHourTeaching insert(int type, int UID, int rank, int hoursOffline, int hoursOnline, int hoursrealOffline,int hoursrealOnline, int hoursabsentOffline,int hoursabsentOnline, int totalhour, int totalhouronline,
            int hoursCourseSelfStudy, int hoursCourseSelfStudyOnline, int totalCourseSelfStudy, int totalCourseSelfStudyOnline,
            int leaderhoursOffline, int leaderhoursOnline, int leaderhoursrealOffline, int leaderhoursrealOnline, int leaderhoursabsentOffline, int leaderhoursabsentOnline, int leadertotalhour, int leadertotalhouronline,
            int leaderhoursCourseSelfStudy, int leaderhoursCourseSelfStudyOnline, int leadertotalCourseSelfStudy, int leadertotalCourseSelfStudyOnline,
            string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_AccountTotalHourTeaching ck = new tbl_AccountTotalHourTeaching();
                ck.Type = type;//1 ngày, 2 tuần, 3 tháng, 4 quý, 5 năm
                if (type == 1)
                    ck.TypeName = "Ngày";
                if (type == 2)
                    ck.TypeName = "Tuần";
                if (type == 3)
                    ck.TypeName = "Tháng";
                if (type == 4)
                    ck.TypeName = "Quý";
                if (type == 5)
                    ck.TypeName = "Năm";
                ck.UID = UID;
                ck.DateRun = GetDateTime.Now.Date;
                ck.Rank = rank;
                ck.Hours = hoursOffline;
                ck.HoursOnline = hoursOnline;
                ck.RealHour = hoursrealOffline;
                ck.RealHourOnline = hoursrealOnline;
                ck.AbsentHour = hoursabsentOffline;
                ck.AbsentHourOnline = hoursabsentOnline;
                ck.TotalHour = totalhour;
                ck.TotalHourOnline = totalhouronline;
                ck.HoursCourseSelfStudy = hoursCourseSelfStudy;
                ck.HoursCourseSelfStudyOnline = hoursCourseSelfStudyOnline;
                ck.TotalHourCourseSelfStudy = totalCourseSelfStudy;
                ck.TotalHourCourseSelfStudyOnline = totalCourseSelfStudyOnline;

                ck.LeaderHoursOffline = leaderhoursOffline;
                ck.LeaderHoursOnline = leaderhoursOnline;
                ck.LeaderRealHour = leaderhoursrealOffline;
                ck.LeaderRealHourOnline = leaderhoursrealOnline;
                ck.LeaderAbsentHour = leaderhoursabsentOffline;
                ck.LeaderAbsentHourOnline = leaderhoursabsentOnline;
                ck.LeaderTotalHour = leadertotalhour;
                ck.LeaderTotalHourOnline = leadertotalhouronline;
                ck.LeaderHoursCourseSelfStudy = leaderhoursCourseSelfStudy;
                ck.LeaderHoursCourseSelfStudyOnline = leaderhoursCourseSelfStudyOnline;
                ck.LeaderTotalHourCourseSelfStudy = leadertotalCourseSelfStudy;
                ck.LeaderTotalHourCourseSelfStudyOnline = leadertotalCourseSelfStudyOnline;

                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AccountTotalHourTeaching.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_AccountTotalHourTeaching> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountTotalHourTeaching.Where(n => n.UID == uid).ToList();
                return l;
            }
        }

        public static List<tbl_AccountTotalHourTeaching> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountTotalHourTeaching.ToList();
                return l;
            }
        }

        public static tbl_AccountTotalHourTeaching getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountTotalHourTeaching.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
    }
}