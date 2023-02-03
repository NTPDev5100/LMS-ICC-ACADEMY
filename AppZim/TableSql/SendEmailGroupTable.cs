using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.TableSql;
using AppZim.Models;

namespace AppZim.TableSql
{
    public class SendEmailGroupTable
    {

        public static tbl_SendEmailGroup insert(int type, int groupid, int aclogid, int targetid)
        {
            using (var db = new ZimEntities())
            {
                tbl_SendEmailGroup s = new tbl_SendEmailGroup();
                s.Type = type;
                s.GroupId = groupid;
                s.AclogId = aclogid;
                s.TargetId = targetid;
                s.Status = 1;
                db.tbl_SendEmailGroup.Add(s);
                db.SaveChanges();
                return s;
            }
        }
        public static tbl_SendEmailGroup sendDone(int id)
        {
            using (var db = new ZimEntities())
            {
                var s = db.tbl_SendEmailGroup.Where(n => n.ID == id).FirstOrDefault();
                if (s != null)
                {
                    s.Status = 2;
                    db.SaveChanges();
                }
                return s;
            }
        }
        public static List<tbl_SendEmailGroup> getallundone()
        {
            using (var db = new ZimEntities())
            {
                return db.tbl_SendEmailGroup.Where(n => n.Status == 1).ToList();

            }
        }
    }
}