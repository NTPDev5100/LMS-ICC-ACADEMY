using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SalaryTeacherOnlineTable
    {
        public static tbl_SalaryTeacherOnline Insert(int TeacherUID, int CourseID, string CourseName, int PersentSalarySetting, int Year, int Month, bool PersentCourseType, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryTeacherOnline ck = new tbl_SalaryTeacherOnline();
                ck.TeacherUID = TeacherUID;
                ck.CourseID = CourseID;
                ck.CourseName = CourseName;
                ck.PersentSalarySetting = PersentSalarySetting; //phần trăm cấu hình
                ck.PersentCourseType = PersentCourseType;
                if (PersentCourseType == true)
                {
                    ck.PersentCourseName = "Thanh toán lương 100%";
                }
                else
                {
                    ck.PersentCourseName = "Thanh toán lương 50%";
                }
                ck.Year = Year; //lương năm hiện tại
                ck.Month = Month; //lương tháng hiện tại
                ck.CreatedBy = CreatedBy;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_SalaryTeacherOnline.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        //public static tbl_SalaryTeacherOnline Update(int ID, int PersentSalarySetting, int TotalSalary)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        tbl_SalaryTeacherOnline ck = db.tbl_SalaryTeacherOnline.Where(x => x.ID == ID).FirstOrDefault();
        //        if (ck != null)
        //        {
        //            ck.PersentSalarySetting = PersentSalarySetting; //phần trăm cấu hình
        //            ck.ModifiedBy = "Auto";
        //            ck.ModifiedDate = GetDateTime.Now;
        //            db.SaveChanges();
        //        }
        //        return ck;
        //    }
        //}
        public static tbl_SalaryTeacherOnline GetByTeacherUID(int TeacherUID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryTeacherOnline.Where(n => n.TeacherUID == TeacherUID).FirstOrDefault();
                return l;
            }
        }
        public static tbl_SalaryTeacherOnline GetByTeacherUIDYearMonth(int TeacherUID, int Year, int Month)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryTeacherOnline.Where(n => n.TeacherUID == TeacherUID && n.Year == Year && n.Month == Month).FirstOrDefault();
                return l;
            }
        }
    }
}