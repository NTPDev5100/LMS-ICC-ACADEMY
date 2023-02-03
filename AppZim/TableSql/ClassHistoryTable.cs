using AppZim.Models;
using System.Collections.Generic;
using System.Linq;

namespace AppZim.TableSql
{
    public class ClassHistoryTable
    {
        //thêm dữ liệu
        public static tbl_ClassHistory insert(int classid, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ClassHistory ck = new tbl_ClassHistory();
                ck.ClassID = classid;
                ck.HistoryContent = content;
                ck.CreatedBy= createby;
                ck.CreatedDate =  GetDateTime.Now;
                db.tbl_ClassHistory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static List<tbl_ClassHistory> getbyclassid(int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ClassHistory.Where(n => n.ClassID == classid).ToList();
                return l;
            }
        }
    }
}