using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DMIEditor.Tools
{
    public abstract class EditorTool
    {
        public abstract string Name { get; }
        //public readonly XYZ icon;
        internal MainWindow main;

        public EditorTool(MainWindow main)
        {
            this.main = main;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
