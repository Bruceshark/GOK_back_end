using System;
using System.IO;
using System.Web;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;

namespace GOKManage
{
    public class pub
    {
        /// <summary>
        /// sql参数校验，防止sql注入和xss注入；默认global
        /// </summary>
        /// <param name="input">输入的字符串</param>
        public static string QueryCheck(string input)
        {
            return QueryCheck(input, "global");
        }
        /// <summary>
        /// sql参数校验，防止sql注入和xss注入
        /// </summary>
        /// <param name="input">输入的字符串</param>
        /// <param name="rigour">校验的严格程度，可选：global（防sql注入）；strict（防二次sql和xss注入）</param>
        public static string QueryCheck(string input, string rigour)
        {
            string result = "ok";
            //非法字符表，不包含单引号；单引号仅替换
            string[,] illegalChars = new string[28, 3] {
                {"0x","global","请勿输入16进制标识"},
                {";","global","请勿输入半角分号"},
                {"%","strict","请勿输入百分号"},
                {"<","strict","请勿输入半角书名号"},
                {">","strict","请勿输入半角书名号"},
                {"--","global","请勿输入双减号"},
                {"/*","global","请勿输入注释符"},
                {"*/","global","请勿输入注释符"},
                {"select","strict","请勿输入单词“select”"},
                {"exec","strict","请勿输入单词“exec”"},
                {"update","strict","请勿输入单词“update”"},
                {"create","strict","请勿输入单词“create”"},
                {"drop","strict","请勿输入单词“drop”"},
                {"delete","strict","请勿输入单词“delete”"},
                {"truncate","strict","请勿输入单词“truncate”"},
                {"insert","strict","请勿输入单词“insert”"},
                {"waitfor","strict","请勿输入单词“waitfor”"},
                {"alter","strict","请勿输入单词“alter”"},
                {"grant","strict","请勿输入单词“grant”"},
                {"or","strict","请勿输入单词“or”"},
                {"union","strict","请勿输入单词“union”"},
                {"xp_cmdshell","global","请合法输入参数"},
                {"openrowset","global","请合法输入参数"},
                {"sp_configure","global","请合法输入参数"},
                {"reconfigure","global","请合法输入参数"},
                {"bulk","global","请合法输入参数"},
                {"localgroup","global","请合法输入参数"},
                {"administrator","global","请合法输入参数"}
            };
            for (int i = 0; i < illegalChars.GetUpperBound(0) + 1; i++)
            {
                if ((rigour.Equals("strict") || rigour.Equals(illegalChars[i, 1])) && ContainsLower(input, illegalChars[i, 0]))
                {
                    //严格模式下检查所有关键字；全局模式下检查全局模式关键字
                    result = illegalChars[i, 2];
                    break;
                }
            }
            return result;
        }
        /// <summary>
        /// sql字符串替换-防止sql注入并加单引号
        /// </summary>
        /// <param name="input">输入的字符串</param>
        public static string QuotedStr(string input)
        {
            if (input == null)
                return "''";
            else
                return "'" + input.Replace("'", "''").Replace("--", "") + "'";
        }
        /// <summary>
        /// Contains的忽略大小写检验法
        /// </summary>
        /// <param name="source">输入的字符串</param>
        /// <param name="toCheck">检查的部分</param>
        public static bool ContainsLower(string source, string toCheck)
        {
            return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// MD5
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string md5(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
            byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));
            StringBuilder sBuilder = new StringBuilder();
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("X2"));
            }
            return sBuilder.ToString();
        }
    }
}