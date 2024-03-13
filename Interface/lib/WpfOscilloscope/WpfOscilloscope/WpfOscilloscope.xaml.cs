using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WpfOscilloscopeControl
{
    /// <summary>
    /// Logique d'interaction pour UserControl1.xaml
    /// </summary>
    public partial class WpfOscilloscope : UserControl
    {
        public bool isDisplayActivated = true;
        ConcurrentDictionary<int, XyDataSeries<double, double>> lineDictionary = new ConcurrentDictionary<int, XyDataSeries<double, double>>();
        ConcurrentDictionary<int, ConcurrentQueue<Point>> lineDataDictionary = new ConcurrentDictionary<int, ConcurrentQueue<Point>>();
        Dictionary<int, bool> useDisplayTimerRenderingDictionary = new Dictionary<int, bool>();
        DispatcherTimer displayTimer;
        public WpfOscilloscope()
        {
            InitializeComponent();
            displayTimer = new DispatcherTimer(priority: DispatcherPriority.Background);
            displayTimer.Interval = new System.TimeSpan(0, 0, 0, 0, 100);
            displayTimer.Tick += DisplayTimer_Tick;
            displayTimer.Start();
        }

        private void DisplayTimer_Tick(object sender, System.EventArgs e)
        {
            if (isDisplayActivated)
            {
                for (int i = 0; i < lineDataDictionary.Count; i++)
                {
                    var dataSerie = lineDataDictionary.ElementAt(i);
                    var lineId = dataSerie.Key;
                    if (useDisplayTimerRenderingDictionary[lineId] == true)
                    {
                        /// On ajoute les données aux données à afficher existantes
                        var data = dataSerie.Value;
                        List<double> xData = new List<double>();
                        List<double> yData = new List<double>();

                        xData = data.Select(e1 => e1.X).ToList();
                        yData = data.Select(e2 => e2.Y).ToList();

                        if (xData.Count == yData.Count)
                        {

                            lineDictionary[lineId].Append(xData, yData);
                            /// On supprime les données copiées de la liste de départ
                            lineDataDictionary[lineId] = new ConcurrentQueue<Point>();
                        }
                    }
                }
            }
        }

        public void AddOrUpdateLine(int lineId, int maxNumberOfPoints, string lineName, bool useYAxisRight = true)
        {
            if (LineExist(lineId))
            {
                lineDictionary[lineId] = new XyDataSeries<double, double>(maxNumberOfPoints) { SeriesName = lineName };
                lineDictionary[lineId].FifoCapacity = maxNumberOfPoints;
                lineDataDictionary[lineId] = new ConcurrentQueue<Point>();

                useDisplayTimerRenderingDictionary[lineId] = false;
                //sciChart.RenderableSeries.RemoveAt(id);
            }
            else
            {
                var xyDataSeries = new XyDataSeries<double, double>(maxNumberOfPoints) { SeriesName = lineName };
                lineDictionary.AddOrUpdate(lineId, xyDataSeries, (key, value) => xyDataSeries);
                lineDictionary[lineId].FifoCapacity = maxNumberOfPoints;
                var dataPointSerie = new ConcurrentQueue<Point>();
                lineDataDictionary.AddOrUpdate(lineId, dataPointSerie, (key, value) => dataPointSerie);
                useDisplayTimerRenderingDictionary.Add(lineId, false);

                var lineRenderableSerie = new FastLineRenderableSeries();
                lineRenderableSerie.Name = "lineRenderableSerie" + lineId.ToString();
                lineRenderableSerie.DataSeries = lineDictionary[lineId];
                lineRenderableSerie.DataSeries.AcceptsUnsortedData = true;
                if (useYAxisRight)
                    lineRenderableSerie.YAxisId = "RightYAxis";
                else
                    lineRenderableSerie.YAxisId = "LeftYAxis";

                sciChart.RenderableSeries.Add(lineRenderableSerie);
            }
        }

        public void ResetGraph()
        {
            foreach (var serie in sciChart.RenderableSeries)
            {
                serie.DataSeries.Clear();
            }
        }

        public bool LineExist(int lineId)
        {
            return lineDictionary.ContainsKey(lineId);
        }


        //public void SetTitle(string title)
        //{
        //    sciChartBox.Header = title;
        //}
        public void SetSerieName(int lineId, string name)
        {
            if (LineExist(lineId))
            {
                lineDictionary[lineId].SeriesName = name;
            }
        }

        public void ChangeLineColor(string lineName, Color color)
        {
            sciChart.RenderableSeries.Single(x => x.DataSeries.SeriesName == lineName).Stroke = color;
        }

        public void ChangeLineColor(int lineId, Color color)
        {
            if (LineExist(lineId))
            {
                sciChart.RenderableSeries.Single(x => x.DataSeries.SeriesName == lineDictionary[lineId].SeriesName).Stroke = color;
            }
        }

        public void DrawOnlyPoints(int lineId)
        {
            if (LineExist(lineId))
            {
                sciChart.RenderableSeries.Single(x => x.DataSeries.SeriesName == lineDictionary[lineId].SeriesName).Stroke = Color.FromArgb(0, 255, 255, 255);
            }
        }

        public void AddPointToLine(int lineId, double x, double y)
        {
            if (isDisplayActivated)
            {
                if (LineExist(lineId))
                {
                    lineDataDictionary[lineId].Enqueue(new Point(x, y));
                    useDisplayTimerRenderingDictionary[lineId] = true;
                }
            }
        }

        //public void AddPointToLine(int lineId, Point point)
        //{
        //    if (isDisplayActivated)
        //    {
        //        if (LineExist(lineId))
        //        {
        //            lineDictionary[lineId].Append(point.X, point.Y);
        //            if (lineDictionary[lineId].Count > lineDictionary[lineId].Capacity)
        //                lineDictionary[lineId].RemoveAt(0);
        //        }
        //    }
        //}

        //public void AddPointListToLine(int lineId, List<Point> pointList)
        //{
        //    if (isDisplayActivated)
        //    {
        //        if (LineExist(lineId))
        //        {
        //            lineDictionary[lineId].Append(pointList.Select(e => e.X).ToList(), pointList.Select(e2 => e2.Y).ToList());
        //            if (lineDictionary[lineId].Count > lineDictionary[lineId].Capacity)
        //                lineDictionary[lineId].RemoveAt(0);
        //        }
        //    }
        //}
        public void UpdatePointListOfLine(int lineId, List<Point> pointList)
        {
            if (isDisplayActivated)
            {
                if (LineExist(lineId))
                {
                    lineDataDictionary[lineId] = new ConcurrentQueue<Point>(pointList);
                    useDisplayTimerRenderingDictionary[lineId] = true;
                }
            }
        }

    }
}
