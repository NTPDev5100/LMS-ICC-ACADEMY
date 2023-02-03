using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace AppZim.ZIM
{
    public class Zoom
    {
        public class Room
        {
            public int ID { get; set; }
            public string RoomID { get; set; }
            public string RoomPass { get; set; }
            public int Role { get; set; }
            public string Key { get; set; }
            public string Secrect { get; set; }
            public string FullName { get; set; }
        }

        public static string OptionDiemDanh(int attid)
        {
            string attname = "";
            if (attid == 1)
                attname = "Có";
            else if (attid == 2)
                attname = "Vắng có phép";
            else if (attid == 3)
                attname = "Vắng không phép";
            else if (attid == 4)
                attname = "Đi muộn";
            else if (attid == 5)
                attname = "Về sớm";
            else if (attid == 6)
                attname = "Nghỉ lễ";
            return attname;
        }
        public static string OptionNhanXet(int learningid)
        {
            string learingname = "";
            if (learningid == 1)
                learingname = "Giỏi";
            else if (learningid == 2)
                learingname = "Khá";
            else if (learningid == 3)
                learingname = "Trung bình";
            else if (learningid == 4)
                learingname = "Kém";
            else if (learningid == 5)
                learingname = "Theo dõi đặc biệt";
            else if (learningid == 6)
                learingname = "Có cố gắng";
            else if (learningid == 7)
                learingname = "Không cố gắng";
            else if (learningid == 8)
                learingname = "Không nhận xét";
            return learingname;
        }

        /// <summary>
        /// Xác thực và tạo token
        /// </summary>
        /// <param name="apiSecret"></param>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public static string AccessToken(string apiSecret, string apiKey)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var now = DateTime.UtcNow;
            //var apiSecret = "7ltYSsOvahjPOP8DobbEEzlKC60tJ4juPAxj";
            byte[] symmetricKey = Encoding.ASCII.GetBytes(apiSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                //Issuer = "BQMrBCtJRsOgShv81OQK4w",
                Issuer = apiKey,
                Expires = now.AddSeconds(300),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            //var tokenString = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOm51bGwsImlzcyI6IkJRTXJCQ3RKUnNPZ1NodjgxT1FLNHciLCJleHAiOjE2NTE0MjUwNjAsImlhdCI6MTYxOTg4NDYzNH0.1HaL-PV81VRd_NvSlusQ5UoFZP_uDWs4LdssUT7sfV8";
            return tokenString;
        }
        

        public class RecordingPagination
        {
            public int page_size { get; set; }
            public int total_records { get; set; }
            public string from_date { get; set; }
            public string to_date { get; set; }
        }
        public class RecordingMeeting
        {
            public string id { get; set; }
            public string share_url { get; set; }
            public string start_time { get; set; }
            public string topic { get; set; }
            public string total_size { get; set; }
        }
        public class RecordingFiles
        {
            public string id { get; set; }
            public string meeting_id { get; set; }
            public string download_url { get; set; }
            public string file_type { get; set; }
            public string play_url { get; set; }
            public string recording_start { get; set; }
            public string recording_end { get; set; }
            public string file_size { get; set; }
            public string file_extension { get; set; }
            public string recording_type { get; set; }
        }
        public class Meeting
        {
            public string id { get; set; }
            public string join_url { get; set; }
            public string created_at { get; set; }
            public string start_time { get; set; }
            public string topic { get; set; }
            public string timezone { get; set; }
            public string password { get; set; }
            public int CoursesScheduleID { get; set; }
            public string teacherName { get; set; }
        }
    }
}