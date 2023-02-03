using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DocumentTable
    {
        //thêm dữ liệu
        public static tbl_DocumentCategory insertcate(int classid, int giaotrinhid, string catedocname, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_DocumentCategory ck = new tbl_DocumentCategory();
                ck.ClassID = classid;
                ck.CurriculumID = giaotrinhid;
                ck.CategoryName = catedocname;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_DocumentCategory.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Document Insert(int cateid, int giaotrinhid, string docname, string doclink, int courseid, string createby, string filename, string name)
        {
            using (var db = new ZimEntities())
            {
                tbl_Document ck = new tbl_Document();
                ck.Name = name;  //tên tài liệu
                ck.CategoryID = cateid;
                ck.CurriculumID = giaotrinhid;
                ck.DocumentName = docname;
                ck.DocumentLink = doclink;
                ck.FileName = filename;
                ck.CourseID = courseid;
                ck.isHide = false;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Document.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_Document Update(int id, string docname, string doclink, string filename, string name, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Document ck = db.tbl_Document.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Name = name;  //tên tài liệu
                    ck.DocumentName = docname;
                    ck.DocumentLink = doclink;
                    ck.FileName = filename;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static List<tbl_DocumentCategory> getcatebycurrid(int cruid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentCategory.Where(n => n.CurriculumID == cruid).ToList();
                return l;
            }
        }

        public static List<tbl_Document> GetByCurriculumID(int CurriculumID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Document.Where(n => n.CurriculumID == CurriculumID && n.IsDelete == false).ToList();
                return l;
            }
        }

        public static tbl_DocumentCategory getcatebyid(int cateid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_DocumentCategory.Where(n => n.ID == cateid).FirstOrDefault();
                return l;
            }
        }

        public static tbl_Document getdocid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Document.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Document> getdocbycateid(int cateid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Document.Where(n => n.CategoryID == cateid && n.isHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }

        public static List<tbl_Document> getdocbycurrid(int currid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Document.Where(n => n.CurriculumID == currid && n.IsDelete == false).ToList();
                return l;
            }
        }

        //thêm dữ liệu
        public static tbl_Document insertdoc(int cateid, int giaotrinhid, string docname, string doclink, int courseid, string createby, string filename)
        {
            using (var db = new ZimEntities())
            {
                tbl_Document ck = new tbl_Document();
                ck.CategoryID = cateid;
                ck.CurriculumID = giaotrinhid;
                ck.DocumentName = docname;
                ck.DocumentLink = doclink;
                ck.FileName = filename;
                ck.CourseID = courseid;
                ck.isHide = false;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Document.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Document hiddendoc(int id, bool ishide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Document ck = db.tbl_Document.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isHide = ishide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        public static tbl_Document Delete(int id, bool isdelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Document ck = db.tbl_Document.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.IsDelete = isdelete;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}