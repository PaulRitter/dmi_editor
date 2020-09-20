using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;

namespace DMIEditor
{
    public static class BitmapHelper
    {
        public static Bitmap CreateTransparentBackgroundMap(int width, int height)
        {
            var backgroundMap = new Bitmap(width, height);
            var s = true;
            for (var i = 0; i < backgroundMap.Width; i++)
            {
                for (var j = 0; j < backgroundMap.Height; j++)
                {
                    var c = s ? Color.LightGray : Color.White;
                    s = !s;
                    backgroundMap.SetPixel(i, j, c);
                }
                s = !s; //offsetting every row
            }

            return backgroundMap;
        }

        public static Bitmap CreateSelectionBox(bool up, bool right, bool down,
            bool left, int resolution)
        {
            Bitmap bm = new Bitmap(6,6);
            if (up)
            {
                bool s = true;
                for (int i = 0; i < bm.Width; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(i, 0, c);
                    s = !s;
                }
            }
            
            if (right)
            {
                bool s = true;
                for (int i = 0; i < bm.Height; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(bm.Width-1, i, c);
                    s = !s;
                }
            }
            
            if (down)
            {
                bool s = true;
                for (int i = 0; i < bm.Width; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(i, bm.Height-1, c);
                    s = !s;
                }
            }
            
            if (left)
            {
                bool s = true;
                for (int i = 0; i < bm.Height; i++)
                {
                    var c = s ? Color.Black : Color.White;
                    bm.SetPixel(0, i, c);
                    s = !s;
                }
            }

            return bm;
        }

        public static List<Point> GetSimilarPoints(this Bitmap bitmap, Point p, int int_tolerance = 10)
        {
            bool CheckByte(byte value, byte target)
            {
                var diff = Math.Abs(target - value);
                return diff <= int_tolerance;
            }
            
            List<Point> resultList = new List<Point>();

            var oldColor = bitmap.GetPixel(p.X, p.Y);
            List<Point> alreadyProcessed = new List<Point>();
            List<Point> workList = new List<Point> {p};
            while (workList.Count > 0)
            {
                Point current_point = workList[^1];
                workList.Remove(current_point);
                    
                if (alreadyProcessed.Any(point => point.Equals(current_point)))
                    continue;
                    
                alreadyProcessed.Add(current_point);
                //validate position
                if (current_point.X >= bitmap.Width || current_point.X < 0)
                    continue;
                if (current_point.Y >= bitmap.Height || current_point.Y < 0)
                    continue;

                //check color
                var color = bitmap.GetPixel(current_point.X, current_point.Y);
                if(!CheckByte(color.A, oldColor.A) ||
                   !CheckByte(color.R, oldColor.R) || 
                   !CheckByte(color.G, oldColor.G) ||
                   !CheckByte(color.B, oldColor.B))
                    continue;

                resultList.Add(current_point);
                    
                workList.Add(new Point(current_point.X + 1, current_point.Y));
                workList.Add(new Point(current_point.X - 1, current_point.Y));
                workList.Add(new Point(current_point.X, current_point.Y + 1));
                workList.Add(new Point(current_point.X, current_point.Y - 1));
            }

            return resultList;
        }
    }
}