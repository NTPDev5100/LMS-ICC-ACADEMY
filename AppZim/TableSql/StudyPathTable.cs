using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StudyPathTable
    {
        public static tbl_StudyPath update(int ID, string name, string linkDriver, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudyPath ck = db.tbl_StudyPath.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.PathName = name;
                    ck.LinkCurriculumn = linkDriver;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_StudyPath getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudyPath.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_StudyPath> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudyPath.OrderByDescending(x => x.ID).ToList();
                return l;
            }
        }

        public static tbl_StudyPath insert(string name, string link, string userName)
        {
            using (var db = new ZimEntities())
            {
                var ck = new tbl_StudyPath();
                ck.PathName = name;
                ck.LinkCurriculumn = link;
                ck.ModifiedBy = userName;
                ck.ModifiedDate = DateTime.Now;
                db.tbl_StudyPath.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
    }
}