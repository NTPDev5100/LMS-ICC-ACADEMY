using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppZim.TableSql
{
    public class DesiredStudyTimeTable
    {
        /// <summary>
        /// DayOfWeekID
        /// 0 -- chủ nhật
        /// 1 -- thứ 2
        /// 2 -- thứ 3
        /// 3 -- thứ 4 
        /// 4 -- thứ 5
        /// 5 -- thứ 6
        /// 6 -- thứ 7
        /// </summary>
        /// <param name="desiredStudyTime"></param>
        /// <returns></returns>
        public static tbl_DesiredStudyTime insert(tbl_DesiredStudyTime desiredStudyTime, string user)
        {
            using (var db = new ZimEntities())
            {
                var studytime = db.tbl_StudyTime.SingleOrDefault(x => x.ID == desiredStudyTime.StudyTimeID);
                if (studytime == null) studytime = new tbl_StudyTime();
                var data = new tbl_DesiredStudyTime
                {
                    StudentAppointmentID = desiredStudyTime.StudentAppointmentID,
                    DayOfWeekID = desiredStudyTime.DayOfWeekID,
                    DayOfWeekName = desiredStudyTime.DayOfWeekID == 0 ? "Chủ nhật"
                                    : desiredStudyTime.DayOfWeekID == 1 ? "Thứ 2"
                                    : desiredStudyTime.DayOfWeekID == 2 ? "Thứ 3"
                                    : desiredStudyTime.DayOfWeekID == 3 ? "Thứ 4"
                                    : desiredStudyTime.DayOfWeekID == 4 ? "Thứ 5"
                                    : desiredStudyTime.DayOfWeekID == 5 ? "Thứ 6"
                                    : desiredStudyTime.DayOfWeekID == 6 ? "Thứ 7" : "",
                    StudyTimeID = desiredStudyTime.StudyTimeID,
                    StudyTimeName = studytime.StudyTimeName,
                    isHide = false,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user,
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = user
                };
                db.tbl_DesiredStudyTime.Add(data);
                db.SaveChanges();
                return data;
            }
        }
        public static List<tbl_DesiredStudyTime> getbyStudentAppointmentID(int studentAppointmentID)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_DesiredStudyTime
                    .Where(x => x.StudentAppointmentID == studentAppointmentID && x.isHide == false)
                    .ToList();
                return data;
            }
        }

        public static tbl_DesiredStudyTime Delete(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_DesiredStudyTime
                    .Where(x => x.ID == id && x.isHide == false)
                    .FirstOrDefault();
                if (data == null)
                    return null;
                data.isHide = true;
                db.SaveChanges();
                return data;
            }
        }
    }
}