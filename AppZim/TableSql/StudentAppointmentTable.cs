using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AppZim.TableSql
{
    public class StudentAppointmentTable
    {
        //thêm dữ liệu
        public static tbl_StudentAppointment insert(tbl_Account ac, tbl_District district, tbl_School school, tbl_Class lop, tbl_StudyTime study, int rewardTeachers,
            int deposit, string note, int payid, string payname, string createby, bool reserve, int coursestudentid,
            string datelinerecerve, string dateappont, tbl_Class commitment, int? discountID, int? discountPrice,
            int paymentreceiveid, int coefficient, string listwanttoday, int customPrice, int lesson)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointment ck = new tbl_StudentAppointment();
                ck.StudentUID = ac.ID;
                ck.StudentPhone = ac.Phone;
                ck.StudentName = ac.FullName;
                ck.RewardTeachers = rewardTeachers;
                if (district != null)
                {
                    ck.CityID = district.LocationID;
                    ck.CityName = district.LocationName;
                    ck.DistrictID = district.ID;
                    ck.DistrictName = district.DistrictName;
                }
                ck.SchoolID = school.ID;
                ck.SchoolName = school.SchoolName;
                if (lop != null)
                {
                    ck.ClassID = lop.ID;
                    ck.ClassName = lop.ClassName;
                    ck.ClassPrice = lop.PriceClass.Value;
                }
                if (commitment != null)
                {
                    ck.CommitmentID = commitment.ID;
                    ck.CommitmentName = commitment.ClassName;
                    ck.CommitmentPrice = commitment.PriceClass.Value;
                }
                if (study != null)
                {
                    ck.StudyID = study.ID;
                    ck.StudyName = study.StudyTimeName;
                }
                ck.Deposit = deposit;
                ck.Note = note;
                ck.PaymentMethodID = payid;
                ck.PaymentMethodName = payname;
                ck.PaymentTeacherReceiveID = MoneyReceiverTable.GetUserReceiverId(paymentreceiveid) ?? 0;
                ck.PaymentTeacherReceiveName = AccountTable.GetFullName(MoneyReceiverTable.GetUserReceiverId(paymentreceiveid) ?? 0) ?? "";
                ck.MoneyReceiverId = paymentreceiveid;
                ck.DiscountID = discountID;
                ck.DiscountPrice = discountPrice;
                ck.CustomPrice = customPrice;
                ck.isCreateContract = false;
                ck.isHide = false;
                ck.ComboID = 0;
                ck.CloseAppointment = 0;//1 đã hủy
                ck.CourseStudentID = coursestudentid;// id cần bảo lưu
                ck.CourseStudentReserve = reserve;// true bao lưu
                if (!string.IsNullOrEmpty(datelinerecerve))// hạn chót bảo lưu
                {
                    try
                    {
                        ck.DeadLineReserve = DateTime.ParseExact(datelinerecerve, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        return null;
                    }
                }

                if (!string.IsNullOrEmpty(dateappont))// ngày đặt hẹn
                {
                    try
                    {
                        ck.DateAppointment = DateTime.ParseExact(dateappont, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        return null;
                    }
                }

                //kiểm tra xem nếu người thêm là tvv và trong thời gian chiến dịch thì add id chiến dịch vào
                //var acc = db.tbl_Account.Where(x => x.UserName == createby).First();
                //if(acc.RoleID == 6)
                //{
                //    // lay danh sach chien dich con hieu luc
                //    var timeNow = DateTime.Now.Date;
                //    var CampaignSale = db.tbl_CampaignSale.Where(x => x.sTime <= timeNow && timeNow <= x.eTime && x.IsHide == false).FirstOrDefault();
                //    if(CampaignSale != null)
                //    {
                //        ck.CampaignSaleID = CampaignSale.ID;
                //    }
                //}
                ck.Lesson = lesson;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                //tính phần trăm đóng tiền lần đầu
                //if(ck.ClassPrice == 0 || ck.ClassPrice == null)
                //{
                //    ck.DepositPercent = 100;
                //}
                //else
                //{
                //    ck.DepositPercent = Math.Round((ck.Deposit.Value + ck.DiscountPrice.Value) / (double)(ck.ClassPrice.Value + ck.CommitmentPrice), 1) * 100;
                //}
                ck.CoefficientID = coefficient;
                ck.CoefficientName = GetCoefficientName(coefficient);
                //ck.ListWantToDay = listwanttoday;
                db.tbl_StudentAppointment.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static string GetCoefficientName(int id)
        {
            string name = "";
            switch (id)
            {
                case 1:
                    name = "Tháng";
                    break;
                case 2:
                    name = "50% học phí";
                    break;
                case 3:
                    name = "100% học phí";
                    break;
                default:
                    name = "Khác";
                    break;
            }
            return name;
        }
        //Thêm dữ liệu kèm comboID
        public static tbl_StudentAppointment insertwithCombo(tbl_Account ac, tbl_District district, tbl_School school, tbl_Class lop, tbl_StudyTime study,
            int deposit, string note, int payid, string payname, string createby, bool reserve,
            int coursestudentid, string datelinerecerve, string dateappont, int comboID, int lesson,
            tbl_Class commitment, tbl_Discount discount, int rewardTeachers, int customPrice, int paymentreceiveid, int coefficient)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointment ck = new tbl_StudentAppointment();
                ck.StudentUID = ac.ID;
                ck.StudentPhone = ac.UserName;
                ck.CustomPrice = customPrice;
                ck.RewardTeachers = rewardTeachers;
                ck.StudentName = ac.FullName;
                ck.CommitmentID = commitment.ID;
                ck.CommitmentName = commitment.ClassName;
                ck.CommitmentPrice = commitment.PriceClass;
                ck.DiscountID = discount.ID;
                ck.DiscountPrice = discount.DiscountPrice;
                ck.Lesson = lesson;
                ck.PaymentTeacherReceiveID = MoneyReceiverTable.GetUserReceiverId(paymentreceiveid) ?? 0;
                ck.PaymentTeacherReceiveName = AccountTable.GetFullName(MoneyReceiverTable.GetUserReceiverId(paymentreceiveid) ?? 0) ?? "";
                ck.MoneyReceiverId = paymentreceiveid;
                if (district != null)
                {
                    ck.CityID = district.LocationID;
                    ck.CityName = district.LocationName;
                    ck.DistrictID = district.ID;
                    ck.DistrictName = district.DistrictName;
                }
                if (school != null)
                {
                    ck.SchoolID = school.ID;
                    ck.SchoolName = school.SchoolName;
                }
                if (lop != null)
                {
                    ck.ClassID = lop.ID;
                    ck.ClassName = lop.ClassName;
                    ck.ClassPrice = lop.PriceClass;
                }
                if (study != null)
                {
                    ck.StudyID = study.ID;
                    ck.StudyName = study.StudyTimeName;
                }
                ck.Deposit = deposit;
                ck.Note = note;
                ck.PaymentMethodID = payid;
                ck.PaymentMethodName = payname;
                ck.isHide = false;
                ck.CloseAppointment = 0;//1 đã hủy
                ck.CourseStudentID = coursestudentid;// id cần bảo lưu
                ck.CourseStudentReserve = reserve;// true bao lưu
                ck.ComboID = comboID;
                if (!string.IsNullOrEmpty(datelinerecerve))// hạn chót bảo lưu
                {
                    try
                    {
                        ck.DeadLineReserve = DateTime.ParseExact(datelinerecerve, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        return null;
                    }
                }
                if (!string.IsNullOrEmpty(dateappont))// ngày đặt hẹn
                {
                    try
                    {
                        ck.DateAppointment = DateTime.ParseExact(dateappont, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        return null;
                    }
                }
                ck.CoefficientID = coefficient;
                ck.CoefficientName = GetCoefficientName(coefficient);
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_StudentAppointment.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //thêm dữ liệu
        public static tbl_StudentAppointment update(int id, tbl_Account ac, tbl_City city, tbl_School school, tbl_Class lop, tbl_StudyTime study,
            int deposit, string note, int payid, string payname, string createby, string dateappointment, int lesson)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointment ck = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.StudentUID = ac.ID;
                    ck.StudentName = ac.FullName;
                    if (city != null)
                    {
                        ck.CityID = city.ID;
                        ck.CityName = city.CityName;
                    }
                    //ck.DistrictID = district.ID;
                    //ck.DistrictName = district.DistrictName;
                    if (school != null)
                    {
                        ck.SchoolID = school.ID;
                        ck.SchoolName = school.SchoolName;
                    }
                    ck.ClassID = lop.ID;
                    ck.ClassName = lop.ClassName;
                    if (study != null)
                    {
                        ck.StudyID = study.ID;
                        ck.StudyName = study.StudyTimeName;
                    }
                    if (0 != deposit)
                        ck.Deposit = ck.Deposit + deposit;

                    if (!string.IsNullOrEmpty(note))
                        ck.Note = note;

                    ck.PaymentMethodID = payid;
                    ck.PaymentMethodName = payname;
                    ck.isHide = false;
                    if (!string.IsNullOrEmpty(dateappointment))// ngày đặt hẹn
                    {
                        try
                        {
                            ck.DateAppointment = DateTime.ParseExact(dateappointment, "dd/MM/yyyy", null);
                        }
                        catch
                        {
                            return null;
                        }
                    }
                    ck.Lesson = lesson;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_StudentAppointment edit(int id, int program, string discountCode, int commitment,
            int paymentMethod, int customPrice, string note, string createBy, int teacher, DateTime? createdDate)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointment ck = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    if (ck.ClassID != program)
                    {
                        var classes = ClassTable.getbyID(program);
                        ck.ClassID = classes?.ID;
                        ck.ClassName = classes?.ClassName;
                        ck.ClassPrice = classes?.PriceClass;
                    }
                    if (ck.DiscountID == 0)
                    {
                        var discount = DiscountTable.getbycode(discountCode);
                        ck.DiscountID = discount?.ID;
                        ck.DiscountPrice = discount?.DiscountPrice;
                    }
                    if (ck.CommitmentID != commitment)
                    {
                        var commitments = ClassTable.getbyID(commitment);
                        ck.CommitmentID = commitments?.ID;
                        ck.CommitmentName = commitments?.ClassName;
                        ck.CommitmentPrice = commitments?.PriceClass;
                    }
                    if (ck.PaymentMethodID != paymentMethod)
                    {
                        ck.PaymentMethodID = paymentMethod;
                        switch (paymentMethod)
                        {
                            case 1:
                                ck.PaymentMethodName = "Tiền mặt";
                                break;
                            case 2:
                                ck.PaymentMethodName = "Thanh toán online";
                                break;
                            case 4:
                                ck.PaymentMethodName = "Quẹt thẻ";
                                break;
                            case 5:
                                ck.PaymentMethodName = "Chuyển khoản";
                                break;
                            case 6:
                                ck.PaymentMethodName = "Giáo viên thu hộ";
                                break;
                            default:
                                ck.PaymentMethodName = "Khác";
                                break;
                        }
                        if (ck.PaymentMethodID == 6)
                        {
                            ck.PaymentTeacherReceiveID = teacher;
                            var tea = AccountTable.getbyID(teacher);
                            ck.PaymentTeacherReceiveName = tea?.FullName;
                        }
                        else
                        {
                            ck.PaymentTeacherReceiveID = 0;
                            ck.PaymentTeacherReceiveName = "";
                        }
                    }
                    ck.CustomPrice = customPrice;
                    ck.Note = note;
                    ck.CreatedDate = createdDate == null ? ck.CreatedDate : createdDate;
                    ck.ModifiedBy = createBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static bool updatedone(int id, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l == null)
                    return false;
                else
                {
                    l.isHide = true;
                    l.CourseStudentReserve = false;
                    l.DeadLineReserve = null;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return true;
                }
            }
        }

        public static tbl_StudentAppointment updateclose(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l == null)
                    return l;
                else
                {
                    l.CloseAppointment = 1;//đã hủy
                    l.Note = l.Note + " => " + note;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return l;
                }
            }
        }

        public static tbl_StudentAppointment getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_StudentAppointment> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.ToList();
                return l;
            }
        }
        public static tbl_StudentAppointment getbycomboclass(int comboid, int classid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ComboID == comboid && n.ClassID == classid && n.CloseAppointment != 1).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_StudentAppointment> getbystudentid(int studentid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.StudentUID == studentid).ToList();
                return l;
            }
        }
        /// <summary>
        /// Hủy phiếu thu
        /// </summary>
        /// <param name="id"></param>
        /// <param name="note"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_StudentAppointment delete(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
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
        public static List<tbl_StudentAppointment> getbycomboid(int combo)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ComboID == combo && n.isHide != true).ToList();
                return l;
            }
        }

        public static tbl_StudentAppointment udpateDeposit(int id, int priceback, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Deposit = l.Deposit - priceback;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static List<tbl_StudentAppointment> getByCampaignSaleID(int CampaignSaleID, string createBy)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(x => x.isHide == false && (x.CloseAppointment == 0 || x.DoneSalary == true) && x.CampaignSaleID == CampaignSaleID && x.CreatedBy == createBy).ToList();
                return l;
            }
        }
        public static List<tbl_StudentAppointment> getByCampaignSaleID(int CampaignSaleID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(x => x.isHide == false && (x.CloseAppointment == 0 || x.DoneSalary == true) && x.CampaignSaleID == CampaignSaleID).ToList();
                return l;
            }
        }

        public static List<tbl_StudentAppointment> getbydiscountid(int discountid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(x => x.isHide == false && x.DiscountID == discountid).ToList();
                return l;
            }
        }
        /// <summary>
        /// bảo lưu => mở lại chờ xếp lớp
        /// </summary>
        /// <param name="id"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_StudentAppointment ReActive(int id, string createby, string deadline)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.CourseStudentReserve = true;
                    try
                    {
                        l.DeadLineReserve = DateTime.ParseExact(deadline, "dd/MM/yyyy", null);
                    }
                    catch
                    {
                        l.DeadLineReserve = DateTime.Now;
                    }

                    l.isHide = false;
                    l.CloseAppointment = 0;
                    l.ModifiedBy = createby;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return l;
            }
        }

        public static void UpdatePrice(int appointmentID, int money, string userName)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == appointmentID).FirstOrDefault();
                if (l != null)
                {
                    l.CustomPrice += money;
                    l.ModifiedBy = userName;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        public static void CancelReserve(int id, string userName)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.CourseStudentReserve = false;
                    l.DeadLineReserve = null;
                    l.ModifiedBy = userName;
                    l.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        public async static Task<List<get_list_of_student_appointment_Result>> GetListStudentAppointment(string search,
            int lop, int ca, string from, string to, int pagesize, int pageindex, int aclog, int role, int language)
        {
            using (var db = new ZimEntities())
            {
                var data = db.get_list_of_student_appointment(search, lop, ca, from, to, pagesize, pageindex, aclog, role, language).ToList();
                return await Task.FromResult(data);
            }
        }

        public async static Task UpdateLesson(int id, int registerlesson)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.Lesson = registerlesson;
                    await db.SaveChangesAsync();
                }
            }
        }
    }
}