using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ServicesTable
    {
        //thêm dữ liệu
        public static tbl_Services insert(string servicename,int SupplierID,int SupplierStatusID,int phutrach,int StatusID, string servicedescribe, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Services ck = new tbl_Services();
                ck.ServiceName = servicename;
                ck.DescribeService = servicedescribe;
                ck.SupplierID = SupplierID;
                ck.SupplierStatusID = SupplierStatusID;
                if (phutrach != 0)
                {
                    ck.CuratorUID = phutrach;
                    ck.CuratorFullName = AccountTable.getbyID(phutrach).FullName;
                }
                ck.isHide = false;
                ck.StatusID = StatusID;
                if (StatusID == 1)
                    ck.StatusName = "Sắp hoạt động";
                else if (StatusID == 2)
                    ck.StatusName = "Hoạt động";
                else
                    ck.StatusName = "Hết hoạt động";
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Services.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_Services> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Services.ToList();
                return l;
            }
        }
        public static List<tbl_Services> GetAllBySupplierID(int SupplierID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Services.Where(x => x.SupplierID == SupplierID).ToList();
                return l;
            }
        }

        public static tbl_Services getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Services.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        //thêm dữ liệu
        public static tbl_Services update(int id, string servicename, int SupplierID, int SupplierStatusID, int phutrach, int StatusID, string servicedescribe, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Services ck = db.tbl_Services.Where(n => n.ID == id).FirstOrDefault();
                if (ck == null)
                    return null;
                ck.ServiceName = servicename;
                ck.DescribeService = servicedescribe;
                ck.SupplierID = SupplierID;
                ck.SupplierStatusID = SupplierStatusID;
                if (phutrach != 0)
                {
                    ck.CuratorUID = phutrach;
                    ck.CuratorFullName = AccountTable.getbyID(phutrach).FullName;
                }
                ck.StatusID = StatusID;
                if (StatusID == 1)
                    ck.StatusName = "Sắp hoạt động";
                else if (StatusID == 2)
                    ck.StatusName = "Hoạt động";
                else
                    ck.StatusName = "Hết hoạt động";
                ck.isHide = false;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;             
                db.SaveChanges();
                return ck;
            }
        }
    }
}