using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class StaffSalaryTable
    {
        public static tbl_StaffSalary Insert(int PositionID, int Salary, int Persent, int Type, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_StaffSalary ck = new tbl_StaffSalary();
                ck.PositionID = PositionID;
                ck.Salary = Salary;
                ck.Persent = Persent;
                ck.TypeSalary = Type;
                if (Type == 1)
                {
                    ck.TypeNameSalary = "Online";
                }
                else if (Type == 2)
                {
                    ck.TypeNameSalary = "Offline";
                }
                ck.CreatedBy = CreatedBy;
                ck.CreatedDate = GetDateTime.Now;
                ck.IsHide = false;
                db.tbl_StaffSalary.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_StaffSalary Update(int ID, int Salary, int Persent, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_StaffSalary ck = db.tbl_StaffSalary.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Salary = Salary;
                    ck.Persent = Persent;
                    ck.ModifiedBy = ModifiedBy;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return ck;
            }
        }

        /// <summary>
        /// Xóa lương nhân viên
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IsDelete"></param>
        /// <param name="ModifiedBy"></param>
        /// <returns></returns>
        public static tbl_StaffSalary Delete(int ID, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_StaffSalary.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                ck.IsHide = true;
                ck.ModifiedBy = ModifiedBy;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_StaffSalary GetByUID(int UID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalary.Where(n => n.UID == UID).FirstOrDefault();
                return l;
            }
        }

        public static tbl_StaffSalary GetByPostionID(int PositionID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalary.Where(n => n.PositionID == PositionID).FirstOrDefault();
                return l;
            }
        }
        public static tbl_StaffSalary GetByUIDType(int UID, int TypeSalary)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalary.Where(n => n.UID == UID && n.TypeSalary == TypeSalary).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_StaffSalary> GetAll()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_StaffSalary.ToList();
                return l;
            }
        }
        public static List<Admin_SalarySetting_NotConfigSalary_Result> GetNotConfigSalary()
        {
            using (var db = new ZimEntities())
            {
                var l = db.Admin_SalarySetting_NotConfigSalary().ToList();
                return l;
            }
        }
    }
}