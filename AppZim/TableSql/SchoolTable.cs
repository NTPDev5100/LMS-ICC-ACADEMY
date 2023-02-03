using AppZim.Models;
using AppZim.ZIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SchoolTable
    {
        //thêm dữ liệu
        public static tbl_School insert(string schoolname, string color, string phone, string address, tbl_District district, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_School ck = new tbl_School();
                ck.CityID = district.LocationID;
                ck.CityName = district.LocationName;
                ck.DistrictID = district.ID;
                ck.DistrictName = district.DistrictName;
                ck.SchoolName = schoolname;
                ck.SchoolPhone = phone;
                ck.SchoolAddress = address;
                ck.CodeColor = color;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_School.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_School update(int ID, string schoolname, string color, string phone, string address, int districtID, string districtname, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_School ck = db.tbl_School.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.DistrictID = districtID;
                    ck.DistrictName = districtname;
                    ck.SchoolName = schoolname;
                    ck.SchoolPhone = phone;
                    ck.SchoolAddress = address;
                    ck.isHide = ishide;
                    ck.CodeColor = color;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_School getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_School.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        /// <summary>
        /// Chỉ lấy DistrictID
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public static int getDistrictID(int ID)
        {
            using (var db = new ZimEntities())
            {
                int DistrictID = db.tbl_School.Where(n => n.ID == ID).Select(x => x.DistrictID.Value).Single();
                return DistrictID;
            }
        }
        public static List<tbl_School> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_School.Where(x => x.isHide == false).ToList();
                return l;
            }
        }
    }
}