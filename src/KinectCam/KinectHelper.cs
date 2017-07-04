using System;
using System.Diagnostics;
using Microsoft.Kinect;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KinectCam
{
    public static class KinectHelper
    {
        class KinectCamApplicationContext : ApplicationContext
        {
            private NotifyIcon TrayIcon;
            private ContextMenuStrip TrayIconContextMenu;
            private ToolStripMenuItem MirroredMenuItem;
            private ToolStripMenuItem DesktopMenuItem;
            private ToolStripMenuItem ZoomMenuItem;
            private ToolStripMenuItem TrackHeadMenuItem;
            public KinectCamApplicationContext()
            {
                Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
                InitializeComponent();
                TrayIcon.Visible = true;
                //TrayIcon.ShowBalloonTip(30000);
            }

            private void InitializeComponent()
            {
                TrayIcon = new NotifyIcon();

                TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
                TrayIcon.BalloonTipText =
                  "For options use this tray icon.";
                TrayIcon.BalloonTipTitle = "KinectCamV2";
                TrayIcon.Text = "KinectCam";

                TrayIcon.Icon = IconExtractor.Extract(117, false);

                TrayIcon.DoubleClick += TrayIcon_DoubleClick;

                TrayIconContextMenu = new ContextMenuStrip();
                MirroredMenuItem = new ToolStripMenuItem();
                DesktopMenuItem = new ToolStripMenuItem();
                ZoomMenuItem = new ToolStripMenuItem();
                TrackHeadMenuItem = new ToolStripMenuItem();
                TrayIconContextMenu.SuspendLayout();

                // 
                // TrayIconContextMenu
                // 
                this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {
                this.MirroredMenuItem,
                this.DesktopMenuItem,
                this.ZoomMenuItem,
                this.TrackHeadMenuItem
                });
                this.TrayIconContextMenu.Name = "TrayIconContextMenu";
                this.TrayIconContextMenu.Size = new Size(153, 70);
                // 
                // MirroredMenuItem
                // 
                this.MirroredMenuItem.Name = "Mirrored";
                this.MirroredMenuItem.Size = new Size(152, 22);
                this.MirroredMenuItem.Text = "Mirrored";
                this.MirroredMenuItem.Click += new EventHandler(this.MirroredMenuItem_Click);

                // 
                // DesktopMenuItem
                // 
                this.DesktopMenuItem.Name = "Desktop";
                this.DesktopMenuItem.Size = new Size(152, 22);
                this.DesktopMenuItem.Text = "Desktop";
                this.DesktopMenuItem.Click += new EventHandler(this.DesktopMenuItem_Click);

                // 
                // ZoomMenuItem
                // 
                this.ZoomMenuItem.Name = "Zoom";
                this.ZoomMenuItem.Size = new Size(152, 22);
                this.ZoomMenuItem.Text = "Zoom";
                this.ZoomMenuItem.Click += new EventHandler(this.ZoomMenuItem_Click);

                // 
                // ZoomMenuItem
                //
                this.TrackHeadMenuItem.Name = "TrackHead";
                this.TrackHeadMenuItem.Size = new Size(152, 22);
                this.TrackHeadMenuItem.Text = "TrackHead";
                this.TrackHeadMenuItem.Click += new EventHandler(this.TrackHeadMenuItem_Click);

                TrayIconContextMenu.ResumeLayout(false);
                TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            }

            private void OnApplicationExit(object sender, EventArgs e)
            {
                TrayIcon.Visible = false;
            }

            private void TrayIcon_DoubleClick(object sender, EventArgs e)
            {
                TrayIcon.ShowBalloonTip(30000);
            }

            private void MirroredMenuItem_Click(object sender, EventArgs e)
            {
                KinectCamSettigns.Default.Mirrored = !KinectCamSettigns.Default.Mirrored;
            }

            private void DesktopMenuItem_Click(object sender, EventArgs e)
            {
                KinectCamSettigns.Default.Desktop = !KinectCamSettigns.Default.Desktop;
            }

            private void ZoomMenuItem_Click(object sender, EventArgs e)
            {
                KinectCamSettigns.Default.Zoom = !KinectCamSettigns.Default.Zoom;
            }

            private void TrackHeadMenuItem_Click(object sender, EventArgs e)
            {
                KinectCamSettigns.Default.TrackHead = !KinectCamSettigns.Default.TrackHead;
            }
            public void Exit()
            {
                TrayIcon.Visible = false;
            }
        }

        static KinectCamApplicationContext context;
        static Thread contexThread;
        static Thread refreshThread;
        static KinectSensor Sensor;

        static void InitializeSensor()
        {
            var sensor = Sensor;
            if (sensor != null) return;

            try
            {
                sensor = KinectSensor.GetDefault();
                if (sensor == null) return;

                //var reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Body | FrameSourceTypes.Depth); //ColorFrameSource.OpenReader();
                var reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth); //ColorFrameSource.OpenReader();
                reader.MultiSourceFrameArrived += reader_FrameArrived;
                sensor.Open();

                Sensor = sensor;

                if (context == null)
                {
                    contexThread = new Thread(() =>
                    {
                        context = new KinectCamApplicationContext();
                        Application.Run(context);
                    });
                    refreshThread = new Thread(() =>
                    {
                        while (true)
                        {
                            Thread.Sleep(250);
                            Application.DoEvents();
                        }
                    });
                    contexThread.IsBackground = true;
                    refreshThread.IsBackground = true;
                    contexThread.SetApartmentState(ApartmentState.STA);
                    refreshThread.SetApartmentState(ApartmentState.STA);
                    contexThread.Start();
                    refreshThread.Start();
                }
            }
            catch
            {
                Trace.WriteLine("Error of enable the Kinect sensor!");
            }
        }

        public delegate void InvokeDelegate();

        static void reader_FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var reference = e.FrameReference.AcquireFrame();
            using (var colorFrame = reference.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    ColorFrameReady(colorFrame);
                }
            }
            using (var depthFrame = reference.DepthFrameReference.AcquireFrame())
            {
                if (depthFrame != null)
                {
                    DepthFrameReady(depthFrame);
                }
            }
            using (var bodyFrame = reference.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    var _bodies = new Body[bodyFrame.BodyFrameSource.BodyCount];

                    bodyFrame.GetAndRefreshBodyData(_bodies);
                    _headFound = false;
                    foreach (var body in _bodies)
                    {
                        if (body.IsTracked)
                        {
                            Joint head = body.Joints[JointType.Head];

                            if (head.TrackingState == TrackingState.NotTracked)
                                continue;
                            _headFound = true;
                            _headPosition = Sensor.CoordinateMapper.MapCameraPointToColorSpace(head.Position);

                        }
                    }
                    UpdateZoomPosition();
                }
            }
        }
        static int SmoothTransitionByStep(int current, int needed, int step = 4)
        {

            if (step == 0)
            {
                return needed;
            }
            if (current > needed)
            {
                current -= step;
                if (current < needed)
                    current = needed;
            }
            else if (current < needed)
            {
                current += step;
                if (current > needed)
                    current = needed;
            }
            return current;
        }
        private static void UpdateZoomPosition()
        {
            // we should be at 30 fps in this place as Kinect Body are at 30 fps 
            int NeededZoomedWidthStart = 0;
            int NeededZoomedHeightStart;
            if (!KinectCamSettigns.Default.TrackHead || !_headFound)
            {
                NeededZoomedWidthStart = (SensorWidth - ZoomedWidth) / 2;
                NeededZoomedHeightStart = (SensorHeight - ZoomedHeight) / 2;
            }
            else
            {
                NeededZoomedWidthStart = (int)Math.Min(MaxZoomedWidthStart, Math.Max(0, _headPosition.X - ZoomedWidth / 2));

                NeededZoomedHeightStart = (int)Math.Min(MaxZoomedHeightStart, Math.Max(0, _headPosition.Y - ZoomedHeight / 2));
            }

            ZoomedWidthStart = SmoothTransitionByStep(ZoomedWidthStart, NeededZoomedWidthStart, 4);
            ZoomedHeightStart = SmoothTransitionByStep(ZoomedHeightStart, NeededZoomedHeightStart, 4);

            ZoomedWidthEnd = ZoomedWidthStart + ZoomedWidth;
            ZoomedHeightEnd = ZoomedHeightStart + ZoomedHeight;
            ZoomedPointerStart = ZoomedHeightStart * 1920 * 4 + ZoomedWidthStart * 4;
            ZoomedPointerEnd = ZoomedHeightEnd * 1920 * 4 + ZoomedWidthEnd * 4;

        }

        static unsafe void ColorFrameReady(ColorFrame frame)
        {
            // Using IntPtr saves us another 1-2% on my computer
            if (frame.RawColorImageFormat == ColorImageFormat.Bgra)
            {
                frame.CopyRawFrameDataToIntPtr(pinnedColorFrame.AddrOfPinnedObject(), pinnedColorFrameLength);
                //frame.CopyRawFrameDataToArray(sensorColorFrameData);
            }
            else
            {
                frame.CopyConvertedFrameDataToIntPtr(pinnedColorFrame.AddrOfPinnedObject(), pinnedColorFrameLength, ColorImageFormat.Bgra);
                //frame.CopyConvertedFrameDataToArray(sensorColorFrameData, ColorImageFormat.Bgra);
            }
        }
        static unsafe void DepthFrameReady(DepthFrame frame)
        {
            if (!pinnedColorFrameSet)
            {
                sensorDepthFrameData = new ushort[frame.FrameDescription.LengthInPixels];
                pinnedDepthFrameLength = frame.FrameDescription.LengthInPixels * frame.FrameDescription.BytesPerPixel;
                pinnedDepthFrame = GCHandle.Alloc(sensorDepthFrameData, GCHandleType.Pinned);
                pinnedColorFrameSet = true;

                sensorDepthSpacePoints = new DepthSpacePoint[sensorDepthFrameData.Length];
                pinnedDepthSpaceLength = (uint)sensorDepthSpacePoints.Length;
                pinnedDepthSpacePoints = GCHandle.Alloc(sensorDepthSpacePoints, GCHandleType.Pinned);
                pinnedColorSpaceSet = true;
            }

            frame.CopyFrameDataToIntPtr(pinnedDepthFrame.AddrOfPinnedObject(), pinnedDepthFrameLength);
            //Sensor.CoordinateMapper.MapDepthFrameToColorSpaceUsingIntPtr(pinnedDepthFrame.AddrOfPinnedObject(),pinnedDepthFrameLength,);
            Sensor.CoordinateMapper.MapColorFrameToDepthSpaceUsingIntPtr(pinnedDepthFrame.AddrOfPinnedObject(), pinnedDepthFrameLength,pinnedDepthSpacePoints.AddrOfPinnedObject(), pinnedDepthSpaceLength);
        }

        public static void DisposeSensor()
        {
            try
            {
                var sensor = Sensor;
                if (sensor != null && sensor.IsOpen)
                {
                    sensor.Close();
                    sensor = null;
                    Sensor = null;
                }

                if (context != null)
                {
                    context.Exit();
                    context.Dispose();
                    context = null;

                    contexThread.Abort();
                    refreshThread.Abort();
                }
            }
            catch
            {
                Trace.WriteLine("Error of disable the Kinect sensor!");
            }
        }

        static bool _headFound;
        static ColorSpacePoint _headPosition;

        public const int SensorWidth = 1920;
        public const int SensorHeight = 1080;
        public const int ZoomedWidth = 960;
        public const int ZoomedHeight = 540;

        public const int DefaultZoomedWidthStart = (SensorWidth - ZoomedWidth) / 2;
        public const int DefaultHeightStart = (SensorHeight - ZoomedHeight) / 2;

        public const int MaxZoomedWidthStart = SensorWidth - ZoomedWidth;
        public const int MaxZoomedHeightStart = SensorHeight - (ZoomedHeight + 1); // +1 to avoid overflow

        public static int ZoomedWidthStart = (SensorWidth - ZoomedWidth) / 2;
        public static int ZoomedHeightStart = (SensorHeight - ZoomedHeight) / 2;
        public static int ZoomedWidthEnd = ZoomedWidthStart + ZoomedWidth;
        public static int ZoomedHeightEnd = ZoomedHeightStart + ZoomedHeight;


        public static int ZoomedPointerStart = ZoomedHeightStart * 1920 * 4 + ZoomedWidthStart * 4;
        public static int ZoomedPointerEnd = ZoomedHeightEnd * 1920 * 4 + ZoomedWidthEnd * 4;
        static readonly byte[] sensorColorFrameData = new byte[1920 * 1080 * 4];
        private static readonly uint pinnedColorFrameLength = 1920 * 1080 * 4;
        private static GCHandle pinnedColorFrame;
        private static bool pinnedColorFrameSet = false;

        private static UInt16[] sensorDepthFrameData;
        private static uint pinnedDepthFrameLength = 512 * 424 * 2;
        private static GCHandle pinnedDepthFrame;
        private static bool pinnedDepthFrameSet = false;

        private static DepthSpacePoint[] sensorDepthSpacePoints;
        private static uint pinnedDepthSpaceLength;
        private static GCHandle pinnedDepthSpacePoints;
        private static bool pinnedColorSpaceSet = false;


        [DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
        public static extern IntPtr memcpy(IntPtr dest, IntPtr src, UInt32 count);

        public unsafe static void GenerateFrame(IntPtr _ptr, int length, int cHeight, int cWidth, int cBitDepth, bool mirrored, bool zoom)
        {
            byte[] colorFrame = sensorColorFrameData;
            void* camData = _ptr.ToPointer();

            try
            {
                InitializeSensor();


                if (colorFrame != null)
                {
                    int colorFramePointerStart = zoom ? ZoomedPointerStart : 0;
                    int colorFramePointerEnd = zoom ? ZoomedPointerEnd - 1 : colorFrame.Length - 1;
                    int width = zoom ? ZoomedWidth : SensorWidth;

                    if (!pinnedColorFrameSet)
                    {
                        pinnedColorFrame = GCHandle.Alloc(colorFrame, GCHandleType.Pinned);
                        pinnedColorFrameSet = true;
                    }

                    memcpy(_ptr, pinnedColorFrame.AddrOfPinnedObject(), (uint)Math.Min(colorFrame.Length, cWidth * cHeight * (cBitDepth / 8)));
                    // I've shortcircuited all fancy functionality simply because unmanaged memory copy uses around 8% less CPU on my computer

                    return;
                    if (!mirrored)
                    {
                        fixed (byte* sDataB = &colorFrame[colorFramePointerStart])
                        fixed (byte* sDataE = &colorFrame[colorFramePointerEnd])
                        {
                            byte* pData = (byte*)camData;
                            byte* sData = (byte*)sDataE;
                            bool redo = true;

                            for (; sData > sDataB;)
                            {
                                for (var i = 0; i < width; ++i)
                                {
                                    var p = sData - 3;
                                    *pData++ = *p++;
                                    *pData++ = *p++;
                                    *pData++ = *p++;
                                    if (zoom)
                                    {
                                        p = sData - 3;
                                        *pData++ = *p++;
                                        *pData++ = *p++;
                                        *pData++ = *p++;
                                    }
                                    sData -= 4;
                                }
                                if (zoom)
                                {
                                    if (redo)
                                    {
                                        sData += width * 4;
                                    }
                                    else
                                    {
                                        sData -= (SensorWidth - ZoomedWidth) * 4;
                                    }
                                    redo = !redo;

                                }
                            }

                        }
                    }
                    else
                    {
                        fixed (byte* sDataB = &colorFrame[colorFramePointerStart])
                        fixed (byte* sDataE = &colorFrame[colorFramePointerEnd])
                        {
                            byte* pData = (byte*)camData;
                            byte* sData = (byte*)sDataE;

                            var sDataBE = sData;
                            var p = sData;
                            var r = sData;
                            bool redo = true;

                            while (sData == (sDataBE = sData) &&
                                   sDataB <= (sData -= (width * 4 - 1)))
                            {

                                r = sData;
                                do
                                {
                                    p = sData;
                                    *pData++ = *p++;
                                    *pData++ = *p++;
                                    *pData++ = *p++;
                                    if (zoom)
                                    {
                                        p = sData;
                                        *pData++ = *p++;
                                        *pData++ = *p++;
                                        *pData++ = *p++;
                                    }

                                }
                                while ((sData += 4) <= sDataBE);
                                sData = r - 1;
                                if (zoom)
                                {
                                    if (redo)
                                    {
                                        sData += width * 4;
                                    }
                                    else
                                    {
                                        sData -= (SensorWidth - ZoomedWidth) * 4;
                                    }
                                    redo = !redo;

                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                byte* pData = (byte*)camData;
                for (int i = 0; i < length; ++i)
                    *pData++ = 0;
            }
        }
    }
}
