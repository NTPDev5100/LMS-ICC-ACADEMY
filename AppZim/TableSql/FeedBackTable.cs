using AppZim.Models;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace AppZim.TableSql
{
    public class FeedBackTable
    {
        public static tbl_FeedBack insert(int UID, string fullname, int supportid, string supportname, int typeid, string typename, string title, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_FeedBack ck = new tbl_FeedBack();
                ck.UID = UID;
                ck.FullName = fullname;
                ck.TypeID = typeid;
                ck.TypeName = typename;
                ck.Title = title;
                ck.ContentFeedBack = content;
                ck.isNew = true;
                ck.IsHidden = false;
                ck.SupportID = supportid;
                ck.SupportName = supportname;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_FeedBack.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_FeedBack> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.ToList();
                return l;
            }
        }
        public static int getnotdone(int role, int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.IsHidden == false).ToList();
                if (role == 6)
                    l = l.Where(n => n.SupportID == uid).ToList();
                return l.Count;
            }
        }
        public static List<tbl_FeedBack> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.UID == uid).OrderByDescending(n => n.CreatedDate).ToList();
                return l;
            }
        }
        //Duy Khoa
        public static int getallbyrate(int rate)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.Rate == rate).Count();
                return l;
            }
        }
        public static List<tbl_FeedBack> getTop5Rate()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.Rate != null && n.RateDate != null).OrderByDescending(n => n.RateDate).Take(5).ToList();
                return l;
            }
        }

        public static List<tbl_FeedBack> getbysupportid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.SupportID == uid).OrderByDescending(n => n.CreatedDate).ToList();
                return l;
            }
        }

        public static tbl_FeedBack getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static tbl_FeedBack updateview(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.isNew = false;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    int i = db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_FeedBack updaterate(int id, int rate, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Rate = rate;
                    l.RateDate = GetDateTime.Now;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    int i = db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_FeedBack updatedone(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsHidden = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    int i = db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_FeedBack updatemodified(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    int i = db.SaveChanges();
                }
                return l;
            }
        }
        /// <summary>
        /// cập nhật bài đó bị reject để ko tính rate
        /// </summary>
        /// <param name="id"></param>
        /// <param name="reject"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_FeedBack rejectfix(int id, bool reject, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_FeedBack.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.RejectCheckFix = reject;
                    l.RejectCheckFixDate = GetDateTime.Now.Date;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        // số lượng yêu cầu học bù
        public async static Task<int> GetRedList()
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_FeedBack.Where(n => n.IsHidden == false && n.TypeID == 16).ToListAsync();
                return l.Count;
            }
        }

        // số lượng yêu cầu chuyển ca
        public async static Task<int>  GetOrangeList()
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_FeedBack.Where(n => n.IsHidden == false && n.TypeID == 17).ToListAsync();
                return  l.Count;
            }
        }
    }
}