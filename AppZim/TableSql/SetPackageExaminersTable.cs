using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.Models;
using AppZim.TableSql;
using AppZim.ZIM;
using MB.Extensions;

namespace AppZim.TableSql
{
    public class SetPackageExaminersTable
    {
        public static tbl_SetPackageExaminers insert(int id, int teacherID, string teacherName,string teacherPhone,bool isSelect,string create)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageExaminers p = db.tbl_SetPackageExaminers.Where(n => n.ID == id).FirstOrDefault();
                if (p == null)
                {
                    p = new tbl_SetPackageExaminers();   
                    p.TeacherID = teacherID;
                    p.TeacherName = teacherName;
                    p.TeacherPhone = teacherPhone;
                    p.CreatedBy = p.ModifiedBy = create;
                    p.CreatedDate = p.ModifiedDate = GetDateTime.Now;
                    p.IsHide = false;
                    p.IsSelect = isSelect;
                    db.tbl_SetPackageExaminers.Add(p);
                    db.SaveChanges();
                }
                else
                {
                    p.IsSelect = isSelect;
                    p.ModifiedBy = create;
                    p.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                }
                return p;
            }
        }
        public static bool CheckActive(int teacherID)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageExaminers p = db.tbl_SetPackageExaminers.Where(n => n.TeacherID==teacherID ).FirstOrDefault();
                if (p == null)
                {
                    return false;
                }
                else
                {
                    return p.IsSelect.Value;
                }
            }
        }
        /// <summary>
        /// lấy giáo viên chấm bài của set này
        /// </summary>
        /// <param name="teacherID"></param>
        /// <param name="setdetailID"></param>
        /// <returns></returns>
        public static tbl_SetPackageExaminers getbyexaminer(int teacherID)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageExaminers p = db.tbl_SetPackageExaminers.Where(n =>  n.TeacherID == teacherID).FirstOrDefault();
                return p;
            }
        }
        public static List<tbl_SetPackageExaminers> getlistexaminer()
        {
            using (var db = new ZimEntities())
            {
                List<tbl_SetPackageExaminers> l = db.tbl_SetPackageExaminers.Where(n => n.IsSelect.Value == true).ToList();
                return l;
            }
        }
    }
}