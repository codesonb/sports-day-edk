using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EDKv5.Utility
{
    public static class Extension
    {
        #region Array Copy
        public static T[] CopyColumn<T>(this T[,] ori, int colIdx)
        {
            T[] rtn = new T[ori.GetUpperBound(0) + 1];
            for (int i = 0; i <= ori.GetUpperBound(0); i++)
                rtn[i] = ori[i, colIdx];
            return rtn;
        }
        public static T[] CopyRow<T>(this T[,] ori, int rowIdx)
        {
            T[] rtn = new T[ori.GetUpperBound(1) + 1];
            for (int i = 0; i <= ori.GetUpperBound(1); i++)
                rtn[i] = ori[rowIdx, i];
            return rtn;
        }
        #endregion

        #region PointF Vector-ization
        public static PointF Add(this PointF a, PointF b)
        {
            a.X += b.X;
            a.Y += b.Y;
            return a;
        }
        public static PointF Multiply(this PointF a, float r)
        {
            a.X *= r;
            a.Y *= r;
            return a;
        }
        public static PointF RotateRad(this PointF v, double radian)
        {
            //copy
            float tx = v.X;
            float ty = v.Y;
            //calulate
            v.X = (float)Math.Round((tx * Math.Cos(radian) - ty * Math.Sin(radian)), 3);
            v.Y = (float)Math.Round((tx * Math.Sin(radian) + ty * Math.Cos(radian)), 3);
            return v;
        }
        public static PointF RotateDeg(this PointF v, double degree)
        {
            return v.RotateRad(degree / 180d * Math.PI);
        }
        #endregion

        #region String Operation
        public static string Reverse2(this string ori)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in ori.Reverse())
                sb.Append(c);
            return sb.ToString();
        }
        #endregion

        #region Specify List<Student> Filters
        public static List<Student> Filter(this List<Student> ls_st, Class cls)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
            {
                if (cls == stu.Class)
                    lsOut.Add(stu);
            }
            return lsOut;
        }
        public static List<Student> Filter(this List<Student> ls_st, House hs)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
            {
                if (hs == stu.House)
                    lsOut.Add(stu);
            }
            return lsOut;
        }
        public static List<Student> Filter(this List<Student> ls_st, Group grp)
        {
            List<Student> lsOut = new List<Student>();
            foreach (Student stu in ls_st)
                if (grp == stu.Group)
                    lsOut.Add(stu);
            return lsOut;
        }
        #endregion

        #region Networking (IP Address)
        public static IPAddress GetLocalIPAddress()
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                throw new Exception("Network Adapter is not available");
                //return null;

            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip;
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }
        #endregion
    }
}
