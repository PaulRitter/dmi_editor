using System;
using System.Collections.Generic;
using DMI_Parser;

namespace DMIEditor
{
    public class DmiEX : Dmi
    {
        public DmiEX(float version, int width, int height) : base(version, width, height)
        {
        }

        public override ICloneable CreateEmptyImage()
        {
            return new DmiEXImage(Width, Height);
        }

        public static DmiEX FromDMI(string path) => FromDMI(Dmi.FromFile(path));
        
        public static DmiEX FromDMI(Dmi dmi)
        {
            DmiEX dmiEx = new DmiEX(dmi.Version, dmi.Width, dmi.Height);
            
            List<DMIState> states = new List<DMIState>();
            foreach (var state in dmi.States)
            {
                states.Add(DmiEXState.FromDMIState(dmiEx, state));
            }

            dmiEx.States = states;

            return dmiEx;
        }
    }
}