using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SupplierServicesTable
    {
        //thêm dữ liệu
        public static tbl_SupplierServices insert(string Grade, int StatusID, string Phone, string MST, string Address, string Description, string phutrach,string Represent, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SupplierServices ck = new tbl_SupplierServices();
                ck.SupplierName = Grade;
                ck.Description = Description;
                ck.Address = Address;
                ck.MST = MST;
                ck.StatusID = StatusID;
                if (StatusID == 1)
                    ck.StatusName = "Sắp hoạt động";
                else if (StatusID == 2)
                    ck.StatusName = "Hoạt động";
                else
                    ck.StatusName = "Hết hoạt động";
                ck.Phone = Phone;
                ck.CuratorFullName = phutrach;
                ck.Represent = Represent;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SupplierServices.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_SupplierServices update(int ID, string GradeName, int StatusID, string Phone, string MST, string Address, string Description, string phutrach,string Represent, bool hide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_SupplierServices ck = db.tbl_SupplierServices.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.SupplierName = GradeName;
                    ck.Description = Description;
                    ck.Address = Address;
                    ck.MST = MST;
                    ck.StatusID = StatusID;
                    if (StatusID == 1)
                        ck.StatusName = "Sắp hoạt động";
                    else if (StatusID == 2)
                        ck.StatusName = "Hoạt động";
                    else
                        ck.StatusName = "Hết hoạt động";
                    ck.Phone = Phone;
                    ck.CuratorFullName = phutrach;
                    ck.Represent = Represent;
                    ck.isHide = hide;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_SupplierServices getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SupplierServices.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_SupplierServices> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SupplierServices.Where(x => x.StatusID == 2).ToList();
                return l;
            }
        }
    }
}