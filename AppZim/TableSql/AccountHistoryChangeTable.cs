using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    /// <summary>
    /// lịch sửu thay đổi tài khoản customer
    /// </summary>
    public class AccountHistoryChangeTable
    {
        //thêm dữ liệu
        public static tbl_AccountHistoryChange insert(int uid, string fullname, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                var lcre = db.tbl_Account.Where(n => n.UserName == createby).FirstOrDefault();
                tbl_AccountHistoryChange ck = new tbl_AccountHistoryChange();
                ck.UID = uid;
                ck.AccountName = fullname;
                ck.ContenChange = content;
                ck.Date = GetDateTime.Now.Date;
                ck.ChangeName = lcre.FullName;
                ck.CreatedBy = ck.ModifiedBy = lcre.UserName;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AccountHistoryChange.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_AccountHistoryChange> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AccountHistoryChange.Where(n => n.UID == uid).ToList();
                return l;
            }
        }
    }
}