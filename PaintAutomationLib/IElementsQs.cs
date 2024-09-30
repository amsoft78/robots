using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintAutomationLib
{
    public interface IElementsQs
    {
        // filling input Q by probucer
        void PushNewElement(Element element);
        void SignalProductionFinished();
        
        // working
        Element PopNextUnprocessed(); // blocking methos
        void ReturnBackUnprocessed(Element element); // return the unprocessed element back
        void PushProcessed(Element element); // push processed element to further processing or to completed Q

        // stops immediatelly, skips remaining items
        void StopNow();
    }
}
