using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class MemeberTable
    {
        public static tbl_Member insert(int memberID, int groupID, int status, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                       
                var m = db.tbl_Member.Where(n => n.MemberID == memberID && n.GroupID == groupID).FirstOrDefault();
                if (m != null)
                {

                    m.Status = status;//1 có trong group, 2 là đã bị đá
                    m.ModifiedDate = GetDateTime.Now;
                    m.ModifiedBy = CreatedBy;
                    db.SaveChanges();
                    return m;
                }
                else
                {           
                    tbl_Member G = new tbl_Member();
                    G.MemberID = memberID;             
                    G.GroupID = groupID;
                    G.Status = status;//1 có trong group, 2 là đã bị đá
                    G.CreatedBy = CreatedBy;
                    G.CreatedDate = GetDateTime.Now;
                    db.tbl_Member.Add(G);
                    db.SaveChanges();
                    return G;
                }

            }
        }
        public static tbl_Member getbymenberid(int memberID, int groupID)
        {
            using (var db = new ZimEntities())
            {
                var G = db.tbl_Member.Where(n => n.MemberID == memberID && n.GroupID == groupID).FirstOrDefault();
                return G;
            }
        }
        public static IEnumerable<tbl_Account> GetbyGroupIDandStatus(int groupid, int status)
        {
            using (var db = new ZimEntities())
            {
                IEnumerable<tbl_Account> l = db.tbl_Account.Where(n => n.Status == 2).ToList();
                var gr = db.tbl_Member.Where(n => n.GroupID == groupid && n.Status == status).ToList();
                l = from t in l
                    join g in gr on t.ID equals g.MemberID
                    select t;
                return l;
            }
        }
    }
}