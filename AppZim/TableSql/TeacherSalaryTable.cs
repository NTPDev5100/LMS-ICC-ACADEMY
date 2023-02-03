using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AppZim.TableSql
{
    public class TeacherSalaryTable
    {
        public static tbl_TeacherSalary insert(tbl_TeacherSalary teacherSalary,string user)
        {
            using (var db = new ZimEntities())
            {
                var data = new tbl_TeacherSalary
                {
                      TeacherID = teacherSalary.TeacherID,
                      Year = teacherSalary.Year,
                      Month = teacherSalary.Month,
                      Bonus = teacherSalary.Bonus == null ? 0 : teacherSalary.Bonus,
                      isDonePaid = false,
                      TotalSalary = teacherSalary.TotalSalary,
                      isHide = false,
                      CreatedDate = DateTime.Now,
                      CreatedBy = user,
                      ModifiedDate = DateTime.Now,
                      ModifiedBy = user
                };
                db.tbl_TeacherSalary.Add(data);
                db.SaveChanges();
                return data;
            }
        }
        public static tbl_TeacherSalary update(tbl_TeacherSalary teacherSalary,string user)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalary
                    .SingleOrDefault(x => x.ID == teacherSalary.ID);
                if (data == null) return null;
                data.Bonus = teacherSalary.Bonus == null ? data.Bonus : teacherSalary.Bonus;
                data.isDonePaid = teacherSalary.isDonePaid == null ? data.isDonePaid : teacherSalary.isDonePaid;
                data.TotalSalary += data.Bonus;
                data.ModifiedBy = user;
                data.ModifiedDate = DateTime.Now;
                db.SaveChanges();
                return data;
            }
        }
        public static tbl_TeacherSalary getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalary.SingleOrDefault(x => x.ID == id);
                return data;
            }
        }
        public static List<tbl_TeacherSalary> getall()
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_TeacherSalary
                    .Where(x => x.isHide == false)
                    .ToList();
                return data;
            }
        }

        public async static Task<List<Get_ListTeacherSalaryIsReject_Result>> Get_ListTeacherSalaryIsReject(int courseId, 
            int teacherId, string from, string to, int start, int length)
        {
            using (var db = new ZimEntities())
            {
                var data = db.Get_ListTeacherSalaryIsReject(courseId, teacherId, from, to, start, length).ToList();
                return await Task.FromResult(data);
            }
        }
    }
}