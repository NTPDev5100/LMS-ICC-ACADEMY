using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AppZim.TableSql
{
    public class SignatureTable
    {
        public async static Task<tbl_Signature> Insert(int uid, string signature, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = await db.tbl_Signature.Where(x => x.UserId == uid && x.IsHide == false).FirstOrDefaultAsync();
                if (ck != null)
                {
                    ck.Signature = signature;
                }
                else
                {
                    ck = new tbl_Signature();
                    ck.UserId = uid;
                    ck.Signature = signature;
                    ck.IsHide = false;
                    ck.CreatedBy = createby;
                    ck.CreatedDate = DateTime.Now;
                    db.tbl_Signature.Add(ck);
                }
                await db.SaveChangesAsync();
                return ck;
            }
        }

        public async static Task<List<tbl_Signature>> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_Signature.Where(x => x.IsHide == false).ToListAsync();
                return l;
            }
        }
        
        public async static Task<tbl_Signature> GetByUserId(int userId)
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_Signature.Where(x => x.IsHide == false && x.UserId == userId).FirstOrDefaultAsync();
                return l;
            }
        }
    }
}