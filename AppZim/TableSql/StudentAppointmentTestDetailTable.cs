using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StudentAppointmentTestDetailTable
    {
        //thêm dữ liệu
        public static tbl_StudentAppointmentTestDetail insert(int StudentAppointmentTestID, int advisoryid, int teacherid, string listening, string speaking, string reading,
            string writing, string vocab, int price, string note, string createby, int reasonType, int reasonID)
        {
            using (var db = new ZimEntities())
            {
                var rs = db.tbl_StudentAppointmentTestDetail.Where(x => x.StudentAppointmentTestID == StudentAppointmentTestID).FirstOrDefault();
                if (rs != null)
                {
                    rs.Listening = listening;
                    rs.Speaking = speaking;
                    rs.Reading = reading;
                    rs.Writing = writing;
                    rs.Note = note;
                    db.SaveChanges();
                    return rs;
                }
                else
                {
                    tbl_StudentAppointmentTestDetail ck = new tbl_StudentAppointmentTestDetail();
                    ck.StudentAppointmentTestID = StudentAppointmentTestID;
                    ck.AdvisoryUID = advisoryid;
                    ck.TeacherTestID = teacherid;
                    ck.Listening = listening;
                    ck.Speaking = speaking;
                    ck.Reading = reading;
                    ck.Writing = writing;
                    ck.Vocab = vocab;
                    ck.ConsultingPrice = price;
                    ck.Note = note;
                    ck.isHide = false;
                    ck.IsSendMail = false;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    ck.ReasonType = reasonType;
                    ck.ReasonID = reasonID;
                    db.tbl_StudentAppointmentTestDetail.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static tbl_StudentAppointmentTestDetail getbyAppointmentTestID(int StudentAppointmentTestID)
        {
            using (var db = new ZimEntities())
            {
                tbl_StudentAppointmentTestDetail ck = db.tbl_StudentAppointmentTestDetail.Where(n => n.StudentAppointmentTestID == StudentAppointmentTestID).FirstOrDefault();
                return ck;
            }
        }
        public static List<tbl_StudentAppointmentTestDetail> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTestDetail.Where(x => x.isHide != true).ToList();
                return l;
            }
        }
        public static List<tbl_StudentAppointmentTestDetail> GetByReasonID(int reasonID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StudentAppointmentTestDetail.Where(x => x.isHide != true && x.ReasonID == reasonID).ToList();
                return l;
            }
        }
    }
}