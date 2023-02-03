using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ComboDetailTable
    {
        /// <summary>
        /// thêm dữ liệu
        /// </summary>
        /// <param name="comboid">combo id</param>
        /// <param name="course">khóa học</param>
        /// <param name="createby">người tạo</param>
        /// <returns></returns>
        public static tbl_ComboDetail insert(int comboid, int classId, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ComboDetail ck = new tbl_ComboDetail();
                ck.ComboID = comboid;
                ck.ClassID = classId;
                ck.IsDone = false;
                ck.CourseID = 0;
                ck.CourseName = "";
                ck.CoursePrice = 0;
                ck.DiscountPrice = 0;
                ck.IsHide = false;
                ck.TypeID = 1;//1 là id khóa, 2 là id lớp
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ComboDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ComboDetail insertButClass(int comboid, tbl_Class classOfClan, int discount, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ComboDetail ck = new tbl_ComboDetail();
                ck.ComboID = comboid;
                ck.CourseID = classOfClan.ID;
                ck.CourseName = classOfClan.ClassName;
                ck.CoursePrice = classOfClan.PriceClass;
                ck.DiscountPrice = discount;
                ck.TypeID = 2;// là id khóa, 2 là id lớp
                ck.IsHide = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ComboDetail.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        /// <summary>
        /// Lấy ra dữ liệu theo comboid
        /// </summary>
        /// <param name="comboid">comboid</param>
        /// <returns></returns>
        public static List<tbl_ComboDetail> getbycomboid(int comboid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ComboDetail.Where(n => n.ComboID == comboid && n.IsHide == false && n.IsDone == false).ToList();
                return l;
            }
        }
        /// <summary>
        /// Lấy dữ liệu theo combo và khóa học id
        /// </summary>
        /// <param name="comboid">id combo</param>
        /// <param name="courseid">id khóa</param>
        /// <returns></returns>
        public static tbl_ComboDetail getbycomboidvscourseid(int comboid, int courseid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ComboDetail.Where(n => n.ComboID == comboid && n.CourseID == courseid && n.IsHide == false).FirstOrDefault();
                return l;
            }
        }
        public static List<tbl_ComboDetail> LoadBack(int comboID, int moneypaid)
        {
            using (var db = new ZimEntities())
            {
                List<tbl_ComboDetail> lcbd = new List<tbl_ComboDetail>();
                var Combo = db.tbl_Combo.Where(n => n.ID == comboID).FirstOrDefault();
                var combodetail = db.tbl_ComboDetail.Where(n => n.ComboID == comboID).ToList();
                List<tbl_ComboDetail> temp = new List<tbl_ComboDetail>();
                foreach (var item in combodetail)
                {
                    if (item.TypeID == 1 || item.TypeID == null)
                    {
                        var studentcourse = CoursesStudentTable.getbycourseid(item.CourseID.Value).Where(n => n.ComboID == item.ComboID).FirstOrDefault();
                        tbl_ComboDetail c = new tbl_ComboDetail();
                        c.ComboID = Combo.ID;
                        c.CoursePrice = studentcourse.PriceLeft;
                        c.TypeID = 1;
                        c.CourseID = studentcourse.CourseID;
                        temp.Add(c);



                    }
                    else
                    {
                        var appointment = StudentAppointmentTable.getbycomboclass(item.ComboID.Value, item.CourseID.Value);
                        if (appointment.isHide == true)
                        {
                            var studentcourse = CoursesStudentTable.getbystudentid(appointment.StudentUID.Value).Where(n => n.ComboID == Combo.ID && n.AppointmentID == appointment.ID).FirstOrDefault();
                            tbl_ComboDetail c = new tbl_ComboDetail();
                            c.ComboID = Combo.ID;
                            c.CoursePrice = studentcourse.PriceLeft;
                            c.TypeID = 1;
                            c.CourseID = studentcourse.CourseID;
                            temp.Add(c);


                        }
                        else
                        {
                            var cl = ClassTable.getbyID(appointment.ClassID.Value);
                            var discount = ComboDetailTable.getbycomboidvscourseid(Combo.ID, item.CourseID.Value);
                            int priceleft = cl.PriceClass.Value - appointment.Deposit.Value - discount.DiscountPrice.Value;
                            tbl_ComboDetail c = new tbl_ComboDetail();
                            c.ComboID = Combo.ID;
                            c.CoursePrice = priceleft;
                            c.TypeID = 2;
                            c.CourseID = appointment.ClassID;
                            temp.Add(c);
                        }
                    }
                }
                temp.OrderBy(n => n.CoursePrice);
                foreach (var item in temp)
                {

                }




                return null;
            }

        }
        public static void Delete(int comboId, int classId, string username)
        {
            using (var db = new ZimEntities())
            {
                var rs = db.tbl_ComboDetail.Where(x => x.ComboID == comboId && x.ClassID == classId).FirstOrDefault();
                if (rs != null)
                {
                    rs.IsHide = true;
                    rs.ModifiedBy = username;
                    rs.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// cập nhật chương trình học đã dc học
        /// </summary>
        /// <param name="comboId"></param>
        /// <param name="classId"></param>
        /// <param name="username"></param>
        public static void UpdateDone(int id, int comboId, int classId, string username, int oldClass)
        {
            using (var db = new ZimEntities())
            {
                var rs = db.tbl_ComboDetail.Where(x => x.ComboID == comboId && x.ClassID == classId).FirstOrDefault();
                if (rs != null)
                {
                    rs.IsDone = true;
                    rs.ModifiedBy = username;
                    rs.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    tbl_ComboDetail ck = new tbl_ComboDetail();
                    ck.ComboID = comboId;
                    ck.ClassID = classId;
                    ck.IsDone = true;
                    ck.CourseID = 0;
                    ck.CourseName = "";
                    ck.CoursePrice = 0;
                    ck.DiscountPrice = 0;
                    ck.IsHide = false;
                    ck.TypeID = 1;
                    ck.CreatedBy = ck.ModifiedBy = username;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_ComboDetail.Add(ck);

                    var old = db.tbl_ComboDetail.Where(x => x.ClassID == oldClass && x.ComboID == comboId && oldClass != 0).FirstOrDefault();
                    if (old != null)
                    {
                        old.IsHide = true;//xóa cái cũ nếu insert cái mới, đảm bảo k dư chương trình học đã đăng ký
                    }

                    db.SaveChanges();
                }

                var check = db.tbl_ComboDetail.Where(x => x.ComboID == comboId && x.IsDone == false).ToList();
                if (check.Count() > 0)
                {
                    var l = db.tbl_StudentAppointment.Where(n => n.ID == id).FirstOrDefault();
                    if (l != null)
                    {
                        l.isHide = true;
                        l.CourseStudentReserve = false;
                        l.DeadLineReserve = null;
                        l.ModifiedBy = username;
                        l.ModifiedDate = GetDateTime.Now;
                        db.SaveChanges();
                    }
                }
            }
        }

        
        public static void ReActive(int comboId, int classId, string username)
        {
            using (var db = new ZimEntities())
            {
                var rs = db.tbl_ComboDetail.Where(x => x.ComboID == comboId && x.ClassID == classId).FirstOrDefault();
                if (rs != null)
                {
                    rs.IsDone = false;
                    rs.ModifiedBy = username;
                    rs.ModifiedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }
    }
}