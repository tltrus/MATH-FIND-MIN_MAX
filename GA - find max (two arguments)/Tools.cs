
using System;
using System.Windows;
using System.Windows.Media;

namespace _3D_Chart_WPF_v1
{
    internal class Tools
    {
        public static double Constrain(double n, double low, double high) => Math.Max(Math.Min(n, high), low);

        public static double Map(double n, double start1, double stop1, double start2, double stop2, bool withinBounds = false)
        {
            var newval = (n - start1) / (stop1 - start1) * (stop2 - start2) + start2;
            if (!withinBounds)
            {
                return newval;
            }
            if (start2 < stop2)
            {
                return Constrain(newval, start2, stop2);
            }
            else
            {
                return Constrain(newval, stop2, start2);
            }
        }

        public static Color HsvToRgb(float h, float s, float v)
        {
            int i;
            float f, p, q, t;

            if (s < float.Epsilon)
            {
                byte c = (byte)(v * 255);
                return Color.FromRgb(c, c, c);
            }

            h /= 60;
            i = (int)Math.Floor(h);
            f = h - i;
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));

            float r, g, b;
            switch (i)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }

            return Color.FromRgb((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        public static Point Normalize(Point pt, double width, double height, double Xmin, double Xmax, double Ymin, double Ymax)
        {
            Point result = new Point();
            result.X = (pt.X - Xmin) * width / (Xmax - Xmin);
            result.Y = height - (pt.Y - Ymin) * height / (Ymax - Ymin);
            return result;
        }
    }
}
