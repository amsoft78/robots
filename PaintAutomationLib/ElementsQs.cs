using System;
//using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PaintAutomationLib
{

    public class ElementsQs
        : IElementsQs
    {
        private Queue<Element> inQ;
        private Queue<Element> outQ;
        private bool production_in_progress;
        private uint work_in_progress; // element being processed. Need to calculate stop condition
        Object guard;
        IStateChangeObserver progress_observer;

        public ElementsQs(IStateChangeObserver _progress_observer)
        {
            inQ = new Queue<Element>(); // copy input elements collection into input Q
            outQ = new Queue<Element>();
            production_in_progress = true; // waiting for new elements to come
            work_in_progress = 0;
            guard = new Object();
            progress_observer = _progress_observer;
        }

        public void PushNewElement(Element element)
        {
            lock (guard)
            {
                if (!production_in_progress)
                {
                    throw new InvalidOperationException("Production arleady finished");
                }
                inQ.Enqueue(element);
                System.Threading.Monitor.PulseAll(guard);
            }
        }

        public void SignalProductionFinished()
        {
            lock (guard)
            {
                production_in_progress = false;
                System.Threading.Monitor.PulseAll(guard);
            }
        }

        // hang if some elements will be availaible later
        // return null if ultimately nothing to do
        public Element PopNextUnprocessed()
        {
            bool checkAgain = false;
            do
            {
                lock (guard)
                {
                    if (inQ.Count > 0)
                    {
                        ++work_in_progress;
                        return inQ.Dequeue();
                    }

                    if (production_in_progress || work_in_progress > 0)
                    {
                        // wait for newly procuded elem, or some processed by somebody else
                        System.Threading.Monitor.Wait(guard);
                        checkAgain = true;
                    }
                    else
                    {
                        checkAgain = false;
                    }
                }
            }
            while (checkAgain);

            // no more chances to see any new element
            return null;
        }

        public void PushProcessed(Element element)
        {
            // check if it is completed or needs furher processing
            bool isCompleted = 
                element.IsPaintedOn(Color.Red) &&
                element.IsPaintedOn(Color.Green) &&
                element.IsPaintedOn(Color.Blue);
            lock(guard)
            {
                --work_in_progress;
                if (isCompleted)
                {
                    outQ.Enqueue(element);
                    progress_observer.OnPaintedAllColors(element.elementId);
                }
                else
                { 
                    inQ.Enqueue(element);
                }
                System.Threading.Monitor.PulseAll(guard);
            }
        }

        public void ReturnBackUnprocessed(Element element)
        {
            lock (guard)
            {
                --work_in_progress;
                inQ.Enqueue(element);
                System.Threading.Monitor.PulseAll(guard);
            }
        }

        // just cancell all the remaining items
        public void StopNow()
        {
            lock (guard)
            {
                inQ.Clear();
            }
        }

    }
}
