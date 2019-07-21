using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Web.SessionState;
using LitJson;

namespace GOK_client
{
    /// <summary>
    /// MessageReadHandler 处理消息已读。
    /// </summary>
    public class MessageReadHandler : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string user = "";
            HttpContext contextid = HttpContext.Current;
            string msgId_ = "";
            if (contextid.Session["user_id"] == null) context.Response.Write(0);
            else
            {
                user = contextid.Session["user_id"].ToString();
            }
            string msgId = context.Request["msg_id"];
            //post输入的user2需要进行参数校验
            if (pub.QueryCheck(msgId) != "ok") context.Response.Write("illegal");
            else
            {
                msgId_ = pub.QuotedStr(msgId);
            }
            MessageRead(user, msgId_);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 将前端传入的消息在数据库中标为已读，再把消息整理成json传回前端
        /// </summary>
        /// <param name="user">当前用户的id</param>
        /// <param name="msgId">用户点击的消息的id</param>
        private void MessageRead(string user, string msgId)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("UPDATE USER_MESSAGE_TBL SET MSG_IS_READ = 1 WHERE MSG_ID =" + msgId + "AND USER_ID =" + user, conn);
            cmd.ExecuteNonQuery();
            var message = new JsonData();
            SqlCommand cmd2 = new SqlCommand("SELECT MSG_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME " +
                "FROM USER_MESSAGE_TBL WHERE USER_ID =" + user + "ORDER BY MSG_ID DESC", conn);
            using (SqlDataReader reader = cmd2.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var singleMsg = new JsonData();
                        singleMsg["id"] = reader.GetInt32(reader.GetOrdinal("MSG_ID"));
                        singleMsg["is_read"] = reader.GetInt32(reader.GetOrdinal("MSG_IS_READ"));
                        singleMsg["title"] = reader.GetString(reader.GetOrdinal("MSG_TITLE"));
                        singleMsg["content"] = reader.GetString(reader.GetOrdinal("MSG_CONTENT"));
                        singleMsg["time"] = reader.GetString(reader.GetOrdinal("MSG_TIME"));
                        singleMsg.ToJson();
                        message.Add(singleMsg);
                    }
                }
            }

            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["message"] = message;
            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
    }
}