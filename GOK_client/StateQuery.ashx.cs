using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using System.Web;


namespace GOK_client
{
    /// <summary>
    /// StateQuery 的摘要说明
    /// </summary>
    public class StateQuery : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            state();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 给usermain页面传递游戏进程
        /// </summary>
        private void state()
        {
            int state = 0;
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        state = reader.GetInt32(reader.GetOrdinal("SWITCH"));
                    }
                }
            }
            context.Response.Write(state);
            conn.Close();//关闭数据库
        }
    }
}