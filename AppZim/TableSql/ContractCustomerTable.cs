using AppZim.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System;

namespace AppZim.TableSql
{
    public class ContractCustomerTable
    {
        public static tbl_ContractCustomer insert(int uid, int studentAppointmentID, string content, string note, string code, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractCustomer ck = db.tbl_ContractCustomer.Where(n => n.StudentID == uid && n.StudentAppointmentID == studentAppointmentID).FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_ContractCustomer();
                    ck.StudentID = uid;
                    ck.StudentAppointmentID = studentAppointmentID;
                    ck.ContractContent = content;
                    ck.Status = 1;//1 chưa duyệt, 2 đã duyệt
                    ck.CodeCheck = code;
                    ck.isHide = false;
                    ck.Signed = false;//student ký hay chưa
                    ck.Note = note;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_ContractCustomer.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    ck.StudentID = uid;
                    ck.StudentAppointmentID = studentAppointmentID;
                    ck.ContractContent = content;
                    ck.Status = 1;//1 chưa duyệt, 2 đã duyệt
                    ck.isHide = false;
                    ck.Note = note;
                    ck.CodeCheck = code;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static tbl_ContractCustomer getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractCustomer ck = db.tbl_ContractCustomer.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }
        public static tbl_ContractCustomer getbystudentAppointmentID(int studentAppointmentID)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractCustomer ck = db.tbl_ContractCustomer.Where(n => n.StudentAppointmentID == studentAppointmentID).FirstOrDefault();
                return ck;
            }
        }

        public async static Task<List<tbl_ContractCustomer>> GetByStudentId(int studentId)
        {
            using (var db = new ZimEntities())
            {
                var ck = await db.tbl_ContractCustomer.Where(n => n.StudentID == studentId && n.isHide == false && n.Status == 2).ToListAsync();
                return ck;
            }
        }

        public static tbl_ContractCustomer updateaccept(int id, int status, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractCustomer ck = db.tbl_ContractCustomer.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;//1 admin chưa duyệt, 2 admin đã duyệt
                    ck.isHide = false;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        public async static Task updateContent(int id, string content, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractCustomer ck = await db.tbl_ContractCustomer.Where(n => n.ID == id).FirstOrDefaultAsync();
                if (ck != null)
                {
                    if (ck.Signed != true)
                    {
                        ck.ContractContent = content;//1 admin chưa duyệt, 2 admin đã duyệt
                        ck.Signed = true;
                        ck.ModifiedBy = createby;
                        ck.ModifiedDate = DateTime.Now;
                        await db.SaveChangesAsync();
                    }
                }
            }
        }
    }
}