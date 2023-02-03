using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ContractTable
    {
        public static tbl_Contract update(int id, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Contract ck = db.tbl_Contract.Where(x => x.ID == id).FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_Contract();
                    ck.ContractContent = content;
                    ck.IsHidden = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_Contract.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    ck.ContractContent = content;
                    ck.IsHidden = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static tbl_Contract getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_Contract ck = db.tbl_Contract.Where(x => x.ID == id).FirstOrDefault();
                return ck;
            }
        }
    }
}