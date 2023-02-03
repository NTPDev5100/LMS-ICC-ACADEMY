using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class RefundsTable
    {
        //thêm dữ liệu
        public static tbl_Refunds insert(int coursestudentID, int appointmentid, int servicestudentID, tbl_Account cus, int price, string note,
            tbl_School school, bool kichout, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = new tbl_Refunds();
                ck.SchoolID = school.ID;
                ck.SchoolName = school.SchoolName;

                ck.CourseStudentID = coursestudentID;
                if (coursestudentID != 0)
                    ck.SourceRefund = "Khóa học";

                ck.AppointmentID = appointmentid;
                if (appointmentid != 0)
                    ck.SourceRefund = "Giữ chỗ";

                ck.ServiceStudent = servicestudentID;
                if (servicestudentID != 0)
                    ck.SourceRefund = "Dịch vụ";

                ck.StudentUID = cus.ID;
                ck.StudentFullName = cus.FullName;
                ck.StudentPhone = cus.Phone;
                ck.Price = price;
                ck.ReasonNote = note;
                ck.StatusID = 1;// 1 Yêu cầu mới, 2 Duyệt, 3 Không duyệt
                ck.StatusName = "<span>Yêu cầu mới</span>";
                ck.RefundDone = false;
                ck.KickOut = kichout;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_Refunds.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Refunds updatestatus(int id, int price, int status, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Price = price;
                    ck.StatusID = status;// 1 Yêu cầu mới, 2 Duyệt, 3 Không duyệt
                    if (status == 1)
                        ck.StatusName = "<span>Yêu cầu mới</span>";
                    if (status == 2)
                        ck.StatusName = "<span style=\"color: #00e632;\">Duyệt</span>";
                    if (status == 3)
                        ck.StatusName = "<span style=\"color: #ff0000;\">Không duyệt</span>";
                    if (!string.IsNullOrEmpty(note))
                        ck.ReasonNote = note;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Refunds updatedone(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.RefundDone = true;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_Refunds getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }
        public static tbl_Refunds getbycoursestudentid(int coursestudentid)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.CourseStudentID == coursestudentid).FirstOrDefault();
                return ck;
            }
        }
        public static tbl_Refunds getbyappointmentid(int appointmentid)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.AppointmentID == appointmentid).FirstOrDefault();
                return ck;
            }
        }
        public static tbl_Refunds getbyservicestudentid(int servicestudentid)
        {
            using (var db = new ZimEntities())
            {
                tbl_Refunds ck = db.tbl_Refunds.Where(n => n.ServiceStudent == servicestudentid).FirstOrDefault();
                return ck;
            }
        }

        public static List<tbl_Refunds> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Refunds.ToList();
                return l;
            }
        }
    }
}