using AppZim.Models;
using MB.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class GroupTable
    {
        public static tbl_Group insert(string GroupName, string CoverIMG, int center, int course, string CreatedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Group G = new tbl_Group();
                G.GroupName = GroupName;
                G.ListRole = "0";
                G.CourseID = course;
                G.CenterID = center;
                G.CoverIMG = CoverIMG;
                G.CreatedBy = CreatedBy;
                G.CreatedDate = GetDateTime.Now;
                G.IsHide = false;
                db.tbl_Group.Add(G);
                db.SaveChanges();
                return G;
            }
        }
        public static tbl_Group update(int ID, string GroupName, string CoverIMG, int center, int course, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                tbl_Group G = db.tbl_Group.Where(n => n.ID == ID).FirstOrDefault();
                if (!string.IsNullOrEmpty(GroupName))
                    G.GroupName = GroupName;
                //if (!string.IsNullOrEmpty(ListRole))
                //    G.ListRole = ListRole;
                if (!string.IsNullOrEmpty(CoverIMG))
                    G.CoverIMG = CoverIMG;
                G.CenterID = center;
                G.CourseID = course;
                G.ModifiedBy = ModifiedBy;
                G.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return G;
            }
        }
        public static tbl_Group getbyid(int groupID)
        {
            using (var db = new ZimEntities())
            {
                tbl_Group G = db.tbl_Group.Where(n => n.ID == groupID).FirstOrDefault();
                return G;
            }
        }
        public static List<int> GetGroupMember(int groupID)
        {
            List<int> lid = new List<int>();
            List<tbl_Account> l = new List<tbl_Account>();
            using (var db = new ZimEntities())
            {
                tbl_Group G = db.tbl_Group.Where(n => n.ID == groupID && n.IsHide != true).FirstOrDefault();
                if (G == null)
                {
                    return lid;
                }
                var owner = db.tbl_Account.Where(n => n.UserName == G.CreatedBy).FirstOrDefault();
                l.Add(owner);
                //List<tbl_Account> listadmin = db.tbl_Account.Where(n => n.RoleID == 1 && n.Status == 2).ToList();
                //l.AddRange(listadmin);
                List<tbl_Account> listquanly = new List<tbl_Account>();
                if (G.CenterID != 0)
                {
                    listquanly = db.tbl_Account.Where(n => n.RoleID == 2 && n.Status == 2 && n.SchoolID == G.CenterID).ToList();
                }
                l.AddRange(listquanly);
                //List<tbl_Account> listhocvu = new List<tbl_Account>();
                //if (G.CenterID != 0)
                //{
                //    listhocvu = db.tbl_Account.Where(n => n.RoleID == 7 && n.Status == 2 && n.SchoolID == G.CenterID).ToList();
                //}
                //l.AddRange(listhocvu);

                if (G.CourseID != 0)
                {
                    List<tbl_Account> lhocvien = new List<tbl_Account>();
                    lhocvien = (from account in db.tbl_Account.Where(n => n.RoleID == 5 && n.Status == 2).ToList()
                                join course in db.tbl_CoursesStudent.Where(n => n.CourseID.Value == G.CourseID.Value).ToList() on account.ID equals course.StudentUID.Value
                                select account
                               ).ToList();
                    l.AddRange(lhocvien);
                    List<tbl_Account> lgiaovien = new List<tbl_Account>();
                    lgiaovien = (from account in db.tbl_Account.Where(n => n.RoleID == 4 && n.Status == 2).ToList()
                                 join course in db.tbl_TeacherCourse.Where(n => n.CourseID.Value == G.CourseID.Value).ToList() on account.ID equals course.TeacherUID.Value
                                 select account
                               ).ToList();
                    l.AddRange(lgiaovien);
                }
                IEnumerable<tbl_Account> listmember1 = MemeberTable.GetbyGroupIDandStatus(groupID, 1);
                IEnumerable<tbl_Account> listmember2 = MemeberTable.GetbyGroupIDandStatus(groupID, 2);
                l.AddRange(listmember1);
                foreach (var item in listmember2)
                {
                    l = l.Where(m => m.ID != item.ID).ToList();
                }
            }
            l = l.OrderBy(n => n.FullName).ToList();
            lid = l.Select(n => n.ID).Distinct().ToList();
            lid = lid.Where(n => n != 1).ToList();
            return lid;
        }
        public static List<tbl_Group> getAvailableGroup(int UID)
        {
            using (var db = new ZimEntities())
            {
                var ac = db.tbl_Account.Where(n => n.ID == UID).FirstOrDefault();
                if (ac == null)
                {
                    return null;
                }
                List<tbl_Group> G = new List<tbl_Group>();
                List<tbl_Group> L = db.tbl_Group.Where(n => n.IsHide != true).ToList();
                foreach (var item in L)
                {
                    if (item != null)
                    {
                        if (item.CreatedBy != ac.UserName)
                        {
                            var member = db.tbl_Member.Where(n => n.MemberID == ac.ID && n.GroupID == item.ID).FirstOrDefault();
                            if (member != null)
                            {
                                if (member.Status == 1)
                                {
                                    G.Add(item);
                                }
                            }
                            else
                            {
                                if (ac.RoleID == 1 || ac.RoleID == 2)
                                {
                                    if (item.CenterID != 0)
                                    {
                                        if (ac.RoleID == 1 || (ac.SchoolID.Value == item.CenterID.Value && (ac.RoleID == 2)))
                                        {
                                            G.Add(item);
                                        }
                                    }
                                    else
                                    {
                                        G.Add(item);
                                    }
                                }
                                else
                                {
                                    if (item.CourseID != 0)
                                    {
                                        //Kiểm tra có thuộc khóa này hay ko 
                                        if (ac.RoleID == 4)
                                        {
                                            List<tbl_TeacherCourse> lcourse = TeacherCourseTable.getbyteacherid(ac.ID).Where(n => n.CourseID == item.CourseID).ToList();
                                            if (lcourse.Count > 0)
                                            {
                                                G.Add(item);
                                            }
                                        }
                                        if (ac.RoleID == 5)
                                        {
                                            tbl_CoursesStudent lcourse = CoursesStudentTable.getbyCourseIDdandStudentID(item.CourseID.Value, ac.ID);
                                            if (lcourse != null)
                                            {
                                                G.Add(item);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            G.Add(item);
                        }

                    }
                }
                return G;
            }
        }
        public static tbl_Group remove(int id)
        {
            using (var db = new ZimEntities())
            {
                var g = db.tbl_Group.Where(n => n.ID == id).FirstOrDefault();
                if (g != null)
                {
                    g.IsHide = true;
                    db.SaveChanges();
                }
                return g;
            }

        }

        public static List<tbl_Courses> getAvaiableCourse(int aclogId)
        {
            List<tbl_Courses> l = CoursesTable.getall().Where(n => n.Status != 3).ToList();
            tbl_Account ac = AccountTable.getbyID(aclogId);
            if (ac.RoleID == 1)
            {
                return l;
            }
            else if (ac.RoleID == 2 || ac.RoleID == 7)
            {
                if (ac.SchoolID != null)
                {
                    return l.Where(n => n.SchoolID == ac.SchoolID.Value).ToList();
                }
                else
                {
                    return l;
                }
            }
            else
            {
                return new List<tbl_Courses>();
            }
        }
    }
}