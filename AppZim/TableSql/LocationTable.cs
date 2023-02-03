using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class LocationTable
    {
        //thêm dữ liệu
        public static tbl_City insert(string locationname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_City ck = new tbl_City();
                ck.CityName = locationname;
                ck.IsHidden = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_City.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_City update(int ID, string locationname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_City ck = db.tbl_City.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.CityName = locationname;
                    ck.IsHidden = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_City getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_City.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_City> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_City.ToList();
                return l;
            }
        }
        ///
        public static tbl_City updatecode(int ID, string code)
        {
            using (var db = new ZimEntities())
            {
                tbl_City ck = db.tbl_City.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.ModifiedBy = code;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}