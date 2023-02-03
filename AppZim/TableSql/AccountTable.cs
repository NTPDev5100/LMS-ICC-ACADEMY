using AppZim.Models;
using AppZim.ZIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;

namespace AppZim.TableSql
{
    public class AccountTable
    {
        //thêm dữ liệu
        public static tbl_Account insertstaff(tbl_District district, int schoolid, string schoolname, string fullname, string email, string phone, string address,
            int roleID, int status, string avata, string birthday, string createby, int language, string certificate
            , string cv, string zoomid, string zoompassword, string bank, string linkfb, string datestartwork,
            int position, double revenue)
        {
            using (var db = new ZimEntities())
            {
                var ckus = db.tbl_Account.Where(n => n.UserName == email).FirstOrDefault();
                if (ckus != null)
                    return null;
                if (district == null)
                {
                    district = new tbl_District();
                    district.LocationID = 0;
                    district.LocationName = "";
                    district.ID = 0;
                    district.DistrictName = "";
                }
                tbl_Account ck = new tbl_Account();
                var v = viewrole(roleID, status, 3);
                ck.LocationID = district.LocationID;
                ck.LocationName = district.LocationName;
                ck.DistrictID = district.ID;
                ck.DistrictName = district.DistrictName;
                ck.SchoolID = schoolid;
                ck.SchoolName = schoolname;
                ck.FullName = fullname;
                ck.UserName = email;
                ck.Password = Encryptor.Encrypt("iccacademy");
                ck.Phone = phone;
                ck.Email = email;
                ck.Address = address;
                ck.RoleID = roleID;
                var rolename = db.tbl_Role.Where(x => x.RoleID == roleID).FirstOrDefault().RoleName;
                ck.RoleName = rolename;
                ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = v.status;
                ck.Avatar = ck.AvatarThumbnail = avata;
                ck.GenderID = 3;
                ck.Gender = v.gender;
                ck.SalaryLesson = 0;
                ck.Birthday = GetDateTime.Now.Date;
                ck.LinkCertificate = certificate;//chứng chỉ
                ck.LinkCV = cv;
                ck.LinkFB = linkfb;
                ck.ZoomID = zoomid;
                ck.NationID = 0;
                ck.ZoomPassword = zoompassword;
                ck.BankInformation = bank;
                ck.Revenue = revenue;
                try
                {
                    if (!string.IsNullOrEmpty(birthday))
                        ck.Birthday = DateTime.ParseExact(birthday.Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch { }

                try
                {
                    ck.DateStartWork = DateTime.ParseExact(datestartwork.Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch { ck.DateStartWork = DateTime.Now; }
                ck.Position = position;
                ck.IsDelete = false;
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.Language = language;
                db.tbl_Account.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        //thêm dữ liệu khách
        public static tbl_Account insertcustomer(tbl_District district, string fullname, string email, string phone, string address, int status, string avata,
            int supportID, int sourceid, string sourcename, string birthday, int jobID, string jobname, string workplace, int academicpurid, string academicpur, string scorein,
            string scoreout, string dateexam, string typeedu, string identitycard, int identitycardcityid, string identitycardcity, string identitycarddate,
            string notehome, string createby, bool advisory, tbl_Ward ward, tbl_Street street, string homeumber, int language, string linkfb, int nation)
        {
            using (var db = new ZimEntities())
            {
                phone = phone.Trim();
                tbl_Account ckus = new tbl_Account();
                ckus = db.tbl_Account.Where(n => n.UserName == email).FirstOrDefault();
                if (ckus != null)
                {
                    if (string.IsNullOrEmpty(ckus.IdentityCard) && !string.IsNullOrEmpty(identitycard))
                    {
                        ckus.IdentityCard = identitycard;
                        ckus.ModifiedBy = createby;
                        ckus.ModifiedDate = GetDateTime.Now;
                        db.SaveChanges();
                    }
                    if (advisory)
                    {
                        ckus.AdvisoryCustomer = advisory;//nhện tư vấn
                        ckus.ModifiedBy = createby;
                        ckus.ModifiedDate = GetDateTime.Now;
                        db.SaveChanges();
                    }
                    return ckus;
                }

                tbl_Account ck = new tbl_Account();
                var v = viewrole(5, status, 3);
                if (district != null)
                {
                    ck.LocationID = district.LocationID;// tỉnh thành phố
                    ck.LocationName = district.LocationName;// tỉnh thành phố                 
                }
                if (district != null)
                {
                    ck.DistrictID = district.ID;// ID quận
                    ck.DistrictName = district.DistrictName;// quận 
                }
                if (ward != null)
                {
                    ck.WardID = ward.ID;//phường
                    ck.WardName = ward.WardName;
                }
                if (street != null)
                {
                    ck.StreetID = street.ID;//id đường
                    ck.StreetName = street.StreetName;

                }
                ck.HomeNumber = homeumber;
                ck.FullName = fullname;// họ và tên
                if (!string.IsNullOrEmpty(phone))//không có sdt bỏ qua email
                {
                    ck.UserName = email.Trim().Replace(" ", "");//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = phone;// số điện thoại
                }
                else
                {
                    ck.UserName = email;//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = "0";// số điện thoại
                }
                ck.Email = email;// email
                ck.Address = address;// địa chỉ
                ck.RoleID = 5;//1 admin, 2 manager, 3 cashier lễ tân, 4 giáo viên, 5 học viên, 6 saler, 7 học vụ, 8 quản lý chuyên môn
                ck.RoleName = v.role;// tên chứ vụ
                ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = v.status;// trên trạng thái
                ck.Avatar = ck.AvatarThumbnail = avata;// hình đại diện
                ck.GenderID = 3;// giới tính
                ck.Gender = v.gender;// tên giới tính                
                ck.Birthday = GetDateTime.Now.Date;// sinh nhât
                try
                {
                    if (!string.IsNullOrEmpty(birthday))
                        ck.Birthday = DateTime.ParseExact(birthday.Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch { }
                ck.SupportUID = supportID;// tư vấn viên
                ck.SupportName = "";// tên tư vấn viên
                var sp = getbyID(supportID);
                if (sp != null)
                    ck.SupportName = sp.FullName;// tên tư vấn viên
                ck.SourceID = sourceid;// nguồn học viên == 0 nguồn tư vấn viên
                ck.SourceName = sourcename;// tên nguồn học viên
                ck.CreatedBy = ck.ModifiedBy = createby;
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.JobID = jobID;//công việc
                ck.Job = jobname;// tên công viện
                ck.WorkPlace = workplace;// nơi làm việc
                ck.AcademicPurposesID = academicpurid;// mục đích học tập
                ck.AcademicPurposes = academicpur;// mục đích học tập
                ck.ScoreIn = new string(scorein.Where(c => char.IsDigit(c)).ToArray());// điểm đầu vào
                ck.ScoreOut = new string(scoreout.Where(c => char.IsDigit(c)).ToArray()); ;// điểm đầu ra
                if (!string.IsNullOrEmpty(dateexam))
                    ck.DateExam = DateTime.ParseExact(dateexam.Replace(" ", ""), "dd/MM/yyyy", null);// ngày thi
                ck.TypeOfEducation = typeedu;// loại hình đào tạo, Academic hay General

                if (!string.IsNullOrEmpty(identitycard))
                {
                    ck.IdentityCard = identitycard;// chứng mình nhân dân
                    ck.IdentityCardCityID = identitycardcityid;// nơi cấp CMND
                    ck.IdentityCardCityName = identitycardcity;// tên nơi cấp CMND
                    try
                    {
                        if (!string.IsNullOrEmpty(identitycarddate))
                            ck.IdentityCardDate = DateTime.ParseExact(identitycarddate.Replace(" ", ""), "dd/MM/yyyy", null);// ngày cấp
                    }
                    catch { }
                }
                ck.IsDelete = false;
                ck.LearningStatus = 1; //1.chưa đăng ký 2.đã đăng ký 3.đã xong
                ck.NoteHome = notehome;//người nhà
                ck.AdvisoryCustomer = advisory;//khách cần tư vấn
                ck.LinkFB = linkfb;
                ck.Language = language;
                ck.NationID = nation;
                db.tbl_Account.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }

        public static tbl_Account InsertCus(string fullname, string email, string phone)
        {
            using (var db = new ZimEntities())
            {
                phone = phone.Trim();
                tbl_Account ckus = new tbl_Account();
                ckus = db.tbl_Account.Where(n => n.UserName == email).FirstOrDefault();
                if (ckus != null)
                {
                    return ckus;
                }

                tbl_Account ck = new tbl_Account();
                var v = viewrole(5, 2, 3);
                ck.FullName = fullname;// họ và tên
                if (!string.IsNullOrEmpty(phone))//không có sdt bỏ qua email
                {
                    ck.UserName = phone.Trim().Replace(" ", "");//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = phone;// số điện thoại
                }
                else
                {
                    ck.UserName = email;//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = "0";// số điện thoại
                }
                ck.Email = email;// email
                ck.RoleID = 5;//1 admin, 2 manager, 3 cashier lễ tân, 4 giáo viên, 5 học viên, 6 saler, 7 học vụ, 8 quản lý chuyên môn
                ck.RoleName = v.role;// tên chứ vụ
                ck.Status = 2;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = v.status;// trên trạng thái
                ck.SourceID = 1;
                ck.SourceName = "Datawweb";
                ck.Avatar = ck.AvatarThumbnail = "";// hình đại diện
                ck.GenderID = 3;// giới tính
                ck.Gender = v.gender;// tên giới tính     
                ck.CreatedDate = ck.ModifiedDate = GetDateTime.Now;
                ck.IsDelete = false;
                ck.NationID = 0;
                ck.LearningStatus = 1; //1.chưa đăng ký 2.đã đăng ký 3.đã xong
                db.tbl_Account.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        //update dữ liệu khách
        public static tbl_Account updatecustomer(int ID, tbl_District district, string fullname, string email, string phone, string address, int status, string pass,
            int supportID, int sourceid, string sourcename, string birthday, int jobID, string jobname, string workplace, int academicpurid, string academicpur, string scorein,
            string scoreout, string dateexam, string typeedu, string identitycard, int identitycardcityid, string identitycardcity, string identitycarddate,
            string notehome, string createby, tbl_Ward ward, tbl_Street street, string homeumber, int language, string linkfb, int nation)
        {
            using (var db = new ZimEntities())
            {
                tbl_Account ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    string ch = "";
                    var v = viewrole(5, status, 3);
                    ck.LocationID = district.LocationID;// tỉnh thành phố

                    if (ck.LocationName != district.LocationName)
                        ch += "Tỉnh, thành phố: " + ck.LocationName + " => " + district.LocationName + "<br/>";
                    ck.LocationName = district.LocationName;// tỉnh thành phố

                    ck.DistrictID = district.ID;// ID quận
                    if (ck.DistrictName != district.DistrictName)
                        ch += "Quận, huyện: " + ck.DistrictName + " => " + district.DistrictName + "<br/>";
                    ck.DistrictName = district.DistrictName;// quận 

                    ck.WardID = ward.ID;// ID phường
                    if (ck.WardName != ward.WardName)
                        ch += "Phường xã: " + ck.WardName + " => " + ward.WardName + "<br/>";
                    ck.WardName = ward.WardName;// phường 

                    ck.StreetID = street.ID;// ID phường
                    if (ck.StreetName != street.StreetName)
                        ch += "Đường: " + ck.WardName + " => " + street.StreetName + "<br/>";
                    ck.StreetName = street.StreetName;// phường 

                    if (ck.HomeNumber != homeumber)
                        ch += "Số nhà: " + ck.HomeNumber + " => " + homeumber + "<br/>";
                    ck.HomeNumber = homeumber;// số nhà 

                    if (ck.FullName != fullname)
                        ch += "Họ tên: " + ck.FullName + " => " + fullname + "<br/>";
                    ck.FullName = fullname;// họ và tên

                    if (ck.Phone != phone)
                        ch += "Số điện thoại: " + ck.Phone + " => " + phone + "<br/>";
                    ck.Phone = phone;// số điện thoại

                    if (ck.Email != email)
                        ch += "Email: " + ck.Email + " => " + email + "<br/>";
                    ck.Email = email;// email


                    if (!string.IsNullOrEmpty(pass))
                    {
                        if (ck.Password != Encryptor.Encrypt(pass))
                            ch += "Password: Thay đổi mật khẩu<br/>";
                        ck.Password = Encryptor.Encrypt(pass);// mật khẩu
                    }

                    if (ck.Address != address)
                        ch += "Địa chỉ: " + ck.Address + " => " + address + "<br/>";
                    ck.Address = address;// địa chỉ

                    ck.RoleID = 5;//1 admin, 2 manager, 3 cashier lễ tân, 4 giáo viên, 5 học viên, 6 saler, 7 học vụ, 8 quản lý chuyên môn

                    if (ck.RoleName != v.role)
                        ch += "Role: " + ck.RoleName + " => " + v.role + "<br/>";
                    ck.RoleName = v.role;// tên chứ vụ

                    ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa

                    if (ck.StatusName != v.status)
                        ch += "Trạng thái: " + ck.StatusName + " => " + v.status + "<br/>";
                    ck.StatusName = v.status;// trên trạng thái

                    ck.GenderID = 3;// giới tính
                    ck.Gender = v.gender;// tên giới tính                    
                    try
                    {
                        if (!string.IsNullOrEmpty(birthday.Trim()))
                        {
                            DateTime dbd = DateTime.ParseExact(birthday.Replace(" ", ""), "dd/MM/yyyy", null);
                            if (ck.Birthday != null)
                            {
                                if (ck.Birthday.Value.ToString("dd/MM/yyyy") != dbd.ToString("dd/MM/yyyy"))
                                    ch += "Ngày sinh: " + ck.Birthday.Value.ToString("dd/MM/yyyy") + " => " + dbd.ToString("dd/MM/yyyy") + "<br/>";
                            }
                            ck.Birthday = dbd.Date;
                        }
                    }
                    catch
                    {
                        //ck.Birthday = GetDateTime.Now.Date;// sinh nhât
                    }

                    var aclog = getbyusername(createby);
                    var sp = getbyID(supportID);
                    //if (sp != null && (aclog.RoleID == 1 || aclog.RoleID == 2))
                    //{
                    //    ck.SupportUID = supportID;// tư vấn viên

                    //    if (ck.SupportUID != supportID)
                    //        ch += "TVV: " + ck.SupportName + " => " + sp.FullName + "<br/>";
                    //    ck.SupportName = sp.FullName;// tên tư vấn viên
                    //}
                    if (sp != null)
                    {
                        if (ck.SupportUID != supportID)
                            ch += "TVV: " + ck.SupportName + " => " + sp.FullName + "<br/>";
                        ck.SupportUID = supportID;// tư vấn viên
                        ck.SupportName = sp.FullName;// tên tư vấn viên
                    }
                    ck.SourceID = sourceid;// nguồn học viên == 0 nguồn tư vấn viên

                    if (ck.SourceName != sourcename)
                        ch += "Nguồn học viên: " + ck.SourceName + " => " + sourcename + "<br/>";
                    ck.SourceName = sourcename;// tên nguồn học viên

                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    ck.JobID = jobID;//công việc

                    if (ck.Job != jobname)
                        ch += "Công việc: " + ck.Job + " => " + jobname + "<br/>";
                    ck.Job = jobname;// tên công viện

                    if (ck.WorkPlace != workplace)
                        ch += "Nơi làm việc: " + ck.WorkPlace + " => " + workplace + "<br/>";
                    ck.WorkPlace = workplace;// nơi làm việc

                    ck.AcademicPurposesID = academicpurid;
                    if (ck.AcademicPurposes != academicpur)
                        ch += "Mục đích học tập: " + ck.AcademicPurposes + " => " + academicpur + "<br/>";
                    ck.AcademicPurposes = academicpur;// mục đích học tập

                    if (ck.ScoreIn != scorein && scorein != null)
                        ch += "Điểm đầu vào: " + ck.ScoreIn + " => " + scorein + "<br/>";
                    ck.ScoreIn = new string(scorein.Where(c => char.IsDigit(c)).ToArray()); ;// điểm đầu vào

                    if (ck.ScoreOut != scoreout && scoreout != null)
                        ch += "Điểm đầu ra: " + ck.ScoreOut + " => " + scoreout + "<br/>";
                    ck.ScoreOut = new string(scoreout.Where(c => char.IsDigit(c)).ToArray()); ;// điểm đầu ra

                    if (!string.IsNullOrEmpty(dateexam.Trim()))
                    {
                        DateTime exa = DateTime.ParseExact(dateexam.Replace(" ", ""), "dd/MM/yyyy", null);// ngày thi
                        if (ck.DateExam != null)
                        {
                            if (ck.DateExam.Value.ToString("dd/MM/yyyy") != exa.ToString("dd/MM/yyyy"))
                                ch += "Ngày thi: " + ck.DateExam.Value.ToString("dd/MM/yyyy") + " => " + exa.ToString("dd/MM/yyyy") + "<br/>";
                        }
                        ck.DateExam = exa.Date;
                    }

                    if (ck.TypeOfEducation != typeedu)
                        ch += "Loại đào tạo: " + ck.TypeOfEducation + " => " + typeedu + "<br/>";
                    ck.TypeOfEducation = typeedu;// loại hình đào tạo, Academic hay General

                    if (!string.IsNullOrEmpty(identitycard))
                    {
                        if (ck.IdentityCard != identitycard)
                            ch += "CMND: " + ck.IdentityCard + " => " + identitycard + "<br/>";
                        ck.IdentityCard = identitycard;// chứng mình nhân dân

                        ck.IdentityCardCityID = identitycardcityid;// nơi cấp CMND

                        if (ck.IdentityCardCityID != identitycardcityid)
                            ch += "Nơi cấp CMND: " + ck.IdentityCardCityName + " => " + identitycardcity + "<br/>";
                        ck.IdentityCardCityName = identitycardcity;// tên nơi cấp CMND
                        try
                        {
                            if (!string.IsNullOrEmpty(identitycarddate))
                            {
                                DateTime carddate = DateTime.ParseExact(identitycarddate.Replace(" ", ""), "dd/MM/yyyy", null);// ngày cấp
                                if (ck.IdentityCardDate != null)
                                {
                                    if (ck.IdentityCardDate.Value.ToString("dd/MM/yyyy") != carddate.ToString("dd/MM/yyyy"))
                                        ch += "Ngày cấp CMND: " + ck.IdentityCardDate.Value.ToString("dd/MM/yyyy") + " => " + carddate.ToString("dd/MM/yyyy") + "<br/>";
                                }
                                ck.IdentityCardDate = carddate.Date;
                            }
                        }
                        catch { }
                    }
                    if (ck.NoteHome != notehome)
                        ch += "Người nhà: " + ck.NoteHome + " => " + notehome + "<br/>";
                    ck.NoteHome = notehome;//người nhà
                    ck.Language = language;
                    ck.NationID = nation;
                    ck.LinkFB = linkfb;
                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(ch))
                        AccountHistoryChangeTable.insert(ck.ID, ck.FullName, ch, createby);
                }
                return ck;
            }
        }


        //cập nhật
        public static tbl_Account updatestaff(int ID, int locationID, string locationname, int schoolID, string schoolName, string fullname, string password, string email, string phone, string address,
            int roleID, int status, DateTime Birthday, int gender, string createby, int districtID,
            string districtName, int language, string certificate, string cv, string zoomid, string zoompassword,
            string bank, string linkfb, string datestartwork, int position, double revenue)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                var v = viewrole(roleID, status, gender);
                if (locationID != 0)
                {
                    ck.LocationID = locationID;
                    ck.LocationName = locationname;
                }
                if (districtID != 0)
                {
                    ck.DistrictID = districtID;
                    ck.DistrictName = districtName;
                }
                if (schoolID != 0)
                {
                    ck.SchoolID = schoolID;
                    ck.SchoolName = schoolName;
                }
                ck.FullName = fullname;

                if (!string.IsNullOrEmpty(password))
                    ck.Password = Encryptor.Encrypt(password);
                ck.Phone = phone;
                ck.Email = email;
                ck.Address = address;
                ck.RoleID = roleID;
                var rolename = db.tbl_Role.Where(x => x.RoleID == roleID).FirstOrDefault()?.RoleName;
                ck.RoleName = rolename;
                ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = v.status;
                if (gender == 1 || gender == 2 || gender == 3)
                    ck.GenderID = gender;
                else
                    ck.GenderID = 3;
                ck.Gender = v.gender;
                ck.SalaryLesson = 0;
                ck.Birthday = Birthday;
                ck.ZoomID = zoomid;
                ck.ZoomPassword = zoompassword;
                ck.BankInformation = bank;
                ck.LinkFB = linkfb;
                ck.Position = position;
                ck.Revenue = revenue;
                try
                {
                    ck.DateStartWork = DateTime.ParseExact(datestartwork.Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch { ck.DateStartWork = DateTime.Now; }
                if (!string.IsNullOrEmpty(certificate))
                    ck.LinkCertificate = certificate;//chứng chỉ
                if (!string.IsNullOrEmpty(cv))
                    ck.LinkCV = cv;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                if (ck.RoleID != 4 && ck.RoleID != 5 && ck.Status == 3)
                {
                    ck.UserName = "deactivated@gmail.com";
                    ck.Email = "deactivated@gmail.com";
                    ck.Phone = "";
                }
                ck.Language = language;
                db.SaveChanges();
                return ck;
            }
        }

        //public static tbl_Account updatecustomer(int ID, int locationID, string locationname, string fullname, string password, string email, string phone, string address,
        //    int status, string avata, DateTime Birthday, int sshoolid, string schoolname, string createby, string avatathumbnail)
        //{
        //    using (var db = new ZimEntities())
        //    {
        //        var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
        //        if (ck == null)
        //            return ck;
        //        var v = viewrole(ck.RoleID.Value, status, ck.GenderID.Value);
        //        ck.LocationID = locationID;
        //        ck.LocationName = locationname;
        //        ck.FullName = fullname;
        //        ck.SchoolID = sshoolid;
        //        ck.SchoolName = schoolname;
        //        if (!string.IsNullOrEmpty(password))
        //            ck.Password = Encryptor.Encrypt(password);
        //        ck.Phone = phone;
        //        ck.Email = email;
        //        ck.Address = address;
        //        ck.RoleName = v.role;
        //        ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
        //        ck.StatusName = v.status;
        //        if (!string.IsNullOrEmpty(avata))
        //        {
        //            ck.Avatar = avata;
        //            ck.AvatarThumbnail = avatathumbnail;
        //        }
        //        ck.GenderID = 3;
        //        ck.Gender = v.gender;
        //        ck.SalaryLesson = 0;
        //        ck.Birthday = Birthday;
        //        ck.ModifiedBy = createby;
        //        ck.ModifiedDate = GetDateTime.Now;
        //        db.SaveChanges();
        //        return ck;
        //    }
        //}

        //cập nhật
        public static tbl_Account updateprofile(int ID, string fullname, int gender, string password, string email, string phone, string address,
            DateTime Birthday, string avatar, string avatarthumbnail, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                var v = viewrole(ck.RoleID.Value, ck.Status.Value, gender);
                ck.FullName = fullname;
                if (!string.IsNullOrEmpty(password))
                    ck.Password = Encryptor.Encrypt(password);
                ck.Phone = phone;
                ck.Email = email;
                ck.Address = address;
                ck.RoleName = v.role;
                ck.StatusName = v.status;
                if (gender == 1 || gender == 2 || gender == 3)
                    ck.GenderID = gender;
                else
                    ck.GenderID = 3;
                ck.Gender = v.gender;
                if (!string.IsNullOrEmpty(avatar))
                {
                    ck.Avatar = avatar;
                    ck.AvatarThumbnail = avatarthumbnail;
                }
                ck.SalaryLesson = 0;
                ck.Birthday = Birthday;
                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }

        public static void UpdatePosition(int id, int position)
        {
            using (var db = new ZimEntities())
            {
                var user = db.tbl_Account.Where(x => x.IsDelete == false && x.Status == 2 && x.ID == id).FirstOrDefault();
                if (user != null)
                {
                    user.Position = position;
                    db.SaveChanges();
                }    
            }
        }
        /// <summary>
        /// Xóa giáo viên
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="IsDelete"></param>
        /// <param name="ModifiedBy"></param>
        /// <returns></returns>
        public static tbl_Account DeleteAccount(int ID, bool IsDelete, string ModifiedBy)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                ck.IsDelete = IsDelete;
                ck.ModifiedBy = ModifiedBy;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }

        public class AccName
        {
            public string role { get; set; }
            public string status { get; set; }
            public string gender { get; set; }
        }

        public static AccName viewrole(int role, int status, int gender)
        {
            AccName l = new AccName();
            if (role > 0)
            {
                if (role == 1)
                    l.role = "Super Admin";
                else if (role == 2)
                    l.role = "Trưởng phòng";
                else if (role == 3)
                    l.role = "Trưởng nhóm";
                else if (role == 4)
                    l.role = "Giáo viên";
                else if (role == 5)
                    l.role = "Học viên";
                else if (role == 6)
                    l.role = "Tư vấn viên";
                else if (role == 7)
                    l.role = "Học vụ";
                else if (role == 8)
                    l.role = "Quản lý chuyên môn";
                else if (role == 9)
                    l.role = "Kế toán";
            }

            if (status > 0)
            {
                if (status == 1)
                    l.status = "Chưa kích hoạt";
                else if (status == 2)
                    l.status = "Hoạt động";
                else if (status == 3)
                    l.status = "Khóa";
            }

            if (gender > 0)
            {
                if (gender == 1)
                    l.gender = "Nam";
                else if (gender == 2)
                    l.gender = "Nữ";
                else if (gender == 3)
                    l.gender = "Không xác định";
                else
                    l.gender = "Không xác định";
            }
            return l;
        }

        //lấy theo ID
        public static tbl_Account getbyID(int ID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                return l;
            }
        }

        //lấy theo role
        public static List<tbl_Account> getbyrole(int roleid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.RoleID == roleid && n.IsDelete == false && n.Status == 2).ToList();
                return l;
            }
        }
        //public static List<>
        public static List<tbl_Account> GetStaff()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.RoleID == 1 || n.RoleID == 7 && n.IsDelete == false && n.ID != 1 && n.Status == 2).ToList(); //supperadmin/học vụ
                return l;
            }
        }
        //lấy tất cả
        public static List<tbl_Account> getall()
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(x => x.IsDelete == false && x.Status == 2).ToList();
                return l;
            }
        }

        //lấy theo tài khoản
        public static tbl_Account getbyusername(string UserName)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.UserName == UserName).FirstOrDefault();
                return l;
            }
        }
        //check email
        public static tbl_Account getbyemail(string email)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.Email == email).FirstOrDefault();
                return l;
            }
        }

        //kiểm tra đăng nhập
        public static tbl_Account login(string username, string pass)
        {
            using (var db = new ZimEntities())
            {
                string password = Encryptor.Encrypt(pass);
                var l = db.tbl_Account.Where(n => n.UserName == username && n.Password == password).FirstOrDefault();
                return l;
            }
        }

        public static string getrolehtml(int roleid)
        {
            if (roleid == 1)
                return "Super Admin";
            if (roleid == 2)
                return "Quản lý";
            if (roleid == 3)
                return "Lễ tân";
            if (roleid == 4)
                return "Giáo viên";
            if (roleid == 5)
                return "Học viên";
            if (roleid == 6)
                return "Saler";
            if (roleid == 7)
                return "Học vụ";
            if (roleid == 8)
                return "Quản lý chuyên môn";
            return "Không xác định";
        }

        //cập nhật trạng thái online
        public static tbl_Account updatestatussnline(int UID, bool online)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.ID == UID).FirstOrDefault();
                if (a != null)
                {
                    a.StatusOnline = online;
                    db.SaveChanges();
                    return a;
                }
                else
                    return null;
            }
        }

        //set ket để reset password
        public static tbl_Account setkeyforgotpass(int ID, string key)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.KeyForgotPassword = key;
                    a.CreatedDateKeyForgot = GetDateTime.Now;
                    db.SaveChanges();
                    return a;
                }
                return a;
            }
        }
        //kiểm tra key
        public static tbl_Account checkkeyforgotpass(string user, string key)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.UserName == user && n.KeyForgotPassword == key).FirstOrDefault();
                return a;
            }
        }
        //cập nhật mật khẩu qua key
        public static tbl_Account setpassnewforgotpass(int ID, string passnew)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.Password = Encryptor.Encrypt(passnew);
                    a.KeyForgotPassword = null;
                    a.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return a;
                }
                return a;
            }
        }
        /// <summary>
        /// cập nhật trạng thái cần tư vấn
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Advisory"></param>
        /// <param name="createby"></param>
        /// <returns></returns>
        public static tbl_Account updateddvisory(int ID, bool Advisory, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.AdvisoryCustomer = Advisory;
                    ck.ModifiedBy = createby;
                    ck.ModifiedDate = GetDateTime.Now;
                    db.SaveChanges();
                    return ck;
                }
                return ck;
            }
        }

        //cập nhật lại role, tạm thôi có thể xóa được
        public static tbl_Account updatethumnail(int ID, string thumb)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    a.AvatarThumbnail = thumb;
                    db.SaveChanges();
                    return a;
                }
                else
                    return null;
            }
        }

        //Check Exam
        public static bool CheckExamComing(int ID)
        {
            using (var db = new ZimEntities())
            {
                var a = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (a != null)
                {
                    if (a.DateExam != null)
                    {
                        if ((a.DateExam.Value - GetDateTime.Now).Days >= 0 && (a.DateExam.Value - GetDateTime.Now).Days <= 90)
                            return true;
                        else
                            return false;
                    }
                    else
                        return false;
                }
                else
                    return false;
            }
        }
        public static List<tbl_Account> getallbysupportID(int supportID)
        {
            using (var db = new ZimEntities())
            {
                return db.tbl_Account.Where(n => n.SupportUID == supportID && n.IsDelete == false).ToList();
            }
        }
        public static tbl_Account UpdateLearningStatus(int ID, int learning)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.LearningStatus = learning;
                    db.SaveChanges();
                    return ck;
                }
                return ck;
            }
        }
        public static tbl_Account ImportExcel(Areas.Admin.Controllers.CustomerController.ExcelObject model)
        {
            using (var db = new ZimEntities())
            {
                model.PhoneNumber = model.PhoneNumber.Trim();
                tbl_Account ck = new tbl_Account();

                ck.FullName = model.FullName;// họ và tên
                if (!string.IsNullOrEmpty(model.PhoneNumber))//không có sdt bỏ qua email
                {
                    ck.UserName = model.Email.Trim().Replace(" ", "");//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = model.PhoneNumber;// số điện thoại
                }
                else
                {
                    ck.UserName = model.Email;//tài khoản
                    ck.Password = Encryptor.Encrypt("iccacademy");// mật khẩu
                    ck.Phone = "0";// số điện thoại
                }
                ck.LocationID = 0;
                ck.JobID = 0;
                ck.SupportUID = 0;
                ck.SourceID = 0;
                ck.ScoreIn = "0";
                ck.ScoreOut = "0";
                ck.Email = model.Email;// email
                ck.Address = model.Address;// địa chỉ
                ck.RoleID = 5;//1 admin, 2 manager, 3 cashier lễ tân, 4 giáo viên, 5 học viên, 6 saler, 7 học vụ, 8 quản lý chuyên môn
                ck.RoleName = "Học viên";// tên chứ vụ
                ck.Status = 2;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = "Hoạt động";// trên trạng thái
                ck.GenderID = 3;// giới tính
                ck.Gender = "Chưa xác định";// tên giới tính                
                ck.Birthday = GetDateTime.Now.Date;// sinh nhât
                try
                {
                    if (!string.IsNullOrEmpty(model.BirthDay))
                        ck.Birthday = DateTime.ParseExact(model.BirthDay.Replace(" ", ""), "dd/MM/yyyy", null);
                }
                catch { }

                ck.CreatedBy = "ImportExcel";
                ck.CreatedDate = GetDateTime.Now;
                ck.IsDelete = false;
                ck.LearningStatus = 1; //1.chưa đăng ký 2.đã đăng ký 3.đã xong
                ck.NoteHome = "";//người nhà
                ck.AdvisoryCustomer = false;//khách cần tư vấn
                ck.NationID = 0;
                db.tbl_Account.Add(ck);
                db.SaveChanges();
                return ck;
            }
        }
        //lấy theo token
        public static tbl_Account getByToken(string token)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.Token == token).FirstOrDefault();
                return l;
            }
        }
        public static void UpdateToken(int id, string token)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Token = token;
                    ck.ExpiredToken = DateTime.Now.AddDays(5);
                    db.SaveChanges();
                }
            }
        }
        public static void Logout(int id)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(x => x.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    ck.Token = null;
                    ck.ExpiredToken = DateTime.Now.AddDays(-5);
                    db.SaveChanges();
                }
            }
        }
        /// <summary>
        /// lấy danh sách giáo viên có lịch giảng dậy trong khóa học
        /// </summary>
        /// <returns></returns>
        public static List<tbl_Account> getbyTeacherCourse()
        {
            using (var db = new ZimEntities())
            {
                var account = db.tbl_Account.Where(x => x.RoleID == 4).ToList();
                List<int> teacher = new List<int>();
                var teacherCourse = db.tbl_TeacherCourse.Where(x => x.isHide == false).ToList();
                if (teacherCourse != null)
                {
                    teacher.AddRange(teacherCourse.Select(x => (int)x.TeacherUID).Distinct().ToList());
                }
                var result = (from a in account
                              join t in teacher
                              on a.ID equals t
                              select a).ToList();
                return result;
            }
        }
        public static tbl_Account UpdateProfile(int id, int cityID, string cityName, int districtID,
        string districtName, int wardID, string wardname, int streetID, string streetName, string homeNumber, string address,
        string fullName, string phoneNumber, string email, string birhtDay, string identityCard, int identityCardCityID,
        string identityCardCityName, string identityCardDate, string avatar, int jobID, string jobName, string workPlace,
        int academicPurposesID, string academicPurposesName, string username)
        {
            using (var db = new ZimEntities())
            {
                tbl_Account ck = db.tbl_Account.Where(n => n.ID == id).FirstOrDefault();
                if (ck != null)
                {
                    string ch = "";
                    ck.LocationID = cityID;// tỉnh thành phố
                    ck.Avatar = avatar;
                    ck.AvatarThumbnail = avatar;
                    if (ck.LocationName != cityName)
                        ch += "Tỉnh, thành phố: " + ck.LocationName + " => " + cityName + "<br/>";
                    ck.LocationName = cityName;// tỉnh thành phố

                    ck.DistrictID = districtID;// ID quận
                    if (ck.DistrictName != districtName)
                        ch += "Quận, huyện: " + ck.DistrictName + " => " + districtName + "<br/>";
                    ck.DistrictName = districtName;// quận 

                    ck.WardID = wardID;// ID phường
                    if (ck.WardName != wardname)
                        ch += "Phường xã: " + ck.WardName + " => " + wardname + "<br/>";
                    ck.WardName = wardname;// phường 

                    ck.StreetID = streetID;// ID phường
                    if (ck.StreetName != streetName)
                        ch += "Đường: " + ck.WardName + " => " + streetName + "<br/>";
                    ck.StreetName = streetName;// phường 

                    if (ck.HomeNumber != homeNumber)
                        ch += "Số nhà: " + ck.HomeNumber + " => " + homeNumber + "<br/>";
                    ck.HomeNumber = homeNumber;// số nhà 

                    if (ck.FullName != fullName)
                        ch += "Họ tên: " + ck.FullName + " => " + fullName + "<br/>";
                    ck.FullName = fullName;// họ và tên

                    if (ck.Phone != phoneNumber)
                        ch += "Số điện thoại: " + ck.Phone + " => " + phoneNumber + "<br/>";
                    ck.Phone = phoneNumber;// số điện thoại

                    if (ck.Email != email)
                        ch += "Email: " + ck.Email + " => " + email + "<br/>";
                    ck.Email = email;// email

                    if (ck.Address != address)
                        ch += "Địa chỉ: " + ck.Address + " => " + address + "<br/>";
                    ck.Address = address;// địa chỉ

                    try
                    {
                        if (!string.IsNullOrEmpty(birhtDay.Trim()))
                        {
                            DateTime dbd = DateTime.ParseExact(birhtDay.Replace(" ", ""), "dd/MM/yyyy", null);
                            if (ck.Birthday != null)
                            {
                                if (ck.Birthday.Value.ToString("dd/MM/yyyy") != dbd.ToString("dd/MM/yyyy"))
                                    ch += "Ngày sinh: " + ck.Birthday.Value.ToString("dd/MM/yyyy") + " => " + dbd.ToString("dd/MM/yyyy") + "<br/>";
                            }
                            ck.Birthday = dbd.Date;
                        }
                    }
                    catch
                    {
                        //ck.Birthday = GetDateTime.Now.Date;// sinh nhât
                    }

                    ck.ModifiedBy = username;
                    ck.ModifiedDate = GetDateTime.Now;
                    ck.JobID = jobID;//công việc

                    if (ck.Job != jobName)
                        ch += "Công việc: " + ck.Job + " => " + jobName + "<br/>";
                    ck.Job = jobName;// tên công viện

                    if (ck.WorkPlace != workPlace)
                        ch += "Nơi làm việc: " + ck.WorkPlace + " => " + workPlace + "<br/>";
                    ck.WorkPlace = workPlace;// nơi làm việc

                    ck.AcademicPurposesID = academicPurposesID;
                    if (ck.AcademicPurposes != academicPurposesName)
                        ch += "Mục đích học tập: " + ck.AcademicPurposes + " => " + academicPurposesName + "<br/>";
                    ck.AcademicPurposes = academicPurposesName;// mục đích học tập

                    if (!string.IsNullOrEmpty(identityCard))
                    {
                        if (ck.IdentityCard != identityCard)
                            ch += "CMND: " + ck.IdentityCard + " => " + identityCard + "<br/>";
                        ck.IdentityCard = identityCard;// chứng mình nhân dân

                        ck.IdentityCardCityID = identityCardCityID;// nơi cấp CMND

                        if (ck.IdentityCardCityID != identityCardCityID)
                            ch += "Nơi cấp CMND: " + ck.IdentityCardCityName + " => " + identityCardCityName + "<br/>";
                        ck.IdentityCardCityName = identityCardCityName;// tên nơi cấp CMND
                        try
                        {
                            if (!string.IsNullOrEmpty(identityCardDate))
                            {
                                DateTime carddate = DateTime.ParseExact(identityCardDate.Replace(" ", ""), "dd/MM/yyyy", null);// ngày cấp
                                if (ck.IdentityCardDate != null)
                                {
                                    if (ck.IdentityCardDate.Value.ToString("dd/MM/yyyy") != carddate.ToString("dd/MM/yyyy"))
                                        ch += "Ngày cấp CMND: " + ck.IdentityCardDate.Value.ToString("dd/MM/yyyy") + " => " + carddate.ToString("dd/MM/yyyy") + "<br/>";
                                }
                                ck.IdentityCardDate = carddate.Date;
                            }
                        }
                        catch { }
                    }

                    db.SaveChanges();
                    if (!string.IsNullOrEmpty(ch))
                        AccountHistoryChangeTable.insert(ck.ID, ck.FullName, ch, username);
                }
                return ck;
            }
        }


        //cập nhật
        public static tbl_Account updatestaff(int ID, int locationID, string locationname, int districtID, string districtName, int schoolID, string schoolName, string fullname, string password, string email, string phone, string address,
            int roleID, int status, DateTime Birthday, int gender, string createby)
        {
            using (var db = new ZimEntities())
            {
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck == null)
                    return ck;
                var v = viewrole(roleID, status, gender);
                if (locationID != 0)
                {
                    ck.LocationID = locationID;
                    ck.LocationName = locationname;
                }
                if (districtID != 0)
                {
                    ck.DistrictID = districtID;
                    ck.DistrictName = districtName;
                }
                if (schoolID != 0)
                {
                    ck.SchoolID = schoolID;
                    ck.SchoolName = schoolName;
                }
                ck.FullName = fullname;

                if (!string.IsNullOrEmpty(password))
                    ck.Password = Encryptor.Encrypt(password);
                ck.Phone = phone;
                ck.Email = email;
                ck.Address = address;
                ck.RoleID = roleID;//1 admin, 2 manager, 3 cashier lễ tân, 4 giáo viên, 5 học viên, 6 saler, 7 quản lý học vụ, 8 quản lý chuyên môn
                ck.RoleName = v.role;
                ck.Status = status;//1 chưa kích hoạt, 2 hoạt động, 3 khóa
                ck.StatusName = v.status;
                if (gender == 1 || gender == 2 || gender == 3)
                    ck.GenderID = gender;
                else
                    ck.GenderID = 3;
                ck.Gender = v.gender;
                ck.SalaryLesson = 0;
                ck.Birthday = Birthday;

                ck.ModifiedBy = createby;
                ck.ModifiedDate = GetDateTime.Now;
                db.SaveChanges();
                return ck;
            }
        }
        //lấy danh sách các học viên theo parentID
        public static List<tbl_Account> GetStudentByParentID(int parentID)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.ParentID == parentID && n.Status == 2).ToList();
                return l;
            }
        }

        //lay full name
        public static string GetFullName(int id)
        {
            using (var db = new ZimEntities())
            {
                string name = "";
                var l = db.tbl_Account.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    name = l.FullName;
                }
                return name;
            }
        }
        public static void UpdateLeader(int id, int leaderid)
        {
            using (var db = new ZimEntities())
            {
                var l = db.tbl_Account.Where(n => n.ID == id).FirstOrDefault();
                if (l != null)
                {
                    l.LeaderID = leaderid;
                    db.SaveChanges();
                }
            }
        }
        public static tbl_Account UpdateStatus(int ID, int status)
        {
            using (var db = new ZimEntities())
            {
                var v = viewrole(0, status, 0);
                var ck = db.tbl_Account.Where(n => n.ID == ID).FirstOrDefault();
                if (ck != null)
                {
                    ck.Status = status;
                    ck.StatusName = v.status;
                    db.SaveChanges();
                    return ck;
                }
                return ck;
            }
        }

        public static List<Admin_Teacher_GetAvailable_Result> GetTeacherAvailable(int course, int program)
        {
            using (var db = new ZimEntities())
            {
                var teachers = db.Admin_Teacher_GetAvailable(0, course, program, 0, "").ToList();
                return teachers;
            }
        }

        public static double GetRevenueById(int id)
        {
            using (var db = new ZimEntities())
            {
                var data = db.Admin_GetTotalRevenue(id).FirstOrDefault();
                return data ?? 0;
            }
        }

        public static void UpdateAcademic(int id, int academic, string username)
        {
            using (var db = new ZimEntities())
            {
                string academicName = db.tbl_Account.Where(x => x.ID == academic).FirstOrDefault()?.FullName;
                var data = db.tbl_Account.Where(x => x.ID == id).FirstOrDefault();
                if (data != null)
                {
                    data.SupportUID = academic;
                    data.SupportName = academicName;
                    data.CreatedBy = username;
                    data.CreatedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }
        public static void UpdatePosition(int id, int positionId, string username)
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_Account.Where(x => x.ID == id).FirstOrDefault();
                if (data != null)
                {
                    data.Position = positionId;
                    data.CreatedBy = username;
                    data.CreatedDate = DateTime.Now;
                    db.SaveChanges();
                }
            }
        }

        public static List<tbl_Account> GetAcademics()
        {
            using (var db = new ZimEntities())
            {
                var data = db.tbl_Account.Where(n => n.Status == 2 && (n.RoleID == 6 || n.RoleID == 3 || n.RoleID == 2)).OrderBy(n => n.FullName).ToList();
                return data;
            }
        }

        public static List<Get_RolesAndLanguage_Result> GetByRolesAndLanguage(string roles, string languages)
        {
            using (var db = new ZimEntities())
            {
                List<Get_RolesAndLanguage_Result> l = new List<Get_RolesAndLanguage_Result>();
                l = db.Get_RolesAndLanguage(roles, languages).ToList();
                return l;
            }
        }

        public static object GetByLangue(int[] languageID)
        {
            using (var db = new ZimEntities())
            {
                //var data = db.tbl_Account.Where(n => n.Status == 2 && languageID.Contains(n.Language ?? 0)).OrderBy(n => n.FullName).Select(x=>x.ID).ToList();
                var data = (from a in db.tbl_Account
                            where (a.Status == 2 && languageID.Contains(a.Language ?? 0))
                            orderby a.FullName
                            select new
                            {
                                a.FullName,
                                a.ID
                            }).ToList();
                if (!data.Any())
                {
                    return null;
                }
                else
                {
                    return data;
                }
            }
        }

        public async static Task<List<tbl_Account>> GetManagerAsync()
        {
            using (var db = new ZimEntities())
            {
                var data = await db.tbl_Account.Where(x => (x.RoleID == 1 || x.RoleID == 2 || x.RoleID == 3 || x.RoleID == 6) && x.Status == 2).ToListAsync();
                return data;
            }
        }
    }
}