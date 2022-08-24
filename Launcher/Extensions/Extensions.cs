
using EDKv5;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Launcher
{
    public static class Extensions
    {
        private static Dictionary<Group, string> dic_grp_names;
        public static string GetName(this Group grp)
        {
            if (null == dic_grp_names)
            {
                dic_grp_names = new Dictionary<Group, string>();

                dic_grp_names.Add(Group.Team, Properties.Resources.strFullGrpPub);
                dic_grp_names.Add(Group.MA | Group.MB | Group.MC | Group.FA | Group.FB | Group.FC, Properties.Resources.strFullGrpPub + "*");

                dic_grp_names.Add(Group.Male, Properties.Resources.strMGrpPub);
                dic_grp_names.Add(Group.MA | Group.MB | Group.MC, Properties.Resources.strMGrpPub + "*");

                dic_grp_names.Add(Group.Female, Properties.Resources.strFGrpPub);
                dic_grp_names.Add(Group.FA | Group.FB | Group.FC, Properties.Resources.strFGrpPub + "*");

                dic_grp_names.Add(Group.MA, Properties.Resources.strMA);
                dic_grp_names.Add(Group.MB, Properties.Resources.strMB);
                dic_grp_names.Add(Group.MC, Properties.Resources.strMC);
                dic_grp_names.Add(Group.MD, Properties.Resources.strMD);
                dic_grp_names.Add(Group.FA, Properties.Resources.strFA);
                dic_grp_names.Add(Group.FB, Properties.Resources.strFB);
                dic_grp_names.Add(Group.FC, Properties.Resources.strFC);
                dic_grp_names.Add(Group.FD, Properties.Resources.strFD);

            }

            string name;
            if (dic_grp_names.TryGetValue(grp, out name))
                return name;
            else
                return Properties.Resources.strMixGroup;
        }
        
        // ---

        
        public static T First<T>(this DependencyObject obj) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int j = 0; j < count; j++)
            {
                var child = VisualTreeHelper.GetChild(obj, j);
                if (child is T)
                    return (T)child;
                else
                {
                    T subChild = child.First<T>();
                    if (null != subChild)
                        return subChild;
                }
            }
            return null;
        }
        public static T nth<T>(this DependencyObject obj, int n) where T : DependencyObject
        {
            return nth_ref<T>(obj, ref n);
        }
        private static T nth_ref<T>(DependencyObject obj, ref int n) where T : DependencyObject
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int j = 0; j < count; j++)
            {
                var child = VisualTreeHelper.GetChild(obj, j);
                if (child is T && 0 >= --n)
                    return (T)child;
                else
                {
                    T subChild = nth_ref<T>(child, ref n);
                    if (null != subChild && 0 >= --n)
                        return subChild;
                }
            }
            return null;
        }

        public static DependencyObject Next(this DependencyObject obj)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            int n = VisualTreeHelper.GetChildrenCount(parent);

            DependencyObject last = null;
            while (--n >= 0)
            {
                DependencyObject prev = VisualTreeHelper.GetChild(parent, n);
                if (prev == obj) { return last; }
                last = prev;
            }
            return null;
        }
        public static DependencyObject Prev(this DependencyObject obj)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(obj);
            int n = VisualTreeHelper.GetChildrenCount(parent);

            DependencyObject last = null;
            for (int i = 0; i < n; i++)
            {
                DependencyObject prev = VisualTreeHelper.GetChild(parent, i);
                if (prev == obj) { return last; }
                last = prev;
            }
            return null;
        }
        //public static T Next<T>(this DependencyObject obj) where T : DependencyObject
        //{
        //    DependencyObject parent = VisualTreeHelper.GetParent(obj);
        //    int n = VisualTreeHelper.GetChildrenCount(parent);

        //    T last = null;
        //    while (--n >= 0)
        //    {
        //        DependencyObject prev = VisualTreeHelper.GetChild(parent, n);
        //        if (prev == obj)
        //            { return last; }
        //        if (prev is T)
        //            { last = (T)prev; }
        //    }
        //    return null;
        //}
        //public static T Prev<T>(this DependencyObject obj) where T : DependencyObject
        //{
        //    DependencyObject parent = VisualTreeHelper.GetParent(obj);
        //    int n = VisualTreeHelper.GetChildrenCount(parent);

        //    T last = null;
        //    for (int i = 0; i < n; i++)
        //    {
        //        DependencyObject prev = VisualTreeHelper.GetChild(parent, i);
        //        if (prev == obj) { return last; }
        //        if (prev is T) { last = (T)prev; }
        //    }
        //    return null;
        //}

        public static T ClosestParent<T>(this DependencyObject obj) where T : DependencyObject
        {
            do
            {
                obj = VisualTreeHelper.GetParent(obj);
                if (null == obj) throw new Exception("Cannot find parent");
                if (obj is T) { return (T)obj; }
            } while (true);
        }

        public static void ScreenShot(this UIElement element, ImageFormat format, System.IO.Stream output)
        {
            // quadruple the size such that the image is clear
            int width = (int)element.RenderSize.Width * 2;
            int height = (int)element.RenderSize.Height * 2;

            var bgvisual = new DrawingVisual();
            var bgContext = bgvisual.RenderOpen();
            bgContext.DrawRectangle(Brushes.White, null, new Rect(0, 0, width, height));
            bgContext.Close();

            var target = new RenderTargetBitmap(
                width, height,
                192, 192, PixelFormats.Pbgra32          // double each side from 96 to 192, where 96dpi is the original output resolution
            );
            target.Render(bgvisual);
            target.Render(element);

            BitmapEncoder encoder;
            switch (format)
            {
                case ImageFormat.PNG:
                    encoder = new PngBitmapEncoder();
                    break;
                case ImageFormat.JPEG:
                    encoder = new JpegBitmapEncoder();
                    break;
                case ImageFormat.GIF:
                    encoder = new GifBitmapEncoder();
                    break;
                case ImageFormat.BMP:
                    encoder = new BmpBitmapEncoder();
                    break;
                case ImageFormat.TIFF:
                    encoder = new TiffBitmapEncoder();
                    break;
                default:
                    encoder = new PngBitmapEncoder();
                    break;
            }

            var outputFrame = BitmapFrame.Create(target);
            encoder.Frames.Add(outputFrame);

            encoder.Save(output);
        }
    }
}
