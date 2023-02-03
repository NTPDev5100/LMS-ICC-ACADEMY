using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AccountHistoryAdvisoryTable
    {
        //thêm dữ liệu
        public static tbl_AccountHistoryAdvisory insert(int uid, string note, string createby)
        {
            using (var db = new ZimEntities())
            {               
                tbl_AccountHistoryAdvisory ck = new tbl_AccountHistoryAdvisory();
                ck.UID = uid;
                ck.Note = note;               
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AccountHistoryAdvisory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_AccountHistoryAdvisory> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountHistoryAdvisory.Where(n => n.UID == uid).ToList();
                return l;
            }
        }
    }
}