using System;
using System.Runtime.InteropServices;
using Alea;
using KinectCam.Gpu;

namespace KinectCam
{
    internal class ManagedArrayWithPointer<T>: IDisposable
    {
        public readonly object Lock = new object();
        public readonly UInt32 Length;
        //public readonly T[] Array;
        public readonly GCHandle GCHandle;
        public readonly IntPtr IntPtr;
        public readonly PinnedMemory<T> GpuPinnedMemory;
        public ManagedArrayWithPointer(int capacity)
        {
            GpuImageProcessing.SetUp();
            //Array = new T[capacity];
            Length = (UInt32) capacity;
            //this.GCHandle = GCHandle.Alloc(Array, GCHandleType.Pinned);
            //IntPtr = this.GCHandle.AddrOfPinnedObject();

            GpuPinnedMemory = Alea.Gpu.Default.AllocatePinned<T>(capacity);
            IntPtr = GpuPinnedMemory.Handle;


        }

        private void ReleaseUnmanagedResources()
        {
            // TODO release unmanaged resources here
            //this.GCHandle.Free();

            GpuPinnedMemory.Dispose();
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        ~ManagedArrayWithPointer()
        {
            ReleaseUnmanagedResources();
        }
    }
}