using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class InvoicesTable
    {
        /// <summary>
        /// thêm dữ liệu
        /// </summary>
        /// <param name="refund">true là phiếu chi, false phiếu thu</param>
        /// <param name="coursestudentID">id học viên trong khóa</param>
        /// <param name="appointmentid">id học viên hẹn đăng ký</param>
        /// <param name="servicestudentID">id học viên mua dịch vụ</param>
        /// <param name="refundid">id yêu cầu hoàn tiền</param>
        /// <param name="comboid">id đăng ký combo khóa học</param>
        /// <param name="cus">khách hàng</param>
        /// <param name="price">Số tiền</param>
        /// <param name="priceleft">số tiền còn lại</param>
        /// <param name="note">ghi chú</param>
        /// <param name="qrcode">hình barcode</param>
        /// <param name="code">mã</param>
        /// <param name="createby">tạo bởi</param>
        /// <param name="datepriceleft">ngày hẹn thu nơn</param>
        /// <param name="payid">phương thức thanh toán</param>
        /// <param name="payname">tên phương thức thanh toán</param>
        /// <param name="school">nơi nhập, xuất phiếu</param>
        /// <param name="setpackageStudentID">nơi nhập, xuất phiếu</param>
        /// <returns></returns>
        public static tbl_Invoices insert(int rewardTeachers,bool refund, int coursestudentID, int appointmentid, int servicestudentID, int refundid, int comboid, tbl_Account cus, int price, int priceleft, string note, string camket,
            string qrcode, string code, tbl_Account createby, string datepriceleft, int payid, string payname, tbl_School school, int setpackageStudentID)
        {
            using (var db = new ZimEntities())
            {
                // string name = "3-" + cs.ID + "-" + GetDateTime.Now.ToString("ddMMyyyyHHmmss") + "";//0 mua khóa, 1 đăng ký giữ chỗ, 2 mua dịch vụ, 3 thanh toán tiền thiếu, 4 xuất  phiếu thu, 5 đóng thêm giữ chỗ, 6 cái gì mà quên rồi, 7 thanh toán set
                tbl_Invoices ck = new tbl_Invoices();
                ck.Refund = refund;// true phiếu chi
                ck.CourseStudentID = coursestudentID;
                ck.RewardTeachers = rewardTeachers;
                ck.AppointmentID = appointmentid;
                ck.ServiceStudent = servicestudentID;
                ck.RefundID = refundid;
                ck.ComboID = comboid;
                
                ck.Price = price;
                ck.PriceLeft = priceleft;
                ck.ReasonNote = note;
                ck.QRCode = qrcode;
                ck.Code = code;
                ck.Camket = camket;
                try
                {
                    ck.StudentUID = cus.ID;
                    ck.StudentFullName = cus.FullName;
                    ck.StudentPhone = cus.Phone;
                    ck.StudentAddress = cus.Address;
                    ck.IdentityCard = cus.IdentityCard;
                    ck.IdentityCardDate = cus.IdentityCardDate;
                    ck.IdentityCardCity = cus.IdentityCardCityName;
                    ck.NumberInvoice = getnumberinvoice(GetDateTime.Now.Date, refund);
                    ck.CreatedBy = ck.ModifiedBy = createby.UserName;
                    ck.CreatedInvoice = createby.FullName;
                    ck.SchoolID = school.ID;
                    ck.SchoolName = school.SchoolName;
                }
                catch { }
                
                if (!string.IsNullOrEmpty(datepriceleft))
                {
                    try
                    {
                        ck.DatePriceLeft = DateTime.ParseExact(datepriceleft.Replace(" ", ""), "dd/MM/yyyy", null);
                    }
                    catch { }
                }
                
                ck.PaymentmethodID = payid;
                ck.PaymentmethodName = payname;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.SetPackageStudentID = setpackageStudentID;
                ck.isHide = false;
                ck.IsDelete = false;
                db.tbl_Invoices.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static string getnumberinvoice(DateTime date, bool refund)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => DbFunctions.TruncateTime(n.CreatedDate) == date && n.Refund == refund && n.isHide != true).ToList();
                int count = l.Count + 1;
                string c = count.ToString();
                if (count < 10)
                    c = "00" + count.ToString();
                if (count >= 10 && count < 100)
                    c = "0" + count.ToString();
                string num = GetDateTime.Now.ToString("yyyyMMdd") + "-" + c;
                return num;
            }
        }

        public static tbl_Invoices getbycode(string code)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.Code == code && n.isHide != true).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Invoices> getbystudentid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.StudentUID == studentid && n.isHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }

        public static tbl_Invoices getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Invoices> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.isHide != true && n.IsDelete == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// lấy số lượng hóa đơn được tạo bỏi user
        /// </summary>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static int getbycreateby(string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.Refund != true && n.CreatedBy == createby && n.isHide != true).ToList();
                return l.Count;
            }
        }

        public static tbl_Invoices updatecode(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.ID == ID).FirstOrDefault();
                if (l != null)
                {
                    string qrcode = l.QRCode.Replace("/Upload/qrcode/", "");
                    qrcode = qrcode.Replace(".jpg", "");
                    var code = AppZim.ZIM.Encryptor.EncryptURL(qrcode);
                    if (l.Code != code)
                        l.Code = code;
                    if (string.IsNullOrEmpty(l.CreatedInvoice))
                    {
                        var ac = db.tbl_Account.Where(n => n.UserName == l.CreatedBy).FirstOrDefault();
                        if (ac != null)
                            l.CreatedInvoice = ac.FullName;
                    }
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static tbl_Invoices Delete(int ID,bool IsDelete)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.ID == ID).FirstOrDefault();
                if (l != null)
                {
                    l.IsDelete = IsDelete;
                    db.SaveChanges();
                }
                return l;
            }
        }
        /// <summary>
        /// Hủy phiếu thu
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Invoices deleteinvoice(int ID, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.ID == ID).FirstOrDefault();
                if (l != null)
                {
                    l.isHide = true;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static void UpdatePrice(int studentId, int appointmentId, int money, string username)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Invoices.Where(n => n.StudentUID == studentId && n.AppointmentID == appointmentId).FirstOrDefault();
                if (l != null)
                {
                    l.Price += money;
                    l.ModifiedBy = username;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }
    }
}