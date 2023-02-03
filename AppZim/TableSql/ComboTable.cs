using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ComboTable
    {
        /// <summary>
        /// thêm dữ liệu
        /// </summary>
        /// <param name="uid">ID khách</param>
        /// <param name="totalprice">Tổng tiền combo</param>
        /// <param name="paid">Số tiền đã thanh toán</param>
        /// <param name="createby">Tạo bởi</param>
        /// <returns></returns>
        public static tbl_Combo insert(int uid, int totalprice, int discountId, int discountPrice, int paid, string datepriceleft, string createby)
        {
            using (var db = new ZimEntities())
            {
                int dis = 0;
                tbl_Combo ck = new tbl_Combo();
                ck.UID = uid;
                ck.TotalPrice = totalprice;
                ck.DiscountID = discountId;
                ck.PriceDiscount = discountPrice;
                ck.Paid = paid;
                ck.PriceLeft = 0;
                try
                {
                    if (!string.IsNullOrEmpty(datepriceleft.Trim()))
                        ck.PriceLeftDate = DateTime.ParseExact(datepriceleft, "dd/MM/yyyy", null);// ngày đóng học phí
                }
                catch { }

                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Combo.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// Lấy theo id
        /// </summary>
        /// <param name="id">id cần lấy ra dữ liệu</param>
        /// <returns></returns>
        public static tbl_Combo getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Combo.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Combo> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Combo.ToList();
                return l;
            }
        }
        //public static int GetCurrentPriceLeft(int comboid)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        var cb = ComboTable.getbyid(comboid);
        //        List<tbl_ComboDetail> cbd = db.tbl_ComboDetail.Where(n => n.ComboID == comboid).ToList();
        //        int priceleft = 0;
        //        foreach (var item in cbd)
        //        {
        //            if (item.TypeID == 1 || item.TypeID is null)
        //            {
        //                var course = db.tbl_CoursesStudent.Where(n => n.ComboID == comboid && n.CourseID == item.CourseID).FirstOrDefault();
        //                if (course != null)
        //                    priceleft += course.PriceLeft.Value;
        //            }
        //            else
        //            {
        //                var appoint = db.tbl_StudentAppointment.Where(n => n.ComboID == comboid && n.ClassID == item.CourseID).FirstOrDefault();
        //                if (appoint != null)
        //                {
        //                    //if (appoint.isHide == true)
        //                    //{
        //                    //    var course = db.tbl_CoursesStudent.Where(n => n.ComboID == comboid && n.AppointmentID == appoint.ID).FirstOrDefault();
        //                    //    if (course != null)
        //                    //        priceleft += course.PriceLeft.Value;
        //                    //}
        //                    //else
        //                    //{
        //                    //    var cls = db.tbl_Class.Where(n => n.ID == appoint.ClassID).FirstOrDefault();
        //                    //    if (cls != null)
        //                    //    {
        //                    //        //int pricel = cls.PriceClass.Value - appoint.Deposit.Value - item.DiscountPrice.Value;
        //                    //        int pricel = cls.PriceClass.Value - appoint.Deposit.Value;
        //                    //        priceleft += pricel;
        //                    //    }

        //                    //}
        //                    var cls = db.tbl_Class.Where(n => n.ID == appoint.ClassID).FirstOrDefault();
        //                    if (cls != null)
        //                    {
        //                        //int pricel = cls.PriceClass.Value - appoint.Deposit.Value - item.DiscountPrice.Value;
        //                        int pricel = cls.PriceClass.Value - appoint.Deposit.Value;
        //                        priceleft += pricel;
        //                    }
        //                }
        //            }
        //        }
        //        priceleft = priceleft - cb.PriceDiscount.Value;
        //        if (priceleft < 0)
        //            priceleft = 0;
        //        return priceleft;
        //    }
        //}
        public static int GetCurrentPriceLeft(int comboid)
        {
            using (var db = new ZimEntities())
            {
                var cb = db.tbl_Combo.Where(n => n.ID == comboid).FirstOrDefault();
                if (cb != null)
                {
                    return cb.PriceLeft.Value;
                }
                return 0;
            }
        }
        public static tbl_Combo UpdateTotalPrice(int ID,int totalPrice, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Combo c = db.tbl_Combo.Where(n => n.ID == ID).FirstOrDefault();
                if (c != null)
                {
                    c.TotalPrice = c.TotalPrice + totalPrice;
                    c.PriceLeft = c.TotalPrice - c.PriceDiscount - c.Paid;
                    c.ModifiedBy = createby;
                    c.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return c;
            }
        }

        public static tbl_Combo UpdatePriceLeft(int ID,int Paid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Combo c = db.tbl_Combo.Where(n => n.ID == ID).FirstOrDefault();
                if (c != null)
                {
                    c.Paid = c.Paid + Paid;
                    c.PriceLeft = c.TotalPrice - c.PriceDiscount - c.Paid;
                    c.ModifiedBy = createby;
                    c.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return c;
            }
        }
    }
}