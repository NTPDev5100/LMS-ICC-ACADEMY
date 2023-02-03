using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ReasonTable
    {
        public static int GetIDbyName(string name)
        {
            using (var db = new ZimEntities())
            {
                int id = 0;
                var check = db.tbl_Reason.Where(x => x.Name == name).FirstOrDefault();
                if(check != null)
                {
                    id = check.ID;
                }
                return id;
            }
        }
        public static List<tbl_Reason> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Reason.Where(x => x.isDelete != true).ToList();
                return l;
            }
        }
        public static List<tbl_Reason> GetByID(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Reason.Where(x => x.isDelete != true && x.ID == id).ToList();
                return l;
            }
        }
        public static List<tbl_Reason> GetAllByType(int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Reason.Where(x => x.isDelete != true && x.Type == type).ToList();
                return l;
            }
        }
    }
}