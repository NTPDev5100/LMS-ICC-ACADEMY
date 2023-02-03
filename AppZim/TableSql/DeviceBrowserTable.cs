using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DeviceBrowserTable
    {
        public static tbl_DeviceBrowser insert(int UID, string PushEndpoint, string PushP256DH, string PushAuth, string crateBy)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_DeviceBrowser.Where(n => n.UID == UID && n.PushAuth == PushAuth && n.PushEndpoint == PushEndpoint && n.PushP256DH == PushP256DH).FirstOrDefault();
                if (ck != null)
                    return ck;
                tbl_DeviceBrowser dv = new tbl_DeviceBrowser();
                dv.UID = UID;
                dv.PushEndpoint = PushEndpoint;
                dv.PushP256DH = PushP256DH;
                dv.PushAuth = PushAuth;
                dv.isHide = false;
                dv.CreatedDate = dv.ModifiedDate = GetDateTime.Now;
                dv.CreatedBy = dv.ModifiedBy = crateBy;
                db.tbl_DeviceBrowser.Add(dv);
                db.SaveChanges();
                return dv;
            }
        }

        public static List<tbl_DeviceBrowser> getbyuid(int UID)
        {
            using (var db = new ZimEntities())
            {
                var sdata = db.tbl_DeviceBrowser.Where(n => n.UID == UID && n.isHide != true).ToList();
                return sdata;
            }
        }

        public static tbl_DeviceBrowser getbyid(int ID)
        {
            using (var db = new ZimEntities())
            {
                var sdata = db.tbl_DeviceBrowser.Where(n => n.ID == ID).FirstOrDefault();
                return sdata;
            }
        }

        public static tbl_DeviceBrowser updatehide(int ID, string createBy)
        {
            using (var db = new ZimEntities())
            {
                var sdata = db.tbl_DeviceBrowser.Where(n => n.ID == ID).FirstOrDefault();
                if (sdata != null)
                {
                    sdata.isHide = true;
                    sdata.ModifiedDate = GetDateTime.Now;
                    sdata.ModifiedBy = createBy;
                    db.SaveChanges();
                }
                return sdata;
            }
        }
    }
}