using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintAutomationLib
{
    public interface IStateChangeObserver
    {
        // painting monitoring on robots
        void OnBeginPainting(ElementId elem, Color col);
        void OnPainted(ElementId elem, Color col);
        // total element state monitoring
        void OnPaintedAllColors(ElementId elem);
    }
}
