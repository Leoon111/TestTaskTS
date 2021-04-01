using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Test.Service
{
    internal static class ReadBytes
    {
        internal static string StreamOfBytesToString(FileStream fs, int fieldNameLenght)
        {
            byte[] moreInfoSizeBytes = new byte[fieldNameLenght];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            var m = Convert.ToBase64String(moreInfoSizeBytes);
            var st = System.Text.Encoding.ASCII.GetString(moreInfoSizeBytes);
            return st;
        }

        //private static byte[] StreamOfBytes(FileStream fileStream, )

        /// <summary>
        /// Преобразует байты в число
        /// </summary>
        /// <param name="fs">поток байтов типа FileStream</param>
        /// <returns>число</returns>
        internal static short TwoBytesToShort(FileStream fs)
        {
            byte[] moreInfoSizeBytes = new byte[2];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return BitConverter.ToInt16(moreInfoSizeBytes, 0);
        }

        internal static int FourBytesToInt(FileStream fs)
        {
            byte[] moreInfoSizeBytes = new byte[4];
            fs.Read(moreInfoSizeBytes, 0, moreInfoSizeBytes.Length);
            return BitConverter.ToInt32(moreInfoSizeBytes, 0);
        }

        internal static byte ReadOneByte(FileStream fileStream)
        {
            byte[] moreInfoSizeBytes = new byte[1];
            fileStream.Read(moreInfoSizeBytes, 0, 1);
            byte i = moreInfoSizeBytes[0];
            return i; //BitConverter.to ToInt16(moreInfoSizeBytes);
        }

        internal static T BuffToStruct<T>(byte[] arr) where T : struct
        {
            GCHandle gch = GCHandle.Alloc(arr, GCHandleType.Pinned); // зафиксировать в памяти
            IntPtr ptr = Marshal.UnsafeAddrOfPinnedArrayElement(arr, 0); // и взять его адрес
            T ret = (T)Marshal.PtrToStructure(ptr, typeof(T)); // создать структуру
            gch.Free(); // снять фиксацию
            return ret;
        }
    }
}
