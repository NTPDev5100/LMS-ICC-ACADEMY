using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SalarySettingTable
    {
        //thêm dữ liệu
        public static tbl_SalarySetting insert(string positionName, double million, double millionDown, double percent, string createby, int type)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalarySetting ck = new tbl_SalarySetting();
                ck.PositionName = positionName;
                ck.Million = million;
                ck.MinimumMonthlyRevenue = millionDown;
                ck.PercentSalary = percent;
                ck.SalaryType = type;//tạm thời k có ý nghĩa gì
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SalarySetting.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }


        public static tbl_SalarySetting update(int ID, string positionName, double million, double millionDown, double percent, string createby, int type)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalarySetting ck = db.tbl_SalarySetting.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.PositionName = positionName;
                    ck.Million = million;
                    ck.MinimumMonthlyRevenue = millionDown;
                    ck.PercentSalary = percent;
                    ck.SalaryType = type;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        
        public static tbl_SalarySetting Delete(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SalarySetting ck = db.tbl_SalarySetting.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = true;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_SalarySetting getbyid(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalarySetting.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_SalarySetting> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SalarySetting.Where(x => x.isHide == false).ToList();
                return l;
            }
        }
        // input tong luong -> phan trang hoa hong
        public static double getPercent(int doanhthu)
        {
            using (var db = new ZimEntities())
            {
                double pt = 0;
                var l = db.tbl_SalarySetting.Where(x => x.Million < doanhthu).OrderByDescending(x => x.Million).FirstOrDefault();
                if(l != null)
                {
                    pt = l.PercentSalary.Value;
                }
                return pt;
            }
        }

        public static List<Admin_SalarySetting_NotConfigSalary_Result> GetNotConfigSalary()
        {
            using (var db = new ZimEntities())
            {
                var l = db.Admin_SalarySetting_NotConfigSalary().ToList();
                return l;
            }
        }
    }
}