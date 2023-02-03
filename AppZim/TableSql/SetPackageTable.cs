using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class SetPackageTable
    {
        public static tbl_SetPackage insert(string packgename, int price, string img, string imgthumbnail, string note,
            bool lis, bool speak, bool read, bool write, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackage ck = new tbl_SetPackage();
                ck.PackageName = packgename;
                ck.Price = price;
                ck.Show = false;
                ck.Deadline = GetDateTime.Now.Date;
                ck.PackageIMG = img;
                ck.PackageIMGThumbnail = imgthumbnail;
                ck.Note = note;
                ck.Listening = lis;
                ck.Speaking = speak;
                ck.Reading = read;
                ck.Writing = write;
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SetPackage.Add(ck);
                db.SaveChanges();
                //tạo kỹ năng set
                SetPackageDetailTable.insert(ck.ID, 1, "", 0, "", !lis, createby);
                SetPackageDetailTable.insert(ck.ID, 2, "", 0, "", !read, createby);
                SetPackageDetailTable.insert(ck.ID, 3, "", 0, "", !write, createby);
                SetPackageDetailTable.insert(ck.ID, 4, "", 0, "", !speak, createby);                 
                return ck;
            }
        }

        /// <summary>
        /// Hàng custom không rờ vào được đâu
        /// </summary>
        /// <param name="packgename"></param>
        /// <param name="price"></param>
        /// <param name="img"></param>
        /// <param name="imgthumbnail"></param>
        /// <param name="note"></param>
        /// <param name="lis"></param>
        /// <param name="vocabulary"></param>
        /// <param name="read"></param>
        /// <param name="grammar"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_SetPackage Inserts(string packgename, int price, string img, string imgthumbnail, string note,
            bool lis, bool vocabulary, bool read, bool grammar, string createby)
        {
            using (var db = new ZimEntities())
            {
                //Custom lại, bỏ phần nói,viết => đổi thành dạng giống phần đọc
                tbl_SetPackage ck = new tbl_SetPackage();
                ck.PackageName = packgename;
                ck.Price = price;
                ck.Show = false;
                ck.Deadline = GetDateTime.Now.Date;
                ck.PackageIMG = img;
                ck.PackageIMGThumbnail = imgthumbnail;
                ck.Note = note;
                ck.Listening = lis;  //nghe hiểu
                ck.Speaking = vocabulary;   //chơi đè  - từ vựng hán tự
                ck.Reading = read;  //đọc hiểu
                ck.Writing = grammar; //chơi đè  - ngữ pháp
                ck.isHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_SetPackage.Add(ck);
                db.SaveChanges();
                //tạo kỹ năng set
                SetPackageDetailTable.insert(ck.ID, 1, "", 0, "", !lis, createby); //nghe hiểu
                SetPackageDetailTable.insert(ck.ID, 2, "", 0, "", !read, createby); //đọc hiểu
                SetPackageDetailTable.insert(ck.ID, 3, "", 0, "", !grammar, createby); //ngữ pháp
                SetPackageDetailTable.insert(ck.ID, 4, "", 0, "", !vocabulary, createby); //từ vựng - hán tự
                return ck;
            }
        }

        public static tbl_SetPackage getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_SetPackage> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.ToList();
                return l;
            }
        }
        public static List<tbl_SetPackage> getSetPremium()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.Where(n => n.Price > 0 && n.isHide != true && n.Show == true).ToList();
                return l;
            }
        }
        public static List<tbl_SetPackage> getSetFree()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.Where(n => n.Price == 0 && n.isHide != true && n.Show == true).ToList();
                return l;
            }
        }
        public static tbl_SetPackage Update(int id, string packgename, int price, string img, string imgthumbnail, string note, string modifiedby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    if (!string.IsNullOrEmpty(packgename))
                        l.PackageName = packgename;
                    l.Price = price;
                    if (!string.IsNullOrEmpty(img))
                        l.PackageIMG = img;
                    if (!string.IsNullOrEmpty(imgthumbnail))
                        l.PackageIMGThumbnail = imgthumbnail;
                    if (!string.IsNullOrEmpty(note))
                        l.Note = note;
                    l.ModifiedBy = modifiedby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static tbl_SetPackage UpdateShow(int id, string modifiedby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_SetPackage.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Show = !l.Show.Value;
                    l.ModifiedBy = modifiedby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
    }
}