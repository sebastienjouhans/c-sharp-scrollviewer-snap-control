using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinrou.Controls.Events
{
    public class ScrollViewerWithSnapSelectedItemChangedEventArgs : EventArgs
    {
        public int index { get; internal set; }

        public ScrollViewerWithSnapSelectedItemChangedEventArgs(int index)
        {
            this.index = index;
        }
    }
}
