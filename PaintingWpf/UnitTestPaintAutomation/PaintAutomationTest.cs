using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MoqExtensions;
using PaintAutomationLib;

namespace UnitTestPaintAutomation
{

    #region ManulMock of IElementsQs returning single
    class SingleElementsQs : IElementsQs
    {
        private Element single;

        public SingleElementsQs(Element _single)
        {
            single = _single;
        }

        public Element PopNextUnprocessed()
        {
            // one and only element returned once, and immediatelly forgotten
            if (single != null)
            {
                Element to_return = single;
                single = null;
                return to_return;
            }
            return null;

        }

        public void PushNewElement(Element element)
        {
            //nothing to do
        }

        public void PushProcessed(Element element)
        {
            //nothing to do
        }

        public void ReturnBackUnprocessed(Element element)
        {
            //nothing to do
        }

        public void SignalProductionFinished()
        {
            //nothing to do
        }

        public void StopNow()
        {
            // nothing to do
        }
    }
    #endregion

    class NulObserver : IStateChangeObserver
    {
        public void OnBeginPainting(ElementId elem, Color col)
        {
            // do nothing
        }

        public void OnPainted(ElementId elem, Color col)
        {
            // do nothing
        }

        public void OnPaintedAllColors(ElementId elem)
        {
            // do nothing
        }
    }

    [TestClass]
    public class PaintAutomationTest
    {
        [TestMethod]
        public void RobotPaintsInProperColorManual()
        {
            // using manually mocked class "SingleElementsQs"
            Element element = new Element(new ElementId { id = 1 });
            IElementsQs fakeQs = new SingleElementsQs(element);
            NulObserver nulObserver = new NulObserver();

            PaintRobot robot = new PaintRobot
                (Color.Green, TimeSpan.FromMilliseconds(10), fakeQs, nulObserver);
            robot.PaintingLoop();
            Assert.IsTrue(element.IsPaintedOn(Color.Green));
            Assert.IsFalse(element.IsPaintedOn(Color.Red));
            Assert.IsFalse(element.IsPaintedOn(Color.Blue));
        }

        [TestMethod]
        public void RobotPaintsInProperColorWithMoq()
        {
            Element element = new Element(new ElementId { id = 1 });
            var qsMock = new Mock<IElementsQs>();

            qsMock.Setup(x => x.PopNextUnprocessed()).ReturnsInOrder(
                element, null);

            NulObserver nulObserver = new NulObserver();

            PaintRobot robot = new PaintRobot
                (Color.Green, TimeSpan.FromMilliseconds(10), qsMock.Object, nulObserver);
            robot.PaintingLoop();
            qsMock.Verify(mock => mock.PushProcessed(element), Times.Once());
            qsMock.Verify(mock => mock.ReturnBackUnprocessed(element), Times.Never());
            Assert.IsTrue(element.IsPaintedOn(Color.Green));
            Assert.IsFalse(element.IsPaintedOn(Color.Red));
            Assert.IsFalse(element.IsPaintedOn(Color.Blue));
        }

        [TestMethod]
        public void RobotDoesNotPaintTwice()
        {
            Element element = new Element(new ElementId { id = 1 });
            element.SetPaintedOn(Color.Red);
            element.SetPaintedOn(Color.Green);
            element.SetPaintedOn(Color.Blue);
            var qsMock = new Mock<IElementsQs>();

            qsMock.Setup(x => x.PopNextUnprocessed()).ReturnsInOrder(
                element, null);

            NulObserver nulObserver = new NulObserver();

            PaintRobot robot = new PaintRobot
                (Color.Red, TimeSpan.FromMilliseconds(10), qsMock.Object, nulObserver);
            robot.PaintingLoop();

            qsMock.Verify(mock => mock.ReturnBackUnprocessed(element), Times.Once());
            qsMock.Verify(mock => mock.PushProcessed(element), Times.Never());
        }

    }
}
