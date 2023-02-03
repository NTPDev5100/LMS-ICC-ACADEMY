using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class NationTable
    {
        public static tbl_Nation Insert(string Nation, string timezone, int value, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Nation l = new tbl_Nation();
                l.Nation = Nation;
                l.TimeZone = "";
                l.TimeZoneName = timezone;
                l.TimeZoneValue = value;
                l.isHide = false;
                l.CreatedBy = CreatedBy;
                l.CreatedDate = DateTime.Now;
                db.tbl_Nation.Add(l);
                db.SaveChanges();
                return l;
            }
        }
        public static tbl_Nation Update(int id, string Nation, string timezone, int value, bool ishide, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Nation.Where(x => x.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Nation = Nation;
                    l.TimeZoneName = timezone;
                    l.TimeZoneValue = value;
                    l.CreatedBy = CreatedBy;
                    l.isHide = ishide;
                    l.CreatedDate = DateTime.Now;
                    db.SaveChanges();
                    return l;
                }
                return null;
            }
        }

        public static tbl_Nation GetByID(int ID)
        {
            using (var db = new ZimEntities())
            {
                tbl_Nation l = db.tbl_Nation.Where(x => x.ID == ID).FirstOrDefault();
                return l;
            }
        }


        public static List<tbl_Nation> GetAll()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_Nation> l = db.tbl_Nation.OrderBy(x => x.ID).ToList();
                return l;
            }
        }
    }
}