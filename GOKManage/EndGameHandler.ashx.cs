using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace GOKManage
{
    /// <summary>
    /// EndGameHandler 的摘要说明
    /// </summary>
    public class EndGameHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string kings = context.Request["kings"];
            switch (kings)
            {
                case "fail":
                    calculateResult(0);
                    break;
                case "success":
                    calculateResult(1);
                    break;
            }
            context.Response.Write("ok");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        private void calculateResult (int king)
        {
            int kingSuccess = king;
            int xGangBoss = 0;
            int yGangBoss = 0;
            int zGangBoss = 0;
            int copBoss = 0;
            int userNum = 0;
            SqlConnection conn = new SqlConnection("server=.;database=GOK;uid=admin1;pwd=GOK2019");
            conn.Open();//打开数据库
            //判断应许派是否存在，若不存在默认老大存活
            SqlCommand cmdy = new SqlCommand("SELECT COUNT (*) AS NUM FROM IDEN_TBL WHERE IDEN_1_ID = 8", conn);
            using (SqlDataReader reader = cmdy.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        int yNum = reader.GetInt32(reader.GetOrdinal("NUM"));
                        if(yNum == 0)
                        {
                            yGangBoss = 0;
                        }
                    }
                }
            }
            //判断兄弟派老大是否存活
            SqlCommand cmd1 = new SqlCommand("SELECT STATUS AS XNUM FROM STATUS_TBL, IDEN_TBL " +
                                                "WHERE STATUS_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND IDEN_TBL.IDEN_1_ID = 7", conn);
            using (SqlDataReader reader = cmd1.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        xGangBoss = reader.GetInt32(reader.GetOrdinal("XNUM"));
                    }
                }
            }
            //判断应许派老大是否存活
            SqlCommand cmd2 = new SqlCommand("SELECT STATUS AS YNUM FROM STATUS_TBL, IDEN_TBL " +
                                                "WHERE STATUS_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND IDEN_TBL.IDEN_1_ID = 8", conn);
            using (SqlDataReader reader = cmd2.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        yGangBoss = reader.GetInt32(reader.GetOrdinal("YNUM"));
                    }
                }
            }
            //判断止戈派老大是否存活
            SqlCommand cmd3 = new SqlCommand("SELECT STATUS AS ZNUM FROM STATUS_TBL, IDEN_TBL " +
                                                "WHERE STATUS_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND IDEN_TBL.IDEN_1_ID = 6", conn);
            using (SqlDataReader reader = cmd3.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        zGangBoss = reader.GetInt32(reader.GetOrdinal("ZNUM"));
                    }
                }
            }
            //判断警察老大是否存活
            SqlCommand cmd4 = new SqlCommand("SELECT STATUS AS CNUM FROM STATUS_TBL, IDEN_TBL " +
                                                "WHERE STATUS_TBL.USER_ID = IDEN_TBL.USER_ID " +
                                                "AND IDEN_TBL.IDEN_1_ID = 1", conn);
            using (SqlDataReader reader = cmd4.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        copBoss = reader.GetInt32(reader.GetOrdinal("CNUM"));
                    }
                }
            }
            //当前游戏有多少用户
            SqlCommand cmd5 = new SqlCommand("SELECT COUNT(*) AS USERNUM FROM USER_TBL", conn);
            using (SqlDataReader reader = cmd5.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        userNum = reader.GetInt32(reader.GetOrdinal("USERNUM"));
                    }
                }
            }

             for(int i = 1; i <= userNum; i++)
            {
                int incl = 0;
                int result = 0;
                int alive = 0;
                //检查当前用户的倾向
                SqlCommand cmd6 = new SqlCommand("SELECT INCL_ID FROM INCL_TBL WHERE USER_ID = " + i, conn);
                using (SqlDataReader reader = cmd6.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            incl = reader.GetInt32(reader.GetOrdinal("INCL_ID"));
                        }
                    }
                }
                //如果始终没有登陆选择倾向，默认倾向为中立
                if(incl == 0)
                {
                    incl = 4;
                }
                //检查当前用户是否存活
                SqlCommand cmd7 = new SqlCommand("SELECT STATUS FROM STATUS_TBL WHERE USER_ID = " + i, conn);
                using (SqlDataReader reader = cmd7.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            alive = reader.GetInt32(reader.GetOrdinal("STATUS"));
                        }
                    }
                }
                //王：1.会合成功，2.存活
                if (incl == 1)
                {
                    if (kingSuccess == 1)
                    {
                        result += 60;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'促成五王汇合事件：60分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 40;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：40分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //正义：1.会合失败 2.存活 3.警长存活 4.三老大死亡
                else if (incl == 2)
                {
                    if (kingSuccess == 0)
                    {
                        result += 30;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'阻止五王汇合事件：30分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (copBoss == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'特别行动组组长存活：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (zGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'止戈派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (yGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'应许派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (xGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'兄弟派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //邪恶：1，会合失败，2.存活，3.警长死亡，4.至少一个黑老大存活
                else if (incl == 3)
                {
                    if (kingSuccess == 0)
                    {
                        result += 30;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'阻止五王汇合事件：30分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 30;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：30分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (copBoss != 0)
                    {
                        result += 30;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'特别行动组组长死亡：30分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (xGangBoss == 0 || yGangBoss == 0 || zGangBoss == 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'至少有一个派系老大存活：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //中立：1.存活，2.子弹10分1颗
                else if (incl == 4)
                {
                    int bulletNum = 0;
                    int bulletScore = 0;
                    if (alive == 0)
                    {
                        result += 50;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：50分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    //子弹10分1颗，上限为5颗
                    SqlCommand bullet = new SqlCommand("SELECT BULLET_NUM FROM BULLET_TBL WHERE USER_ID = " + i, conn);
                    using (SqlDataReader reader = bullet.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                bulletNum = reader.GetInt32(reader.GetOrdinal("BULLET_NUM"));
                            }
                        }
                    }
                    if(bulletNum <= 5)
                    {
                        bulletScore = bulletNum * 10;
                    }
                    else
                    {
                        bulletScore = 50;
                    }
                    result += bulletScore;
                    SqlCommand cmd9 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'拥有子弹加分：" + bulletScore + "分')", conn);
                    cmd9.ExecuteNonQuery();
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //应许派：1.会合失败 2.存活 3.自己老大存活 4.警长死亡 5.另外两个老大死亡
                else if(incl == 5)
                {
                    if (kingSuccess == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'阻止五王汇合事件：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (copBoss != 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'特别行动组组长死亡：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (yGangBoss == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'应许派老大存活：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (zGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'止戈派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (xGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'兄弟派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //兄弟派：1.会合失败 2.存活 3.自己老大存活 4.警长死亡 5.另外两个老大死亡
                else if (incl == 6)
                {
                    if (kingSuccess == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'阻止五王汇合事件：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (copBoss != 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'特别行动组组长死亡：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (xGangBoss == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'兄弟派老大存活：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (zGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'止戈派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (yGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'应许派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
                //止戈派：1.会合失败 2.存活 3.自己老大存活 4.警长死亡 5.另外两个老大死亡
                else if (incl == 7)
                {
                    if (kingSuccess == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'阻止五王汇合事件：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (alive == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'活到最后：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (copBoss != 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'特别行动组组长死亡：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (zGangBoss == 0)
                    {
                        result += 20;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'止戈派老大存活：20分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (xGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'兄弟派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    if (yGangBoss != 0)
                    {
                        result += 10;
                        SqlCommand cmd8 = new SqlCommand("INSERT INTO RESULT_LOG_TBL (USER_ID, LOG) VALUES (" + i + ",'应许派老大死亡：10分')", conn);
                        cmd8.ExecuteNonQuery();
                    }
                    SqlCommand score = new SqlCommand("INSERT INTO RESULT_SCORE_TBL (USER_ID, SCORE) VALUES (" + i + "," + result + ")", conn);
                    score.ExecuteNonQuery();
                }
            }
            SqlCommand end = new SqlCommand("UPDATE GAME_PROCESS_TBL SET SWITCH = -2 WHERE PROCESS_NAME = 'STATE'", conn);
            end.ExecuteNonQuery();
            conn.Close();
        }
    }
}