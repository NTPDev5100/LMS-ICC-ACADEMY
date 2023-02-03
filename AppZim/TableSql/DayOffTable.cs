using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DayOffTable
    {
        //thêm dữ liệu
        public static tbl_DayOff insert(DateTime dayoff, string dayoffname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DayOff ck = new tbl_DayOff();
                ck.DayOff = dayoff.Date;
                ck.DayOffName = dayoffname;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_DayOff.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_DayOff updatehide(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DayOff ck = db.tbl_DayOff.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = true;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_DayOff getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOff.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static tbl_DayOff getbydate(DateTime day)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOff.Where(n => n.DayOff.Value == day.Date && n.isHide != true).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_DayOff> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DayOff.Where(x => x.isHide == false).ToList();
                return l;
            }
        }
    }
}