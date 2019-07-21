using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlClient;

namespace CountDown
{
    class Program
    {
        static void Main(string[] args)
        {
            AutoTask();
        }
        static private void AutoTask ()
        {
            int duration = 0;
            while (true)
            {
                SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
                conn.Open();//打开数据库
                //取出state用于判断游戏进程，0为停止，1为正常开始，-1为开始但是处于禁枪期
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
                conn.Close();
                //如果游戏开始，死亡倒计时开启，每2秒减一次时间
                if(state == 1 || state == -1)
                {
                    duration = duration + 2;
                    //给濒死的人减去两秒倒计时
                    DeathCountDown();
                    //如果是正常开始，则执行王转移判定（禁枪期王不能转移）
                    if(state == 1)
                    {
                        KingTransfer();
                    }
                    PutToDeath();
                    //每到整点，给技能添加点数，给商人添加子弹
                    DateTime dt = System.DateTime.Now;
                    if (dt.ToString("mm:ss") == "00:00"|| dt.ToString("mm:ss") == "00:01")
                    {
                        AddSkillLimit();
                        AddBullet();
                    }

                }

                Thread.Sleep(2000);
            }
        }
        /// <summary>
        /// 判断有无处于濒死的人，若有则扣两秒倒计时
        /// </summary>
        private static void DeathCountDown()
        {
            string currentTime = DateTime.Now.ToLocalTime().ToString();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库

            //当前濒死的人数
            int dyingPeopleNum = 0;
            SqlCommand cmd1 = new SqlCommand("SELECT COUNT (*) A FROM COUNTDOWN_TBL, USER_TBL " +
                                                "WHERE USER_TBL.USER_ID = COUNTDOWN_TBL.USER_ID " +
                                                "AND DIETIME IS NOT NULL " +
                                                "AND USER_TBL.ALIVE = 1", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        dyingPeopleNum = reader.GetInt32(reader.GetOrdinal("A"));
                    }
                }
            }

            if (dyingPeopleNum > 0)
            {
                List<int> IDlst20 = new List<int>();
                List<int> IDlst10 = new List<int>();
                //先给所有活着的且有倒计时的人减去两秒倒计时
                SqlCommand cmd2 = new SqlCommand("UPDATE COUNTDOWN_TBL SET DIETIME -= 2 " +
                                                    "FROM COUNTDOWN_TBL, USER_TBL " +
                                                    "WHERE USER_TBL.USER_ID = COUNTDOWN_TBL.USER_ID " +
                                                    "AND DIETIME IS NOT NULL " +
                                                    "AND USER_TBL.ALIVE = 1 ", conn);
                cmd2.ExecuteNonQuery();
                //选择1200秒 20分钟倒计时的人 发送提示
                SqlCommand cmd3 = new SqlCommand("SELECT USER_ID FROM COUNTDOWN_TBL WHERE DIETIME = 1200 OR DIETIME = 1199", conn);
                using (SqlDataReader reader = cmd3.ExecuteReader())
                {
                    do
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                IDlst20.Add(reader.GetInt32(reader.GetOrdinal("USER_ID")));
                            }
                        }
                    } while (reader.NextResult());
                }
                for (int i = 0; i < IDlst20.Count(); i++)
                {
                    SqlCommand cmd4 = new SqlCommand("INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                        "VALUES(" + IDlst20[i] + ",'" + currentTime + "','你的濒死倒计时剩余20分钟',0)", conn);
                    cmd4.ExecuteNonQuery();
                }

                //选择600秒 10分钟倒计时的人 发送提示
                SqlCommand cmd5 = new SqlCommand("SELECT USER_ID FROM COUNTDOWN_TBL WHERE DIETIME = 600 OR DIETIME = 599", conn);
                using (SqlDataReader reader = cmd5.ExecuteReader())
                {
                    do
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                IDlst10.Add(reader.GetInt32(reader.GetOrdinal("USER_ID")));
                            }
                        }
                    } while (reader.NextResult());
                }
                for (int i = 0; i < IDlst10.Count(); i++)
                {
                    SqlCommand cmd6 = new SqlCommand("INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                        "VALUES(" + IDlst10[i] + ",'" + currentTime + "','你的濒死倒计时剩余10分钟',0)", conn);
                    cmd6.ExecuteNonQuery();
                }

                conn.Close();
                Console.WriteLine(currentTime + " 濒死倒计时完成" + dyingPeopleNum + "人处于濒死");
            }
            else
            {
                Console.WriteLine(currentTime + " 濒死倒计时完成无人濒死");
                conn.Close();
            }
        }
        /// <summary>
        /// 判断有无倒计时结束的人，若有则将alive改为死亡；同时统计当前死亡的王牌数量，调用KingTransfer函数并传入死亡王牌数量
        /// </summary>
        private static void PutToDeath()
        {
            List<string> Namelst = new List<string>();
            List<string> Incllst = new List<string>();
            List<int> IDlst = new List<int>();
            string currentTime = DateTime.Now.ToLocalTime().ToString();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            SqlCommand cmd1 = new SqlCommand("SELECT USER_TBL.NAME,USER_TBL.USER_ID, INCL_NAME FROM COUNTDOWN_TBL, USER_TBL, INCL_TBL, INCL_LIST_TBL " +
                                                "WHERE USER_TBL.USER_ID = COUNTDOWN_TBL.USER_ID " +
                                                "AND INCL_TBL.INCL_ID = INCL_LIST_TBL.INCL_ID " +
                                                "AND INCL_TBL.USER_ID = USER_TBL.USER_ID " +
                                                "AND COUNTDOWN_TBL.DIETIME <= 0 " +
                                                "AND USER_TBL.ALIVE = 1 ", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                do
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Namelst.Add(reader.GetString(reader.GetOrdinal("NAME")));
                            IDlst.Add(reader.GetInt32(reader.GetOrdinal("USER_ID")));
                            Incllst.Add(reader.GetString(reader.GetOrdinal("INCL_NAME")));
                        }
                    }
                } while (reader.NextResult());
            }
            for (int i=0; i<Namelst.Count(); i++)
            {
                SqlCommand cmd2 = new SqlCommand("INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                    "VALUES( '[" + Incllst[i] + "] " + Namelst[i] + "死亡', '" + currentTime + "')" +
                                                ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                    "VALUES(" + IDlst[i] + ",'" + currentTime + "','你的五王之乱角色已经死亡，无法对该角色进行任何操作，但你仍然可以通过线上交流影响游戏进程，游戏结束后你仍然可以获得生存条件以外的得分', 0)", conn);
                cmd2.ExecuteNonQuery();
            }

            //把要死亡的人的alive值改为0
            SqlCommand cmd3 = new SqlCommand("UPDATE USER_TBL SET ALIVE = 0 " +
                                                "FROM COUNTDOWN_TBL, USER_TBL " +
                                                "WHERE USER_TBL.USER_ID = COUNTDOWN_TBL.USER_ID " +
                                                "AND COUNTDOWN_TBL.DIETIME <= 0 " +
                                                "AND USER_TBL.ALIVE = 1 ", conn);

            int dead = cmd3.ExecuteNonQuery();
            if (dead > 0)
            {
                Console.WriteLine(currentTime + " 死亡判定完成 " + dead + " 人死亡");
            }
            conn.Close();
        }
        /// <summary>
        /// 自动添加技能点数，给达到技能点上限的人发送失败信息
        /// </summary>
        private static void AddSkillLimit()
        {
            List<string> Addlst = new List<string>();
            List<string> Faillst = new List<string>();
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            string currentTime = DateTime.Now.ToLocalTime().ToString();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库

            //收集当前需要添加技能点的人的id值
            SqlCommand cmd1 = new SqlCommand("SELECT USER_TBL.USER_ID " +
                                    "FROM SKILL_TBL, SKILL_LIST_TBL, USER_TBL " +
                                    "WHERE SKILL_TBL.SKILL_ID = SKILL_LIST_TBL.SKILL_ID " +
                                    "AND SKILL_LIST_TBL.LIMIT_ON = 1 " +
                                    "AND USER_TBL.ALIVE = 1 " +
                                    "AND USER_TBL.USER_ID = SKILL_TBL.USER_ID " +
                                    "AND SKILL_LIMIT < 2", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Addlst.Add(reader[0].ToString());
                    }
                }
            }

            for (int i=0; i<Addlst.Count(); i++)
            {
                SqlCommand cmd3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                    "VALUES(" + Addlst[i] + ", 0, '技能点补充', '系统你的主动技能添补一点技能点，每小时添补一次', '" + msgTime + "')", conn);
                cmd3.ExecuteNonQuery();
            }

            //收集当前由于技能点达上限添加技能点失败的人的id值
            SqlCommand cmd2 = new SqlCommand("SELECT USER_TBL.USER_ID " +
                                    "FROM SKILL_TBL, SKILL_LIST_TBL, USER_TBL " +
                                    "WHERE SKILL_TBL.SKILL_ID = SKILL_LIST_TBL.SKILL_ID " +
                                    "AND SKILL_LIST_TBL.LIMIT_ON = 1 " +
                                    "AND USER_TBL.ALIVE = 1 " +
                                    "AND USER_TBL.USER_ID = SKILL_TBL.USER_ID " +
                                    "AND SKILL_LIMIT >= 2", conn);
            using (SqlDataReader reader = cmd2.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        Faillst.Add(reader[0].ToString());
                    }
                }
            }

            for (int i = 0; i < Faillst.Count(); i++)
            {
                SqlCommand cmd3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                    "VALUES(" + Faillst[i] + ", 0, '技能点补充', '系统你的主动技能添补一点技能点，由于你技能点已达上限，添补失败', '" + msgTime + "')", conn);
                cmd3.ExecuteNonQuery();
            }

            //执行添加技能点
            SqlCommand cmd4 = new SqlCommand("UPDATE A SET " +
                                    "SKILL_LIMIT += 1 " +
                                    "FROM SKILL_TBL A, SKILL_LIST_TBL, USER_TBL " +
                                    "WHERE A.SKILL_ID = SKILL_LIST_TBL.SKILL_ID " +
                                    "AND SKILL_LIST_TBL.LIMIT_ON = 1 " +
                                    "AND USER_TBL.ALIVE = 1 " +
                                    "AND USER_TBL.USER_ID = a.USER_ID " +
                                    "AND SKILL_LIMIT < 2; ", conn);
            cmd4.ExecuteNonQuery();

            SqlCommand cmd5 = new SqlCommand("INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                     "VALUES( '技能点发放" + Addlst.Count()  + "人成功" + Faillst.Count() + "人失败','" + currentTime + "')", conn);
            cmd5.ExecuteNonQuery();
            conn.Close();
            Console.WriteLine(currentTime + " 技能点添加成功" + Addlst.Count() + "人，已达上限失败" + Faillst.Count() + "人");
        }
        /// <summary>
        /// 给商人添加随机的一或者二颗子弹
        /// </summary>
        private static void AddBullet()
        {
            string currentTime = DateTime.Now.ToLocalTime().ToString();
            List<string> lst = new List<string>();
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库

            //收集所有活着的商人的id值
            SqlCommand cmd1 = new SqlCommand("SELECT USER_TBL.USER_ID FROM USER_TBL, JOB_TBL " +
                                             "WHERE USER_TBL.USER_ID = JOB_TBL.USER_ID AND JOB_TBL.JOB_ID = 4 AND USER_TBL.ALIVE = 1", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        lst.Add(reader[0].ToString());
                    }
                }
            }

            Random coin = new Random();
            int result = coin.Next(2);
            //抛硬币为1，添加两颗子弹
            if (result == 1)
            {
                SqlCommand cmd2 = new SqlCommand("UPDATE BULLET_TBL SET " +
                                                "BULLET_NUM += 2 " +
                                                "FROM BULLET_TBL, JOB_TBL, USER_TBL " +
                                                "WHERE JOB_TBL.JOB_ID = 4 " +
                                                "AND JOB_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND USER_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND USER_TBL.ALIVE = 1", conn);
                cmd2.ExecuteNonQuery();

                for(int i = 0; i < lst.Count(); i++)
                {
                    SqlCommand cmd3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                        "VALUES(" + lst[i] + ", 0, '资源补充', '系统自动为你添补两颗子弹，每小时添补一次', '" + msgTime + "')", conn);
                    cmd3.ExecuteNonQuery();
                }
            }
            //不然，添加一颗子弹
            else
            {
                SqlCommand cmd2 = new SqlCommand("UPDATE BULLET_TBL SET " +
                                                "BULLET_NUM += 1 " +
                                                "FROM BULLET_TBL, JOB_TBL, USER_TBL " +
                                                "WHERE JOB_TBL.JOB_ID = 4 " +
                                                "AND JOB_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND USER_TBL.USER_ID = BULLET_TBL.USER_ID " +
                                                "AND USER_TBL.ALIVE = 1", conn);
                cmd2.ExecuteNonQuery();

                for (int i = 0; i < lst.Count(); i++)
                {
                    SqlCommand cmd3 = new SqlCommand("INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME)" +
                        "VALUES(" + lst[i] + ", 0, '资源补充', '系统自动为你添补一颗子弹，每小时添补一次', '" + msgTime + "')", conn);
                    cmd3.ExecuteNonQuery();

                }
            }
            SqlCommand cmd4 = new SqlCommand("INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                "VALUES( '子弹发放，已为" + lst.Count() + "个商人成功发放子弹','" + currentTime + "')", conn);
            cmd4.ExecuteNonQuery();
            conn.Close();
            Console.WriteLine(currentTime + " 已为" + lst.Count() + "个商人添加子弹");
        }
        /// <summary>
        /// 随机找到一个平民，将其身份改为王牌
        /// </summary>
        private static void KingTransfer ()
        {
            int dyingKing = 0;
            string msgTime = DateTime.Now.ToShortTimeString().ToString();
            string currentTime = DateTime.Now.ToLocalTime().ToString();
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            //统计出即将要死亡的王的数量
            SqlCommand cmd1 = new SqlCommand("SELECT COUNT (*) A FROM COUNTDOWN_TBL, USER_TBL, IDEN_TBL " +
                                                "WHERE USER_TBL.USER_ID = COUNTDOWN_TBL.USER_ID " +
                                                "AND USER_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND COUNTDOWN_TBL.DIETIME <= 0 " +
                                                "AND USER_TBL.ALIVE = 1 " +
                                                "AND(IDEN_TBL.IDEN_2_ID = 2 " +
                                                "OR IDEN_TBL.IDEN_2_ID = 3 " +
                                                "OR IDEN_TBL.IDEN_2_ID = 4)", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        dyingKing = reader.GetInt32(reader.GetOrdinal("A"));
                    }
                }
            }
            //如果有死亡的王，开始转移
            if (dyingKing > 0)
            {
                List<string> Normallst = new List<string>();

                SqlCommand cmd2 = new SqlCommand("SELECT USER_TBL.USER_ID FROM USER_TBL, IDEN_TBL " +
                                                    "WHERE USER_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                    "AND IDEN_TBL.IDEN_1_ID = 9 " +
                                                    "AND IDEN_TBL.IDEN_2_ID = 0 " +
                                                    "AND USER_TBL.ALIVE = 1", conn);
                using (SqlDataReader reader = cmd2.ExecuteReader())
                {
                    do
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Normallst.Add(reader[0].ToString());
                            }
                        }
                    } while (reader.NextResult());
                }

                if (Normallst.Count() < dyingKing)
                {
                    SqlCommand cmd = new SqlCommand("INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                    "VALUES( '平民数小于需要的王的数量，王转移失败','" + currentTime + "')", conn);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine(currentTime + " 王传递失败，当前平民数小于需要的王的数量");
                }
                else
                {
                    for (int i = 0; i < dyingKing; i++)
                    {
                        Random lottery = new Random();
                        int result = lottery.Next(Normallst.Count());
                        string name = "";
                        SqlCommand cmd3 = new SqlCommand("SELECT NAME FROM USER_TBL WHERE USER_ID = " + Normallst[result], conn);
                        using (SqlDataReader reader = cmd3.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    name = reader.GetString(reader.GetOrdinal("NAME"));
                                }
                            }
                        }

                        SqlCommand cmd = new SqlCommand("INSERT INTO ADMIN_MSGLINE_TBL(MSG_CONTENT, MSG_TIME)" +
                                                            "VALUES( '" + name + "成为新的王','" + currentTime + "')", conn);
                        cmd.ExecuteNonQuery();

                        //将选中用户的1.id2，2.倾向，3.技能，4.原有技能全部更新，并且发送消息
                        SqlCommand cmd4 = new SqlCommand("UPDATE IDEN_TBL SET IDEN_2_ID = 4, IDEN_2_CARD_SRC = '大王2' " +
                                                            "WHERE USER_ID = " + Normallst[result] +
                                                        ";UPDATE INCL_TBL SET INCL_ID = 1 " +
                                                            "WHERE USER_ID = " + Normallst[result] +
                                                        ";INSERT INTO SKILL_TBL (USER_ID, SKILL_ID, SKILL_LIMIT) " +
                                                            "VALUES(" + Normallst[result] + ", 12, 0 ) " +
                                                        ";INSERT INTO SKILL_TBL (USER_ID, SKILL_ID, SKILL_LIMIT) " +
                                                            "VALUES(" + Normallst[result] + ", 24, 0 ) " +
                                                        ";INSERT INTO SKILL_TBL (USER_ID, SKILL_ID, SKILL_LIMIT) " +
                                                            "VALUES(" + Normallst[result] + ", 25, 0 )" +
                                                        ";DELETE FROM SKILL_TBL " +
                                                            "WHERE SKILL_ID = 23 AND USER_ID = " + Normallst[result] +
                                                        ";INSERT INTO USER_MESSAGE_TBL(USER_ID, MSG_IS_READ, MSG_TITLE, MSG_CONTENT, MSG_TIME) " +
                                                            "VALUES(" + Normallst[result] + ", 0, '王牌传递', '你被随机选为新的王，你的身份和倾向已转变', '" + msgTime + "')" +
                                                        ";INSERT INTO ADMIN_MSG_TBL(USER_ID, TIME, MSG_CONTENT, IS_PROCESSED) " +
                                                            "VALUES(" + Normallst[result] + ",'" + currentTime + "','你被随机选为新的王，你的身份和倾向已转变', 0)", conn);
                        cmd4.ExecuteNonQuery();
                        Console.WriteLine(currentTime + " 王牌传递已发生");
                    }
                }
            }
            conn.Close();
        }
    }
}
