using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class TempPayrollTable
    {
        public static tbl_TempPayroll insert(int uid, int campaignId, string fullname, double basicSalary, 
            double revenue, double revenuePercent, double bonusSalary, double deductedSalary, int careQuality,
            double carePrice, int carePlusQuality, double carePlusPrice, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_TempPayroll ck = new tbl_TempPayroll();
                ck.UID = uid;
                ck.CampaignID = campaignId;
                ck.FullName = fullname;
                ck.BasicSalary = basicSalary;
                ck.Revenue = revenue;
                ck.RevenuePercent = revenuePercent;
                ck.BonusSalary = bonusSalary;
                ck.DeductedSalary = deductedSalary;
                ck.CareQuantity = careQuality;
                ck.CarePrice = carePrice;
                ck.CarePlusQuantity = carePlusQuality;
                ck.CarePlusPrice = carePlusPrice;
                ck.Note = note;
                ck.IsDone = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_TempPayroll.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_TempPayroll update(int id, double bonusSalary, double deductedSalary, string note
            , string modifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_TempPayroll.Where(x => x.ID == id && x.IsDone == false).FirstOrDefault();
                if (ck != null)
                {
                    ck.Note = note;
                    ck.BonusSalary = bonusSalary;
                    ck.DeductedSalary = deductedSalary;
                    ck.CreatedBy = ck.ModifiedBy = modifiedBy;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static tbl_TempPayroll getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_TempPayroll.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_TempPayroll UpdateDone(int ID, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_TempPayroll.Where(n => n.ID == ID && n.IsDone == false).FirstOrDefault();
                if (a != null)
                {
                    a.IsDone = true;
                    a.ModifiedBy = ModifiedBy;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
    }
}