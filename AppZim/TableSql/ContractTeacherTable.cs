using AppZim.Models;
using System.Linq;

namespace AppZim.TableSql
{
    public class ContractTeacherTable
    {

        public static tbl_ContractTeacher insert(int teacherId, string content, int status, bool isHide, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractTeacher ck = db.tbl_ContractTeacher.Where(n => n.TeacherID == teacherId).FirstOrDefault();
                if (ck == null)
                {
                    ck = new tbl_ContractTeacher();
                    ck.TeacherID = teacherId;
                    ck.ContractContent = content;
                    ck.Status = status;//1 chưa duyệt, 2 đã duyệt
                    ck.isHide = isHide;
                    ck.CreatedBy = ck.ModifiedBy = createby;
                    ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                    db.tbl_ContractTeacher.Add(ck);
                    db.SaveChanges();
                    return ck;
                }
                else
                {
                    ck.TeacherID =  teacherId;
                    ck.ContractContent = content;
                    ck.Status = status;//1 chưa duyệt, 2 đã duyệt
                    ck.isHide = isHide;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
            }
        }

        public static tbl_ContractTeacher getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                tbl_ContractTeacher ck = db.tbl_ContractTeacher.Where(n => n.ID == id).FirstOrDefault();
                return ck;
            }
        }
    }
}