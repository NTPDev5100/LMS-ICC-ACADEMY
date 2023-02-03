using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ServiceExamsTable
    {
        //thêm dữ liệu
        public static tbl_ServiceExams insert(int cityid, int schoolid, int supplierid, int ServiceID, int slot, int price,int giavon, bool real, string examservice, DateTime date, string createby, string hours, int DiscountID)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServiceExams ck = new tbl_ServiceExams();
                ck.CityID = cityid;
                ck.SchoolID = schoolid;
                ck.SupplierID = supplierid;
                ck.ServiceID = ServiceID;
                ck.Slot = slot;
                ck.ExamPrice = price; //giá bán
                ck.OriginalPrice = giavon; //giá vốn
                ck.ExamReal = real;
                ck.ExamService = examservice;
                ck.ExamDate = date;
                ck.Hour = hours;
                ck.DiscountID = DiscountID;
                ck.IsDelete = false;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ServiceExams.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_ServiceExams update(int ID, int cityid, int schoolid, int supplierid, int slot, int price, bool real, string examservice, DateTime date, string createby, string hours)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServiceExams ck = db.tbl_ServiceExams.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.CityID = cityid;
                    ck.SchoolID = schoolid;
                    ck.SupplierID = supplierid;
                    ck.Slot = slot;
                    ck.ExamReal = real;
                    ck.ExamPrice = price;
                    ck.ExamService = examservice;
                    ck.ExamDate = date.Date;
                    ck.Hour = hours;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_ServiceExams Delete(int ID, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServiceExams ck = db.tbl_ServiceExams.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = IsDelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_ServiceExams getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServiceExams.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_ServiceExams> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServiceExams.Where(x => x.IsDelete == false).ToList();
                return l;
            }
        }
        public static List<tbl_ServiceExams> GetAllByServiceID(int ServiceID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServiceExams.Where(x => x.IsDelete == false && x.ServiceID == ServiceID).ToList();
                return l;
            }
        }
    }
}