//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace EDKv5.Utility.Cryptography
//{
//    static class CryptoService
//    {
//        private delegate byte[] dlgAlgorithm(ref byte[] buffer, byte[] IV, byte[] key);

//        public static void Encrypt(Stream inStream, Stream outStream, string key)
//        { Encrypt(inStream, outStream, Encoding.ASCII.GetBytes(key)); }
//        public static void Encrypt(Stream inStream, Stream outStream, byte[] key)
//        {
//            dlgAlgorithm algorithm = new dlgAlgorithm(encrypt_algorithm);
//            execute(inStream, outStream, key, algorithm);
//        }
//        public static void Decrypt(Stream inStream, Stream outStream, string key)
//        { Decrypt(inStream, outStream, Encoding.ASCII.GetBytes(key)); }
//        public static void Decrypt(Stream inStream, Stream outStream, byte[] key)
//        {
//            dlgAlgorithm algorithm = new dlgAlgorithm(decrypt_algorithm);
//            execute(inStream, outStream, key, algorithm);
//        }

//        private static void execute(Stream inStream, Stream outStream, byte[] key, dlgAlgorithm algorithm)
//        {
//            const string IVStr = @"d9b4855fc39bb7d9063dfca5a621f57d3012006bc2262609cb4015f57d167230";
//            char[] IVchr = IVStr.ToArray();
//            byte[] IV = new byte[key.Length];

//            //prepare IV
//            for (int i = 0; i < key.Length; i++)
//                IV[i] = (byte)IVchr[i % IVchr.Length];

//            //
//            while (inStream.Position < inStream.Length)
//            {
//                //read
//                byte[] buffer = new byte[key.Length];
//                inStream.Read(buffer, 0, buffer.Length);

//                //update IV
//                IV = algorithm(ref buffer, IV, key);

//                //output buffer
//                outStream.Write(buffer, 0, buffer.Length);
//            }
//        }

//        private static byte[] encrypt_algorithm(ref byte[] buffer, byte[] IV, byte[] key)
//        {
//            for (int i = 0; i < buffer.Length; i++)
//            {
//                int x = (buffer[i] ^ IV[i]) + key[i];
//                buffer[i] = (byte)(x % 256);
//            }
//            return buffer;
//        }
//        private static byte[] decrypt_algorithm(ref byte[] buffer, byte[] IV, byte[] key)
//        {
//            byte[] rtn = new byte[buffer.Length];
//            buffer.CopyTo(rtn, 0);
//            for (int i = 0; i < buffer.Length; i++)
//            {
//                int x = buffer[i] + 256 - key[i];
//                x = x % 256;
//                buffer[i] = (byte)(x ^ IV[i]);
//            }
//            return rtn;
//        }

//    }
//}
