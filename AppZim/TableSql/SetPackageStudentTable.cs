using System;
using System.Collections.Generic;
using System.Linq;
using AppZim.Models;
using System.Web;

namespace AppZim.TableSql
{
    public class SetPackageStudentTable
    {
        public static tbl_SetPackageStudent insert(int studentID, string studentname, int setpackageID, int setPrice, int paymentType, int discountID, int discountPrice,int examinerid, string createdby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageStudent a = new tbl_SetPackageStudent();
                a.StudentID = studentID;
                a.StudentName = studentname;
                a.SetPackageID = setpackageID;
                a.SetPrice = setPrice;
                a.DiscountID = discountID;
                a.DiscountPrice = discountPrice;
                a.PaymentType = paymentType;//1 zim, 2 onepay
                a.CreatedBy = createdby;
                a.CreatedDate = GetDateTime.Now;
                a.ExaminerID = examinerid;
                a.IsHide = false;
                a.Paid = false;
                db.tbl_SetPackageStudent.Add(a);
                db.SaveChanges();
                return a;
            }
        }
        public static List<tbl_SetPackageStudent> getbyStudentID(int studentID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.StudentID == studentID).ToList();
                return a;
            }
        }
        public static List<tbl_SetPackageStudent> getall()
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.ToList();
                return a;
            }
        }
        public static List<tbl_SetPackageStudent> getbysetid(int setid)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.IsHide != true && n.SetPackageID==setid).ToList();
                return a;
            }
        }
        public static tbl_SetPackageStudent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.ID == id).FirstOrDefault();
                return a;
            }
        }
        /// <summary>
        /// Lay set theo Id học viên và id set . coi nó có mua set đó chưa á mà
        /// </summary>
        /// <param name="setID"></param>
        /// <param name="studentID"></param>
        /// <returns></returns>
        public static tbl_SetPackageStudent get_by_setID_and_studentID(int setID, int studentID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.SetPackageID == setID && n.StudentID == studentID).FirstOrDefault();
                return a;
            }
        }
        public static tbl_SetPackageStudent updatePaid(int ID, bool paid, string createby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.Paid = paid;
                    a.ModifiedBy = createby;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return a;
            }
        }
        public static tbl_SetPackageStudent udpatePaymentType(int ID,int paymentType, string createby)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_SetPackageStudent.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.PaymentType = paymentType;
                    db.SaveChanges();
                }
                return a;
            }
        }
    }
}