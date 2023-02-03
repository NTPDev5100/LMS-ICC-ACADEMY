using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StreetTable
    {
        public static tbl_Street insert(tbl_District d, string street, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Street.Where(n => n.CityID == d.LocationID && n.DistrictID == d.ID && n.StreetName == street).FirstOrDefault();
                if (l == null)
                {
                    tbl_Street ck = new tbl_Street();
                    ck.CityID = d.LocationID;
                    ck.CityName = d.LocationName;
                    ck.DistrictID = d.ID;
                    ck.DistrictName = d.DistrictName;
                    ck.StreetName = street;
                    ck.IsHidden = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_Street.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                return l;
            }
        }
        public static List<tbl_Street> getbycityid(int cityid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Street.Where(n => n.CityID == cityid).ToList();
                return l;
            }
        }
        public static tbl_Street getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Street.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_Street> getbydistrict(int districtid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Street.Where(n => n.DistrictID == districtid).ToList();
                return l;
            }
        }
    }
}