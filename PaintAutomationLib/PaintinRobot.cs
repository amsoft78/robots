using System;
using System.Threading;

namespace PaintAutomationLib
{
    public class PaintRobot
    {
        private readonly Color color;
        private readonly TimeSpan sleepTime;
        private IElementsQs providerQ;
        IStateChangeObserver progress_observer;

        // robot needs to know what colour does he paint...
        // ... where is the input and where is the output.
        public PaintRobot(Color _color, TimeSpan _sleepTime, IElementsQs _providerQ, IStateChangeObserver _progress_observer )
        {
            color = _color;
            sleepTime = _sleepTime;
            providerQ = _providerQ;
            progress_observer = _progress_observer;
        }

        public void PaintingLoop()
        {
            // empty input Q means that the work is done
            Element element = providerQ.PopNextUnprocessed();
            while (element != null)
            {
                if (element.IsPaintedOn(color))
                {
                    // arleady painted, return it to the Q.
                    providerQ.ReturnBackUnprocessed(element);
                }
                else
                {
                    DoPaint(element);
                    providerQ.PushProcessed(element);
                }
                // get new one for next iteration
                element = providerQ.PopNextUnprocessed();
            }
        }

        private void DoPaint(Element element)
        {
            progress_observer.OnBeginPainting(element.elementId, color);
            // painting takes some time...
            Thread.Sleep(sleepTime);
            element.SetPaintedOn(color);
            progress_observer.OnPainted(element.elementId, color);
        }
    }
}