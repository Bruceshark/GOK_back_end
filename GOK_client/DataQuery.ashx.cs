using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Web.SessionState;
using LitJson;

namespace GOK_client
{
    /// <summary>
    /// DataQuery 的摘要说明：
    /// 用于向前端页面发送json数据，通过判断get接受的page值来调用不同的函数发送请求
    /// </summary>
    public class DataQuery : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string id = "";
            string name = "";
            HttpContext contextid = HttpContext.Current;
            if (contextid.Session["user_id"] == null) context.Response.Write(0);
            else
            {
                name = contextid.Session["user_name"].ToString();
                id = contextid.Session["user_id"].ToString();
            }

            string page = context.Request["page"];
            switch (page)
            {
                case "firstCome":
                    firstCome(id);
                    break;
                case "usermain":
                    usermain(id);
                    break;
                case "mainPolling":
                    mainPolling(id);
                    break;
                case "id_1_detail":
                    id_1_detail(id, name);
                    break;
                case "id_2_detail":
                    id_2_detail(id, name);
                    break;
                case "inclDetail":
                    inclDetail(id);
                    break;
                case "jobDetail":
                    jobDetail(id);
                    break;
                case "people":
                    people(id);
                    break;
                case "shootChoose":
                    shootChoose(id);
                    break;
                case "skillChoose":
                    skillChoose(id);
                    break;
                case "statusDetail":
                    statusDetail(id);
                    break;
                case "messageDetail":
                    messageDetail(id);
                    break;
                case "statusPolling":
                    statusPolling(id);
                    break;
                case "inclChoose":
                    inclChoose(id);
                    break;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 给usermain页面传递相对不会改变的数据 i.e.用户第二身份id，职业名，倾向
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void usermain(string id)
        {
            List<string> lst = new List<string>();
            int inclId = 0;
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            //先读取倾向，如果倾向为0，说明尚未选择
            SqlCommand incl = new SqlCommand("SELECT INCL_ID FROM INCL_TBL WHERE USER_ID =" + id, conn);
            using (SqlDataReader reader = incl.ExecuteReader())
            {
                do
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            inclId = reader.GetInt32(reader.GetOrdinal("INCL_ID"));
                        }
                    }
                } while (reader.NextResult());
            }
            if (inclId == 0)
            {
                context.Response.Write("inclNone");
            }
            else
            {
                SqlCommand cmd = new SqlCommand("SELECT IDEN_2_ID FROM IDEN_TBL WHERE USER_ID =" + id +
                                                ";SELECT JOB_LIST_TBL.JOB_NAME FROM JOB_TBL, JOB_LIST_TBL " +
                                                    "WHERE JOB_TBL.JOB_ID = JOB_LIST_TBL.JOB_ID AND USER_ID =" + id +
                                                ";SELECT INCL_LIST_TBL.INCL_NAME FROM INCL_TBL, INCL_LIST_TBL " +
                                                    "WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID =" + id +
                                                ";SELECT COUNT (*) FROM SKILL_TBL WHERE USER_ID =" + id +
                                                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID =" + id, conn);
                using (SqlDataReader reader = cmd.ExecuteReader())
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
                JsonData resultJson = new JsonData();
                resultJson.SetJsonType(JsonType.Object);
                resultJson["iden_2_id"] = lst[0];
                resultJson["job_name"] = lst[1];
                resultJson["incl_name"] = lst[2];
                resultJson["skill_num"] = lst[3];
                resultJson["bullet_num"] = lst[4];
                context.Response.Write(resultJson.ToJson());
            }
            conn.Close();//关闭数据库
     
        }
        /// <summary>
        /// 给usermain页面传递变化比较大的数据 i.e.用户血量状态，用户未读消息数量, 用户生命状态
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void mainPolling (string id)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT STATUS FROM STATUS_TBL WHERE USER_ID =" + id +
                                ";SELECT COUNT (*) FROM USER_MESSAGE_TBL WHERE MSG_IS_READ = 0 AND USER_ID =" + id +
                                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
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
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["status"] = lst[0];
            resultJson["msg_count"] = lst[1];
            resultJson["is_alive"] = lst[2];
            context.Response.Write(resultJson.ToJson());
            conn.Close();//关闭数据库
        }
        /// <summary>
        /// 给用户的第一身份详情页面传递数据 i.e.身份名称，身份描述，身份所对应扑克牌
        /// </summary>
        /// <param name="id">当前用户id</param>
        /// <param name="name">当前用户名，原封不动传回详情页用于显示</param>
        private void id_1_detail(string id, string name)
        {
            string iden_1_name="";
            string iden_1_desc="";
            string iden_1_card_src="";
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT IDEN_1_NAME, IDEN_1_DESC, IDEN_1_CARD_SRC " +
                "FROM IDEN_TBL, IDEN_1_LIST_TBL " +
                "WHERE IDEN_TBL.IDEN_1_ID = IDEN_1_LIST_TBL.IDEN_1_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        iden_1_name = reader.GetString(reader.GetOrdinal("IDEN_1_NAME"));
                        iden_1_desc = reader.GetString(reader.GetOrdinal("IDEN_1_DESC"));
                        iden_1_card_src = reader.GetString(reader.GetOrdinal("IDEN_1_CARD_SRC"));
                    }
                }
            }

            var skills = new JsonData();
            SqlCommand cmdskill = new SqlCommand("SELECT SKILL_LIST_TBL.SKILL_NAME, SKILL_DESC FROM IDEN_1_SKILL_TBL, IDEN_TBL, SKILL_LIST_TBL WHERE IDEN_1_SKILL_TBL.IDEN_1_ID = IDEN_TBL.IDEN_1_ID AND IDEN_1_SKILL_TBL.SKILL_ID = SKILL_LIST_TBL.SKILL_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmdskill.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var skill = new JsonData();
                        skill["name"] = reader.GetString(reader.GetOrdinal("SKILL_NAME"));
                        skill["desc"] = reader.GetString(reader.GetOrdinal("SKILL_DESC"));
                        skill.ToJson();
                        skills.Add(skill);
                    }
                }
            }

            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["name"] = name;
            resultJson["iden_1_name"] = iden_1_name;
            resultJson["iden_1_desc"] = iden_1_desc;
            resultJson["iden_1_card_src"] = iden_1_card_src;
            resultJson["id_1_skill"] = skills;

            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 给用户的第二身份详情页面传递数据 i.e.身份名称，身份描述，身份所对应扑克牌
        /// </summary>
        /// <param name="id">当前用户id</param>
        /// <param name="name">当前用户名，原封不动传回详情页用于显示</param>
        private void id_2_detail(string id, string name)
        {
            string iden_2_name = "";
            string iden_2_desc = "";
            string iden_2_card_src = "";
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT IDEN_2_NAME, IDEN_2_DESC, IDEN_2_CARD_SRC " +
                "FROM IDEN_TBL, IDEN_2_LIST_TBL " +
                "WHERE IDEN_TBL.IDEN_2_ID = IDEN_2_LIST_TBL.IDEN_2_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        iden_2_name = reader.GetString(reader.GetOrdinal("IDEN_2_NAME"));
                        iden_2_desc = reader.GetString(reader.GetOrdinal("IDEN_2_DESC"));
                        iden_2_card_src = reader.GetString(reader.GetOrdinal("IDEN_2_CARD_SRC"));
                    }
                }
            }

            var skills = new JsonData();
            SqlCommand cmdskill = new SqlCommand("SELECT SKILL_LIST_TBL.SKILL_NAME, SKILL_DESC FROM IDEN_2_SKILL_TBL, IDEN_TBL, SKILL_LIST_TBL WHERE IDEN_2_SKILL_TBL.IDEN_2_ID = IDEN_TBL.IDEN_2_ID AND IDEN_2_SKILL_TBL.SKILL_ID = SKILL_LIST_TBL.SKILL_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmdskill.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var skill = new JsonData();
                        skill["name"] = reader.GetString(reader.GetOrdinal("SKILL_NAME"));
                        skill["desc"] = reader.GetString(reader.GetOrdinal("SKILL_DESC"));
                        skill.ToJson();
                        skills.Add(skill);
                    }
                }
            }

            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["name"] = name;
            resultJson["iden_2_name"] = iden_2_name;
            resultJson["iden_2_desc"] = iden_2_desc;
            resultJson["iden_2_card_src"] = iden_2_card_src;
            resultJson["id_2_skill"] = skills;

            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 给用户的倾向详情页面传递数据 i.e.倾向名称，倾向描述，胜利条件
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void inclDetail(string id)
        {
            string incl_name = "";
            string incl_desc = "";
            string incl_condition = "";
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT INCL_LIST_TBL.INCL_NAME, INCL_LIST_TBL.incl_desc, INCL_LIST_TBL.INCL_CONDITION " +
                "FROM INCL_TBL, INCL_LIST_TBL  " +
                "WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        incl_name = reader.GetString(reader.GetOrdinal("INCL_NAME"));
                        incl_desc = reader.GetString(reader.GetOrdinal("INCL_DESC"));
                        incl_condition = reader.GetString(reader.GetOrdinal("INCL_CONDITION"));
                    }
                }
            }
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["incl_name"] = incl_name;
            resultJson["incl_desc"] = incl_desc;
            resultJson["incl_condition"] = incl_condition;
            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 给用户职业详情页面传递数据 i.e.职业名称，职业描述，职业头像路径
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void jobDetail(string id)
        {
            string job_name = "";
            string job_desc = "";
            string job_src = "";
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT JOB_LIST_TBL.JOB_NAME, JOB_LIST_TBL.JOB_DESC, JOB_LIST_TBL.SRC " +
                "FROM JOB_TBL, JOB_LIST_TBL  " +
                "WHERE JOB_TBL.JOB_ID = JOB_LIST_TBL.JOB_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        job_name = reader.GetString(reader.GetOrdinal("JOB_NAME"));
                        job_desc = reader.GetString(reader.GetOrdinal("JOB_DESC"));
                        job_src = reader.GetString(reader.GetOrdinal("SRC"));
                    }
                }
            }

            var skills = new JsonData();
            SqlCommand cmdskill = new SqlCommand("SELECT SKILL_LIST_TBL.SKILL_NAME, SKILL_DESC FROM JOB_SKILL_TBL, JOB_TBL, SKILL_LIST_TBL WHERE JOB_SKILL_TBL.JOB_ID = JOB_TBL.JOB_ID AND JOB_SKILL_TBL.SKILL_ID = SKILL_LIST_TBL.SKILL_ID AND USER_ID =" + id, conn);
            using (SqlDataReader reader = cmdskill.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var skill = new JsonData();
                        skill["name"] = reader.GetString(reader.GetOrdinal("SKILL_NAME"));
                        skill["desc"] = reader.GetString(reader.GetOrdinal("SKILL_DESC"));
                        skill.ToJson();
                        skills.Add(skill);
                    }
                }
            }

            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["job_name"] = job_name;
            resultJson["job_desc"] = job_desc;
            resultJson["src"] = job_src;
            resultJson["job_skill"] = skills;

            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 给技能和射击所调出来的用户列表页传输数据 i.e.每一个用户的名字和id
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void people(string id)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            var people = new JsonData();
            SqlCommand cmd = new SqlCommand("SELECT USER_ID, NAME " +
                "FROM USER_TBL " +
                "WHERE USER_ID !=" + id, conn);  //注意：取出人名时不能包括用户自己
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var person = new JsonData();
                        person["name"] = reader.GetString(reader.GetOrdinal("NAME"));
                        person["id"] = reader.GetInt32(reader.GetOrdinal("USER_ID"));
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
        /// <summary>
        /// 给射击模式选择页面传输用户的职业以判断是否是商人，商人不能用暗杀和处决组件显示
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void shootChoose(string id)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT BULLET_NUM, JOB_LIST_TBL.JOB_NAME FROM JOB_TBL, JOB_LIST_TBL, BULLET_TBL " +
                "WHERE JOB_TBL.JOB_ID = JOB_LIST_TBL.JOB_ID " +
                "AND BULLET_TBL.USER_ID = JOB_TBL.USER_ID " +
                "AND JOB_TBL.USER_ID =" + id, conn);
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        resultJson["job_name"] = reader.GetString(reader.GetOrdinal("JOB_NAME"));
                        resultJson["bullet_num"] = reader.GetInt32(reader.GetOrdinal("BULLET_NUM"));
                    }
                }
            }
            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 给用户的技能选择页面传输数据 i.e.每一个技能的名字，是否是主动技，是否存在技能点，技能点剩余，描述
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void skillChoose(string id)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            var skill = new JsonData();
            SqlCommand cmd = new SqlCommand("SELECT SKILL_NAME, ACTIVE_ON, SKILL_LIMIT, SKILL_DESC, LIMIT_ON " +
                "FROM SKILL_LIST_TBL, SKILL_TBL " +
                "WHERE SKILL_LIST_TBL.SKILL_ID = SKILL_TBL.SKILL_ID AND SKILL_TBL.USER_ID =" + id, conn);  
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var singleSkill = new JsonData();
                        singleSkill["name"] = reader.GetString(reader.GetOrdinal("SKILL_NAME"));
                        singleSkill["active_on"] = reader.GetInt32(reader.GetOrdinal("ACTIVE_ON"));
                        singleSkill["limit_on"] = reader.GetInt32(reader.GetOrdinal("LIMIT_ON"));
                        singleSkill["limit_num"] = reader.GetInt32(reader.GetOrdinal("SKILL_LIMIT"));
                        singleSkill["desc"] = reader.GetString(reader.GetOrdinal("SKILL_DESC"));
                        singleSkill.ToJson();
                        skill.Add(singleSkill);
                    }
                }
            }

            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            resultJson["skill"] = skill;
            context.Response.Write(resultJson.ToJson());
            conn.Close();//关闭数据库
        }
        /// <summary>
        /// 给用户状态详情页面传递数据 i.e.血量状态
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void statusDetail(string id)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT STATUS FROM STATUS_TBL WHERE USER_ID =" + id, conn);
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        resultJson["status"] = reader.GetInt32(reader.GetOrdinal("STATUS"));
                    }
                }
            }

            context.Response.Write(resultJson.ToJson());

            conn.Close();//关闭数据库
        }
        /// <summary>
        /// 返回当前用户收到的所有消息
        /// </summary>
        /// <param name="id">当前用户id</param>
        private void messageDetail(string id)
        {
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            var message = new JsonData();
            message.SetJsonType(JsonType.Array);
            SqlCommand cmd = new SqlCommand("SELECT MSG_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME " +
                "FROM USER_MESSAGE_TBL WHERE USER_ID =" + id + "ORDER BY MSG_ID DESC", conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
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
        /// <summary>
        /// 用于给初次界面传输当前玩家的对应扑克牌的地址
        /// </summary>
        /// <param name="id"></param>
        private void firstCome(string id)
        {
            string src1 = "";
            string src2 = "";
            int id2 = 0;
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT IDEN_1_CARD_SRC, IDEN_2_CARD_SRC, IDEN_2_ID " +
                "FROM IDEN_TBL WHERE USER_ID = " + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        src1 = reader.GetString(reader.GetOrdinal("IDEN_1_CARD_SRC"));
                        src2 = reader.GetString(reader.GetOrdinal("IDEN_2_CARD_SRC"));
                        id2 = reader.GetInt32(reader.GetOrdinal("IDEN_2_ID"));
                    }
                }
            }
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            //如果id2存在，就用id2的卡牌地址
            if (id2 != 0)
            {
                resultJson["src"] = src2;
            }
            //不然，就用id1的卡牌地址
            else
            {
                resultJson["src"] = src1;
            }
            conn.Close();//关闭数据库
            context.Response.Write(resultJson.ToJson());
        }
        /// <summary>
        /// 用于给statusdetail页面传输用户的死亡倒计时和是否活着
        /// </summary>
        /// <param name="id"></param>
        private void statusPolling(string id)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT DIETIME FROM COUNTDOWN_TBL WHERE USER_ID =" + id +
                                            ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + id, conn);
            JsonData resultJson = new JsonData();
            resultJson.SetJsonType(JsonType.Object);
            using (SqlDataReader reader = cmd.ExecuteReader())
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
            resultJson["dietime"] = lst[0];
            resultJson["alive"] = lst[1];
            context.Response.Write(resultJson.ToJson());
            conn.Close();//关闭数据库

        }
        /// <summary>
        /// 第一次进入时，判断是否是平民且没有倾向，是否需要倾向选择
        /// </summary>
        /// <param name="id"></param>
        private void inclChoose (string id)
        {
            int id1 = 0;
            int incl = 0;
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT IDEN_1_ID, INCL_ID " +
                                                "FROM IDEN_TBL, INCL_TBL " +
                                                "WHERE IDEN_TBL.USER_ID = INCL_TBL.USER_ID " +
                                                "AND IDEN_TBL.USER_ID =" + id, conn);
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        id1 = reader.GetInt32(reader.GetOrdinal("IDEN_1_ID"));
                        incl = reader.GetInt32(reader.GetOrdinal("INCL_ID"));
                    }
                }
            }
            if (id1 == 9 && incl == 0)
            {
                context.Response.Write(1);
            }
            else
            {
                context.Response.Write(0);
            }
            conn.Close();
        }
    }
}