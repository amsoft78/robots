using System.Collections.Generic;

namespace PaintAutomationLib
{
    public class Element
    {
        public ElementId elementId { get;  }
        private HashSet<Color> paintedOn; // what colours am I painted on?

        public Element(ElementId _elementId)
        {
            elementId = _elementId;
            paintedOn = new HashSet<Color>();
        }

        public bool IsPaintedOn (Color col)
        {
            return paintedOn.Contains(col);
        }

        public void SetPaintedOn (Color col)
        {
            // should be checked first or not at this point?
            paintedOn.Add(col);
        }
    }
}