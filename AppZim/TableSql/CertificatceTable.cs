using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;

namespace AppZim.TableSql
{
    public class CertificatceTable
    {
        public static tbl_Certificatce InsertUpdate(string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Certificatce ck = db.tbl_Certificatce.FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_Certificatce();
                    ck.CertificatceContent = content;
                    ck.isHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_Certificatce.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    ck.CertificatceContent = content;
                    ck.isHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static tbl_Certificatce getfirst()
        {
            using (var db = new ZimEntities())
            {
                tbl_Certificatce ck = db.tbl_Certificatce.FirstOrDefault();
                return ck;
            }
        }
    }
}