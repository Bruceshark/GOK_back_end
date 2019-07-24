using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using LitJson;

namespace GOKManage
{
    /// <summary>
    /// AdminDataQuery 的摘要说明
    /// </summary>
    public class AdminDataQuery : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            GetData();
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        private void GetData()
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            var messages = new JsonData();
            var infos = new JsonData();
            var datas = new JsonData();
            var historys = new JsonData();
            messages.SetJsonType(JsonType.Array);
            infos.SetJsonType(JsonType.Array);
            historys.SetJsonType(JsonType.Array);
            //取出数据库里管理员需要处理的所有消息
            SqlCommand cmd1 = new SqlCommand("SELECT MSG_ID, NAME, MSG_CONTENT, QQ_ID, IS_PROCESSED, TIME " +
                                                "FROM USER_TBL, QQ_TBL, ADMIN_MSG_TBL " +
                                                "WHERE USER_TBL.USER_ID = ADMIN_MSG_TBL.USER_ID " +
                                                "AND USER_TBL.USER_ID = QQ_TBL.USER_ID " +
                                                "ORDER BY MSG_ID DESC", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var message = new JsonData();
                        message["id"] = reader.GetInt32(reader.GetOrdinal("MSG_ID"));
                        message["name"] = reader.GetString(reader.GetOrdinal("NAME"));
                        message["qq"] = reader.GetString(reader.GetOrdinal("QQ_ID"));
                        message["content"] = reader.GetString(reader.GetOrdinal("MSG_CONTENT"));
                        message["is_processed"] = reader.GetInt32(reader.GetOrdinal("IS_PROCESSED"));
                        message["time"] = reader.GetString(reader.GetOrdinal("TIME"));
                        message.ToJson();
                        messages.Add(message);
                    }
                }
            }
            //取出所有个人数据
            SqlCommand cmd2 = new SqlCommand("SELECT USER_TBL.USER_ID, NAME, ALIVE, JOB_NAME, IDEN_2_NAME, IDEN_1_NAME, BULLET_NUM, SUM(SKILL_LIMIT) AS SKILL_LIMIT, " +
                                                    "STATUS, DIETIME=ISNULL(DIETIME, -1), INCL_NAME, TIME=ISNULL(TIME, 0) " +
                                                "FROM USER_TBL, JOB_TBL, JOB_LIST_TBL, INCL_TBL, INCL_LIST_TBL, IDEN_TBL, IDEN_1_LIST_TBL, IDEN_2_LIST_TBL," +
                                                    " BULLET_TBL, SKILL_TBL, STATUS_TBL, COUNTDOWN_TBL, USER_ACTION_TIME_TBL " +
                                                "WHERE USER_TBL.USER_ID = JOB_TBL.USER_ID " +
                                                "AND JOB_TBL.JOB_ID = JOB_LIST_TBL.JOB_ID " +
                                                "AND USER_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND INCL_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID " +
                                                "AND IDEN_TBL.IDEN_1_ID = IDEN_1_LIST_TBL.IDEN_1_ID " +
                                                "AND IDEN_TBL.IDEN_2_ID = IDEN_2_LIST_TBL.IDEN_2_ID " +
                                                "AND BULLET_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND STATUS_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND SKILL_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND COUNTDOWN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND USER_ACTION_TIME_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "GROUP BY USER_TBL.USER_ID, NAME, ALIVE, JOB_NAME, INCL_NAME, IDEN_2_NAME, IDEN_1_NAME, BULLET_NUM, STATUS, DIETIME, USER_ACTION_TIME_TBL.TIME", conn);
            using (SqlDataReader reader = cmd2.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var info = new JsonData();
                        info["id"] = reader.GetInt32(reader.GetOrdinal("USER_ID"));
                        info["name"] = reader.GetString(reader.GetOrdinal("NAME"));
                        info["alive"] = reader.GetInt32(reader.GetOrdinal("ALIVE"));
                        info["job"] = reader.GetString(reader.GetOrdinal("JOB_NAME"));
                        info["iden_2_name"] = reader.GetString(reader.GetOrdinal("IDEN_2_NAME"));
                        info["iden_1_name"] = reader.GetString(reader.GetOrdinal("IDEN_1_NAME"));
                        info["incl_name"] = reader.GetString(reader.GetOrdinal("INCL_NAME"));
                        info["bullet_num"] = reader.GetInt32(reader.GetOrdinal("BULLET_NUM"));
                        info["skill_limit"] = reader.GetInt32(reader.GetOrdinal("SKILL_LIMIT"));
                        info["status"] = reader.GetInt32(reader.GetOrdinal("STATUS"));
                        info["dietime"] = reader.GetInt32(reader.GetOrdinal("DIETIME"));
                        info["time"] = reader.GetString(reader.GetOrdinal("TIME"));
                        info.ToJson();
                        infos.Add(info);
                    }
                }
            }
            //取出所有场上发生的事件
            SqlCommand cmd3 = new SqlCommand("SELECT MSG_ID, MSG_TIME, MSG_CONTENT FROM ADMIN_MSGLINE_TBL ORDER BY MSG_ID DESC", conn);
            using (SqlDataReader reader = cmd3.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var history = new JsonData();
                        history["id"] = reader.GetInt32(reader.GetOrdinal("MSG_ID"));
                        history["time"] = reader.GetString(reader.GetOrdinal("MSG_TIME"));
                        history["content"] = reader.GetString(reader.GetOrdinal("MSG_CONTENT"));
                        history.ToJson();
                        historys.Add(history);
                    }
                }
            }
            //取出场上所有数据，数据量较大维护需仔细参考前端
            List<string> lst = new List<string>();
            SqlCommand cmd4 = new SqlCommand("SELECT COUNT (*) FROM USER_TBL, STATUS_TBL " +
                                                "WHERE USER_TBL.USER_ID = STATUS_TBL.USER_ID " +
                                                "AND STATUS = 0; " +
                                            "SELECT COUNT(*) FROM USER_TBL, STATUS_TBL " +
                                                "WHERE USER_TBL.USER_ID = STATUS_TBL.USER_ID " +
                                                "AND STATUS < 0 " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM USER_TBL " +
                                                "WHERE ALIVE = 0;" +
                                            "SELECT SUM(SKILL_LIMIT) FROM SKILL_TBL, USER_TBL " +
                                                "WHERE SKILL_ID = 1 " +
                                                "AND USER_TBL.USER_ID = SKILL_TBL.USER_ID " +
                                                "AND ALIVE = 1;" +
                                            "SELECT SUM(BULLET_NUM) FROM BULLET_TBL, USER_TBL " +
                                                "WHERE USER_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT MAX(BULLET_NUM) FROM BULLET_TBL, USER_TBL " +
                                                "WHERE USER_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE(IDEN_1_ID = 1 OR IDEN_1_ID = 2) " +
                                                "AND IDEN_2_ID = 0 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE(IDEN_1_ID = 1 OR IDEN_1_ID = 2) " +
                                                "AND IDEN_2_ID = 0;" +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE(IDEN_1_ID = 4 OR IDEN_1_ID = 7) " +
                                                "AND IDEN_2_ID = 0 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE(IDEN_1_ID = 4 OR IDEN_1_ID = 7) " +
                                                "AND IDEN_2_ID = 0; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE(IDEN_1_ID = 5 OR IDEN_1_ID = 8) " +
                                                "AND IDEN_2_ID = 0 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE(IDEN_1_ID = 5 OR IDEN_1_ID = 8) " +
                                                "AND IDEN_2_ID = 0; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE(IDEN_1_ID = 3 OR IDEN_1_ID = 6) " +
                                                "AND IDEN_2_ID = 0 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE(IDEN_1_ID = 3 OR IDEN_1_ID = 6) " +
                                                "AND IDEN_2_ID = 0; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE IDEN_1_ID = 9 " +
                                                "AND IDEN_2_ID = 0 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE IDEN_1_ID = 9 " +
                                                "AND IDEN_2_ID = 0; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE IDEN_2_ID = 2 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE IDEN_2_ID = 2; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE IDEN_2_ID = 3 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE IDEN_2_ID = 3; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE IDEN_2_ID = 4 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE IDEN_2_ID = 4; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL, USER_TBL " +
                                                "WHERE IDEN_2_ID = 1 " +
                                                "AND IDEN_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM IDEN_TBL " +
                                                "WHERE IDEN_2_ID = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 1 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 5 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 5; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 3 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 3; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 2 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 2; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 6 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 6; " +
                                            "SELECT COUNT(*) FROM JOB_TBL, USER_TBL " +
                                                "WHERE JOB_ID = 4 " +
                                                "AND JOB_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND ALIVE = 1; " +
                                            "SELECT COUNT(*) FROM JOB_TBL " +
                                                "WHERE JOB_ID = 4", conn);
            using (SqlDataReader reader = cmd4.ExecuteReader())
            {
                do
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            lst.Add(reader[0].ToString());
                        }
                    }
                } while (reader.NextResult());
            }
            datas["healthy_num"] = lst[0];
            datas["dying_num"] = lst[1];
            datas["dead_num"] = lst[2];
            datas["doctor_skill"] = lst[3];
            datas["bullet_num"] = lst[4];
            datas["max_bullet_num"] = lst[5];
            datas["cop_num_alive"] = lst[6];
            datas["cop_num_total"] = lst[7];
            datas["x_gang_num_alive"] = lst[8];
            datas["x_gang_num_total"] = lst[9];
            datas["y_gang_num_alive"] = lst[10];
            datas["y_gang_num_total"] = lst[11];
            datas["z_gang_num_alive"] = lst[12];
            datas["z_gang_num_total"] = lst[13];
            datas["normal_num_alive"] = lst[14];
            datas["normal_num_total"] = lst[15];
            datas["cop_king_num_alive"] = lst[16];
            datas["cop_king_num_total"] = lst[17];
            datas["gang_king_num_alive"] = lst[18];
            datas["gang_king_num_total"] = lst[19];
            datas["normal_king_num_alive"] = lst[20];
            datas["normal_king_num_total"] = lst[21];
            datas["spy_num_alive"] = lst[22];
            datas["spy_num_total"] = lst[23];
            datas["doctor_num_alive"] = lst[24];
            datas["doctor_num_total"] = lst[25];
            datas["warrior_num_alive"] = lst[26];
            datas["warrior_num_total"] = lst[27];
            datas["detect_num_alive"] = lst[28];
            datas["detect_num_total"] = lst[29];
            datas["hunter_num_alive"] = lst[30];
            datas["hunter_num_total"] = lst[31];
            datas["thief_num_alive"] = lst[32];
            datas["thief_num_total"] = lst[33];
            datas["trader_num_alive"] = lst[34];
            datas["trader_num_total"] = lst[35];

            //取出游戏目前状况：开始/暂停/禁枪
            int state = 0;
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
            
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["message"] = messages;
            resultJson["info"] = infos;
            resultJson["history"] = historys;
            resultJson["data"] = datas;
            resultJson["state"] = state;
            context.Response.Write(resultJson.ToJson());
            conn.Close();//关闭数据库

        }
    }
}