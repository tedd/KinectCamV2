using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tedd.KinectCam.Test
{

    [TestClass]
    public class ColorComparison
    {
        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UInt32 count);

        [TestMethod]
        public void TestMethod1()
        {
            var rng = new RNGCryptoServiceProvider();
            var image = new byte[800 * 600 * 4];
            var imagePtr = GCHandle.Alloc(image, GCHandleType.Pinned);
            var imageLab = new byte[800 * 600 * 4];
            var imageLabPtr = GCHandle.Alloc(imageLab, GCHandleType.Pinned);
            rng.GetBytes(image);

            //RgbToLabConverter.ConvertArray(image);


            imagePtr.Free();
            imageLabPtr.Free();
            

        }
    }
}
