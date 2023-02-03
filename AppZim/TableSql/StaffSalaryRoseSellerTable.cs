using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StaffSalaryRoseSellerTable
    {
        public static tbl_StaffSalaryRoseSeller Insert(int UID, int Persent, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_StaffSalaryRoseSeller ck = new tbl_StaffSalaryRoseSeller();
                ck.UID = UID;
                ck.SalaryPresent = Persent;
                ck.CreatedBy = CreatedBy;
                ck.CreatedDate = GetDateTime.Now;
                db.tbl_StaffSalaryRoseSeller.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_StaffSalaryRoseSeller Update(int ID, int Persent, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_StaffSalaryRoseSeller ck = db.tbl_StaffSalaryRoseSeller.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.SalaryPresent = Persent;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_StaffSalaryRoseSeller GetByUID(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalaryRoseSeller.Where(n => n.UID == UID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_StaffSalaryRoseSeller> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalaryRoseSeller.ToList();
                return l;
            }
        }
    }
}