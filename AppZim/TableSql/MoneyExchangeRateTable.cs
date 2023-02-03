using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class MoneyExchangeRateTable
    {
        public static tbl_MoneyExchangeRate Insert(string name, string code, int rate, string createby)
        {
            using (var db = new ZimEntities())
            {
                if(db.tbl_MoneyExchangeRate.Where(x=>x.Code == code).FirstOrDefault() != null)
                {
                    return null;
                }
                var ck = new tbl_MoneyExchangeRate();
                ck.Name = name;
                ck.Code = code;
                ck.ExchangeRate = rate;
                ck.CreateBy = createby;
                ck.CreateDate = DateTime.Now;
                db.tbl_MoneyExchangeRate.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_MoneyExchangeRate> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_MoneyExchangeRate.Where(x => x.isDelete != true).ToList();
                return l;
            }
        }
        public static tbl_MoneyExchangeRate Delete(int id)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_MoneyExchangeRate.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isDelete = true;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_MoneyExchangeRate Update(int id, string name, string code, int rate)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_MoneyExchangeRate.Where(x => x.ID == id).FirstOrDefault();
                if(ck != null)
                {
                    ck.Name = name;
                    ck.Code = code;
                    ck.ExchangeRate = rate;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}