using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public class Fill : ClickTool
    {
        public override string Name => "Fill";
        public Fill(MainWindow main) : base(main) { }

        public override void PixelAct(Point p)
        {
            Color oldColor = getPixel(p);

            if (oldColor == main.GetColor())
                return;

            processPixel(oldColor, p, new List<Point>());
        }

        //checks and modifies current pixel and proceeds to check cardinal neighbours
        //ignore list is shared to avoid loops
        private void processPixel(Color oldColor, Point pixel, List<Point> alreadyProcessed)
        {
            foreach (Point point in alreadyProcessed)
            {
                if (point.Equals(pixel))
                {
                    return;
                }
            }
            alreadyProcessed.Add(pixel);
            if (pixel.X >= ImageWidth || pixel.X < 0)
                return;
            if (pixel.Y >= ImageHeight || pixel.Y < 0)
                return;

            if (oldColor.Equals(getPixel(pixel)))
            {
                setPixel(pixel, main.GetColor());

                processPixel(oldColor, new Point(pixel.X + 1, pixel.Y), alreadyProcessed);
                processPixel(oldColor, new Point(pixel.X - 1, pixel.Y), alreadyProcessed);
                processPixel(oldColor, new Point(pixel.X, pixel.Y + 1), alreadyProcessed);
                processPixel(oldColor, new Point(pixel.X, pixel.Y - 1), alreadyProcessed);
            }
        }
    }
}
