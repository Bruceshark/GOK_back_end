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
    /// LoginCheck 的摘要说明:
    /// </summary>
    public class LoginCheck : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";

            HttpContext context_ = HttpContext.Current;
            if (context_.Session["user_name"] == null) context.Response.Write(0);
            else
            {
                string name = context_.Session["user_name"].ToString();
                string userId = context_.Session["user_id"].ToString();

                SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
                conn.Open();//打开数据库

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

                conn.Close();

                if (state == -2)
                {
                    context.Response.Write("end");
                }
                else if (alive == 0)
                {
                    context.Response.Write("dead");
                }
                else
                {
                    context.Response.Write(name);
                }
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