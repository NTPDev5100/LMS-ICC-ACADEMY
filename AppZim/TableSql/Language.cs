using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class Language
    {
        public static List<tbl_Language> GetAll()
        {
            using(var db = new ZimEntities())
            {
                var l = db.tbl_Language.Where(x => x.isDelete != true).OrderBy(x => x.Name).ToList();
                return l;
            }
        }
        public static tbl_Language GetByID(int id)
        {
            using(var db = new ZimEntities())
            {
                var l = db.tbl_Language.Where(x => x.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static int GetIdByName(string name)
        {
            using (var db = new ZimEntities())
            {
                int id = 0;
                var check = db.tbl_Language.Where(x => x.Name == name).FirstOrDefault();
                if(check != null)
                {
                    id = check.ID;
                }
                return id;
            }
        }
    }
}