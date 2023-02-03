using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SalaryTeacherOfflineTable
    {
        public static tbl_SalaryTeacherOffline Insert(int TeacherUID, int SalarySettingOffline, int TotalHourOffline, int Year, int Month, int TotalSalary)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryTeacherOffline ck = new tbl_SalaryTeacherOffline();
                ck.TeacherUID = TeacherUID;
                ck.TotalHourOffline = TotalHourOffline; //tổng giờ dạy offline
                ck.Year = Year; //lương năm hiện tại
                ck.Month = Month; //lương tháng hiện tại
                ck.SalarySettingOffline = SalarySettingOffline; //lương/giờ
                ck.TotalSalary = TotalSalary; //tổng lương
                ck.CreatedBy = "Auto";
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_SalaryTeacherOffline.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_SalaryTeacherOffline Update(int ID, int SalarySettingOffline, int TotalHourOffline, int TotalSalary)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryTeacherOffline ck = db.tbl_SalaryTeacherOffline.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.TotalHourOffline = TotalHourOffline; //tổng giờ dạy offline
                    ck.SalarySettingOffline = SalarySettingOffline; //lương/giờ
                    ck.TotalSalary = TotalSalary; //tổng lương
                    ck.ModifiedBy = "Auto";
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_SalaryTeacherOffline getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryTeacherOffline.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static tbl_SalaryTeacherOffline GetByTeacherUID(int TeacherUID, int Year, int Month)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryTeacherOffline.Where(n => n.TeacherUID == TeacherUID && n.Year == Year && n.Month == Month).FirstOrDefault();
                return l;
            }
        }
    }
}