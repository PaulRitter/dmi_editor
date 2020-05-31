using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    //TODO hotspot-tool
    public abstract class EditorTool
    {
        public abstract string Name { get; }
        //public readonly XYZ icon;
        internal MainWindow main;

        public EditorTool(MainWindow main)
        {
            this.main = main;
        }

        //returns true if bitmap was changed
        public virtual bool pixelAct(Bitmap current, int x, int y) { return false; }

        public override string ToString()
        {
            return Name;
        }
    }
}
