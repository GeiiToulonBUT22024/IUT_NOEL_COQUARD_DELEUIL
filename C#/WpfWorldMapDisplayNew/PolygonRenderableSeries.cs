using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Visuals.RenderableSeries;
using SciChart.Drawing.Common;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using Utilities;

namespace WpfWorldMapDisplay
{
    public class PolygonRenderableSeries : CustomRenderableSeries
    {
        ConcurrentDictionary<int, PolygonExtended> polygonList = new ConcurrentDictionary<int, PolygonExtended>();
        XyDataSeries<double, double> lineData = new XyDataSeries<double, double> { }; //Nécessaire pour l'update d'affichage

        public PolygonRenderableSeries()
        {
        }

        public void AddOrUpdatePolygonExtended(int id, PolygonExtended p)
        {
            polygonList.AddOrUpdate(id, p, (key, value) => p);
        }

        public void UpdatePolygonExtendedList(ConcurrentDictionary<int, PolygonExtended> pList)
        {
            polygonList = pList;
        }

        public void Clear()
        {
            polygonList.Clear();
        }

        public int Count()
        {
            return polygonList.Count();
        }

        public void RedrawAll()
        {
            //TODO Attention : Permet de déclencher l'update : workaround pas classe du tout
            lineData.Clear();
            lineData.Append(1, 1);
            DataSeries = lineData;
        }

        protected override void Draw(IRenderContext2D renderContext, IRenderPassData renderPassData)
        {
            base.Draw(renderContext, renderPassData);

            // Create a line drawing context. Make sure you dispose it!
            // NOTE: You can create mutliple line drawing contexts to draw segments if you want
            //       You can also call renderContext.DrawLine() and renderContext.DrawLines(), but the lineDrawingContext is higher performance
            CustomDraw(renderContext);
        }

        private void CustomDraw(IRenderContext2D renderContext)
        {
            IPen2D linePen = null;
            float currentBorderWidth = 1; ;
            Color currentBorderColor = Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            double currentBorderOpacity = 1;

            IPathDrawingContext lineDrawingContext;

            foreach (var p in polygonList)
            {
                PolygonD polygon = p.Value.polygon;

                if (polygon.Points.Count > 0)
                {
                    Point initialPoint = GetRenderingPoint(new Point(polygon.Points[0].X, polygon.Points[0].Y));

                    System.Windows.Media.Color backgroundColor = System.Windows.Media.Color.FromArgb(p.Value.backgroundColor.A, p.Value.backgroundColor.R, p.Value.backgroundColor.G, p.Value.backgroundColor.B);

                    using (var brush = renderContext.CreateBrush(backgroundColor))
                    {
                        //IEnumerable<Point> points; // define your points
                        PointCollection ptColl = new PointCollection();
                        for(int i=0; i<polygon.Points.Count; i++)
                        {
                            ptColl.Add(new Point(polygon.Points[i].X, polygon.Points[i].Y));
                        }
                        renderContext.FillPolygon(brush, GetRenderingPoints(ptColl));
                    }

                    //// Create a pen to draw. Make sure you dispose it! 
                    System.Windows.Media.Color borderColor = System.Windows.Media.Color.FromArgb(p.Value.borderColor.A, p.Value.borderColor.R, p.Value.borderColor.G, p.Value.borderColor.B);
                    try
                    {
                        if (p.Value.borderWidth != currentBorderWidth || p.Value.borderOpacity != currentBorderOpacity || borderColor != currentBorderColor || linePen == null)
                        {
                            /// Cette fonction prend du temps, on ne l'exécute que quand on doit changer de type de linePen
                            currentBorderColor = borderColor;
                            currentBorderOpacity = p.Value.borderOpacity;
                            currentBorderWidth = p.Value.borderWidth;
                            linePen = renderContext.CreatePen(borderColor, this.AntiAliasing, p.Value.borderWidth, p.Value.borderOpacity, p.Value.borderDashPattern);
                        }
                        lineDrawingContext = renderContext.BeginLine(linePen, initialPoint.X, initialPoint.Y);
                        for (int i = 1; i < polygon.Points.Count; i++)
                        {
                            try
                            {
                                var pt = GetRenderingPoint(new Point(polygon.Points[i].X, polygon.Points[i].Y));
                                lineDrawingContext.MoveTo(pt.X, pt.Y);
                            }
                            catch
                            {
                                System.Console.WriteLine("Exception PolygonRenderableSeries : CustomDraw");
                            }
                        }
                        var ptFinal = GetRenderingPoint(new Point(polygon.Points[0].X, polygon.Points[0].Y));
                        lineDrawingContext.MoveTo(ptFinal.X, ptFinal.Y);
                        lineDrawingContext.End();
                    }
                    catch (Exception e)
                    {
                        System.Console.WriteLine("Crash LinePen Polygon Renderable Series");
                        System.Console.WriteLine(e.ToString());

                    };
                }
            }
        }

        private Point GetRenderingPoint(Point pt)
        {
            // Get the coordinateCalculators. See 'Converting Pixel Coordinates to Data Coordinates' documentation for coordinate transforms
            var xCoord = CurrentRenderPassData.XCoordinateCalculator.GetCoordinate(pt.X);
            var yCoord = CurrentRenderPassData.YCoordinateCalculator.GetCoordinate(pt.Y);

            //if (CurrentRenderPassData.IsVerticalChart)
            //{
            //    Swap(ref xCoord, ref yCoord);
            //}

            return new Point(xCoord, yCoord);
        }
        private PointCollection GetRenderingPoints(PointCollection ptColl)
        {
            PointCollection ptCollRender = new PointCollection();
            foreach (var pt in ptColl)
            {
                // Get the coordinateCalculators. See 'Converting Pixel Coordinates to Data Coordinates' documentation for coordinate transforms
                var xCoord = CurrentRenderPassData.XCoordinateCalculator.GetCoordinate(pt.X);
                var yCoord = CurrentRenderPassData.YCoordinateCalculator.GetCoordinate(pt.Y);
                ptCollRender.Add(new Point(xCoord, yCoord));
            }

            return ptCollRender;
        }
    }
}
