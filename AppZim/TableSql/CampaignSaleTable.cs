using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AppZim.TableSql
{
    public class CampaignSaleTable
    {
        //thêm dữ liệu
        public static tbl_CampaignSale insert(string CampaignName, DateTime stime, DateTime etime, string note,
            string createby, int campaignType, int managerId)
        {
            using (var db = new ZimEntities())
            {
                tbl_CampaignSale ck = new tbl_CampaignSale();
                ck.CampaignName = CampaignName;
                ck.CampaignType = campaignType;
                ck.ManagerID = managerId;
                ck.sTime = stime.Date;
                ck.eTime = etime.Date;
                ck.Note = note;
                ck.IsHide = false;
                ck.IsDone = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CampaignSale.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_CampaignSale update(int id, string CampaignName, string note, string createby, int managerId)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CampaignSale.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.CampaignName = CampaignName;
                    ck.ManagerID = managerId;
                    ck.Note = note;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return null;
            }
        }

        public static List<tbl_CampaignSale> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSale.Where(x => x.IsHide == false).ToList();
                return l;
            }
        }
        
        public async static Task<List<tbl_CampaignSale>> getallasync()
        {
            using (var db = new ZimEntities())
            {
                var l = await db.tbl_CampaignSale.Where(x => x.IsHide == false).OrderByDescending(x => x.sTime).ThenBy(x => x.eTime).ToListAsync();
                return l;
            }
        }

        public static List<Admin_GetCampaignsInYear_Result> GetCampaignsInYear()
        {
            using (var db = new ZimEntities())
            {
                var l = db.Admin_GetCampaignsInYear().ToList();
                return l;
            }
        }
        public class CampaignSaleModel
        {
            public int? CampaignId { get; set; }
            public string CampaignName { get; set; }
        }
        public static List<tbl_CampaignSale> getbytype(int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSale.Where(x => x.IsHide == false && x.CampaignType == type).ToList();
                return l;
            }
        }
        
        public static List<tbl_CampaignSale> getsubperiod(int type, DateTime sTime, DateTime eTime)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSale.Where(x => x.IsHide == false && x.CampaignType == type
                                                  && x.sTime <= sTime && eTime <= x.eTime).ToList();
                return l;
            }
        }

        public static tbl_CampaignSale getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSale.Where(n => n.ID == id && n.IsHide == false).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CampaignSale> getcampnow()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CampaignSale.Where(n => n.sTime <= GetDateTime.Now.Date && GetDateTime.Now.Date <= n.eTime && n.IsHide == false && n.IsDone == false).ToList();
                return l;
            }
        }

        public static tbl_CampaignSale updatedone(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_CampaignSale.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.IsDone = true;
                    a.ModifiedBy = createby;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }

        public static tbl_CampaignSale Delete(int ID, bool IsHide, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_CampaignSale.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.IsHide = IsHide;
                    a.ModifiedBy = ModifiedBy;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
        public static tbl_CampaignSale CheckExists(DateTime stime, DateTime etime, int campaignType)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CampaignSale.Where(x => x.sTime == stime && x.eTime == etime 
                && x.CampaignType == campaignType && x.IsHide == false
                && x.IsDone == false).FirstOrDefault();
                return ck;
            }
        }
        
        public static List<tbl_CampaignSale> GetExpired(DateTime etime)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CampaignSale.Where(x => x.eTime <= etime && x.IsHide == false
                && x.IsDone == false && x.CampaignType == 3).ToList();
                return ck;
            }
        }
    }
}