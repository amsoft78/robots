using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PaintAutomationLib
{
    public struct RuntimeParams
    {
        public uint elementsCount;
        public uint redCnt;
        public uint greenCnt;
        public uint blueCnt;

        public TimeSpan redTime;
        public TimeSpan greenTime;
        public TimeSpan blueTime;
    }

    public class Garage
    {
        private readonly RuntimeParams parameters;
        IElementsQs elementsQq;
        List<PaintRobot> robots;
        List<Task> tasks;
        IStateChangeObserver observer;

        public Garage(RuntimeParams _parameters, IStateChangeObserver _observer)
        {
            parameters = _parameters;
            elementsQq = new ElementsQs(_observer);
            robots = new List<PaintRobot>();
            tasks = new List<Task>();
            observer = _observer;
        }

        private void StartRobot(Color color, TimeSpan workTime, List<PaintRobot> robots, List<Task> tasks)
        {
            PaintRobot robot = new PaintRobot(color, parameters.redTime, elementsQq, observer);
            robots.Add(robot);
            var task = Task.Factory.StartNew(() => { robot.PaintingLoop(); });
            tasks.Add(task);
        }

        // can be called both before or after working robots start
        private void ProduceInputElements()
        {
            {
                for (uint j = 1; j <= parameters.elementsCount; ++j)
                {
                    Element nelem = new Element(new ElementId { id = j });
                    elementsQq.PushNewElement(nelem);
                }
                elementsQq.SignalProductionFinished();
            }
        }

        // this is synchronized method
        public void DoWork()
        {
            lock (elementsQq)
            {
                int i = 0;
                while (i < parameters.redCnt || i < parameters.greenCnt || i < parameters.blueCnt)
                {
                    if (i < parameters.redCnt)
                    {
                        StartRobot(Color.Red, parameters.redTime, robots, tasks);
                    }
                    if (i < parameters.greenCnt)
                    {
                        StartRobot(Color.Green, parameters.greenTime, robots, tasks);
                    }
                    if (i < parameters.blueCnt)
                    {
                        StartRobot(Color.Blue, parameters.blueTime, robots, tasks);
                    }
                    i++;
                }

                ProduceInputElements();

                // now it works, wait for finish
                foreach (var t in tasks)
                {
                    t.Wait();
                }
            }
        }

        public void CancelWork()
        {
            elementsQq.StopNow();
        }

    }
}

