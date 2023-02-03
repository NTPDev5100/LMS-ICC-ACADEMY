using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DeviceMobileTable
    {
        public static void AddIncludePlayerID(int UID, string IncludePlayerID)
        {
            using (var db = new ZimEntities())
            {
                tbl_DeviceMobile ck = db.tbl_DeviceMobile.Where(n => n.UID == UID).FirstOrDefault();
                if (ck != null)
                {
                    ck.IncludePlayerID = IncludePlayerID;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    tbl_DeviceMobile insert = new tbl_DeviceMobile();
                    insert.UID = UID;
                    insert.IncludePlayerID = IncludePlayerID;
                    insert.CreatedDate = GetDateTime.Now;
                    db.tbl_DeviceMobile.Add(insert);
                    db.SaveChanges();
                }
            }
        }

        public static tbl_DeviceMobile getbyUID(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DeviceMobile.Where(n => n.UID == UID).FirstOrDefault();
                return l;
            }
        }
    }
}