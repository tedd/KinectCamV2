using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Alea;
using Alea.CSharp;
using Alea.Parallel;

namespace KinectCam.Gpu
{
    internal static class GpuImageProcessing
    {

        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UInt32 count);

        static GpuImageProcessing()
        {
            SetUp();
        }

        public static void SetUp()
        {
            string path1 = System.Reflection.Assembly.GetAssembly(typeof(GpuImageProcessing)).Location;
            string directory = System.IO.Path.GetDirectoryName(path1);
            Alea.Settings.Instance.Resource.Path = directory;
        }

        //[GpuManaged]
        public static unsafe void Process(ManagedArrayWithPointer<byte> src, ManagedArrayWithPointer<byte> dst, int width, int height, int bytesPerPixel)
        {

            ProcessGPU(src.GpuPinnedMemory, dst.GpuPinnedMemory);


        }

        [GpuManaged]
        private static void ProcessGPU(PinnedMemory<byte> src, PinnedMemory<byte> dst)
        {
            var lp = new LaunchParam(16, 256);
            Alea.Gpu.Default.Launch(Kernel, lp, dst.Ptr, src.Ptr, dst.Length);

            //var length = dst.Length;
            //Alea.Gpu.Default.For(0, length,(i) =>
            //{
            //    dst[i] = src[i];
            //});
        }

        private static void Kernel(deviceptr<byte> result, deviceptr<byte> arg1, int length)
        {
            var start = blockIdx.x * blockDim.x + threadIdx.x;
            var stride = gridDim.x * blockDim.x;
            //var len = length - 1;
            for (var i = start; i < length; i += stride)
            {
                //result[len - i] = arg1[i];
                result[i] = arg1[i];
            }
        }
    }
}
