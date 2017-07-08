//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace KinectCam.ColorComparison
//{
//    internal static class RgbToLabConverter
//    {
//        // Based on https://github.com/THEjoezack/ColorMine/blob/master/ColorMine/ColorSpaces/Conversions/LabConverter.cs
//        internal static void ToColorSpace(IRgb color, ILab item)
//        {
//            var xyz = new Xyz();
//            xyz.Initialize(color);

//            var white = XyzConverter.WhiteReference;
//            var x = PivotXyz(xyz.X / white.X);
//            var y = PivotXyz(xyz.Y / white.Y);
//            var z = PivotXyz(xyz.Z / white.Z);

//            item.L = Math.Max(0, 116 * y - 16);
//            item.A = 500 * (x - y);
//            item.B = 200 * (y - z);
//        }

       
//        private static double PivotXyz(double n)
//        {
//            return n > Xyz.Epsilon ? CubicRoot(n) : (Xyz.Kappa * n + 16) / 116;
//        }

//        private static double CubicRoot(double n)
//        {
//            return Math.Pow(n, 1.0 / 3.0);
//        }

//    }

//    internal struct Xyz
//    {
//        // Based on https://github.com/THEjoezack/ColorMine/blob/master/ColorMine/ColorSpaces/Conversions/XyzConverter.cs

//            public static readonly Xyz WhiteReference = new Xyz
//        {
//            X = 95.047,
//            Y = 100.000,
//            Z = 108.883
//        };



//        internal const double Epsilon = 0.008856; // Intent is 216/24389
//        internal const double Kappa = 903.3; // Intent is 24389/27
//        internal static double CubicRoot(double n)
//        {
//            return Math.Pow(n, 1.0 / 3.0);
//        }

//        internal static void ToColorSpace(IRgb color, IXyz item)
//        {
//            var r = PivotRgb(color.R / 255.0);
//            var g = PivotRgb(color.G / 255.0);
//            var b = PivotRgb(color.B / 255.0);

//            // Observer. = 2°, Illuminant = D65
//            item.X = r * 0.4124 + g * 0.3576 + b * 0.1805;
//            item.Y = r * 0.2126 + g * 0.7152 + b * 0.0722;
//            item.Z = r * 0.0193 + g * 0.1192 + b * 0.9505;
//        }

    
//        private static double PivotRgb(double n)
//        {
//            return (n > 0.04045 ? Math.Pow((n + 0.055) / 1.055, 2.4) : n / 12.92) * 100.0;
//        }
//    }
//}
