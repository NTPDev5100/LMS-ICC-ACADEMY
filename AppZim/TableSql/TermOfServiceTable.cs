using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;

namespace AppZim.TableSql
{
    public class TermOfServiceTable
    {
        public static tbl_TermOfService insert (string termcontent, string createdby)
        {
            using (var db = new ZimEntities())
            {
                tbl_TermOfService ck = db.tbl_TermOfService.FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_TermOfService();
                    ck.TermContent = termcontent;
                    ck.IsHide = false;
                    ck.CreatedBy = ck.ModifiedBy = createdby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_TermOfService.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    ck.TermContent = termcontent;
                    ck.IsHide = false;
                    ck.ModifiedBy = createdby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
            }
        }
        public static tbl_TermOfService getfirst()
        {
            using (var db = new ZimEntities())
            {
                tbl_TermOfService ck = db.tbl_TermOfService.FirstOrDefault();
                return ck;
            }
        }
    }
}