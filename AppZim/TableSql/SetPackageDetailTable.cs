using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SetPackageDetailTable
    {
        public static tbl_SetPackageDetail insert(int packgeid, int typedetail, string audio, int minute, string des, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageDetail ck = new tbl_SetPackageDetail();
                ck.SetPackageID = packgeid;
                if (typedetail == 1)
                {
                    ck.TypeDetail = typedetail;
                    ck.PackageDetailName = "Listening";
                }
                if (typedetail == 2)
                {
                    ck.TypeDetail = typedetail;
                    ck.PackageDetailName = "Reading";
                }
                if (typedetail == 3)
                {
                    ck.TypeDetail = typedetail;
                    ck.PackageDetailName = "Writting";
                }
                if (typedetail == 4)
                {
                    ck.TypeDetail = typedetail;
                    ck.PackageDetailName = "Speaking";
                }
                ck.Description = des;
                ck.LinkAudio = audio;
                ck.Minute = minute;           
                ck.isHide = ishide;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SetPackageDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static List<tbl_SetPackageDetail> getbysetid(int setid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackageDetail.Where(n => n.SetPackageID == setid).OrderBy(n => n.isHide).ToList();
                return l;
            }
        }
        public static tbl_SetPackageDetail getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackageDetail.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static tbl_SetPackageDetail update(int id, string audio, int minute, string des, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageDetail ck = db.tbl_SetPackageDetail.Where(n => n.ID == id).FirstOrDefault();
                if (!string.IsNullOrEmpty(audio))
                    ck.LinkAudio = audio;
                if (!string.IsNullOrEmpty(des))
                    ck.Description = des;
                ck.Minute = minute;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }
    }
}