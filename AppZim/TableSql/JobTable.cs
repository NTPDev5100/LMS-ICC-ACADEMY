using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class JobTable
    {
        //thêm dữ liệu
        public static tbl_Job insert(string jobname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Job ck = new tbl_Job();
                ck.JobName = jobname;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Job.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_Job> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Job.ToList();
                return l;
            }
        }

        public static tbl_Job getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Job.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_Job update(int id, string jobname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Job ck = db.tbl_Job.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (!string.IsNullOrEmpty(jobname))
                        ck.JobName = jobname;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}