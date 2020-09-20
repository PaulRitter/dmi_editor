using System.Collections.Generic;
using System.Drawing;
using DMI_Parser.Extended;

namespace DMIEditor.Tools
{
    public class BoxSelector : DragTool
    {
        public override string Name => "Boxselector";

        public override void OnDeselected()
        {
            ImageEditor.ClearSelection();
        }

        protected override void OnDrawMove(DmiEXImage dmiExImage, Point p)
        {
            var startingPoint = StartingPoint.Equals(new Point(-1, -1)) ? p : StartingPoint;
            List<Point> selectedPoints = GetPointsInBox(startingPoint, p);
            ImageEditor.SetSelection(selectedPoints.ToArray());
        }

        private List<Point> GetPointsInBox(Point start, Point end)
        {
            int start_x;
            int end_x;
            if (start.X < end.X)
            {
                start_x = start.X;
                end_x = end.X;
            }
            else
            {
                start_x = end.X;
                end_x = start.X;
            }

            int start_y;
            int end_y;
            if (start.Y < end.Y)
            {
                start_y = start.Y;
                end_y = end.Y;
            }
            else
            {
                start_y = end.Y;
                end_y = start.Y;
            }
            
            List<Point> result = new List<Point>();
            for (int x = start_x; x < end_x+1; x++)
            {
                for (int y = start_y; y < end_y+1; y++)
                {
                    result.Add(new Point(x,y));
                }
            }

            return result;
        }
    }
}