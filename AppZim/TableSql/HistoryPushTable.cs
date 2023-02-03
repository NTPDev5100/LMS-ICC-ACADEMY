using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class HistoryPushTable
    {
        //thêm dữ liệu
        public static tbl_HistoryPush insert(int pushtype, string pushname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_HistoryPush ck = new tbl_HistoryPush();
                ck.TypePush = pushtype;
                ck.PushName = pushname;
                ck.Date = GetDateTime.Now.Date;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_HistoryPush.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        
        public static tbl_HistoryPush getbypushtypevsdate(int pushtype, DateTime date)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_HistoryPush.Where(n => n.TypePush == pushtype && n.Date == date.Date).FirstOrDefault();
                return l;
            }
        }
    }
}