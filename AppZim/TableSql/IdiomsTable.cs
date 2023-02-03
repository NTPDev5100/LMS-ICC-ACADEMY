using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class IdiomsTable//thành ngữ
    {
        public static tbl_Idioms insert(string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Idioms ck = new tbl_Idioms();
                ck.Idioms = content;
                ck.IsHidden = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Idioms.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Idioms update(int id, string content, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Idioms ck = db.tbl_Idioms.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Idioms = content;
                    ck.IsHidden = ishide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static List<tbl_Idioms> getall()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_Idioms> l = db.tbl_Idioms.ToList();
                return l;
            }
        }
        public static tbl_Idioms getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_Idioms l = db.tbl_Idioms.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_Idioms getone()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_Idioms> l = db.tbl_Idioms.Where(n => n.IsHidden != true).ToList();
                if (l.Count > 0)
                {
                    Random rnd = new Random();
                    int i = rnd.Next(0, l.Count - 1);
                    tbl_Idioms ck = l[i];
                    return ck;
                }
                tbl_Idioms ll = new tbl_Idioms();
                return ll;
            }
        }
    }
}