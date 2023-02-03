using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DocumentZimTable
    {
        //thêm dữ liệu
        public static tbl_DocumentCategoryZim insertcatezim(string catedocname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentCategoryZim ck = new tbl_DocumentCategoryZim();
                ck.CategoryZimName = catedocname;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_DocumentCategoryZim.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_DocumentCategoryZim updatecatezim(int id, string catedocname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentCategoryZim ck = db.tbl_DocumentCategoryZim.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.CategoryZimName = catedocname;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static List<tbl_DocumentZim> getalldoc()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentZim.Where(n=>n.isHide != true).ToList();
                return l;
            }
        }
        public static List<tbl_DocumentCategoryZim> getcatezimall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentCategoryZim.ToList();
                return l;
            }
        }

        public static tbl_DocumentCategoryZim getcatezimbyid(int cateid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentCategoryZim.Where(n => n.ID == cateid).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_DocumentZim> getdocbycateid(int cateid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentZim.Where(n => n.CategoryZimID == cateid && n.isHide != true).ToList();
                return l;
            }
        }

        //thêm dữ liệu
        public static tbl_DocumentZim insertdoczim(int cateid, string name, string img, string link, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentZim ck = new tbl_DocumentZim();
                ck.CategoryZimID = cateid;
                ck.DocumentName = name;
                ck.DocumentIMG = img;
                ck.DocumentLink = link;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_DocumentZim.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //thêm dữ liệu
        public static tbl_DocumentZim deletedoczim(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentZim ck = db.tbl_DocumentZim.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = true;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_DocumentZim GetDocByID(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentZim ck = db.tbl_DocumentZim.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }
    }
}