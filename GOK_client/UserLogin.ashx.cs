using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.SessionState;

namespace GOK_client
{
    /// <summary>
    /// userlogin 的摘要说明
    /// </summary>
    public class UserLogin : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string username = context.Request["username"];
            string password = context.Request["password"];
            if (pub.QueryCheck(username) != "ok" || pub.QueryCheck(password) != "ok")
            {
                context.Response.Write("illegal");
            }
            else
            {
                string password_ = pub.md5(password);
                string username_ = pub.QuotedStr(username);
                string res = LoginQuery(username_, password_);
                context.Response.Write(res);
            }
        }

        private string LoginQuery(string username, string password)
        {
            int old = 1;
            int userId = 0;
            string msgTimeLong = DateTime.Now.ToLocalTime().ToString();

            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            //判断用户是否存在，不存在返回error
            SqlCommand exist = new SqlCommand("SELECT COUNT(*) AS NUM FROM USER_TBL WHERE NAME = " + username, conn);
            using (SqlDataReader reader = exist.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int num = reader.GetInt32(reader.GetOrdinal("NUM"));
                        if (num == 0) return "error";
                    }
                }
            }
            //判断用户密码是否对上，对不上返回error，对上则写入session
            SqlCommand user = new SqlCommand("SELECT * FROM USER_TBL WHERE NAME = " + username, conn);
            using (SqlDataReader reader = user.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userId = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                        string userName = reader.GetString(reader.GetOrdinal("NAME"));
                        string db_password = reader.GetString(reader.GetOrdinal("PASSWORD"));
                        old = reader.GetInt32(reader.GetOrdinal("OLD"));
                        if (db_password != password) return "error";
                        else
                        {
                            context.Session["user_id"] = userId.ToString();
                            context.Session["user_name"] = userName;
                        }
                    }
                }
            }

            //判断游戏是否已经结束
            int state = 0;
            SqlCommand stateCmd = new SqlCommand("SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'", conn);
            using (SqlDataReader reader = stateCmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        state = reader.GetInt32(reader.GetOrdinal("SWITCH"));
                    }
                }
            }

            //判断用户是否死亡
            int alive = 0;
            SqlCommand aliveCmd = new SqlCommand("SELECT ALIVE FROM USER_TBL WHERE USER_ID = " + userId, conn);
            using (SqlDataReader reader = aliveCmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        alive = reader.GetInt32(reader.GetOrdinal("ALIVE"));
                    }
                }
            }

            if (state == -2)
            {
                conn.Close();//关闭数据库
                return "end";
            }
            if (alive == 0)
            {
                conn.Close();//关闭数据库
                return "dead";
            }
            if (old == 1)
            {
                SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + userId, conn);
                act.ExecuteNonQuery();
                conn.Close();//关闭数据库
                return "old";
            }
            else
            {
                //把用户设置成老用户
                SqlCommand cmdold = new SqlCommand("UPDATE USER_TBL SET old = 1 WHERE NAME = " + username, conn);
                cmdold.ExecuteNonQuery();//执行sql语句
                SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + userId, conn);
                act.ExecuteNonQuery();
                conn.Close();//关闭数据库
                return "new";
            }
        }


        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
