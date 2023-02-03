using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AppZim.TableSql;
using AppZim.Models;
using AppZim.ZIM;
using MB.Extensions;

namespace AppZim.TableSql
{
    public class SetPackageStudentExerciseTable
    {
        /// <summary>
        /// Thêm câu hỏi (khi random)
        /// </summary>
        /// <param name="setdetailID"></param>
        /// <param name="studentid"></param>
        /// <param name="exerciseid"></param>
        /// <param name="exercisetitle"></param>
        /// <param name="partnumber"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_SetPackageStudentExercise insert(int setpackageID, int studentid, int exerciseid, string exercisetitle, int partnumber, string createby)
        {
            using (var db = new ZimEntities())
            {
                tbl_SetPackageStudentExercise se = new tbl_SetPackageStudentExercise();
                se.SetPackageID = setpackageID;
                se.StudentID = studentid;
                se.ExerciseID = exerciseid;
                se.ExerciseTitle = exercisetitle;
                se.PartNumber = partnumber;
                se.CreatedDate = se.ModifiedDate = GetDateTime.Now;
                se.CreatedBy = se.ModifiedBy = createby;
                db.tbl_SetPackageStudentExercise.Add(se);
                db.SaveChanges();
                return se;
            }
        }
        /// <summary>
        /// lấy danh sách câu hỏi sau khi đã random câu hỏi cho student
        /// </summary>
        /// <param name="setdetailid"></param>
        /// <param name="studentid"></param>
        /// <returns></returns>
        /// p
        public static List<tbl_SetPackageStudentExercise> getall(int setpackageID,int studentid)
        {
            using (var db = new ZimEntities())
            {
                List<tbl_SetPackageStudentExercise> lse = db.tbl_SetPackageStudentExercise.Where(n => n.StudentID == studentid && n.SetPackageID == setpackageID).ToList();
                return lse;
            }

        }
        public static tbl_SetPackageStudentExercise getbyid(int id)
        {
            using (var db = new ZimEntities())
            {
                var lse = db.tbl_SetPackageStudentExercise.Where(n => n.ID == id).FirstOrDefault();
                return lse;
            }
        }       
    }
}