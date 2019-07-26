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
    /// MethodHandler 的摘要说明
    /// 负责暗杀，处决，转赠，治疗，侦察，偷窃的主动技能逻辑；被动技（i.e.闪）的逻辑也被包含
    /// </summary>
    public class MethodHandler : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {

            context.Response.ContentType = "text/plain";
            string user1 = "";
            string user1Name = "";
            HttpContext contextid = HttpContext.Current;
            if (contextid.Session["user_id"] == null || contextid.Session["user_name"] == null) context.Response.Write(0);
            else
            {
                user1 = contextid.Session["user_id"].ToString();
                user1Name = contextid.Session["user_name"].ToString();
            }
            string method = context.Request["method"];
            string user2 = context.Request["user"];
            string user2_ = "";
            //post输入的user2需要进行参数校验
            if (pub.QueryCheck(user2) != "ok" || pub.QueryCheck(method) != "ok") context.Response.Write("illegal");
            else
            {
                user2_ = pub.QuotedStr(user2);
            }

            switch (method)
            {
                case "处决":
                    execute(user1, user2, user1Name);
                    break;
                case "暗杀":
                    assassinate(user1, user2, user1Name);
                    break;
                case "子弹转赠":
                    gift(user1, user2, user1Name);
                    break;
                case "侦查":
                    detect(user1, user2, user1Name);
                    break;
                case "偷窃":
                    steal(user1, user2, user1Name);
                    break;
                case "治疗":
                    cure(user1, user2, user1Name);
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
        /// 处理处决的逻辑
        /// </summary>
        /// <param name="user1">发动处决的人的id，由后端session取出</param>
        /// <param name="user2">被处决的人的id，由前端post</param>
        /// <param name="user1Name">发动处决的人的名字字符串，session中取出，用于信息发送</param>
        private void execute(string user1, string user2, string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT JOB_ID FROM JOB_TBL WHERE USER_ID =" + user1 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID =" + user1 +
                ";SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT JOB_ID FROM JOB_TBL WHERE USER_ID =" + user2 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID=" + user2 +
                ";SELECT STATUS FROM STATUS_TBL WHERE USER_ID =" + user2 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT DIETIME FROM COUNTDOWN_TBL WHERE USER_ID =" + user2 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
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
            string user2Name = lst[2];
            // 如果职业是商人 或子弹数小于零 或user1已死亡 或游戏处于停止或禁枪期：报错
            if (int.Parse(lst[0]) == 4 || int.Parse(lst[1]) <= 0 || int.Parse(lst[6]) == 0 || int.Parse(lst[8]) == 0 || int.Parse(lst[8]) == -1)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                //先将user1的子弹数减去一
                SqlCommand cmd1Bullet = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM -= 1 WHERE USER_ID=" + user1 +
                                                       "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                cmd1Bullet.ExecuteNonQuery();
                // 如果user2已经死亡
                if (int.Parse(lst[9]) == 0)
                {
                    SqlCommand msg1 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                 "VALUES(" + user1 + ", 0, '处决失败', '你刚刚消耗子弹，对" + user2Name + "鞭尸', '" + msgTime + "')" +
                                 ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                 "VALUES( '[" + lst[10] + "] " + user1Name + "对 [" + lst[11] + "] " + user2Name + "发动了一次处决，但目标已死亡', '" + msgTimeLong + "')", conn);
                    msg1.ExecuteNonQuery();
                    context.Response.Write("ok");
                    conn.Close();
                }
                // 如果user2 还活着
                else
                {
                    //给user1发成功信息: 你成功对【user2Name】发动了一次处决； 给管理员发处决信息
                    SqlCommand msg1 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                     "VALUES(" + user1 + ", 0, '处决成功', '你成功对" + user2Name + "发动了一次处决', '" + msgTime + "')" +
                                                     ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                     "VALUES( '[" + lst[10] + "] " + user1Name + "对 [" + lst[11] + "] " + user2Name + "发动了一次处决', '" + msgTimeLong + "')", conn);
                    msg1.ExecuteNonQuery();
                    context.Response.Write("ok");
                    //判断user2的身份是否是武道家，id=5
                    if (int.Parse(lst[3]) == 5)
                    {
                        //判断武道家技能点是否剩余
                        int warrior_skill_limit = 0;
                        SqlCommand cmdskill = new SqlCommand("SELECT SKILL_LIMIT FROM SKILL_TBL WHERE SKILL_ID = 9 AND USER_ID =" + user2, conn);
                        using (SqlDataReader reader = cmdskill.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    warrior_skill_limit = reader.GetInt32(reader.GetOrdinal("SKILL_LIMIT"));
                                }
                            }
                        }
                        //有剩余技能，反向处决
                        if (warrior_skill_limit > 0)
                        {
                            //先给user2加一颗子弹，减去一点闪技能点
                            SqlCommand cmd2user2 = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM += 1 WHERE USER_ID=" + user2
                                    + ";UPDATE SKILL_TBL SET SKILL_LIMIT -= 1 WHERE SKILL_ID = 9 AND USER_ID=" + user2, conn);
                            cmd2user2.ExecuteNonQuery();
                            //给user2发送：你受到了一次来自【user1Name】的处决，发动了武道家技能：一闪。豁免了本次伤害，并对【user1Name】反向发动了一次处决；给管理员发送技能发动信息
                            SqlCommand msg2 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                             "VALUES(" + user2 + ", 0, '受到处决', '你受到了一次来自" + user1Name + "的处决，发动了武道家技能：一闪。豁免了本次伤害，并反向发动了一次处决', '" + msgTime + "')" +
                                                             ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                             "VALUES( '[" + lst[11] + "] " + user2Name + "受到处决，发动一闪技能', '" + msgTimeLong + "')" +
                                                            ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                             "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到" + user1Name +"的处决，被动发动技能一闪，豁免本次伤害并反向发动了一次处决', 0)", conn);
                            msg2.ExecuteNonQuery();
                            execute(user2, user1, user2Name);
                        }
                        //没有剩余技能，user2扣血
                        else
                        {
                            //user2 status减一
                            SqlCommand cmd2Status = new SqlCommand("UPDATE STATUS_TBL SET STATUS -= 1 WHERE USER_ID=" + user2, conn);
                            cmd2Status.ExecuteNonQuery();
                            //给user2发送：你受到了一次来自【user1Name】的处决，进入了濒死状态，点击主页的状态按钮查看更多信息
                            SqlCommand msg3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                             "VALUES(" + user2 + ", 0, '受到处决', '你受到了一次来自" + user1Name + "的处决，点击状态按钮查看详情', '" + msgTime + "')" +
                                                             ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                            "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到" + user1Name + "的处决，点击状态按钮查看详情', 0)", conn);
                            msg3.ExecuteNonQuery();
                            //如果user2的倒计时是null，开始1800濒死倒计时；给管理员发送濒死信息
                            if (lst[7] == "")
                            {
                                SqlCommand count = new SqlCommand("UPDATE COUNTDOWN_TBL SET DIETIME = 1800 WHERE USER_ID = " + user2 +
                                                                  ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                                    "VALUES( '[" + lst[11] + "] " + user2Name + "进入濒死','" + msgTimeLong + "')", conn);
                                count.ExecuteNonQuery();
                            }
                        }
                    }
                    //不是武道家,user2扣血
                    else
                    {
                        //user2 status减一
                        SqlCommand cmd2Status = new SqlCommand("UPDATE STATUS_TBL SET STATUS -= 1 WHERE USER_ID=" + user2, conn);
                        cmd2Status.ExecuteNonQuery();
                        //给user2发送：你受到了一次来自【user1Name】的处决，进入了濒死状态，点击主页的状态按钮查看更多信息
                        SqlCommand msg4 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                        "VALUES(" + user2 + ", 0, '受到处决', '你受到了一次来自" + user1Name + "的处决，点击状态按钮查看详情', '" + msgTime + "')" +
                                                         ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                        "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到" + user1Name + "的处决，点击状态按钮查看详情', 0)", conn);

                        msg4.ExecuteNonQuery();
                        //如果user2的倒计时是null，开始1800濒死倒计时；开始濒死倒计时
                        if (lst[7] == "")
                        {
                            SqlCommand count = new SqlCommand("UPDATE COUNTDOWN_TBL SET DIETIME = 1800 WHERE USER_ID = " + user2 +
                                                              ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                              "VALUES('[" + lst[11] + "] " + user2Name + "进入濒死','" + msgTimeLong + "')", conn);
                            count.ExecuteNonQuery();
                        }
                    }
                    conn.Close();

                }
            }
        }
        /// <summary>
        /// 负责暗杀的逻辑
        /// </summary>
        /// <param name="user1">发动暗杀的人的id，由后端session取出</param>
        /// <param name="user2">被暗杀的人的id，由前端post</param>
        private void assassinate(string user1, string user2, string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd = new SqlCommand("SELECT JOB_ID FROM JOB_TBL WHERE USER_ID =" + user1 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID =" + user1 +
                ";SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT JOB_ID FROM JOB_TBL WHERE USER_ID =" + user2 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID=" + user2 +
                ";SELECT STATUS FROM STATUS_TBL WHERE USER_ID =" + user2 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT DIETIME FROM COUNTDOWN_TBL WHERE USER_ID =" + user2 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
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
            string user2Name = lst[2];
            // 如果职业是商人 id=4,或子弹数小于零 或user1已死亡 或游戏没开始 或处于禁枪期：报错
            if (int.Parse(lst[0]) == 4 || int.Parse(lst[1]) <= 0 || int.Parse(lst[6]) == 0 || int.Parse(lst[8]) == 0 || int.Parse(lst[8]) == -1)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                SqlCommand cmd1Bullet = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM -= 1 WHERE USER_ID=" + user1 +
                                                       "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                cmd1Bullet.ExecuteNonQuery();
                // 如果user2已经死亡
                if (int.Parse(lst[9]) == 0)
                {
                    SqlCommand msg1 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                 "VALUES(" + user1 + ", 0, '暗杀失败', '你刚刚消耗子弹，对" + user2Name + "鞭尸', '" + msgTime + "')" +
                                 ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                 "VALUES( '[" + lst[10] + "] " + user1Name + "对 [" + lst[11] + "] " + user2Name + "发动了一次暗杀，但目标已死亡', '" + msgTimeLong + "')", conn);
                    msg1.ExecuteNonQuery();
                    context.Response.Write("ok");
                    conn.Close();
                }
                // 如果user2 还活着
                else
                {
                    //开始抛硬币判断
                    Random coin = new Random();
                    int result = coin.Next(2);
                    //抛硬币为1，或者职业是赏金猎人 id=2，暗杀成功
                    if (result == 1 || int.Parse(lst[0]) == 2)
                    {
                        //给user1发成功信息: 你成功对【user2Name】发动了一次暗杀;给管理员发送成功暗杀信息
                        SqlCommand msg1 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES(" + user1 + ", 0, '暗杀成功', '你成功对" + user2Name + "发动了一次暗杀', '" + msgTime + "')" +
                                                         ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES('[" + lst[10] + "] " + user1Name + "对 [" + lst[11] + "] " + user2Name + "成功发动了一次暗杀', '" + msgTimeLong + "')", conn);
                        msg1.ExecuteNonQuery();
                        context.Response.Write("ok");
                        //判断user2的身份是否是武道家，id=5
                        if (int.Parse(lst[3]) == 5)
                        {
                            //判断武道家技能点是否剩余
                            int warrior_skill_limit = 0;
                            SqlCommand cmdskill = new SqlCommand("SELECT SKILL_LIMIT FROM SKILL_TBL WHERE SKILL_ID = 9 AND USER_ID =" + user2, conn);
                            using (SqlDataReader reader = cmdskill.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        warrior_skill_limit = reader.GetInt32(reader.GetOrdinal("SKILL_LIMIT"));
                                    }
                                }
                            }
                            //有剩余技能，反向处决
                            if (warrior_skill_limit > 0)
                            {
                                //先给user2加一颗子弹，减去一点闪技能点
                                SqlCommand cmd2user2 = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM += 1WHERE USER_ID=" + user2
                                        + ";UPDATE SKILL_TBL SET SKILL_LIMIT -=1 WHERE SKILL_ID = 9 AND USER_ID=" + user2, conn);
                                cmd2user2.ExecuteNonQuery();
                                //给user2发送：你受到了一次暗杀，发动了武道家技能：一闪。豁免了本次伤害，并反向发动了一次处决；给管理员发送技能发动信息
                                SqlCommand msg2 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                                    "VALUES(" + user2 + ", 0, '受到暗杀', '你受到了一次暗杀，发动了武道家技能：一闪。豁免了本次伤害，并反向发动了一次处决', '" + msgTime + "')" +
                                                                 ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                                    "VALUES('[" + lst[11] + "] " + user2Name + "受到暗杀，发动一闪技能', '" + msgTimeLong + "')" +
                                                                 ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                                    "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到暗杀，被动发动技能一闪，豁免本次伤害并反向发动了一次处决', 0)", conn);

                                msg2.ExecuteNonQuery();
                                execute(user2, user1, user2Name);
                            }
                            //没有剩余技能，user2扣血
                            else
                            {
                                //user2 status减一
                                SqlCommand cmd2Status = new SqlCommand("UPDATE STATUS_TBL SET STATUS -=1 WHERE USER_ID=" + user2, conn);
                                cmd2Status.ExecuteNonQuery();
                                //给user2发送：你受到了一次暗杀，进入了濒死状态，点击主页的状态按钮查看更多信息
                                SqlCommand msg3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                                    "VALUES(" + user2 + ", 0, '受到暗杀', '你受到了一次暗杀，点击主页的状态按钮查看更多信息', '" + msgTime + "')" +
                                                                 ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                                    "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到暗杀，点击状态按钮查看详情', 0)", conn);

                                msg3.ExecuteNonQuery();
                                //如果user2的倒计时是null，开始1800濒死倒计时；开始濒死倒计时
                                if (lst[7] == "")
                                {
                                    SqlCommand count = new SqlCommand("UPDATE COUNTDOWN_TBL SET DIETIME = 1800 WHERE USER_ID = " + user2 +
                                                                      ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                                      "VALUES( '[" + lst[11] + "] " + user2Name + "进入濒死','" + msgTimeLong + "')", conn);
                                    count.ExecuteNonQuery();
                                }
                            }
                        }
                        //不是武道家,扣血
                        else
                        {
                            //user2 status减一
                            SqlCommand cmd2Status = new SqlCommand("UPDATE STATUS_TBL SET STATUS -=1 WHERE USER_ID=" + user2, conn);
                            cmd2Status.ExecuteNonQuery();
                            //给user2发送：你受到了一次暗杀，进入了濒死状态，点击主页的状态按钮查看更多信息
                            SqlCommand msg4 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                                 "VALUES(" + user2 + ", 0, '受到暗杀', '你受到了一次暗杀，点击主页的状态按钮查看更多信息', '" + msgTime + "')" +
                                                            ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                                 "VALUES(" + user2 + ",'" + msgTimeLong + "','你受到暗杀，点击主页的状态按钮查看更多信息', 0)", conn);

                            msg4.ExecuteNonQuery();
                            //如果user2的倒计时是null，开始1800濒死倒计时；开始濒死倒计时
                            if (lst[7] == "")
                            {
                                SqlCommand count = new SqlCommand("UPDATE COUNTDOWN_TBL SET DIETIME = 1800 WHERE USER_ID = " + user2 +
                                                                  ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                                  "VALUES( '[" + lst[11] + "] " + user2Name + "进入濒死','" + msgTimeLong + "')", conn);
                                count.ExecuteNonQuery();
                            }
                        }

                    }
                    //抛硬币为0，而且不是赏金猎人，暗杀失败
                    if (result == 0 && int.Parse(lst[0]) != 2)
                    {
                        //给user1发送：“你对【user2Name】的暗杀失败了。”
                        SqlCommand msg5 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES(" + user1 + ", 0, '暗杀失败', '你对" + user2Name + "发动的暗杀失败了', '" + msgTime + "')" +
                                                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES( '[" + lst[10] + "] " + user1Name + "对 [" + lst[11] + "] " + user2Name + "发动了一次暗杀，暗杀失败', '" + msgTimeLong + "')", conn);
                        msg5.ExecuteNonQuery();

                        context.Response.Write("ok");
                    }
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 负责子弹转赠的逻辑
        /// </summary>
        /// <param name="user1">给予子弹的人的id，由后端session取出</param>
        /// <param name="user2">被给予的人的id，由前端post</param>
        private void gift(string user1, string user2, string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd1 = new SqlCommand("SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID =" + user1 +
                ";SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID=" + user2 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
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
            string user2Name = lst[1];
            // 如果子弹数小于零 或user1已死亡 或游戏尚未开始：报错
            if (int.Parse(lst[0]) <= 0 || int.Parse(lst[3]) == 0 || int.Parse(lst[4]) == 0)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                act.ExecuteNonQuery();
                //如果user2已经死亡
                if (int.Parse(lst[5]) == 0)
                {
                    SqlCommand msg1 = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM -=1 WHERE USER_ID=" + user1 +
                         ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                            "VALUES(" + user1 + ", 0, '转赠子弹失败', '死者" + user2Name + "无法接收子弹，你把子弹弄丢了', '" + msgTime + "')" +
                         ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                            "VALUES('[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行子弹转赠，但目标已死亡', '" + msgTimeLong + "')", conn);
                    msg1.ExecuteNonQuery();
                    context.Response.Write("ok");
                    conn.Close();
                }
                else
                {
                    SqlCommand cmd2 = new SqlCommand("UPDATE BULLET_TBL SET BULLET_NUM -=1 WHERE USER_ID=" + user1 +
                        ";UPDATE BULLET_TBL SET BULLET_NUM +=1 WHERE USER_ID=" + user2 +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '转赠子弹成功', '你成功赠送了" + user2Name + "一颗子弹', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '收到子弹赠送', '有人赠送了你一颗子弹', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES('[" + lst[6] + "] " + user1Name + "给 [" + lst[7] + "] " + user2Name + "转赠了子弹', '" + msgTimeLong + "')", conn);
                    cmd2.ExecuteNonQuery();
                    context.Response.Write("ok");
                    conn.Close();
                }
            }
        }
        /// <summary>
        /// 负责侦探侦查技能 skill_id=2 的逻辑，比较复杂不容易维护，需要注意侦察到的第一身份和第二身份的id的伪装情况
        /// </summary>
        /// <param name="user1">发动侦查的侦探的id，必须侦探，由后端session取出</param>
        /// <param name="user2">被侦查的人的id，由前端post</param>
        private void detect (string user1, string user2,string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd1 = new SqlCommand("SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT SKILL_LIMIT FROM SKILL_TBL WHERE SKILL_ID = 2 AND USER_ID =" + user1 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT JOB_LIST_TBL.JOB_NAME FROM JOB_TBL, JOB_LIST_TBL WHERE JOB_TBL.JOB_ID = JOB_LIST_TBL.JOB_ID AND USER_ID =" + user2 +
                ";SELECT IDEN_1_ID FROM IDEN_TBL WHERE USER_ID =" + user2 +
                ";SELECT IDEN_2_ID FROM IDEN_TBL WHERE USER_ID =" + user2 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
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
            string user2Name = lst[0];
            string user2JobName = lst[3];
            int user2Iden1Id = int.Parse(lst[4]);
            int user2Iden2Id = int.Parse(lst[5]);
            string user2IdenName = "";
            // 如果侦查技能点小于零或user1已死亡：报错
            if (int.Parse(lst[1]) <= 0 || int.Parse(lst[2]) == 0 || int.Parse(lst[6]) == 0)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                //user1技能点减一
                SqlCommand cmdskill = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -=1 WHERE SKILL_ID = 2 AND USER_ID =" + user1 +
                                                     "UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                cmdskill.ExecuteNonQuery();
                //如果user2已经死亡
                if (int.Parse(lst[7]) == 0)
                {
                    //如果user2没有第二身份
                    if (user2Iden2Id == 0)
                    {
                        SqlCommand cmd = new SqlCommand("SELECT IDEN_1_NAME " +
                                                            "FROM IDEN_TBL, IDEN_1_LIST_TBL " +
                                                            "WHERE IDEN_TBL.IDEN_1_ID = IDEN_1_LIST_TBL.IDEN_1_ID AND USER_ID =" + user2, conn);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    user2IdenName = reader.GetString(reader.GetOrdinal("IDEN_1_NAME"));
                                }
                            }
                        }

                        SqlCommand cmd2 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】,目标已死亡', '" + msgTime + "')" +
                            ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                               "VALUES('[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，查到真实身份', '" + msgTimeLong + "')", conn);
                        cmd2.ExecuteNonQuery();
                        context.Response.Write("ok");
                    }
                    //如果user2有第二身份
                    else
                    {
                        SqlCommand cmd = new SqlCommand("SELECT IDEN_2_NAME " +
                                    "FROM IDEN_TBL, IDEN_2_LIST_TBL " +
                                    "WHERE IDEN_TBL.IDEN_2_ID = IDEN_2_LIST_TBL.IDEN_2_ID AND USER_ID =" + user2, conn);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    user2IdenName = reader.GetString(reader.GetOrdinal("IDEN_2_NAME"));
                                }
                            }
                        }

                        SqlCommand cmd2 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】,目标已死亡', '" + msgTime + "')" +
                            ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                               "VALUES( '[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，查到真实身份', '" + msgTimeLong + "')", conn);
                        cmd2.ExecuteNonQuery();
                    }
                    conn.Close();
                    context.Response.Write("ok");
                }
                else
                {
                    //如果user2是派系卧底 id2 =1， 派系老大 id1= 678. 或派系里的王 id2=3
                    if (user2Iden2Id == 1 || user2Iden2Id == 3 || user2Iden1Id == 6 || user2Iden1Id == 7 || user2Iden1Id == 8)
                    {
                        user2IdenName = "派系成员";
                        SqlCommand cmd = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '受到侦查', '有人对你进行了一次侦查，你成功伪装了自己的身份', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES( '[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，但只查到假身份', '" + msgTimeLong + "')", conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();//关闭数据库
                        context.Response.Write("ok");
                    }
                    //如果user2是行动组组长 id1 = 1 或行动组里的王 id2=2
                    else if (user2Iden1Id == 1 || user2Iden2Id == 2)
                    {
                        user2IdenName = "特别行动组组员";
                        SqlCommand cmd = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '受到侦查', '有人对你进行了一次侦查，你成功伪装了自己的身份', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES( '[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，但只查到假身份', '" + msgTimeLong + "')", conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();//关闭数据库
                        context.Response.Write("ok");
                    }
                    //如果user2是平民里的王 id2=4
                    else if (user2Iden2Id == 4)
                    {
                        user2IdenName = "平民";
                        SqlCommand cmd = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '受到侦查', '有人对你进行了一次侦查，你成功伪装了自己的身份', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES( '[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，但只查到假身份', '" + msgTimeLong + "')", conn);
                        cmd.ExecuteNonQuery();
                        conn.Close();//关闭数据库
                        context.Response.Write("ok");
                    }
                    else
                    {
                        SqlCommand cmd = new SqlCommand("SELECT IDEN_1_NAME, IDEN_1_DESC, IDEN_1_CARD_SRC " +
                            "FROM IDEN_TBL, IDEN_1_LIST_TBL " +
                            "WHERE IDEN_TBL.IDEN_1_ID = IDEN_1_LIST_TBL.IDEN_1_ID AND USER_ID =" + user2, conn);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    user2IdenName = reader.GetString(reader.GetOrdinal("IDEN_1_NAME"));
                                }
                            }
                        }
                        SqlCommand cmd2 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES(" + user1 + ", 0, '侦查成功','" + user2Name + "的职业为【" + user2JobName + "】身份为【" + user2IdenName + "】', '" + msgTime + "')" +
                                                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES( '[" + lst[8] + "] " + user1Name + "对 [" + lst[9] + "] " + user2Name + "进行了侦查，查到真实身份', '" + msgTimeLong + "')", conn);
                        cmd2.ExecuteNonQuery();
                        context.Response.Write("ok");
                    }
                    conn.Close();

                }
            }

        }
        /// <summary>
        /// 负责小偷偷窃技能 skill_id=3 的逻辑
        /// </summary>
        /// <param name="user1">发动偷窃的小偷的id，由后端session取出</param>
        /// <param name="user2">被偷窃的人的id，由前端post</param>
        private void steal(string user1, string user2, string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd1 = new SqlCommand("SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID =" + user1 +
                ";SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID=" + user2 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT SKILL_LIMIT FROM SKILL_TBL WHERE SKILL_ID = 3 AND USER_ID =" + user1 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
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
            string user2Name = lst[1];
            // 如果偷窃技能点小于零或user1已死亡：报错
            if (int.Parse(lst[4]) <= 0 || int.Parse(lst[3]) == 0 || int.Parse(lst[5]) == 0)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                act.ExecuteNonQuery();
                //如果user2没有子弹，则发送失败信息
                if (int.Parse(lst[2]) <= 0)
                {
                    SqlCommand cmd2 = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -=1 WHERE SKILL_ID = 3 AND USER_ID=" + user1 +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '偷窃失败', '" + user2Name + "身上没有任何子弹，本次偷窃失败且消耗技能值', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '受到偷窃', '有人对你尝试了一次偷窃，但你身上没有子弹，没有生效', '" + msgTime + "')"+
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES( '[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行了偷窃，但是因为没有子弹失败', '" + msgTimeLong + "')", conn);
                    cmd2.ExecuteNonQuery();
                    context.Response.Write("ok");
                }
                //如果user2有子弹，执行偷窃
                else
                {
                    SqlCommand cmd2 = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -= 1 WHERE SKILL_ID = 3 AND USER_ID=" + user1 +
                        ";UPDATE BULLET_TBL SET BULLET_NUM += 1 WHERE USER_ID=" + user1 +
                        ";UPDATE BULLET_TBL SET BULLET_NUM -= 1 WHERE USER_ID=" + user2 +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '偷窃成功', '你成功从" + user2Name + "那里窃取了一颗子弹', '" + msgTime + "')" +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user2 + ", 0, '受到偷窃', '有人从你身上窃取了一枚子弹', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES('[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行了成功偷窃', '" + msgTimeLong + "')", conn);
                    cmd2.ExecuteNonQuery();
                    context.Response.Write("ok");
                }
                conn.Close();
            }

        }
        /// <summary>
        /// 负责医生治疗技能 skill_id=1 的逻辑
        /// </summary>
        /// <param name="user1"></param>
        /// <param name="user2"></param>
        private void cure (string user1, string user2, string user1Name)
        {
            List<string> lst = new List<string>();
            HttpContext context = HttpContext.Current;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd1 = new SqlCommand("SELECT STATUS FROM STATUS_TBL WHERE USER_ID =" + user2 +
                ";SELECT NAME FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user1 +
                ";SELECT SKILL_LIMIT FROM SKILL_TBL WHERE SKILL_ID = 1 AND USER_ID =" + user1 +
                ";SELECT SWITCH FROM GAME_PROCESS_TBL WHERE PROCESS_NAME = 'STATE'" +
                ";SELECT ALIVE FROM USER_TBL WHERE USER_ID =" + user2 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user1 +
                ";SELECT INCL_NAME FROM INCL_TBL, INCL_LIST_TBL WHERE INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID AND USER_ID = " + user2, conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
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
            string user2Name = lst[1];
            // 如果偷窃技能点小于零或user1已死亡：报错
            if (int.Parse(lst[3]) <= 0 || int.Parse(lst[2]) == 0 || int.Parse(lst[4]) == 0)
            {
                context.Response.Write("no");
                conn.Close();//关闭数据库
            }
            else
            {
                string msgTime = DateTime.Now.ToShortTimeString().ToString();
                string msgTimeLong = DateTime.Now.ToLocalTime().ToString();
                SqlCommand act = new SqlCommand("UPDATE USER_ACTION_TIME_TBL SET TIME = '" + msgTimeLong + "' WHERE USER_ID = " + user1, conn);
                act.ExecuteNonQuery();
                //如果user2已经死亡
                if (int.Parse(lst[5]) == 0)
                {
                    SqlCommand cmd2 = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -=1 WHERE SKILL_ID = 1 AND USER_ID=" + user1 +
                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                           "VALUES(" + user1 + ", 0, '治疗失败', '消耗技能值检查完" + user2Name + "后，你认为对方已经没救了', '" + msgTime + "')" +
                        ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                           "VALUES( '[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行了治疗，但是目标已经死亡', '" + msgTimeLong + "')", conn);
                    cmd2.ExecuteNonQuery();
                    conn.Close();
                    context.Response.Write("ok");
                }
                //如果user2还活着
                else
                {
                    //如果user2 的status等于零，治疗失败，给user11返回成功信息，user2返回失败信息
                    if (int.Parse(lst[0]) == 0)
                    {
                        SqlCommand cmd2 = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -=1 WHERE SKILL_ID = 1 AND USER_ID=" + user1 +
                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user1 + ", 0, '治疗失败', '检查完毕，" + user2Name + "身体健康，消耗技能值', '" + msgTime + "')" +
                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user2 + ", 0, '受到治疗', '有人对你使用了一次治疗，但由于当前身体健康，没有发生任何作用', '" + msgTime + "')" +
                            ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                               "VALUES( '[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行了治疗，但因为身体健康失败', '" + msgTimeLong + "')", conn);
                        cmd2.ExecuteNonQuery();
                        context.Response.Write("ok");
                    }
                    //如果user2 的status 小于零，治疗成功
                    else
                    {
                        int user2_new_status = int.Parse(lst[0]) + 1;
                        SqlCommand cmd2 = new SqlCommand("UPDATE SKILL_TBL SET SKILL_LIMIT -= 1 WHERE SKILL_ID = 1 AND USER_ID=" + user1 +
                            ";UPDATE STATUS_TBL SET STATUS =" + user2_new_status + "WHERE USER_ID=" + user2 +
                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user1 + ", 0, '治疗成功', '你成功对" + user2Name + "使用了一次治疗', '" + msgTime + "')" +
                            ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                               "VALUES(" + user2 + ", 0, '受到治疗', '有人对你使用了一个医疗包，恢复了一点受伤值', '" + msgTime + "')" +
                            ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                               "VALUES('[" + lst[6] + "] " + user1Name + "对 [" + lst[7] + "] " + user2Name + "进行了成功治疗', '" + msgTimeLong + "')" +
                            ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                               "VALUES(" + user2 + ",'" + msgTimeLong + "','有人对你使用了一个医疗包，恢复了一点受伤值', 0)", conn);
                        cmd2.ExecuteNonQuery();
                        //如果user2的status已回归0，则给user2发送健康信息，并将user2的濒死倒计时重新设置为null
                        if (user2_new_status == 0)
                        {
                            SqlCommand cmd3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                                                                "VALUES(" + user2 + ", 0, '恢复健康', '你已经恢复健康，濒死时间清空', '" + msgTime + "')" +
                                                                "UPDATE COUNTDOWN_TBL SET DIETIME = NULL WHERE USER_ID = " + user2 +
                                                             ";INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                                "VALUES( '[" + lst[7] + "] " + user2Name + "脱离濒死，恢复健康', '" + msgTimeLong + "')", conn);
                            cmd3.ExecuteNonQuery();
                        }
                        context.Response.Write("ok");
                    }
                    conn.Close();
                }
            }

        }
    }
}