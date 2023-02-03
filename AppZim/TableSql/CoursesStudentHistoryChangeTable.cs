using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesStudentHistoryChangeTable
    {
        //thêm dữ liệu
        public static tbl_CoursesStudentHistoryChange insert(tbl_CoursesStudent d, int courseNewID, string createby)
        {
            using (var db = new ZimEntities())
            {

                tbl_CoursesStudentHistoryChange ck = new tbl_CoursesStudentHistoryChange();
                ck.CoursesStudentID = d.ID;
                ck.SchoolID = d.SchoolID;
                ck.SchoolName = d.SchoolName;
                ck.StudentUID = d.StudentUID;
                ck.StudentName = d.StudentName;
                ck.CourseID = d.CourseID;
                ck.CourseNewID = courseNewID;
                ck.CourseName = d.CourseName;
                ck.PriceCourse = d.PriceCourse;
                ck.DiscountID = d.DiscountID;
                ck.PriceDiscout = d.PriceDiscout;
                ck.AppointmentID = d.AppointmentID;
                ck.Deposit = d.Deposit;
                ck.Paid = d.Paid;
                ck.TotalPaid = d.TotalPaid;
                ck.PriceLeft = d.PriceLeft;
                ck.Note = d.Note;
                ck.PaymentmethodID = d.PaymentmethodID;
                ck.PaymentmethodName = d.PaymentmethodName;
                ck.isHide = d.isHide;
                ck.CampaignSaleID = d.CampaignSaleID;
                ck.Contract = d.Contract;
                ck.SupportUID = d.SupportUID;
                ck.SourceID = d.SourceID;
                ck.SourceName = d.SourceName;
                ck.DatePriceLeft = d.DatePriceLeft;
                ck.Commitment = d.Commitment;
                ck.CourseReserve = d.CourseReserve;
                ck.FatherID = d.FatherID;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_CoursesStudentHistoryChange.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
       
        public static List<tbl_CoursesStudentHistoryChange> getbyCSID(int CoursesStudentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudentHistoryChange.Where(n => n.CoursesStudentID == CoursesStudentID).ToList();                
                return l;
            }
        }

        public static List<tbl_CoursesStudentHistoryChange> getbystudentuid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudentHistoryChange.Where(n => n.StudentUID == studentid).ToList();
                return l;
            }
        }

        public static tbl_CoursesStudentHistoryChange getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudentHistoryChange.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CoursesStudentHistoryChange> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudentHistoryChange.ToList();
                return l;
            }
        }
    }
}