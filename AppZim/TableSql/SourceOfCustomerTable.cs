using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SourceOfCustomerTable
    {
        //thêm dữ liệu
        public static tbl_SourceOfCustomer insert(string SourceOfCustomer, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SourceOfCustomer ck = new tbl_SourceOfCustomer();
                ck.SourceOfCustomer = SourceOfCustomer;
                ck.IsHidden = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SourceOfCustomer.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_SourceOfCustomer update(int ID, string SourceOfCustomer, bool hide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SourceOfCustomer ck = db.tbl_SourceOfCustomer.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.SourceOfCustomer = SourceOfCustomer;
                    ck.IsHidden = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_SourceOfCustomer getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SourceOfCustomer.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_SourceOfCustomer> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SourceOfCustomer.ToList();
                return l;
            }
        }
    }
}