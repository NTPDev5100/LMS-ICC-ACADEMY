using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
namespace AppZim.TableSql
{
    public class InvoiceDetailTable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="invID"></param>
        /// <param name="comboId"></param>
        /// <param name="courseID"></param>
        /// <param name="TypeAssign">1:vào thẳng, 2:đăng kí lớp, 3:từ lớp vào khóa</param>
        /// <param name="price"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_InvoiceDetail insert(int invID, int comboId, int courseID, int TypeAssign, int price,string createby)
        {
            using (var db=new ZimEntities())
            {
                tbl_InvoiceDetail iv = new tbl_InvoiceDetail();
                iv.InvoiceID = invID;
                iv.ComboID = comboId;
                iv.CourseID = courseID;
                iv.TypeAssign = TypeAssign;//1:vào thẳng, 2:đăng kí lớp, 3:từ lớp vào khóa
                iv.Price = price;
                iv.CreatedDate = iv.ModifiedDate = GetDateTime.Now;
                iv.CreatedBy = iv.ModifiedBy = createby;
                iv.IsHide = false;
                db.tbl_InvoiceDetail.Add(iv);
                db.SaveChanges();
                return iv;
            }
        }
        /// <summary>
        /// Lấy thông tin invoiceDetail
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static tbl_InvoiceDetail getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var iv = db.tbl_InvoiceDetail.Where(n => n.ID == id).FirstOrDefault();
                return iv;
            }
        }
        /// <summary>
        /// Lấy danh sách invoiceDetail
        /// </summary>
        /// <param name="invoiceID"></param>
        /// <returns></returns>
        public static List<tbl_InvoiceDetail> getInvoiceDetail(int invoiceID)
        {
            using (var db = new ZimEntities())
            {
                var iv = db.tbl_InvoiceDetail.Where(n => n.InvoiceID == invoiceID).ToList();
                return iv;
            }
        }
    }
}