using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class WardTable
    {
        public static tbl_Ward insert(tbl_District d, string ward, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Ward.Where(n => n.CityID == d.LocationID && n.DistrictID == d.ID && n.WardName == ward).FirstOrDefault();
                if (l != null)
                    return l;
                tbl_Ward ck = new tbl_Ward();
                ck.CityID = d.LocationID;
                ck.CityName = d.LocationName;
                ck.DistrictID = d.ID;
                ck.DistrictName = d.DistrictName;
                ck.WardName = ward;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Ward.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_Ward> getbycityid(int cityid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Ward.Where(n => n.CityID == cityid).ToList();
                return l;
            }
        }
        public static tbl_Ward getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Ward.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_Ward> getbydistrict(int districtid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Ward.Where(n => n.DistrictID == districtid).ToList();
                return l;
            }
        }
    }
}