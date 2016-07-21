using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using 小U;

namespace BLL
{
    class SM3
    {
        public struct sm3_context
        {
            unsafe public fixed ulong total[2];     /*!< number of bytes processed  */
            unsafe public fixed ulong state[8];     /*!< intermediate digest state  */
            unsafe public fixed ulong buffer[64];   /*!< data block being processed */
            unsafe public fixed ulong ipad[64];     /*!< HMAC: inner padding        */
            unsafe public fixed ulong opad[64];     /*!< HMAC: outer padding        */
        }

        [DllImport("SM3DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm3_starts(ref sm3_context ctx);
        [DllImport("SM3DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm3_update(ref sm3_context ctx, Byte[] input, int ilen);
        [DllImport("SM3DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm3_finish(ref sm3_context ctx, Byte[] output);
      
        /// <summary>
        /// 字符串校验
        /// </summary>
        /// <param name="str">需要校验的字符串</param>
        /// <returns></returns>
        public static string stringCheck(string str)
        {
            sm3_context ctx;
            sm3_starts(ref ctx);
            int length = 0;
            string tmpStr = "";
            char[] charArray;
            while (length <= str.Length - 64)
            {
                tmpStr = str.Substring(length, 64);
                length += 64;
                charArray = new char[64];
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    charArray[i] = (char)tmpStr[i];
                }
                Byte[] tmp = new Byte[64];
                for (int i = 0; i < charArray.Length; i++)
                {
                    tmp[i] = (byte)charArray[i];
                }
                sm3_update(ref ctx, tmp, 64);
            }
            if (length < str.Length)
            {
                tmpStr = str.Substring(length, str.Length - length);
                charArray = new char[str.Length - length];
                for (int i = 0; i < tmpStr.Length; i++)
                {
                    charArray[i] = (char)tmpStr[i];
                }
                Byte[] tmp = new Byte[str.Length - length];
                for (int i = 0; i < charArray.Length; i++)
                {
                    tmp[i] = (byte)charArray[i];
                }
                sm3_update(ref ctx, tmp, str.Length - length);
            }

            Byte[] text = new Byte[32];
            sm3_finish(ref ctx, text);
            Byte[] output = new Byte[text.Length];

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != 0)
                {
                    output[i] = text[i];
                }
            }
            char[] chr = new char[text.Length];
            chr = System.Text.Encoding.Default.GetChars(output);
            return new string(chr);
        }

        /// <summary>
        /// 对数据库生成校验
        /// </summary>
        /// <param name="inFile"></param>
        /// <param name="outFile"></param>
        /// <returns></returns>
        public static bool DBCheck(string inFile, string outFile)
        {
            if (!File.Exists(inFile)) return false;
            if (File.Exists(outFile))
                File.Delete(outFile);
            string word = "";
            string tmpFile = Start.panFu + @"\小U安全\\Config\\tepCheckFile1.xus";
            if (File.Exists(tmpFile))
                File.Delete(tmpFile);
            StreamWriter sw = new StreamWriter(tmpFile);
            string con = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + inFile + ";Jet OLEDB:Database Password=" + Start.access_key;
            OleDbConnection AconnStr1 = new OleDbConnection(con);  //设定连接数据库
            string sql1 = "select * from UserList";
            string sql2 = "select * from KeyList";
            string sql3 = "select * from RoleAccessControl";
            OleDbCommand cmd = null;
            OleDbDataReader odr = null;
            string pwd = "";
            try
            {
                AconnStr1.Open();
                //把用户信息的哈希值录入文件
                cmd = new OleDbCommand(sql1, AconnStr1);
                odr = cmd.ExecuteReader();
                while (odr.Read())
                {
                    if (SM4.DeString(odr["UserRole"].ToString()) == "Admin")
                    {
                        pwd = SM4.DeString(odr["UserPwd"].ToString());
                    }
                    word += SM4.DeString(odr["UserName"].ToString()) + SM4.DeString(odr["UserPwd"].ToString()) +SM4.DeString(odr["Question"].ToString()) + SM4.DeString(odr["Answer"].ToString()) +
                        SM4.DeString(odr["NineLock"].ToString()) + SM4.DeString(odr["UserRole"].ToString()) + odr["IdentityInformation"].ToString() + SM4.DeString(odr["UserState"].ToString());  //加时间就会错
                }

                //把等级密钥的哈希值录入文件
                cmd = new OleDbCommand(sql2, AconnStr1);
                odr = cmd.ExecuteReader();
                while (odr.Read())
                {
                    word += SM4.DeString(odr["AKey"].ToString(), pwd) + SM4.DeString(odr["BKey"].ToString(), pwd) + SM4.DeString(odr["CKey"].ToString(), pwd);
                }

                //把用户类别信息的哈希值录入文件
                cmd = new OleDbCommand(sql3, AconnStr1);
                odr = cmd.ExecuteReader();
                while (odr.Read())
                {
                    word += SM4.DeString(odr["RoleName"].ToString()) + SM4.DeString(odr["AFile"].ToString()) + SM4.DeString(odr["BFile"].ToString()) +
                        SM4.DeString(odr["CFile"].ToString() + SM4.DeString(odr["FolderOperation"].ToString()));
                }
                odr.Close();
                AconnStr1.Close();
                cmd = null;
                sw.Write(stringCheck(word));
                sw.Flush();
                sw.Close();

                //加密哈希校验值文件
                char[] charArray = new char[16];
                if (pwd.Length > 16)
                    pwd = pwd.Substring(0, 16);
                for (int i = 0; i < pwd.Length; i++)
                {
                    charArray[i] = (char)pwd[i];
                }
                Byte[] key = new Byte[16];
                for (int i = 0; i < charArray.Length; i++)
                {
                    key[i] = (byte)charArray[i];
                }
                long N = 0;
                SM4.File_Encrypt(tmpFile.ToCharArray(), key, outFile.ToCharArray(), ref N);
                if (File.Exists(tmpFile))
                    File.Delete(tmpFile);
            }
            catch
            {
                odr.Close();
                AconnStr1.Close();
                sw.Close();
                cmd = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 校验数据库
        /// </summary>
        /// <param name="checkFile"></param>
        /// <param name="DBFile"></param>
        /// <returns></returns>
        public static bool CheckDB(string checkFile, string DBFile)
        {
            try
            {
                if (!File.Exists(checkFile)) return false;
                if (!File.Exists(DBFile)) return false;
                string tepCheckFile = Start.panFu + @"\小U安全\\Config\\tepCheckFile2.xus";
                DBCheck(DBFile, tepCheckFile);

                StreamReader sr1 = null;
                StreamReader sr2 = null;
                sr1 = new StreamReader(checkFile);
                string check1 = sr1.ReadToEnd();
                sr1.Close();
                sr2 = new StreamReader(tepCheckFile);
                string check2 = sr2.ReadToEnd();
                sr2.Close();
                if (check1.Length != check2.Length)
                {
                    if (File.Exists(tepCheckFile))
                    {
                        File.Delete(tepCheckFile);
                    }
                    return false;
                }
                if (check1.CompareTo(check2) != 0)
                {
                    if (File.Exists(tepCheckFile))
                    {
                        File.Delete(tepCheckFile);
                    }
                    return false;
                }
                if (File.Exists(tepCheckFile))
                {
                    File.Delete(tepCheckFile);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
