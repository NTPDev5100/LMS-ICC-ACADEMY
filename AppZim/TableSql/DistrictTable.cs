using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DistrictTable
    {
        //thêm dữ liệu
        public static tbl_District insert(int cityID, string cityname, string districtname, string createby, int idcu)
        {
            using (var db = new ZimEntities())
            {
                tbl_District ck = new tbl_District();
                ck.LocationID = cityID;
                ck.LocationName = cityname;
                ck.DistrictName = districtname;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;

                ck.idcu = idcu;

                db.tbl_District.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_District update(int ID, int cityID, string cityname, string districtname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_District ck = db.tbl_District.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.LocationID = cityID;
                    ck.LocationName = cityname;
                    ck.DistrictName = districtname;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_District getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_District.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        /// <summary>
        /// Chỉ lấy LocationID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static int getLocationID(int ID)
        {
            using (var db = new ZimEntities())
            {
                int LocationID = db.tbl_District.Where(n => n.ID == ID).Select(x => x.LocationID).Single();
                return LocationID;
            }
        }

        public static List<tbl_District> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_District.ToList();
                return l;
            }
        }

        public static List<tbl_District> getbycityid(int cityid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_District.Where(n => n.LocationID == cityid).ToList();
                return l;
            }
        }
    }
}