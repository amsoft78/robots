using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PaintAutomationLib;

namespace PaintingWpf
{
    public class UIStageChangeObserver:
        PaintAutomationLib.IStateChangeObserver
    {
        MainWindow mainWindow;
        Object guard;
        PaintAutomationLib.RuntimeParams parameters;

        // feedback for processing values
        uint total_processed;
        uint processed_by_red;
        uint processed_by_green;
        uint processed_by_blue;
        // robots state
        uint red_robots_assigned;
        uint green_robots_assigned;
        uint blue_robots_assigned;

        public UIStageChangeObserver (MainWindow _mainWindow)
        {
            mainWindow = _mainWindow;
            guard = new object();
        }

        public void ResetProcessingValues(PaintAutomationLib.RuntimeParams _parameters)
        {
            parameters = _parameters;
            total_processed = 0;
            processed_by_red = 0;
            processed_by_green = 0;
            processed_by_blue = 0;
            red_robots_assigned = 0;
            green_robots_assigned = 0;
            blue_robots_assigned = 0;

            mainWindow.Dispatcher.BeginInvoke( new Action(() => {
                mainWindow.lbCompleted.Content = FormatProgress(0);
                mainWindow.lbLeft.Content = FormatProgress(_parameters.elementsCount);

                mainWindow.lbCompletedRed.Content = FormatProgress(0);
                mainWindow.lbCompletedGreen.Content = FormatProgress(0);
                mainWindow.lbCompletedBlue.Content = FormatProgress(0);
            }));
        }

        // formats int o 67 (32%)
        private static string FormatWithPercents(uint val, uint total)
        {
            // cannot format in this case, exception is not the best ide here
            if (total < 0)
                return ""; 

            double perc = (double) val / total * 100.0;
            return string.Format("{0} ({1:0.00}%)", val, perc);
        }

        // formats as a percent of total
        private string FormatProgress(uint val)
        {
            return FormatWithPercents(val, parameters.elementsCount);
        }

        private string FormatRobots(uint used, uint total)
        {
            return string.Format($"{used} / {total}");
        }

        public void OnBeginPainting(ElementId elem, PaintAutomationLib.Color col)
        {

            lock (guard)
            {
                Action action = null;
                switch (col)
                {
                    case PaintAutomationLib.Color.Red:
                        red_robots_assigned++;
                        action = new Action(() =>
                        {
                            mainWindow.elements_view_data[(int)(elem.id - 1)].Status = ElementView.PaintingStatus.Painting_Red;
                            mainWindow.lbRedUsed.Content = FormatRobots(red_robots_assigned, parameters.redCnt);
                        });
                        break;
                    case PaintAutomationLib.Color.Green:
                        green_robots_assigned++;
                        action = new Action(() =>
                        {
                            mainWindow.elements_view_data[(int)(elem.id - 1)].Status = ElementView.PaintingStatus.Painting_Green;
                            mainWindow.lbGreenUsed.Content = FormatRobots(green_robots_assigned, parameters.greenCnt);
                        });
                        break;
                    case PaintAutomationLib.Color.Blue:
                        blue_robots_assigned++;
                        action = new Action(() =>
                        {
                            mainWindow.elements_view_data[(int)(elem.id - 1)].Status = ElementView.PaintingStatus.Painting_Blue;
                            mainWindow.lbRedUsed.Content = FormatRobots(blue_robots_assigned, parameters.blueCnt);
                        });
                        break;
                }
                // there is no 4th color, but who knows...
                if (action != null)
                {
                    mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action();
                        mainWindow.UpdateElapsedTime();
                        mainWindow.gridData.Items.Refresh();
                    }));
                }
            }
        }

        public void OnPainted(ElementId elem, PaintAutomationLib.Color col)
        {
            lock (guard)
            {
                Action action = null;

                switch (col)
                {
                    case PaintAutomationLib.Color.Red:
                        processed_by_red++;
                        red_robots_assigned--;
                        action = new Action(() =>
                        {
                            mainWindow.lbCompletedRed.Content = FormatProgress(processed_by_red);
                            mainWindow.lbRedUsed.Content = FormatRobots(red_robots_assigned, parameters.redCnt);
                            mainWindow.elements_view_data[(int)(elem.id - 1)].IsRed = true;
                        });
                        break;
                    case PaintAutomationLib.Color.Green:
                        processed_by_green++;
                        green_robots_assigned--;
                        action = new Action(() =>
                        {
                            mainWindow.lbCompletedGreen.Content = FormatProgress(processed_by_green);
                            mainWindow.lbGreenUsed.Content = FormatRobots(green_robots_assigned, parameters.greenCnt);
                            mainWindow.elements_view_data[(int)(elem.id - 1)].IsGreen = true;
                        });
                        break;
                    case PaintAutomationLib.Color.Blue:
                        processed_by_blue++;
                        blue_robots_assigned--;
                        action = new Action(() =>
                        {
                            mainWindow.lbCompletedBlue.Content = FormatProgress(processed_by_blue);
                            mainWindow.lbBlueUsed.Content = FormatRobots(blue_robots_assigned, parameters.blueCnt);
                            mainWindow.elements_view_data[(int)(elem.id - 1)].IsBlue = true;
                        });
                        break;
                }
                // there is no 4th color, but who knows...
                if (action != null)
                {
                    mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        action();
                        mainWindow.UpdateElapsedTime();
                        mainWindow.elements_view_data[(int)(elem.id - 1)].Status = ElementView.PaintingStatus.Idle;
                        mainWindow.gridData.Items.Refresh();
                    }));
                }
            }
        }

        public void OnPaintedAllColors(ElementId elem)
        {
            lock (guard)
            {
                total_processed++;
                mainWindow.Dispatcher.BeginInvoke(new Action(() =>
                {
                    mainWindow.lbCompleted.Content = FormatProgress(total_processed);
                    mainWindow.lbLeft.Content = FormatProgress(parameters.elementsCount - total_processed);
                    mainWindow.UpdateElapsedTime();

                    mainWindow.elements_view_data[(int)(elem.id - 1)].Status = ElementView.PaintingStatus.Finished;
                    mainWindow.gridData.Items.Refresh();

                    if (total_processed == parameters.elementsCount)
                    {
                        mainWindow.AfterFinishedPreviousJob();
                    }
                }));
            }
        }
    }

    // class hold viev data of element
    internal class ElementView
    {
        public enum PaintingStatus
        {
            Idle,
            Painting_Red,
            Painting_Green,
            Painting_Blue,
            Finished,
        };

        public uint Id { get; set; }
        public bool IsRed { get; set; }
        public bool IsGreen { get; set; }
        public bool IsBlue { get; set; }
        public PaintingStatus Status { get; set; }
    }
    /// <summary>
    /// Logic of MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private PaintAutomationLib.Garage painting_garage;
        private PaintAutomationLib.RuntimeParams process_parameters;
        private UIStageChangeObserver proecess_observer;
        private Task working_garage_task;
        private DateTime processing_started; // processing start time
        internal ObservableCollection<ElementView> elements_view_data;

        public MainWindow()
        {
            InitializeComponent();
            proecess_observer = new UIStageChangeObserver(this);
            elements_view_data = new ObservableCollection<ElementView>();
            gridData.IsReadOnly = true;
        }

#region Interface handlers

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            if (PrepareStartupParameters())
            {
                btnStart.IsEnabled = false;
                btnStop.IsEnabled = true;
                proecess_observer.ResetProcessingValues(process_parameters);
                painting_garage = new PaintAutomationLib.Garage(process_parameters, proecess_observer);
                processing_started = DateTime.UtcNow;
                elements_view_data.Clear();
                InitializeGridData();
                working_garage_task = Task.Factory.StartNew(() =>
                {
                    painting_garage.DoWork();
                });
            }
            // TODO - handle error
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            btnStop.IsEnabled = false;
            if (painting_garage != null)
            {
                painting_garage.CancelWork();
                AfterFinishedPreviousJob();
            }
        }
#endregion

        private bool PrepareStartupParameters()
        {
            // get values, including nullability. note that default "0" is incorrect in algorithm
            process_parameters.elementsCount = txtElementsCount.Value ?? 0;

            process_parameters.redCnt = txtRedRobotsCount.Value ?? 0;
            process_parameters.greenCnt = txtGreenRobotsCount.Value ?? 0;
            process_parameters.blueCnt = txtBlueRobotsCount.Value ?? 0;

            if (process_parameters.elementsCount == 0 ||
                process_parameters.redCnt == 0 ||
                process_parameters.greenCnt == 0 ||
                process_parameters.blueCnt == 0
                )
            {
                return false;
            }
            // in processsing times "0" is fine
            process_parameters.redTime = TimeSpan.FromMilliseconds(txtRedProcessingTime.Value ?? 0);
            process_parameters.greenTime = TimeSpan.FromMilliseconds(txtGreenProcessingTime.Value ?? 0);
            process_parameters.blueTime = TimeSpan.FromMilliseconds(txtBlueProcessingTime.Value ?? 0);

            return true;
        }

        public void AfterFinishedPreviousJob()
        {
            if (working_garage_task != null)
            {
                working_garage_task.Wait();
                working_garage_task = null;
                painting_garage = null;
            }
            btnStart.IsEnabled = true;
            btnStop.IsEnabled = false;
        }

        internal void UpdateElapsedTime()
        {
            TimeSpan elapsed = DateTime.UtcNow - processing_started;
            lbTimeElapsed.Content = string.Format ("{0:0} ms", elapsed.TotalMilliseconds);
        }

        internal void InitializeGridData()
        {
            for (uint i=0; i< process_parameters.elementsCount; ++i)
            {
                elements_view_data.Add(new ElementView()
                { Id = i+1, IsBlue = false, IsGreen = false, IsRed = false, Status = ElementView.PaintingStatus.Idle });
            }
            gridData.ItemsSource = elements_view_data;
        }
    }
}
