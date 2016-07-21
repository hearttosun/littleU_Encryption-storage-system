using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace BLL
{
    class SM4
    {
        public static Byte[] builtInKey = new Byte[] { 112, 23, 129, 203, 107, 11, 245, 219, 244, 254, 86, 122, 238, 56, 150, 126 };//内置密钥 
        public static long EnSum = 0;
        public static long DeSum = 0;

        public struct sm4_context
        {
            public int mode;               /*!<  encrypt/decrypt   */
            unsafe public fixed UInt32 sk[32];        /*!<  SM4 subkeys       */
        }

        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]//加CallingConvention = CallingConvention.Cdecl，是为了防止对 PInvoke 函数的调用导致堆栈不对称问题  
        public unsafe static extern void sm4_setkey_enc(ref sm4_context ctx, Byte[] key);
        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm4_setkey_dec(ref sm4_context ctx, Byte[] key);
        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm4_crypt_ecb(ref sm4_context ctx, int mode, int length, Byte[] input, Byte[] output);
        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern void sm4_crypt_cbc(ref sm4_context ctx, int mode, int length, Byte[] iv, Byte[] input, Byte[] output);

        /// <summary>
        /// 对文件进行加密
        /// </summary>
        /// <param name="plainFile">待加密文件路径</param>
        /// <param name="key">加密密钥</param>
        /// <param name="cipherFile">加密文件路径</param>
        /// <returns></returns>
        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int File_Encrypt(char[] plainFile, Byte[] key, char[] cipherFile, ref long EnSum);

        /// <summary>
        /// 对文件进行解密
        /// </summary>
        /// <param name="cipherFile">加密文件路径</param>
        /// <param name="key">解密密钥</param>
        /// <param name="plainFile">解密文件路径</param>
        /// <returns></returns>
        [DllImport("SM4DLL.dll", CallingConvention = CallingConvention.Cdecl)]
        public unsafe static extern int File_Decrypt(char[] cipherFile, Byte[] key, char[] plainFile, ref long DeSum);

        public static int File_Encrypt(string plainFile, string strkey, string cipherFile)
        {
            char[] charArray = new char[16];
            if (strkey.Length > 16)
                strkey = strkey.Substring(0, 16);
            for (int i = 0; i < strkey.Length; i++)
            {
                charArray[i] = (char)strkey[i];
            }
            Byte[] key = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                key[i] = (byte)charArray[i];
            }
            int result = File_Encrypt(plainFile.ToCharArray(), key, cipherFile.ToCharArray(), ref EnSum);
            return result;
        }

        public static int File_Decrypt(string cipherFile, string strkey, string plainFile)
        {
            char[] charArray = new char[16];
            if (strkey.Length > 16)
                strkey = strkey.Substring(0, 16);
            for (int i = 0; i < strkey.Length; i++)
            {
                charArray[i] = (char)strkey[i];
            }
            Byte[] key = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                key[i] = (byte)charArray[i];
            }
            int result = File_Decrypt(cipherFile.ToCharArray(), key, plainFile.ToCharArray(), ref DeSum);
            return result;
        }

        /// <summary>
        /// 内置密钥加密字符串
        /// </summary>
        /// <param name="str">待加密的string型字符串</param>
        /// <returns></returns>
        public static String EnString(String str)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
            Byte[] input = new Byte[16];
            int length = byteArray.Length;
            if (length > 16) length = 16;
            //空着的就是0而不是null  
            for (int i = 0; i < length; i++)
            {
                input[i] = byteArray[i];
            }
            Byte[] output = new Byte[16];
            sm4_context ctx = new sm4_context();
            //加密  
            sm4_setkey_enc(ref ctx, builtInKey);
            sm4_crypt_ecb(ref ctx, 1, 16, input, output);
            char[] chr = new char[16];
            for (int i = 0; i < output.Length; i++)
            {
                chr[i] = (char)output[i];
            }
            return new string(chr);
        }

        /// <summary>
        /// 内置密钥解密字符串
        /// </summary>
        /// <param name="str">加密后的string型字符串</param>
        /// <returns></returns>
        public static String DeString(String str)
        {
            char[] charArray = new char[16]; ;
            if (str.Length > 16)
                str = str.Substring(0, 16);
            for (int i = 0; i < str.Length; i++)
            {
                charArray[i] = (char)str[i];
            }
            Byte[] input = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                input[i] = (byte)charArray[i];
            }

            Byte[] output = new Byte[16];
            sm4_context ctx = new sm4_context();
            //解密  
            sm4_setkey_dec(ref ctx, builtInKey);
            sm4_crypt_ecb(ref ctx, 0, 16, input, output);
            Byte[] output_short = new Byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (output[i] != 0)
                {
                    output_short[i] = output[i];
                }
            }
            char[] chr = new char[input.Length];
            chr = System.Text.Encoding.Default.GetChars(output_short);
            return new string(chr);
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="str">待加密的string型字符串</param>
        /// <param name="key">加密密钥</param>
        /// <returns></returns>
        public static String EnString(String str, Byte[] key)
        {
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(str);
            Byte[] input = new Byte[16];
            int length = byteArray.Length;
            if (length > 16) length = 16;
            //空着的就是0而不是null  
            for (int i = 0; i < length; i++)
            {
                input[i] = byteArray[i];
            }
            Byte[] output = new Byte[16];
            sm4_context ctx = new sm4_context();
            //加密  
            sm4_setkey_enc(ref ctx, key);
            sm4_crypt_ecb(ref ctx, 1, 16, input, output);
            char[] chr = new char[16];
            for (int i = 0; i < output.Length; i++)
            {
                chr[i] = (char)output[i];
            }
            return new string(chr);
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="str">加密后的string型字符串</param>
        /// <param name="key">解密密钥</param>
        /// <returns></returns>
        public static String DeString(String str, Byte[] key)
        {
            char[] charArray = new char[16]; ;
            if (str.Length > 16)
                str = str.Substring(0, 16);
            for (int i = 0; i < str.Length; i++)
            {
                charArray[i] = (char)str[i];
            }
            Byte[] input = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                input[i] = (byte)charArray[i];
            }

            Byte[] output = new Byte[16];
            sm4_context ctx = new sm4_context();
            //解密  
            sm4_setkey_dec(ref ctx, key);
            sm4_crypt_ecb(ref ctx, 0, 16, input, output);
            Byte[] output_short = new Byte[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                if (output[i] != 0)
                {
                    output_short[i] = output[i];
                }
            }
            char[] chr = new char[input.Length];
            chr = System.Text.Encoding.Default.GetChars(output_short);
            return new string(chr);
        }

        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="str">待加密的string型字符串</param>
        /// <param name="strkey">加密密钥</param>
        /// <returns></returns>
        public static String EnString(String str, String strkey)
        {
            char[] charArray = new char[16];
            if (strkey.Length > 16)
                strkey = strkey.Substring(0, 16);
            for (int i = 0; i < strkey.Length; i++)
            {
                charArray[i] = (char)strkey[i];
            }
            Byte[] key = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                key[i] = (byte)charArray[i];
            }
            return EnString(str, key);
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="str">加密后的string型字符串</param>
        /// <param name="strkey">解密密钥</param>
        /// <returns></returns>
        public static String DeString(String str, String strkey)
        {
            char[] charArray = new char[16];
            if (strkey.Length > 16)
                strkey = strkey.Substring(0, 16);
            for (int i = 0; i < strkey.Length; i++)
            {
                charArray[i] = (char)strkey[i];
            }
            Byte[] key = new Byte[16];
            for (int i = 0; i < charArray.Length; i++)
            {
                key[i] = (byte)charArray[i];
            }
            return DeString(str, key);
        }

        /// <summary>
        /// 产生字节数组型随机密钥
        /// </summary>
        /// <param name="count">count为数组长度</param>
        /// <returns>返回值为数组</returns>
        public static byte[] GenerateRandomBytes(int count = 16/*长度默认16字节*/)
        {
            RandomNumberGenerator rand = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[count];
            rand.GetBytes(bytes);
            return bytes;
        }
    }
}
