using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SalaryPaidHistoryTable
    {
        //public static tbl_SalaryPaidHistory insertsaler(tbl_Account user, string monthsalary, int campid, int salarysaler, int countzim, int salaryzim, int countuid,
        //    int settingsalaryid, int addition, string note, string createby)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        tbl_SalaryPaidHistory ck = new tbl_SalaryPaidHistory();
        //        ck.UID = user.ID;
        //        ck.SalaryMonth = monthsalary;
        //        int total = salarysaler;
        //        if (user.RoleID == 6)//tính lương saler
        //        {
        //            ck.CampaignID = campid;// id chiến dịch
        //            ck.SalarySaler = salarysaler;// lương cứng saler
        //            // tính theo nguồn hệ thống
        //            ck.CountSourceZim = countzim;// số lượng học viên nguồn hệ thống
        //            ck.SalarySourceZim = salaryzim;//số tiền tính lương nguồn hệ thống 400.000
        //            ck.TotalSourceZim = countzim * salaryzim;// ra lương
        //            total += ck.TotalSourceZim.Value;
        //            // tính theo nguồn của TVV
        //            ck.CountSourceUID = countuid;// số học viên nguồn TVV
        //            var setting = db.tbl_SalarySetting.Where(n => n.ID == settingsalaryid).FirstOrDefault();// cấu hình lương theo tổng doanh thu
        //            ck.SalarySettingID = setting.ID;// id cấu hình lương theo doanh thu
        //            ck.SalarySettingPercent = setting.PercentSalary;// phần trăm
        //            ck.TotalSourceUID = Math.Round((setting.PercentSalary.Value / 100f) * salarysaler, 0).ToString().ToInt(0);// tính lương nguồn tvv
        //            total += ck.TotalSourceUID.Value;
        //        }
        //        ck.AdditionFee = addition;// thêm thưởng
        //        total += addition;
        //        ck.Note = note;// ghi chú lại
        //        ck.PaidReal = total;// lương thanh toán thực tế
        //        ck.isHide = false;// trạng thái
        //        ck.CreatedBy = ck.ModifiedBy = createby;
        //        ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
        //        db.tbl_SalaryPaidHistory.Add(ck);
        //        db.SaveChanges();
        //        return ck;
        //    }
        //}
        public static tbl_SalaryPaidHistory CalcSaler(tbl_Account user, string monthsalary, int campid, int salarysaler, int addition, string note, string createby, int doanhthu)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryPaidHistory ck = new tbl_SalaryPaidHistory();
                ck.UID = user.ID;
                ck.SalaryMonth = monthsalary;
                //int total = salarysaler;
                if (user.RoleID == 6)//tính lương saler
                {
                    ck.CampaignID = campid;
                    ck.SalarySaler = salarysaler;
                    // lay phan tram thuong + tinh tien thuong
                    double ptThuong = SalarySettingTable.getPercent(doanhthu);
                    var tienthuong = doanhthu * ptThuong / 100;
                    // phần trăn hoa hồng cấu hình + tien hoa hong cau hinh
                    double ptHoaHong = 0;
                    var checkHH = StaffSalaryRoseSellerTable.GetByUID(user.ID);
                    if(checkHH != null)
                    {
                        ptHoaHong = checkHH.SalaryPresent.Value;
                    }
                    var tienHoaHong = tienthuong * ptHoaHong / 100;
                    ck.PercentBonus = ptThuong;
                    ck.TotalBonus = tienthuong;
                    ck.PercentTip = ptHoaHong;
                    ck.TotalTip = tienHoaHong;
                }
                ck.AdditionFee = addition;// thêm thưởng
                //total += addition;
                ck.Note = note;// ghi chú lại
                // lương thanh toán thực tế = luong cung + thuong + hoa hong + thêm thưởng
                ck.PaidReal = salarysaler + ck.TotalBonus + ck.TotalTip + ck.AdditionFee;
                ck.isHide = false;// trạng thái
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SalaryPaidHistory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_SalaryPaidHistory insertteacher(tbl_Account user, string monthsalary, int salarylessontecher, int countlesson,
            int addition, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryPaidHistory ck = new tbl_SalaryPaidHistory();
                ck.UID = user.ID;
                ck.SalaryMonth = monthsalary;
                int total = 0;
                if (user.RoleID == 4)//giáo viên
                {
                    ck.CountLesson = countlesson;// tổng tiết dạy
                    ck.SalaryLessonTeacher = salarylessontecher;//lương trên 1 tiết
                    ck.TotalLesson = countlesson * salarylessontecher;//số lương
                    total += ck.TotalLesson.Value;
                }
                ck.AdditionFee = addition;// thêm thưởng
                total += addition;
                ck.Note = note;// ghi chú lại
                ck.PaidReal = total;// lương thanh toán thực tế
                ck.isHide = false;// trạng thái
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SalaryPaidHistory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_SalaryPaidHistory insertstaff(tbl_Account user, string monthsalary, int salarywork, int workday, int addition, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalaryPaidHistory ck = new tbl_SalaryPaidHistory();
                ck.UID = user.ID;
                ck.SalaryMonth = monthsalary;
                int total = 0;
                if (user.RoleID == 2 || user.RoleID == 3 || user.RoleID == 7)//manager, lễ tân, học vụ
                {
                    ck.SalaryWorkStaff = salarywork;// lương tháng
                    ck.WorkDay = workday;// ngày công thực tế
                    int sday = Math.Round(salarywork / 26f, 0).ToString().ToInt();// 26 ngày công chuẩn
                    ck.TotalWork = workday * sday;//số lương
                    total += ck.TotalWork.Value;
                }
                ck.AdditionFee = addition;// thêm thưởng
                total += addition;
                ck.Note = note;// ghi chú lại
                ck.PaidReal = total;// lương thanh toán thực tế
                ck.isHide = false;// trạng thái
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SalaryPaidHistory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_SalaryPaidHistory getbyuidvscampid(int UID, int CampID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryPaidHistory.Where(n => n.UID == UID && n.CampaignID == CampID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_SalaryPaidHistory> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalaryPaidHistory.ToList();
                return l;
            }
        }
    }
}