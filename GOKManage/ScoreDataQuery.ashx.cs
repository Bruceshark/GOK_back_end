using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using LitJson;

namespace GOKManage
{
    /// <summary>
    /// ScoreDataQuery 的摘要说明
    /// </summary>
    public class ScoreDataQuery : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            GetResult();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void GetResult()
        {
            HttpContext context = HttpContext.Current;
            var people = new JsonData();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT USER_TBL.USER_ID, NAME, SCORE FROM USER_TBL, RESULT_SCORE_TBL " +
                                                "WHERE USER_TBL.USER_ID = RESULT_SCORE_TBL.USER_ID", conn); 
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var person = new JsonData();
                        person["id"] = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                        person["name"] = reader.GetString(reader.GetOrdinal("NAME"));
                        person["score"] = reader.GetInt32(reader.GetOrdinal("SCORE"));
                        person.ToJson();
                        people.Add(person);
                    }
                }
            }
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["people"] = people;
            context.Response.Write(resultJson.ToJson());
            conn.Close();//关闭数据库
        }
    }
}