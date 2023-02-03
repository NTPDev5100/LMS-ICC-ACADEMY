using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CertificatceCustomerTable
    {
        public static tbl_CertificatceCustomer insert(int uid, int coursestudentid, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CertificatceCustomer ck = db.tbl_CertificatceCustomer.Where(n => n.StudentID == uid && n.CourseStudentID == coursestudentid).FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_CertificatceCustomer();
                    ck.StudentID = uid;
                    ck.CourseStudentID = coursestudentid;
                    ck.CertificatceContent = "";
                    ck.isAccept = false;//0 chưa duyệt, 1 đã duyệt
                    ck.CodeCheck = "";
                    ck.isHide = false;
                    ck.Note = "";
                    ck.CreatedBy = createby;
                    ck.CreatedDate = GetDateTime.Now;
                    db.tbl_CertificatceCustomer.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    //ck.StudentID = uid;
                    //ck.CourseStudentID = coursestudentid;
                    //ck.isAccept = false;//0 chưa duyệt, 1 đã duyệt
                    //ck.isHide = false;
                    //ck.ModifiedBy = createby;
                    //ck.ModifiedDate = GetDateTime.Now;
                    //db.SaveChanges();
                    return ck;
                }
            }
        }
        public static tbl_CertificatceCustomer getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_CertificatceCustomer ck = db.tbl_CertificatceCustomer.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }
        public static tbl_CertificatceCustomer getbycoursestudentid(int coursestudentid)
        {
            using (var db = new ZimEntities())
            {
                tbl_CertificatceCustomer ck = db.tbl_CertificatceCustomer.Where(n => n.CourseStudentID == coursestudentid).FirstOrDefault();
                return ck;
            }
        }

        /// <summary>
        /// Tìm HV này - khóa học này => đã có chứng chỉ chưa
        /// </summary>
        /// <param name="StudentId"></param>
        /// <param name="coursestudentid"></param>
        /// <returns></returns>
        public static tbl_CertificatceCustomer GetByStudentIdCoursesstudentId(int StudentId, int coursestudentid)
        {
            using (var db = new ZimEntities())
            {
                tbl_CertificatceCustomer ck = db.tbl_CertificatceCustomer.Where(n => n.StudentID == StudentId && n.CourseStudentID == coursestudentid).FirstOrDefault();
                return ck;
            }
        }

        public static tbl_CertificatceCustomer updateaccept(int id, bool isAccept, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_CertificatceCustomer ck = db.tbl_CertificatceCustomer.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.isAccept = isAccept;//0 chưa duyệt, 1 đã duyệt
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
    }
}