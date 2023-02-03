using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class DiscountTable
    {
        //thêm dữ liệu
        public static tbl_Discount insert(string code, int price, int percent, string note, DateTime sDate, DateTime deadline, int quantity, string createby, bool type)
        {
            using (var db = new ZimEntities())
            {
                tbl_Discount ck = new tbl_Discount();
                ck.DiscountCode = code.Trim();
                if (price > 0)
                    ck.DiscountPrice = price;// km theo tiền
                if (percent > 0)
                    ck.DiscountPercent = percent;//khuyến mãi theo %
                ck.Note = note;
                ck.StartDate = sDate; //thỜi gian bắt đầu
                ck.DeadLine = deadline.Date;
                ck.Status = 1;// 1 Đang sử dụng, 2 Đã sử dụng, 3 Hết hạn
                ck.StatusName = "Đang sử dụng";
                ck.DiscountType = type;// false price, true percent
                ck.isHide = false;
                ck.IsDelete = false;
                ck.Quantity = ck.QuantityLeft = quantity;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Discount.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Discount getbycode(string code)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.DiscountCode == code && n.IsDelete == false && n.StartDate <= DateTime.Now).FirstOrDefault();
                return l;
            }
        }

        public static tbl_Discount getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Discount> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.IsDelete == false).ToList();
                return l;
            }
        }

        public static tbl_Discount update(int id, int price, int percent, DateTime dl, string note, int quantity,int slConLai, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.ID == id && n.Status == 1).FirstOrDefault();
                if (l != null)
                {
                    //var total = db.tbl_CoursesStudent.Where(n => n.DiscountID == l.ID).ToList();
                    //int QuantityLeft = quantity - total.Count;

                    //int total = db.tbl_CoursesStudent.Where(n => n.DiscountID == l.ID && n.ComboID == 0).ToList().Count;
                    //int totalcombo = db.tbl_Combo.Where(n => n.DiscountID == l.ID).ToList().Count;
                    //int QuantityLeft = l.Quantity.Value - total - totalcombo;

                    if (price > 0)
                        l.DiscountPrice = price;
                    if (percent > 0)
                        l.DiscountPercent = percent;
                    l.DeadLine = dl.Date;
                    if (!string.IsNullOrEmpty(note))
                        l.Note = note;
                    l.Quantity = quantity;
                    l.QuantityLeft = slConLai;

                    if (slConLai > 0)
                    {
                        l.Status = 1;
                        l.StatusName = "Đang sử dụng";
                    }
                    else
                    {
                        l.Status = 2;
                        l.StatusName = "Đã sử dụng";
                    }

                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        //cập nhật trạng thái
        public static tbl_Discount updatestatus(int id, int status, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.ID == id && n.Status == 1).FirstOrDefault();
                if (l != null)
                {
                    if (status == 1)
                        l.StatusName = "Đang sử dụng";
                    if (status == 2)
                        l.StatusName = "Đã sử dụng";
                    if (status == 3)
                        l.StatusName = "Hết hạn";
                    l.Status = status;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_Discount Delete(int id, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.IsDelete = IsDelete;
                    l.ModifiedBy = ModifiedBy;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        //update lượt dùng
        public static tbl_Discount useupdate(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Discount.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    int total = db.tbl_StudentAppointment.Where(n => n.DiscountID == l.ID).ToList().Count;
                    int totalcombo = db.tbl_Combo.Where(n => n.DiscountID == l.ID).ToList().Count;
                    int totalService = ServicesStudentTable.getall().Where(n => n.DiscountID == l.ID).ToList().Count;
                    int QuantityLeft = l.Quantity.Value - total - totalcombo - totalService;

                    l.QuantityLeft = QuantityLeft;
                    if (QuantityLeft > 0)
                    {
                        l.Status = 1;
                        l.StatusName = "Đang sử dụng";
                    }
                    else
                    {
                        l.Status = 2;
                        l.StatusName = "Đã sử dụng";
                    }
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
    }
}