using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class CoursesStudentTable
    {
        /// <summary>
        /// Thêm dữ liệu học viên vào khóa học
        /// </summary>
        /// <param name="customer">Học viên</param>
        /// <param name="course">Khóa họcv</param>
        /// <param name="codediscount">mã giảm giá</param>
        /// <param name="paid">đã thanh toán</param>
        /// <param name="appomentid">id hẹn đăng ký</param>
        /// <param name="deposit">só tiền đã cọc</param>
        /// <param name="note">ghi chú</param>
        /// <param name="payid">phương thức thanh toán</param>
        /// <param name="payname">tên phương thức</param>
        /// <param name="createby"></param>
        /// <param name="campaignsaleid">id chiến dịch sale</param>
        /// <param name="contract">hợp đồng, true, flase</param>
        /// <param name="datepriceleft">ngày thu nợ</param>
        /// <param name="commitment">cam kết</param>
        /// <param name="FatherID">id bảo lưu trước</param>
        /// <returns></returns>
        /// ,int rewardTeachers
        public static tbl_CoursesStudent insert(int rewardTeachers,tbl_Account customer, tbl_Courses course, int paid, int appomentid,
            int deposit, string note, int payid, string payname, string createby, int campaignsaleid, bool contract, string datepriceleft, string commitment,
            int FatherID, int comboid, tbl_Class commitmentpackage, int discountId, int discountPrice)
        {
            using (var db = new ZimEntities())
            {
                var school = SchoolTable.getbyID(course.SchoolID.Value);
                if (school == null)
                    school = new tbl_School() { ID = 0, CityName = "" };

                int dis = 0;
                tbl_CoursesStudent ck = new tbl_CoursesStudent();
                ck.StudentUID = customer.ID;
                ck.DiscountID = discountId;
                ck.PriceDiscout = discountPrice;
                ck.StudentName = customer.FullName;
                ck.SchoolID = school.ID;
                ck.SchoolName = school.SchoolName;
                ck.CourseID = course.ID;
                ck.CourseName = course.CourseName;
                ck.PriceCourse = course.Price;
                ck.DiscountID = ck.PriceDiscout = 0;
                ck.ComboID = comboid;
                if (commitmentpackage != null)
                {
                    ck.CommitmentID = commitmentpackage.ID;
                    ck.CommitmentName = commitmentpackage.ClassName;
                    ck.CommitmentPrice = commitmentpackage.PriceClass;
                }

                ck.AppointmentID = appomentid;
                ck.Deposit = deposit;
                ck.Paid = 0;
                ck.RewardTeachers = rewardTeachers;
                ck.TotalPaid = paid;
                ck.PriceLeft = course.Price.Value - dis - deposit;
                if (ck.PriceLeft <= 0)
                    ck.PriceLeft = 0;
                if (FatherID != 0)// nếu bảo lưu thì ko phải thu phí
                {
                    ck.PriceLeft = 0;
                    ck.Note = "[BẢO LƯU ID:" + FatherID + "] " + note;
                }
                else
                    ck.Note = note;
                ck.PaymentmethodID = payid;
                ck.PaymentmethodName = payname;
                ck.isHide = false;
                ck.CourseReserve = false;//bảo lưu
                ck.CampaignSaleID = campaignsaleid;
                ck.Contract = contract;
                ck.SupportUID = customer.SupportUID;
                ck.SourceID = customer.SourceID.Value;
                if (customer.SourceID.Value == 0)
                    ck.SourceName = "TVV";
                try
                {
                    if (!string.IsNullOrEmpty(datepriceleft))
                        ck.DatePriceLeft = DateTime.ParseExact(datepriceleft, "dd/MM/yyyy", null);// ngày thu nợ
                }
                catch { }
                ck.Commitment = commitment;//cam kết
                ck.FatherID = FatherID;// id bảo lưu
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.SendMail = false;
                db.tbl_CoursesStudent.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //thanh toán số tiền còn nợ
        public static tbl_CoursesStudent updatepaiddebts(int id, int price, string datepriceleft, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Paid = ck.Paid + price;
                    ck.TotalPaid = ck.Deposit + ck.Paid;
                    ck.PriceLeft = ck.PriceCourse - ck.PriceDiscout - ck.Paid - ck.Deposit;
                    try
                    {
                        if (!string.IsNullOrEmpty(datepriceleft))
                            ck.DatePriceLeft = DateTime.ParseExact(datepriceleft, "dd/MM/yyyy", null);// ngày thu nợ
                    }
                    catch { }
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        //hủy phiếu thu liên quan
        public static tbl_CoursesStudent updatepriceleft(int id, int price, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (ck.Paid - price > 0)
                    {
                        ck.Paid = ck.Paid - price;
                    }
                    else
                    {
                        int conlai = ck.Paid.Value - price;
                        ck.Paid = 0;
                        ck.Deposit = ck.Deposit + conlai;
                    }

                    ck.TotalPaid = ck.Deposit + ck.Paid;
                    ck.PriceLeft = ck.PriceCourse - ck.PriceDiscout - ck.Paid - ck.Deposit;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_CoursesStudent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_CoursesStudent> getbycourseid(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.CourseID == courseid && n.isHide != true).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesStudent> getListStudentInCourse(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.CourseID == courseid && n.isHide != true && n.CourseReserve != true).ToList();
                return l;
            }
        }

        /// <summary>
        /// Cập nhật trạng thái đã soạn chứng chỉ
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isCertificatce"></param>
        /// <returns></returns>
        public static bool UpdateIsCertificatce(int id, bool isCertificatce)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.isCertificatce = isCertificatce;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
        }

        public static List<tbl_CoursesStudent> getbystudentid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.StudentUID == studentid && n.isHide != true).ToList();
                return l;
            }
        }
        /// <summary>
        /// Kiểm tra thằng học viên đó có mặt trong bảng này ko
        /// </summary>
        /// <param name="studentid"></param>
        /// <returns></returns>
        public static string checkinhere(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.StudentUID == studentid).ToList();
                var l2 = db.tbl_StudentAppointment.Where(n => n.StudentUID == studentid).ToList();
                if (l.Count > 0 || l2.Count > 0)
                {
                    if (l.Count > 0)
                    {
                        bool checkhoantat = true;
                        foreach (var item in l)
                        {
                            var course = CoursesTable.getbyid(item.CourseID.Value);
                            if (course.Status != 3)
                            {
                                checkhoantat = false;
                                break;
                            }
                        }
                        if (checkhoantat == true)
                            return "Đã xong";
                        else
                            return "Đã đăng kí";

                    }
                    return "Đã đăng kí";
                }
                else
                {
                    return "";
                }
                //var baoluu = db.tbl_StudentAppointment.Where(n => n.isHide != true && n.CourseStudentID != 0 && n.StudentUID == studentid).ToList();
                //if (baoluu.Count > 0)
                //{
                //    return "Bảo lưu";
                //}

            }
        }

        public static List<tbl_CoursesStudent> getbycampid(int CampaignSaleID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.CampaignSaleID == CampaignSaleID).ToList();
                return l;
            }
        }

        public static List<tbl_CoursesStudent> getbydiscountid(int discountid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.DiscountID == discountid).ToList();
                return l;
            }
        }

        public static tbl_CoursesStudent removestudentoutcourse(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Note = l.Note + " => " + note;
                    l.isHide = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        /// <summary>
        /// cập nhật bảo lưu => kich khoi lop
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_CoursesStudent updatereserve(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Note = l.Note + " => " + note;
                    l.CourseReserve = true;
                    l.isHide = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_CoursesStudent UpdateChungChi(int id, string CertificateFile)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.CertificateFile = CertificateFile;
                    db.SaveChanges();
                }
                return l;
            }
        }
        //Lấy tất cả
        public static List<tbl_CoursesStudent> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.ToList();
                return l;
            }
        }
        //Cập nhật chuyển khóa
        public static tbl_CoursesStudent updatechangecourse(int id, tbl_Courses course, int discountid, int discountprice, int paid, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    var school = SchoolTable.getbyID(course.SchoolID.Value);
                    if (school == null)
                        return null;

                    ck.SchoolID = school.ID;
                    ck.SchoolName = school.SchoolName;
                    ck.CourseID = course.ID;
                    ck.CourseName = course.CourseName;
                    ck.PriceCourse = course.Price;
                    ck.DiscountID = ck.PriceDiscout = 0;

                    var d = DiscountTable.getbyid(discountid);
                    if (d != null)
                    {
                        ck.DiscountID = d.ID;
                        ck.PriceDiscout = discountprice;
                    }
                    ck.Paid = ck.TotalPaid + paid;//logic IEC k quan tâm khóa học bao nhiêu tiền mà quan tâm HV đóng bao nhiêu tiền
                    ck.TotalPaid = ck.TotalPaid + paid;
                    ck.PriceLeft = 0;
                    ck.Note = ck.Note + " [Ghi chú chuyển lớp] > " + note;
                    ck.isHide = false;
                    ck.CourseReserve = false;//bảo lưu
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        //Thanh toán
        public static tbl_CoursesStudent updatePayCourse(int id, int paid, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Paid = ck.Paid + paid;
                    ck.TotalPaid = ck.TotalPaid + paid;
                    ck.PriceLeft = ck.PriceLeft - paid;
                    ck.Note = ck.Note + " [Ghi chú thanh toán] > " + note;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }
        /// <summary>
        /// Hủy phiếu thu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_CoursesStudent delete(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Note = l.Note + " => " + note;
                    l.isHide = false;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        
        public static tbl_CoursesStudent UpdateHideExpired(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.HideInExpired = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        /// <summary>
        /// add vào lại lớp
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_CoursesStudent ReJoin(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Note = l.Note + " => " + note;
                    l.isHide = false;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }
        public static List<tbl_CoursesStudent> getbycomboid(int combo)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.ComboID == combo && n.isHide != true).ToList();
                return l;
            }
        }
        public static tbl_CoursesStudent getbyCourseIDdandStudentID(int courseID, int studentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.CourseID == courseID && n.StudentUID == studentID && n.isHide != true).FirstOrDefault();
                return l;
            }
        }
        public static List<int> getStudentIDByCourseID(int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_CoursesStudent.Where(n => n.CourseID == courseid && n.isHide != true).Select(x => x.StudentUID.Value).ToList();
                return l;
            }
        }
    }
}