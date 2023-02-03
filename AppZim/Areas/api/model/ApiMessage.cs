using AppZim.Models;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppZim.api
{
    public class ApiMessage
    {
        public const string Unauthorized = "Chưa đăng nhập";
        public const string WrongUserNameOrPass = "Sai tên tài khoản hoặc mật khẩu";
        public const string Success = "Thành công";
        public const string Forbidden = "Bạn không có quyền truy cập";
        public const string ExpiredToken = "Phiên đăng nhập hết hạn";
        public const string NotFoundInformation = "Không tìm thấy thông tin";
        public const string SentLink = "Link đổi mật khẩu đã được gửi vào email của bạn";
        public const string WrongPassword = "Sai mật khẩu";
        public const string InvalidFile = "File không đúng định dạng";
        public const string RequiredInformation = "Vui lòng điền đầy đủ thông tin";
        public const string SaveDayOffSuccess = "Đăng ký nghỉ thành công";
        public const string SaveFail = "Bạn đã có lịch, không thể nghỉ";
        public const string CancelDayOffSuccess = "Hủy lịch nghỉ thành công";
        public const string NotFoundGroup = "Không tìm thấy nhóm";
        public const string RequiredQuestion = "Vui lòng trả lời hết tất cả các câu hỏi";
    }
    public class AccountDetail
    {
        public int id { get; set; }
        public string userName { get; set; }
        public int cityID { get; set; }
        public string cityName { get; set; }
        public int districtID { get; set; }
        public string districtName { get; set; }
        public int wardID { get; set; }
        public string wardname { get; set; }
        public int streetID { get; set; }
        public string streetName { get; set; }
        public string homeNumber { get; set; }
        public string address { get; set; }
        public string fullName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public string birthDay { get; set; }
        public string identityCard { get; set; }
        public int identityCardCityID { get; set; }
        public string identityCardCityName { get; set; }
        public string identityCardDate { get; set; }
        public string avatar { get; set; }
        public int jobID { get; set; }
        public string job { get; set; }
        public string workPlace { get; set; }
        public int academicPurposesID { get; set; }
        public string academicPurposes { get; set; }
        public string token { get; set; }
        public int roleID { get; set; }
        public List<tbl_City> cityList { get; set; }
        public List<tbl_Job> jobList { get; set; }
        public List<tbl_AcademicPurposes> academicPurposesList { get; set; }
    }
    public class ChangePass
    {
        public string oldPass { get; set; }
        public string newPass { get; set; }
    }
    public class ScheduleFilter
    {
        public int courseid { get; set; }
        public string subject { get; set; }
        public string start { get; set; }
        public string end { get; set; }
    }
    public class DateOffFilter
    {
        public string date { get; set; }
        public int type { get; set; }
    }
    public class NewsfeedFilter
    {
        public string search { get; set; }
        public int groupID { get; set; }
    }
    public class CreateNewsFeed
    {
        public int id { get; set; }
        public int groupID { get; set; }// có thê có hoặc không
        public string title { get; set; }
        public string content { get; set; }
        public string link { get; set; }
        public int background { get; set; }// chọn mùa background theo mặc định gán cứng
        public int fileType { get; set; }//1 imgage,2 video
        public int postType { get; set; }// 0 bài đăng phải duyệt mới xem được, 1 không cần duyệt
    }
    public class TicketFilter
    {
        public int page { get; set; }
        public int type { get; set; }
    }
    public class DataExercise
    {
        public string Question { get; set; }
        public List<tbl_Exercise> QuestionList { get; set; }
        public List<API_answer_getListByGroupID_Result> AnswerList { get; set; }
    }
    public class SlotSchedule
    {
        public int StudyTimeID { get; set; }
        public string StudyTimeName { get; set; }
        public string StudyDate { get; set; }
        public bool CheckBox { get; set; }
        public bool Checked { get; set; }
    }
    public class Lesson
    {
        public int StudyTimeID { get; set; }
        public string StudyDate { get; set; }
        public string OptionName { get; set; }
        public bool Selected { get; set; }
    }
    public class AttendanceRecord
    {
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string Note { get; set; }
        public int AttendanceID { get; set; }
        public string AttendanceName { get; set; }
        public int LearningID { get; set; }
        public string LearningName { get; set; }
        public bool Warning { get; set; }
        public bool Disible { get; set; }
        public DateTime DateAttendance { get; set; }
    }
    public class QandA
    {
        public int QuesID { get; set; }
        public string AnsID { get; set; }
    }
    public class Comment
    {
        public int CommentID { get; set; }
        public string Text { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string CommentContent { get; set; }
    }
    public class Statiscial
    {
        public int value1 { get; set; }
        public int value2 { get; set; }
        public int value3 { get; set; }
        public int value4 { get; set; }
    }
    public class PostNewsFeed
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string IMG { get; set; }
        public string Content { get; set; }
        public int NumberView { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedName { get; set; }
        public string Avatar { get; set; }
    }
    public class CourseInformation
    {
        public int CourseID { get; set; }
        public int CourseStudentID { get; set; }
        public string CourseName { get; set; }
        public int Status { get; set; } 
        public string StatusName { get; set; }
        public string TeacherLeaderName { get; set; }
        public string TeacherName { get; set; }
        public int Price { get; set; }
        public int TotalPaid { get; set; }
        public int PriceLeft { get; set; }
        public string OpeningDate { get; set; }
        public string SchoolName { get; set; }
        public string StudyTime { get; set; }
        public string EndDate { get; set; }
        public int TotalStudent { get; set; }
        public int TotalStudyDay { get; set; }
    }
    public class LoadComment
    {
        public int CommentID { get; set; }
        public int PostContentID { get; set; }
        public int PostCommentID { get; set; }
        public int UID { get; set; }
        public string CMT { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Avatar { get; set; }
    }
    public class StudentList
    {
        public int StudentID { get; set; }
        public int Dayoff { get; set; }
        public string SupportName { get; set; }
        public string Email { get; set; }
        public bool Warning { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string Avatar { get; set; }
    }
    public class ExerciseList
    {
        public int ID { get; set; }
        public int ExerciseGroupID { get; set; }
        public string ExerciseTitle { get; set; }
        public string FileName { get; set; }
        public int Type { get; set; }
        public string CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool HaveDeadline  { get; set; }
        public bool IsDone  { get; set; }
        public bool IsRemark  { get; set; }
        public string LinkFile  { get; set; }
    }
    public class StudyTime
    {
        public int StudyTimeID { get; set; }
        public string StudyTimeName { get; set; }
    }
    public class Timeline
    {
        public int ID { get; set; }
        public int CourseID { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public string RoleName { get; set; }
    }
    public class ExerciseResult
    {
        public int ID { get; set; }
        public string YourAnswer { get; set; }
        public string KeyResult { get; set; }
    }
    public class ExerciseGroupList
    {
        public int ID { get; set; }
        public int TypeGroup { get; set; }
        public string TypeGroupName { get; set; }
        public string ExerciseGroupName { get; set; }
        public int TimeOut { get; set; }
        public string Introduction { get; set; }
        public bool IsDone { get; set; }
    }
    public class Student
    {
        public int ID { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string StatusName { get; set; }
        public string CreatedDate { get; set; }
    }
    public class SortAccount
    {
        public int ID { get; set; }
        public string FullName { get; set; }
        public string Avatar { get; set; }
        public string RoleName { get; set; }
    }
    public class Score
    {
        public int StudentID { get; set; }
        public int ScoreID { get; set; }
        public double ScoreValue { get; set; }
        public string Remark { get; set; }
    }
    public class ScoreDetail
    {
        public int StudentID { get; set; }
        public int ConfigID { get; set; }
        public string ConfigName { get; set; }
        public int ScoreFactor { get; set; }
        public List<API_Score_GetListByStudentID_Result> Score { get; set; }
    }
    public class SubmitTask
    {
        public int TaskID { get; set; }
        public string Answer { get; set; }
    }
    
    public class UpdateReasonSalary
    {
        public int id { get; set; }
        public string reason { get; set; }
        public int status { get; set; }
    }

    public class StudentSignContract
    {
        /// <summary>
        /// Chữ ký học viên
        /// </summary>
        public string sign { get; set; }
        /// <summary>
        /// id học viên
        /// </summary>
        public int id { get; set; }
    }

    //public class VideoSchedule
    //{
    //    public int ID { get; set; }
    //    public Nullable<int> CourseID { get; set; }
    //    public Nullable<int> SubjectID { get; set; }
    //    public Nullable<int> CurriculumsDetailsID { get; set; }
    //    public Nullable<int> StudyTimeID { get; set; }
    //    public string SubjectName { get; set; }
    //    public string TimeName { get; set; }
    //    public Nullable<System.DateTime> Date { get; set; }
    //    [JsonIgnore]
    //    public string Files { get; set; }
    //    public List<string> FileList
    //    {
    //        get
    //        {
    //            if (!string.IsNullOrEmpty(Files))
    //            {
    //                return Files.Split(',').ToList();
    //            }
    //            else
    //                return new List<string>();
    //        }
    //    }
    //}
}