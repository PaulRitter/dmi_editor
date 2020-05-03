using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Fill : EditorTool
    {
        public override string Name => "Fill";
        public Fill(MainWindow main) : base(main) { }

        public override bool pixelAct(ref Bitmap current, int x, int y)
        {
            Color oldColor = current.GetPixel(x, y);

            if (oldColor == main.getColor())
                return false;

            processPixel(ref current, oldColor, new Point(x,y), new List<Point>());

            return true;
        }

        //checks and modifies current pixel and proceeds to check cardinal neighbours
        //ignore list is shared to avoid loops
        private void processPixel(ref Bitmap current, Color oldColor, Point pixel, List<Point> alreadyProcessed)
        {
            foreach (Point point in alreadyProcessed)
            {
                if (point.Equals(pixel))
                {
                    return;
                }
            }
            alreadyProcessed.Add(pixel);
            if (pixel.X >= current.Width || pixel.X < 0)
                return;
            if (pixel.Y >= current.Height || pixel.Y < 0)
                return;

            if (oldColor.Equals(current.GetPixel(pixel.X, pixel.Y)))
            {
                current.SetPixel(pixel.X, pixel.Y, main.getColor());

                processPixel(ref current, oldColor, new Point(pixel.X + 1, pixel.Y), alreadyProcessed);
                processPixel(ref current, oldColor, new Point(pixel.X - 1, pixel.Y), alreadyProcessed);
                processPixel(ref current, oldColor, new Point(pixel.X, pixel.Y + 1), alreadyProcessed);
                processPixel(ref current, oldColor, new Point(pixel.X, pixel.Y - 1), alreadyProcessed);
            }
        }
    }
}
