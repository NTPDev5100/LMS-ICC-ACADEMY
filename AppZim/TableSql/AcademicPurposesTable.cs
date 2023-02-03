using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class AcademicPurposesTable
    {
        /// <summary>
        /// thêm dữ liệu vào mục đích học tập
        /// </summary>
        /// <param name="AcademicPurposes">Mục đích</param>
        /// <param name="createby">người tạo</param>
        /// <returns></returns>
        public static tbl_AcademicPurposes insert(string AcademicPurposes, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_AcademicPurposes ck = new tbl_AcademicPurposes();
                ck.AcademicPurposesName = AcademicPurposes;
                ck.Description = "";
                ck.IsHidden = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_AcademicPurposes.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_AcademicPurposes update(int ID, string AcademicPurposes, bool hide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_AcademicPurposes ck = db.tbl_AcademicPurposes.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.AcademicPurposesName = AcademicPurposes;
                    ck.IsHidden = hide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_AcademicPurposes getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AcademicPurposes.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_AcademicPurposes> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_AcademicPurposes.ToList();
                return l;
            }
        }
    }
}