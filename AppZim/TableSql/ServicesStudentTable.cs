using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ServicesStudentTable
    {
        //thêm dữ liệu
        public static tbl_ServicesStudent insert(int uid, string studentname, tbl_Services sv, int price, string note, int payid,
            string payname, tbl_SupplierServices supp, string dateservice, string createby, int examid,int DiscountID)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServicesStudent ck = new tbl_ServicesStudent();
                ck.StudentUID = uid;
                ck.StudentName = studentname;
                ck.ServiceID = sv.ID;
                ck.ServiceName = sv.ServiceName;
                ck.PriceService = price;
                ck.Note = note;
                ck.PaymentmethodID = payid;
                ck.PaymentmethodName = payname;
                ck.SupplierServicesID = supp.ID;
                ck.SupplierServices = supp.SupplierName;
                if (!string.IsNullOrEmpty(dateservice))
                {
                    dateservice = dateservice.Replace(" ", "");
                    try
                    {
                        ck.DateService = DateTime.ParseExact(dateservice, "dd/MM/yyyy", null);
                    }
                    catch { }
                }
                ck.isHide = false;
                ck.DiscountID = DiscountID;
                ck.ServiceExamsID = examid;
                if (examid != 0)
                {
                    var x = db.tbl_ServiceExams.Where(n => n.ID == examid).FirstOrDefault();
                    if (x != null)
                    {
                        ck.ExamDate = x.ExamDate.Value.Date;
                        var supppp = SupplierServicesTable.getbyID(x.SupplierID.Value);
                        if (supppp != null)
                        {
                            ck.SupplierServicesID = supppp.ID;
                            ck.SupplierServices = supppp.SupplierName;
                        }
                    }

                }
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                db.tbl_ServicesStudent.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        public static tbl_ServicesStudent getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServicesStudent.Where(n => n.ID == id).FirstOrDefault();
                return l;
            }
        }
        //Duy Khoa
        public static List<tbl_ServicesStudent> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServicesStudent.Where(n => n.isHide != true).ToList();
                return l;
            }
        }
        public static List<tbl_ServicesStudent> getbyuid(int uid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServicesStudent.Where(n => n.StudentUID == uid && n.isHide != true).ToList();
                return l;
            }
        }

        public static List<tbl_ServicesStudent> getbyserviceexamsid(int ServiceExamsID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServicesStudent.Where(n => n.ServiceExamsID == ServiceExamsID && n.isHide != true).ToList();
                return l;
            }
        }

        public static tbl_ServicesStudent updatescore(int id, double lis, double spea, double read, double taskone,
            double tasktwo, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServicesStudent ck = db.tbl_ServicesStudent.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    //ck.ExamDate = date.Date;
                    //ck.ServiceExamsID = ServiceExamsID;
                    ck.Listening = Math.Round(lis, 2);
                    ck.Speaking = Math.Round(spea, 2);
                    ck.Reading = Math.Round(read, 2);

                    double wri = Math.Round((taskone + (tasktwo * 2)) / 3, 2);
                    double wridu = wri - (int)wri;

                    if (wridu < 0.25)
                        wridu = 0;
                    else
                    {
                        if (wridu >= 0.25 && wridu < 0.75)
                            wridu = 0.5;
                        else
                            wridu = 1;
                    }
                    wri = wri - (wri - (int)wri) + wridu;

                    ck.Writing = Math.Round(wri, 2);
                    ck.TaskOne = Math.Round(taskone, 2);
                    ck.TaskTwo = Math.Round(tasktwo, 2);

                    double over = Math.Round((lis + spea + read + wri) / 4, 2);
                    double overdu = over - (int)over;
                    if (overdu < 0.25)
                        overdu = 0;
                    else
                    {
                        if (overdu >= 0.25 && overdu < 0.75)
                            overdu = 0.5;
                        else
                            overdu = 1;
                    }
                    over = over - (over - (int)over) + overdu;

                    ck.OverAll = over;

                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public static tbl_ServicesStudent UpdateFileChungChi(int ID, string CertificateFile)
        {
            using (var db = new ZimEntities())
            {
                tbl_ServicesStudent ck = db.tbl_ServicesStudent.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.CertificateFile = CertificateFile; //Cập nhật file chứng chỉ
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
        public static tbl_ServicesStudent delete(int id, string note, string createby)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_ServicesStudent.Where(n => n.ID == id).FirstOrDefault();
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
    }
}