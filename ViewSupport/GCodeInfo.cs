using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ViewSupport
{
    public class GCodeInfo
    {
        public StringBuilder sbGcode = new StringBuilder();

        public int ArcCount { get; set; }

        public void Clear()
        {
            this.sbGcode.Length = 0;
            this.ArcCount = 0;
        }
    }
}
