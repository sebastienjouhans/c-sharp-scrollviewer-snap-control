using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinrou.Controls.Events
{
    public class ScrollViewerWithSnapTransitionCompleteEventArg : EventArgs
    {
        public int index { get; internal set; }

        public ScrollViewerWithSnapTransitionCompleteEventArg(int index)
        {
            this.index = index;
        }
    }
}
