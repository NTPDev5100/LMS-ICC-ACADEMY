using AppZim.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.TableSql
{
    public class ClassTable
    {
        //thêm dữ liệu
        public static tbl_Class insert(string classname, int gradeID, string gradename, int type, int price,
            int giavon, string createby, int language, int coefficient)
        {
            using (var db = new ZimEntities())
            {
                tbl_Class ck = new tbl_Class();
                ck.ClassName = classname;
                ck.GradeID = gradeID;
                ck.GradeName = gradename;
                ck.PriceClass = price; //giá min
                ck.PriceClassInitial = giavon; //giá max đổi lại theo yêu cầu IEC
                ck.Coefficient = coefficient;
                ck.isHide = false;
                ck.TypeClass = type; // 1 offline, 2 online, 3 gói cam kết
                switch (coefficient)
                {
                    case 1:
                        ck.CoefficientName = "Tháng";
                        break;
                    case 2:
                        ck.CoefficientName = "50%";
                        break;
                    case 3:
                        ck.CoefficientName = "Hoàn tất học phí";
                        break;
                    default:
                        ck.CoefficientName = "Khác";
                        break;
                }
                switch (type)
                {
                    case 2:
                        ck.TypeName = "Online";
                        break;
                    case 3:
                        ck.TypeName = "Gói cam kết";
                        break;
                    default:
                        ck.TypeName = "Offline";
                        break;
                }
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.Language = language;
                db.tbl_Class.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Class update(int ID, string classname, int gradeID, string gradename, bool ishide, int type, int price, 
            int giavon, string createby, int language, int coefficient)
        {
            using (var db = new ZimEntities())
            {
                string change = "";
                tbl_Class ck = db.tbl_Class.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    if (ck.ClassName != classname)
                    {
                        change += "Tên: " + ck.ClassName + " => " + classname + "<br/>";
                        ck.ClassName = classname;
                    }
                    ck.GradeID = gradeID;
                    ck.GradeName = gradename;
                    if (price != ck.PriceClass)
                    {
                        change += "Giá tối thiểu: " + ck.PriceClass + " => " + price + "<br/>";
                        ck.PriceClass = price; //giá min
                    }

                    if (giavon != ck.PriceClassInitial)
                    {
                        change += "Giá tối đa: " + ck.PriceClassInitial + " => " + giavon + "<br/>";
                        ck.PriceClassInitial = giavon; //giá max 
                    }

                    if (ck.TypeClass != type)
                    {
                        if (type == 1)
                        {
                            change += "Hình thức: " + ck.TypeName + " => " + "Offline" + "<br/>";
                        }
                        if (type == 2)
                        {
                            change += "Hình thức: " + ck.TypeName + " => " + "Online" + "<br/>";
                        }
                    }
                    ck.TypeClass = type;
                    if (ck.Coefficient != coefficient)
                    {
                        if (coefficient == 1)
                        {
                            change += "Loại thanh toán: " + ck.CoefficientName + " => " + "Tháng" + "<br/>";
                        }
                        if (coefficient == 2)
                        {
                            change += "Loại thanh toán: " + ck.CoefficientName + " => " + "50%" + "<br/>";
                        }
                        if (coefficient == 3)
                        {
                            change += "Loại thanh toán: " + ck.CoefficientName + " => " + "Hoàn tất học phí" + "<br/>";
                        }
                        if (coefficient == 4)
                        {
                            change += "Loại thanh toán: " + ck.CoefficientName + " => " + "Khác" + "<br/>";
                        }
                    }
                    ck.Coefficient = coefficient;
                    switch (coefficient)
                    {
                        case 1:
                            ck.CoefficientName = "Tháng";
                            break;
                        case 2:
                            ck.CoefficientName = "50%";
                            break;
                        case 3:
                            ck.CoefficientName = "Hoàn tất học phí";
                            break;
                        default:
                            ck.CoefficientName = "Khác";
                            break;
                    }
                    switch (type)
                    {
                        case 2:
                            ck.TypeName = "Online";
                            break;
                        case 3:
                            ck.TypeName = "Gói cam kết";
                            break;
                        default:
                            ck.TypeName = "Offline";
                            break;
                    }
                    if (ck.isHide != ishide)
                    {
                        change += "Tạm ẩn: " + ck.isHide + " => " + ishide + "<br/>";
                        ck.isHide = ishide;
                    }

                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    ck.Language = language;
                    db.SaveChanges();
                    if (change != "")
                    {
                        ClassHistoryTable.insert(ID, change, createby);
                    }
                }
                return ck;
            }
        }

        public static tbl_Class getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        public static List<tbl_Class> getgradeid(int gradeid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(n => n.GradeID == gradeid && n.TypeClass != 3).ToList();
                return l;
            }
        }
        public static List<tbl_Class> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(x => x.TypeClass != 3).ToList();
                return l;
            }
        }
        public static List<tbl_Class> GetByType(int type)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(x => x.TypeClass == type).OrderByDescending(x => x.ID).ToList();
                return l;
            }
        }
        
        public static List<tbl_Class> GetByLanguage(int language)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(x => x.Language == language && x.isHide == false && x.TypeClass != 3).ToList();
                return l;
            }
        }

        public static List<tbl_Class> GetByGradeID(int grade)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Class.Where(x => x.GradeID == grade && x.isHide == false && x.TypeClass != 3).ToList();
                return l;
            }
        }
    }
}