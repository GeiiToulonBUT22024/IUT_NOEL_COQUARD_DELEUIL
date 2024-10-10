
using Constants;
using EventArgsLibrary;
using MessagesNS;
using PlayBook_NS;
using SciChart.Charting.Model.DataSeries;
using SciChart.Charting.Model.DataSeries.Heatmap2DArrayDataSeries;
using SciChart.Charting.Visuals.Annotations;
using SciChart.Charting2D.Interop;
using SciChart.Drawing.VisualXcceleratorRasterizer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Utilities;
using Color = System.Drawing.Color;

namespace WpfWorldMapDisplay
{
    public class BindingClass
    {
        private string imagePath;

        public string ImagePath
        {
            get { return imagePath; }
            set { imagePath = value; }
        }

        private double x1, x2, y1, y2;
        public double X1 { get { return x1; } set { x1 = value; } }
        public double X2 { get { return x2; } set { x2 = value; } }
        public double Y1 { get { return y1; } set { y1 = value; } }
        public double Y2 { get { return y2; } set { y2 = value; } }
    }

    public enum LocalWorldMapDisplayType
    {
        StrategyMap,
        WayPointMap,
    }

    public enum FieldType
    {
        None,
        EuroBot2021Field,
        EuroBot2022Field,
        RoboCupField,
        CachanField,
    }
    /// <summary>
    /// Logique d'interaction pour ExtendedHeatMap.xaml
    /// </summary>    /// 
    public partial class LocalWorldMapDisplay : UserControl
    {
        LocalWorldMap localWorldMap = new LocalWorldMap();

        LocalWorldMapDisplayType lwmdType = LocalWorldMapDisplayType.StrategyMap; //Par défaut

        FieldType fieldType;

        Random random = new Random();

        public bool IsExtended = false;

        //public bool robotRefDisplay = false;

        double fieldLength = 0;
        double fieldWidth = 0;
        double stadiumLength = 0;
        double stadiumWidth = 0;

        //Liste des robots à afficher
        RobotDisplay robot;

        //Liste des adversaires vus par le robot à afficher
        ConcurrentBag<LocationExtended> OpponentDisplayList = new ConcurrentBag<LocationExtended>();

        //Liste des teammates vus par le robot à afficher
        ConcurrentDictionary<int, LocationExtended> TeammateDisplayDictionary = new ConcurrentDictionary<int, LocationExtended>();

        //Liste des segment à afficher pour débuguer
        ConcurrentBag<SegmentExtended> SegmentDisplayList = new ConcurrentBag<SegmentExtended>();

        //Liste des obstacles vus par le robot à afficher
        ConcurrentBag<ObstacleDisplay> ObstacleDisplayList = new ConcurrentBag<ObstacleDisplay>();

        //Liste des éléments de jeu vus par le robot à afficher
        ConcurrentBag<LocationExtended> ArucoDisplayList = new ConcurrentBag<LocationExtended>();

        //Liste des balles vues par le robot à afficher
        ConcurrentBag<BallDisplay> BallDisplayList = new ConcurrentBag<BallDisplay>();

        BindingClass imageBinding = new BindingClass();

        DispatcherTimer timerDisplay;

        public LocalWorldMapDisplay()
        {
            InitializeComponent();

            this.DataContext = imageBinding;

            timerDisplay = new DispatcherTimer( DispatcherPriority.Render);
            timerDisplay.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timerDisplay.Tick += TimerDisplay_Tick;
            timerDisplay.Start();
        }        
        
        public void SetDefaultSize(double fLength, double fWidth, double sLength, double sWidth)
        {
            fieldLength = fLength;
            fieldWidth = fWidth;    
            stadiumLength = sLength;    
            stadiumWidth = sWidth;
        }

        public void InitLocalWorldMapReference(LocalWorldMap lwm)
        {
            localWorldMap = lwm;
        }

        //bool requiringPostInitOperations = false;
        
        //private void UserControl_Initialized(object sender, EventArgs e)
        //{
        //    requiringPostInitOperations = true;
        //}

        //double[,] BaseStrategyHeatMapDataCopy;
        //public void CopyStrategyHeatMap()
        //{
        //    int width = localWorldMap.heatMapStrategy.BaseHeatMapData.GetLength(0);
        //    int height = localWorldMap.heatMapStrategy.BaseHeatMapData.GetLength(1);

        //    BaseStrategyHeatMapDataCopy = new double[width, height];

        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < height; j++)
        //        {
        //            BaseStrategyHeatMapDataCopy[i, j] = localWorldMap.heatMapStrategy.BaseHeatMapData[i, j];// Math.Sin(2 * Math.PI * i / width * 3);
        //        }
        //    }
        //}

        //double[,] BaseWaypointHeatMapDataCopy;
        //public void CopyWaypointHeatMap()
        //{
        //    int width = localWorldMap.heatMapWaypoint.BaseHeatMapData.GetLength(0);
        //    int height = localWorldMap.heatMapWaypoint.BaseHeatMapData.GetLength(1);

        //    BaseWaypointHeatMapDataCopy = new double[width, height];

        //    for (int i = 0; i < width; i++)
        //    {
        //        for (int j = 0; j < height; j++)
        //        {
        //            BaseWaypointHeatMapDataCopy[i, j] = localWorldMap.heatMapWaypoint.BaseHeatMapData[i, j];// Math.Sin(2 * Math.PI * i / width * 3);
        //        }
        //    }
        //}

        private void TimerDisplay_Tick(object sender, EventArgs e)
        {
            if (robot != null)
            {
                UpdateLocalWorldMapDisplay();
                DisplayWorldMap();
            }
        }

        //Stopwatch swTimerDisplayLWM = new Stopwatch();
        //private void TDisplay_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    //waitForDisplayAuthorization.Set();
        //}

        /// <summary>
        /// Définit l'image en fond de carte
        /// </summary>
        /// <param name="imagePath">Chemin de l'image</param>
        public void SetFieldImageBackGround(string imagePath)
        {
            imageBinding.ImagePath = imagePath;
            imageBinding.X1 = -fieldLength / 2;
            imageBinding.X2 = +fieldLength / 2;
            imageBinding.Y1 = -fieldWidth / 2;
            imageBinding.Y2 = +fieldWidth / 2;
        }

        /// <summary>
        /// Initialise la Local World Map
        /// </summary>
        /// <param name="competition">Spécifie le type de compétition, le réglage des dimensions est automatique</param>
        /// <param name="type">Spécifie si on a une LWM affichant la stratégie ou les waypoints</param>
        /// <param name="imagePath">Chemin de l'image de fond</param>
        public void Init(FieldType fType, LocalWorldMapDisplayType type)
        {
            fieldType = fType;
            lwmdType = type;

            PopulateFieldPolygonList();

            this.sciChartSurface.XAxis.VisibleRange.SetMinMax(-stadiumLength / 2, stadiumLength / 2);
            this.sciChartSurface.YAxis.VisibleRange.SetMinMax(-stadiumWidth / 2, stadiumWidth / 2);

            if (sciChartSurface.RenderSurface.GetType() == typeof(VisualXcceleratorRenderSurface))
            {
                Console.WriteLine("Scichart LocalWorldMapDisplay : DirectX enabled");
            }
        }

        private void PopulateFieldPolygonList()
        {
            switch (fieldType)
            {
                case FieldType.CachanField:
                    SetFieldSize(10, 6, 8, 4);
                    DrawCachanField();
                    break;
                case FieldType.EuroBot2021Field:
                    SetFieldSize(3.4, 2.4, 3.0, 2.0);
                    DrawEurobotField();
                    break;
                case FieldType.EuroBot2022Field:
                    SetFieldSize(3.4, 2.4, 3.0, 2.0);
                    DrawEurobotField();
                    break;
                case FieldType.RoboCupField:
                    SetFieldSize(stadiumLength, stadiumWidth, fieldLength, fieldWidth);
                    DrawSoccerField();
                    break;
                default:
                    //SetFieldSize(stadiumLength, stadiumWidth, fieldLength, fieldWidth);
                    DrawSoccerField();
                    break;
            }
        }

        public void OnSetFieldSize(object sender, FieldSizeArgs e)
        {
            SetFieldSize(e.stadiumLength, e.stadiumWidth, e.fieldLength, e.fieldWidth);
        }

        double stadiumLength_1 = 0;
        double stadiumWidth_1 = 0;
        public void SetFieldSize(double? stadiumLength, double? stadiumWidth, double? fieldLength, double? fieldWidth)
        {
            this.stadiumLength = stadiumLength ?? 22;
            this.stadiumWidth = stadiumWidth ?? 14;
            this.fieldLength = fieldLength ?? 24;
            this.fieldWidth = fieldWidth ?? 16;

            /// Si les dimensions du stade ont changé et qu'elles ne sont pas nulles
            if (stadiumLength_1 != stadiumLength && stadiumWidth_1 != stadiumWidth && this.stadiumLength != 0 && this.stadiumWidth != 0)
            {
                stadiumLength_1 = this.stadiumLength;
                stadiumWidth_1 = this.stadiumWidth;
                this.Dispatcher.Invoke(new Action(delegate ()
                {
                    this.sciChartSurface.XAxis.VisibleRange.SetMinMax(-this.stadiumLength / 2, this.stadiumLength / 2);
                    this.sciChartSurface.YAxis.VisibleRange.SetMinMax(-this.stadiumWidth / 2, this.stadiumWidth / 2);
                }
                ));
                //requiringPostInitOperations = false;
            }
        }

        private void DisplayWorldMap()
        {
            if (robot != null)
            {
                var BoxAnnotationList = sciChartSurface.Annotations.Where(p => p.GetType().Name == "BoxAnnotation").ToList();

                //sciChartSurface.Annotations.Clear();
                //foreach (var elt in BoxAnnotationList)
                //    sciChartSurface.Annotations.Add(elt);
                

                if (fieldType == FieldType.RoboCupField)
                    PolygonTerrainSeries.RedrawAll();

                UpdateBallDisplay();
                UpdateObstaclesDisplay();
                UpdateTeammatesDisplay();
                UpdateOpponentDisplay();
                UpdateArucoDisplay();
                UpdateSegmentDisplay();

                //UpdateHeatmapsDisplay();
                UpdateRobotDisplay();
            }
        }

        public void UpdateWorldMapDisplay()
        {
            //waitForDisplayAuthorization.Set();
        }

        public void InitTeamMate(int robotId, CompetitionType gMode, string playerName)
        {
            switch (gMode)
            {
                case CompetitionType.Cachan:
                    {
                        PolygonExtended robotShape = new PolygonExtended();
                        robotShape.polygon.Points.Add(new PointD(-0.14, -0.18));
                        robotShape.polygon.Points.Add(new PointD(0.14, -0.18));
                        robotShape.polygon.Points.Add(new PointD(0.10, 0));
                        robotShape.polygon.Points.Add(new PointD(0.14, 0.18));
                        robotShape.polygon.Points.Add(new PointD(-0.14, 0.18));
                        robotShape.polygon.Points.Add(new PointD(-0.14, -0.18));
                        robotShape.borderColor = System.Drawing.Color.Blue;
                        robotShape.backgroundColor = System.Drawing.Color.Red;

                        PolygonExtended ghostShape = new PolygonExtended();
                        ghostShape.polygon.Points.Add(new PointD(-0.16, -0.2));
                        ghostShape.polygon.Points.Add(new PointD(0.16, -0.2));
                        ghostShape.polygon.Points.Add(new PointD(0.12, 0));
                        ghostShape.polygon.Points.Add(new PointD(0.16, 0.2));
                        ghostShape.polygon.Points.Add(new PointD(-0.16, 0.2));
                        ghostShape.polygon.Points.Add(new PointD(-0.16, -0.2));
                        ghostShape.backgroundColor = System.Drawing.Color.FromArgb(20, 0, 255, 0);
                        ghostShape.borderColor = System.Drawing.Color.Black;

                        RobotDisplay rd = new RobotDisplay(robotShape, ghostShape, playerName);
                        rd.SetLocation(new Location(0, 0, 0, 0, 0, 0));

                        //TeamMatesDisplayDictionary.AddOrUpdate(robotId, rd, (key, value) => rd);
                        robot = rd;
                    }
                    break;
                case CompetitionType.Eurobot2021:
                    {
                        PolygonExtended robotShape = new PolygonExtended();
                        robotShape.polygon.Points.Add(new PointD(-0.12, -0.12));
                        robotShape.polygon.Points.Add(new PointD(0.12, -0.12));
                        robotShape.polygon.Points.Add(new PointD(0.02, 0));
                        robotShape.polygon.Points.Add(new PointD(0.12, 0.12));
                        robotShape.polygon.Points.Add(new PointD(-0.12, 0.12));
                        robotShape.polygon.Points.Add(new PointD(-0.12, -0.12));
                        robotShape.borderColor = System.Drawing.Color.Blue;
                        robotShape.backgroundColor = System.Drawing.Color.DarkRed;

                        PolygonExtended ghostShape = new PolygonExtended();
                        ghostShape.polygon.Points.Add(new PointD(-0.14, -0.14));
                        ghostShape.polygon.Points.Add(new PointD(0.14, -0.14));
                        ghostShape.polygon.Points.Add(new PointD(0.14, 0.02));
                        ghostShape.polygon.Points.Add(new PointD(0.14, 0.14));
                        ghostShape.polygon.Points.Add(new PointD(-0.14, 0.14));
                        ghostShape.polygon.Points.Add(new PointD(-0.14, -0.14));
                        ghostShape.backgroundColor = System.Drawing.Color.FromArgb(20, 0, 255, 0);
                        ghostShape.borderColor = System.Drawing.Color.Black;

                        RobotDisplay rd = new RobotDisplay(robotShape, ghostShape, playerName);
                        rd.SetLocation(new Location(0, 0, 0, 0, 0, 0));

                        //TeamMatesDisplayDictionary.AddOrUpdate(robotId, rd, (key, value) => rd);
                        robot = rd;
                    }
                    break;
                case CompetitionType.Eurobot2022:
                    {
                        PolygonExtended robotShape = new PolygonExtended();
                        robotShape.polygon.Points.Add(new PointD(-0.12, -0.12));
                        robotShape.polygon.Points.Add(new PointD(0.12, -0.12));
                        robotShape.polygon.Points.Add(new PointD(0.02, 0));
                        robotShape.polygon.Points.Add(new PointD(0.12, 0.12));
                        robotShape.polygon.Points.Add(new PointD(-0.12, 0.12));
                        //robotShape.polygon.Points.Add(new PointD(-0.12, -0.12));
                        robotShape.borderColor = System.Drawing.Color.Blue;
                        robotShape.backgroundColor = System.Drawing.Color.DarkRed;

                        PolygonExtended ghostShape = new PolygonExtended();
                        ghostShape.polygon.Points.Add(new PointD(-0.14, -0.14));
                        ghostShape.polygon.Points.Add(new PointD(0.14, -0.14));
                        ghostShape.polygon.Points.Add(new PointD(0.14, 0.02));
                        ghostShape.polygon.Points.Add(new PointD(0.14, 0.14));
                        ghostShape.polygon.Points.Add(new PointD(-0.14, 0.14));
                        //ghostShape.polygon.Points.Add(new PointD(-0.14, -0.14));
                        ghostShape.backgroundColor = System.Drawing.Color.FromArgb(20, 0, 255, 0);
                        ghostShape.borderColor = System.Drawing.Color.Black;

                        RobotDisplay rd = new RobotDisplay(robotShape, ghostShape, playerName);
                        rd.SetLocation(new Location(0, 0, 0, 0, 0, 0));

                        //TeamMatesDisplayDictionary.AddOrUpdate(robotId, rd, (key, value) => rd);
                        robot = rd;
                    }
                    break;
                default:
                    {
                        PolygonExtended robotShape = new PolygonExtended();
                        robotShape.polygon.Points.Add(new PointD(-0.25, -0.25));
                        robotShape.polygon.Points.Add(new PointD(0.25, 0));
                        robotShape.polygon.Points.Add(new PointD(-0.25, 0.25));
                        robotShape.polygon.Points.Add(new PointD(0, 0));
                        //robotShape.polygon.Points.Add(new PointD(-0.25, -0.25));
                        robotShape.borderColor = System.Drawing.Color.Blue;
                        robotShape.backgroundColor = System.Drawing.Color.Red;

                        PolygonExtended ghostShape = new PolygonExtended();
                        ghostShape.polygon.Points.Add(new PointD(-0.27, -0.27));
                        ghostShape.polygon.Points.Add(new PointD(0.27, 0.02));
                        ghostShape.polygon.Points.Add(new PointD(-0.27, 0.27));
                        ghostShape.polygon.Points.Add(new PointD(0, 0));
                        //ghostShape.polygon.Points.Add(new PointD(-0.27, -0.27));
                        ghostShape.backgroundColor = System.Drawing.Color.FromArgb(20, 0, 255, 0);
                        ghostShape.borderColor = System.Drawing.Color.Black;

                        RobotDisplay rd = new RobotDisplay(robotShape, ghostShape, playerName);
                        rd.SetLocation(new Location(0, 0, 0, 0, 0, 0));

                        //TeamMatesDisplayDictionary.AddOrUpdate(robotId, rd, (key, value) => rd);
                        robot = rd;
                    }
                    break;
            }

            //LocalWorldMapTitle.Text = "LWM " + playerName.ToString();
        }

        public void AddOrUpdateTextAnnotation(string annotationName, string annotationText, double posX, double posY, System.Drawing.Color color, int fontsize = 8)
        {
            var textAnnotationList = sciChartSurface.Annotations.Where(annotation => annotation.GetType().Name == "TextAnnotation").ToList();


            var annot = textAnnotationList.FirstOrDefault(c => ((TextAnnotation)c).Name == "R" + annotationName + "r");
            if (annot == null)
            {
                TextAnnotation textAnnot = new TextAnnotation();
                textAnnot.Text = annotationText;
                textAnnot.Name = "R" + annotationName + "r";
                textAnnot.X1 = posX;
                textAnnot.Y1 = posY;
                textAnnot.HorizontalAnchorPoint = HorizontalAnchorPoint.Center;
                textAnnot.VerticalAnchorPoint = VerticalAnchorPoint.Bottom;
                System.Windows.Media.Color mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                textAnnot.Foreground = new System.Windows.Media.SolidColorBrush(mediaColor);
                textAnnot.FontWeight = FontWeights.Bold;
                textAnnot.FontSize = fontsize;
                sciChartSurface.Annotations.Add(textAnnot);
            }
            else
            {
                ((TextAnnotation)annot).Text = annotationText;
                ((TextAnnotation)annot).Name = "R" + annotationName + "r";
                annot.X1 = posX;
                annot.Y1 = posY;
                //((TextAnnotation)annot).Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(0x88, 0xFF, 0xFF, 0xFF));
                System.Windows.Media.Color mediaColor = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
                ((TextAnnotation)annot).Foreground = new System.Windows.Media.SolidColorBrush(mediaColor);
                ((TextAnnotation)annot).FontWeight = FontWeights.Bold;
                ((TextAnnotation)annot).FontSize = fontsize;
            }
        }

        Stopwatch swUpdateLWM = new Stopwatch();
         
        private void UpdateLocalWorldMapDisplay()
        {
            swUpdateLWM.Restart();
            int robotId = localWorldMap.RobotId;
            UpdateField(localWorldMap.mapCenter);
            UpdateRobotLocation(localWorldMap.robotLocation, localWorldMap.mapCenter);
            UpdateRobotPlayingAction(localWorldMap.playingAction);
            UpdatePlayingSide(localWorldMap.playingSide);
            UpdateRobotGhostLocation(localWorldMap.robotGhostLocation, localWorldMap.mapCenter);
            UpdateRobotDestination(localWorldMap.destinationLocation, localWorldMap.mapCenter);
            UpdateRobotWayPointD(localWorldMap.waypointLocation, localWorldMap.mapCenter);

            //AddOrUpdateTextAnnotation("BallStatus", localWorldMap.messageBallHandlingDisplay, localWorldMap.robotLocation.X, localWorldMap.robotLocation.Y + 0.5, System.Drawing.Color.White, 14);
            AddOrUpdateTextAnnotation("BallStatus", localWorldMap.messageBallHandlingDisplay, fieldLength / 2 * 0.7, fieldWidth / 2 - 0.1, System.Drawing.Color.LightGray, 16);
            AddOrUpdateTextAnnotation("PrehensionStatus", localWorldMap.messagePrehensionDisplay, -fieldLength / 2 * 0.7, fieldWidth / 2 - 0.1, System.Drawing.Color.LightGray, 16);
            //AddOrUpdateTextAnnotation("RobotAction", localWorldMap.messageActionDisplay, localWorldMap.robotLocation.X, localWorldMap.robotLocation.Y + 0.1);
            AddOrUpdateTextAnnotation("RobotAction", localWorldMap.messageActionDisplay, fieldLength/2 * 0.0 , fieldWidth/2 -0.1, System.Drawing.Color.White, 22);


            //if (lwmdType == LocalWorldMapDisplayType.StrategyMap)
            //{
            //    if (localWorldMap.heatMapStrategy != null)
            //    {
            //        UpdateHeatMap(BaseStrategyHeatMapDataCopy);
            //    }
            //}
            //else if (lwmdType == LocalWorldMapDisplayType.WayPointMap)
            //{
            //    if (localWorldMap.heatMapWaypoint != null)
            //        UpdateHeatMap(BaseWaypointHeatMapDataCopy);
            //}

            if (localWorldMap.VoronoiFacetList != null)
            {
                ConcurrentBag<PolygonExtended> VoronoiPolygonRefMapCenter = new ConcurrentBag<PolygonExtended>();
                foreach (var facet in localWorldMap.VoronoiFacetList)
                {
                    var p = new PolygonExtended();
                    p.backgroundColor = facet.Polygon.backgroundColor;
                    p.borderColor = facet.Polygon.borderColor;
                    p.borderWidth = facet.Polygon.borderWidth;
                    p.borderDashPattern = facet.Polygon.borderDashPattern;
                    p.borderOpacity = facet.Polygon.borderOpacity;
                    foreach (var pt in facet.Polygon.polygon.Points)
                    {
                        p.polygon.Points.Add(Toolbox.OffsetLocation(pt, localWorldMap.mapCenter));
                    }
                    VoronoiPolygonRefMapCenter.Add(p);
                }
                UpdateVoronoiPolygons(VoronoiPolygonRefMapCenter);
            }

            //Affichage du lidar uniquement dans la strategy map
            if (lwmdType == LocalWorldMapDisplayType.StrategyMap && localWorldMap.lidarRawPtsList != null)
            {
                var lidarRawPtsListRefMapCenter = localWorldMap.lidarRawPtsList.Select(
                           pt => new PointDExtended(Toolbox.OffsetLocation(pt.Pt, localWorldMap.mapCenter),
                               pt.Color, pt.Width)).ToList();
                UpdateLidarMap(lidarRawPtsListRefMapCenter, LidarDataType.RawPtsList);

                var lidarProcessedPtsListRefMapCenter = localWorldMap.lidarProcessedPtsList.Select(
                           pt => new PointDExtended(Toolbox.OffsetLocation(pt.Pt, localWorldMap.mapCenter),
                               pt.Color, pt.Width)).ToList();
                UpdateLidarMap(lidarProcessedPtsListRefMapCenter, LidarDataType.ProcessedPtsList);

                var strategyPtsListRefMapCenter = localWorldMap.strategyPtsList.Select(
                           pt => new PointDExtended(Toolbox.OffsetLocation(pt.Pt, localWorldMap.mapCenter),
                               pt.Color, pt.Width)).ToList();
                UpdateStrategyObjects(strategyPtsListRefMapCenter);

                if (localWorldMap.lidarSegmentList != null)
                {
                    var lidarSegmentListRefMapCenter = localWorldMap.lidarSegmentList.Select(
                               segment => new SegmentExtended(Toolbox.OffsetLocation(segment.Segment.PtDebut, localWorldMap.mapCenter),
                               Toolbox.OffsetLocation(segment.Segment.PtFin, localWorldMap.mapCenter), segment.Color, segment.Width)).ToList();
                    UpdateLidarSegments(lidarSegmentListRefMapCenter);
                }
            }

            UpdateExternalElementsList(localWorldMap.ExternalElementsList, localWorldMap.mapCenter);
            UpdateTeammateList(localWorldMap.TeammateList, localWorldMap.mapCenter);
            UpdateOpponentList(localWorldMap.OpponentList, localWorldMap.mapCenter);
            UpdateBallLocationList(localWorldMap.BallList, localWorldMap.mapCenter);
            UpdateArucoList(localWorldMap.ArucoList, localWorldMap.mapCenter);
            UpdateSegmentList(localWorldMap.SegmentList, localWorldMap.mapCenter);

        }

        //private void UpdateHeatmapsDisplay()
        //{
        //    if (robot != null)
        //    {
        //        UniformHeatmapDataSeries<double, double, double> heatmapDataSeries = null;
        //        if (lwmdType == LocalWorldMapDisplayType.StrategyMap)
        //        {
        //            if (robot.heatMapStrategy == null)
        //                return;
        //            //heatmapSeries.DataSeries = new UniformHeatmapDataSeries<double, double, double>(data, startX, stepX, startY, stepY);
        //            double xStep = (stadiumLength) / (robot.heatMapStrategy.GetUpperBound(1));
        //            double yStep = (stadiumWidth) / (robot.heatMapStrategy.GetUpperBound(0));
        //            heatmapDataSeries = new UniformHeatmapDataSeries<double, double, double>(robot.heatMapStrategy, -stadiumLength / 2 - xStep / 2, xStep, -stadiumWidth / 2 - yStep / 2, yStep);

        //        }
        //        else
        //        {
        //            if (robot.heatMapWaypoint == null)
        //                return;
        //            //heatmapSeries.DataSeries = new UniformHeatmapDataSeries<double, double, double>(data, startX, stepX, startY, stepY);
        //            double xStep = (stadiumLength) / (robot.heatMapWaypoint.GetUpperBound(1));
        //            double yStep = (stadiumWidth) / (robot.heatMapWaypoint.GetUpperBound(0));
        //            heatmapDataSeries = new UniformHeatmapDataSeries<double, double, double>(robot.heatMapWaypoint, -stadiumLength / 2 - xStep / 2, xStep, -stadiumWidth / 2 - yStep / 2, yStep);
        //        }

        //        //// Apply the dataseries to the heatmap
        //        //if (heatmapDataSeries != null)
        //        //{
        //        //    heatmapSeries.DataSeries = heatmapDataSeries;
        //        //    heatmapDataSeries.InvalidateParentSurface(RangeMode.None);
        //        //}
        //    }
        //}


        public void UpdateBallDisplay()
        {
            //BallDisplay[] BallListCopy;
            //lock (BallDisplayList)
            //{
            //    BallListCopy = new BallDisplay[BallDisplayList.Count];
            //    BallDisplayList.CopyTo(BallListCopy);
            //}

            int indexBall = 0;
            BallPolygon.Clear();
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                foreach (var ball in BallDisplayList)
                {
                    if (ball != null)
                    {
                        //Affichage de la balle
                        BallPolygon.AddOrUpdatePolygonExtended((int)BallId.Ball + indexBall, ball.GetBallPolygon());
                        BallPolygon.AddOrUpdatePolygonExtended((int)BallId.Ball + indexBall + (int)Caracteristique.Speed, ball.GetBallSpeedArrow());
                        indexBall++;
                    }
                }
            }));
            BallPolygon.RedrawAll();

        }
        public void UpdateObstaclesDisplay()
        {
            lock (ObstacleDisplayList)
            {
                int indexObstacle = 0;
                //ObstaclePolygons = new PolygonRenderableSeries();
                try
                {
                    var obstaclesPointsList = ObstacleDisplayList.Select(x => new PointD(x.location.X, x.location.Y));
                    var obstaclesPoints = GetXYDataSeriesFromPoints(obstaclesPointsList.ToList());
                    ObstaclePoints.DataSeries = obstaclesPoints;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception LocalWordMapDisplay : " + e.ToString());
                }
            }
        }
        

        public void UpdateTeammatesDisplay()
        {
            var teammatePointsList = TeammateDisplayDictionary.Select(x => new PointD(x.Value.X, x.Value.Y));
            var teammatePoints = GetXYDataSeriesFromPoints(teammatePointsList.ToList());
            //teammatePoints.Append(1, 1);
            //teammatePoints.Append(2, 2);
            //teammatePoints.Append(3, 3);
            TeammatesPoints.DataSeries = teammatePoints;

            for (int i = 0; i < teammatePointsList.Count(); i++)
            {
                var pt = teammatePointsList.ElementAt(i);                
            }            
        }
        public void UpdateOpponentDisplay()
        {
            //var opponentPointsList = OpponentDisplayDictionary.Select(x => new PointD(x.Value.X, x.Value.Y));
            //var opponentPoints = GetXYDataSeriesFromPoints(opponentPointsList.ToList());
            //OpponentPoints.DataSeries = opponentPoints;
            
            OpponentPolygonSeries.Clear();
            for (int i=0; i< OpponentDisplayList.Count; i++)
            {
                var elt = OpponentDisplayList.ElementAt(i);
                PolygonExtended polygon = GetPolygon(elt.ToPointD(), 8, 0.18, System.Drawing.Color.GreenYellow, System.Drawing.Color.White, 2);
                OpponentPolygonSeries.AddOrUpdatePolygonExtended(i, polygon);

                //AddOrUpdateTextAnnotation("Opponent" + elt.Key, "Opponent " + elt.Key, elt.Value.X, elt.Value.Y);
            }
            OpponentPolygonSeries.RedrawAll();
        }


        public void UpdateSegmentDisplay()
        {
            //var opponentPointsList = OpponentDisplayDictionary.Select(x => new PointD(x.Value.X, x.Value.Y));
            //var opponentPoints = GetXYDataSeriesFromPoints(opponentPointsList.ToList());
            //OpponentPoints.DataSeries = opponentPoints;

            SegmentSeries.Clear();
            for (int i = 0; i < SegmentDisplayList.Count; i++)
            {
                var elt = SegmentDisplayList.ElementAt(i);
                SegmentSeries.AddSegmentExtended(SegmentSeries.Count() + 1, elt);
            }
            SegmentSeries.RedrawAll();
        }

        private static PolygonExtended GetPolygon(PointD loc, int nbSommets, double rayon, System.Drawing.Color bgColor, System.Drawing.Color borderColor, int borderWidth = 1)
        {
            var polygon = new PolygonExtended();
            for (int i = 0; i <= nbSommets; i++)
            {
                polygon.polygon.Points.Add(new PointD(
                    loc.X + rayon * Math.Cos(i * 2 * Math.PI / nbSommets),
                    loc.Y + rayon * Math.Sin(i * 2 * Math.PI / nbSommets)));
            }
            polygon.backgroundColor = bgColor;
            polygon.borderColor = borderColor;
            polygon.borderWidth = borderWidth;
            return polygon;
        }

        public void UpdateArucoDisplay()
        {
            lock (ArucoDisplayList)
            {
                try
                {
                    ArucoPtExtendedSeries.Clear();
                    ArucoPolygonSeries.Clear();
                    var ArucoRedList = ArucoDisplayList.Where(p => p.Id == 47 && p.Type == ObjectType.Aruco).Select(x => new PointDExtended(new PointD(x.X, x.Y), System.Drawing.Color.Red, 10)).ToList();
                    var ArucoGreenList = ArucoDisplayList.Where(p => p.Id == 36 && p.Type == ObjectType.Aruco).Select(x => new PointDExtended(new PointD(x.X, x.Y), System.Drawing.Color.Green, 10)).ToList();
                    var ArucoBlueList = ArucoDisplayList.Where(p => p.Id == 13 && p.Type == ObjectType.Aruco).Select(x => new PointDExtended(new PointD(x.X, x.Y), System.Drawing.Color.Blue, 10)).ToList();
                    var ArucoBrownList = ArucoDisplayList.Where(p => p.Id == 17 && p.Type == ObjectType.Aruco).Select(x => new PointDExtended(new PointD(x.X, x.Y), System.Drawing.Color.Orange, 10)).ToList();


                    double rayonHexagone = 0.075;
                    for (int i = 0; i < ArucoRedList.Count; i++)
                    {
                        var elt = ArucoRedList.ElementAt(i);
                        PolygonExtended polygon = GetPolygon(elt.Pt, 6, rayonHexagone, Color.Red, Color.Red);
                        ArucoPolygonSeries.AddOrUpdatePolygonExtended(ArucoPolygonSeries.Count(), polygon);
                    }
                    for (int i = 0; i < ArucoBlueList.Count; i++)
                    {
                        var elt = ArucoBlueList.ElementAt(i);
                        PolygonExtended polygon = GetPolygon(elt.Pt, 6, rayonHexagone, Color.Blue, Color.Blue);
                        ArucoPolygonSeries.AddOrUpdatePolygonExtended(ArucoPolygonSeries.Count(), polygon);
                    }
                    for (int i = 0; i < ArucoGreenList.Count; i++)
                    {
                        var elt = ArucoGreenList.ElementAt(i);
                        PolygonExtended polygon = GetPolygon(elt.Pt, 6, rayonHexagone, Color.Green, Color.Green);
                        ArucoPolygonSeries.AddOrUpdatePolygonExtended(ArucoPolygonSeries.Count(), polygon);
                    }
                    for (int i = 0; i < ArucoBrownList.Count; i++)
                    {
                        var elt = ArucoBrownList.ElementAt(i);
                        PolygonExtended polygon = GetPolygon(elt.Pt, 6, rayonHexagone, Color.Brown, Color.Orange);
                        ArucoPolygonSeries.AddOrUpdatePolygonExtended(ArucoPolygonSeries.Count(), polygon);
                    }


                    //foreach (var elt in ArucoRedList)
                    //    ArucoPtExtendedSeries.AddPtExtended(elt);
                    //foreach (var elt in ArucoBlueList)
                    //    ArucoPtExtendedSeries.AddPtExtended(elt);
                    //foreach (var elt in ArucoGreenList)
                    //    ArucoPtExtendedSeries.AddPtExtended(elt);
                    //foreach (var elt in ArucoBrownList)
                    //    ArucoPtExtendedSeries.AddPtExtended(elt);

                    //ArucoPtExtendedSeries.RedrawAll();
                    ArucoPolygonSeries.RedrawAll();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception LocalWordMapDisplay : " + e.ToString());
                }
            }
        }

        public void UpdateRobotDisplay()
        {
            //ObjectsPolygonSeries.Clear();

            ///On ne fait un rendu Lidar / ObjetsLidar / PtExtended que pour le robot considéré

            XyDataSeries<double, double> lidarPts = new XyDataSeries<double, double>();
            lidarPts.AcceptsUnsortedData = true;
            var lidarData = robot.GetRobotLidarPoints(LidarDataType.RawPtsList);
            lidarPts.Append(lidarData.XValues, lidarData.YValues);
            LidarPoints.DataSeries = lidarPts;

            //SegmentSeries.Clear();
            //foreach (var segment in robot.GetRobotLidarSegments())
            //{
            //    SegmentSeries.AddSegmentExtended(0, segment);
            //}

            LidarPtExtendedSeries.Clear();
            foreach (var pt in robot.GetRobotLidarExtendedPoints())
            {
                LidarPtExtendedSeries.AddPtExtended(pt);
            }

            StrategyPtExtendedSeries.Clear();
            foreach (var pt in robot.GetRobotStrategyPoints())
            {
                StrategyPtExtendedSeries.AddPtExtended(pt);
            }
            
            VoronoiPolygonSeries.Clear();
            int voronoiPolygonIndex = 0;
            foreach(var polygon in robot.GetVoronoiPolygons())
            {
                VoronoiPolygonSeries.AddOrUpdatePolygonExtended(voronoiPolygonIndex++, polygon);
            }

            //Affichage du robot
            RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(0 + (int)Caracteristique.Ghost, robot.GetRobotGhostPolygon());
            RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(0 + (int)Caracteristique.Speed, robot.GetRobotSpeedArrow());
            RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(0 + (int)Caracteristique.Destination, robot.GetRobotDestinationArrow());
            RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(0 + (int)Caracteristique.WayPoint, robot.GetRobotWaypointArrow());

            AddOrUpdateTextAnnotation("NotreRobot", "Robot "+localWorldMap.RobotId.ToString(), robot.GetRobotLocation().X, robot.GetRobotLocation().Y+fieldWidth/12, Color.White, 16);


            //On trace le robot en dernier pour l'avoir en couche de dessus
            RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(0, robot.GetRobotPolygon());

            ////On affiche en plus les différents objets détectés par le robot
            //foreach (var polygonObject in robot.GetRobotLidarObjects())
            //    ObjectsPolygonSeries.AddOrUpdatePolygonExtended(ObjectsPolygonSeries.Count(), polygonObject);

            //foreach (var r in OpponentDisplayDictionary)
            //{
            //    //Affichage des robots
            //    RobotShapesPolygonSeries.AddOrUpdatePolygonExtended(r.Key, OpponentDisplayDictionary[r.Key].GetRobotPolygon());
            //}

            RobotShapesPolygonSeries.RedrawAll();
            LidarPtExtendedSeries.RedrawAll();
            StrategyPtExtendedSeries.RedrawAll();
            SegmentSeries.RedrawAll();
            VoronoiPolygonSeries.RedrawAll();
        }

        private void UpdateRobotLocation(Location location, Location MapCenter)
        {
            if (location == null)
                return;
            robot.SetLocation(Toolbox.OffsetLocation(location, MapCenter));
        }
        private void UpdateRobotPlayingAction(PlayingAction role)
        {
            robot.SetPlayingAction(role);
        }
        private void UpdatePlayingSide(PlayingSide playSide)
        {
            robot.SetPlayingSide(playSide);
        }

        private void UpdateRobotGhostLocation(Location location, Location mapCenter)
        {
            if (location == null)
                return;

            robot.SetGhostLocation(Toolbox.OffsetLocation(location, mapCenter));
        }

        //private void UpdateHeatMap(double[,] data)
        //{
        //    if (data == null)
        //        return;

        //    if (lwmdType == LocalWorldMapDisplayType.StrategyMap)
        //        robot.SetHeatMapStrategy(data);
        //    if (lwmdType == LocalWorldMapDisplayType.WayPointMap)
        //        robot.SetHeatMapWaypoint(data);
        //}

        private void UpdateLidarMap(List<PointDExtended> lidarMap, LidarDataType type)
        {
            if (lidarMap == null)
                return;

            robot.SetLidarMap(lidarMap, type);
        }
        private void UpdateLidarSegments(List<SegmentExtended> lidarSegmentList)
        {
            if (lidarSegmentList == null)
                return;

            robot.SetLidarSegmentList(lidarSegmentList);
        }

        private void UpdateVoronoiPolygons(ConcurrentBag<PolygonExtended> voronoiPolygons)
        {
            if (voronoiPolygons == null)
                return;

            robot.SetVoronoiPolygons(voronoiPolygons);
        }

        private void UpdateLidarObjects(List<PolarPointListExtended> lidarObjectList)
        {
            if (lidarObjectList == null)
                return;

            robot.SetLidarObjectList(lidarObjectList);
        }
        private void UpdateStrategyObjects(List<PointDExtended> strategyPtsList)
        {
            if (strategyPtsList == null)
                return;

            robot.SetStrategyObjectList(strategyPtsList);
        }

        public void UpdateBallLocationList(ConcurrentBag<LocationExtended> ballList, Location mapCenter)
        {
            if (ballList != null)
            {
                BallDisplayList = new ConcurrentBag<BallDisplay>();
                foreach (var ball in ballList)
                {
                    BallDisplayList.Add(new BallDisplay(Toolbox.OffsetLocation(ball, mapCenter)));
                }
            }
        }

        public void UpdateTeammateList(ConcurrentDictionary<int, LocationExtended> teammateList, Location mapCenter)
        {
            try
            {
                if (teammateList != null)
                {
                    TeammateDisplayDictionary.Clear();
                    foreach (var teammate in teammateList.ToList())
                    {
                        var loc = new LocationExtended(teammate.Value.X, teammate.Value.Y, teammate.Value.Theta, ObjectType.RobotTeam1);
                        var shiftedLoc = Toolbox.OffsetLocation(loc, mapCenter);
                        TeammateDisplayDictionary.AddOrUpdate(teammate.Key, shiftedLoc, (key, value) => shiftedLoc);
                    }
                }
            }
            catch { }
        }

        public void UpdateSegmentList(ConcurrentBag<SegmentExtended> segmentList, Location mapCenter)
        {
            if (segmentList != null)
            {
                SegmentDisplayList = new ConcurrentBag<SegmentExtended>();
                foreach(var s in segmentList.ToList())
                {
                    try
                    {
                        var segment = new SegmentExtended(Toolbox.OffsetLocation(s.Segment.PtDebut, localWorldMap.mapCenter),
                               Toolbox.OffsetLocation(s.Segment.PtFin, localWorldMap.mapCenter), s.Color, s.Width);
                        SegmentDisplayList.Add(segment);
                    }
                    catch
                    {
                        Console.WriteLine("Exception LocalWorldMapDisplay : UpdateSegmentList");
                    }
                }
            }
        }
        
        public void UpdateExternalElementsList(ConcurrentBag<LocationExtended> obstacleList, Location mapCenter)
        {
            if (obstacleList != null)
            {
                ObstacleDisplayList = new ConcurrentBag<ObstacleDisplay>();
                foreach (var obstacleLocation in obstacleList.ToList())
                {
                    ObstacleDisplayList.Add(new ObstacleDisplay(Toolbox.OffsetLocation(obstacleLocation, mapCenter)));
                }
            }
        }

        public void UpdateOpponentList(ConcurrentBag<LocationExtended> opponentDictionary, Location mapCenter)
        {
            if (opponentDictionary != null)
            {
                OpponentDisplayList = new ConcurrentBag<LocationExtended>();
                for(int i=0; i< opponentDictionary.Count; i++)
                {
                    OpponentDisplayList.Add(Toolbox.OffsetLocation(opponentDictionary.ElementAt(i), mapCenter));
                }

            }
        }

        public void UpdateArucoList(ConcurrentBag<LocationExtended> ArucoList, Location mapCenter)
        {
            if (ArucoList != null)
            {
                ArucoDisplayList = new ConcurrentBag<LocationExtended>();
                foreach (var gameElement in ArucoList.ToList())
                {
                    var loc = Toolbox.OffsetLocation(gameElement, mapCenter);
                    ArucoDisplayList.Add(new LocationExtended(loc.X, loc.Y, loc.Theta, 0, 0, 0, gameElement.Type, gameElement.Id));
                }
            }
        }

        public void UpdateRobotWayPointD(Location waypointLocation, Location mapCenter)
        {
            if (waypointLocation == null)
                return;

            Location wpRefLocal = Toolbox.OffsetLocation(waypointLocation, mapCenter);
            robot.SetWayPoint(wpRefLocal.X, wpRefLocal.Y, wpRefLocal.Theta);

        }



        public void UpdateRobotDestination(Location destinationLocation, Location mapCenter)
        {
            if (destinationLocation == null)
                return;

            Location destRefLocal = Toolbox.OffsetLocation(destinationLocation, mapCenter);
            robot.SetDestination(destRefLocal.X, destRefLocal.Y, destRefLocal.Theta);
        }

        ConcurrentDictionary<int, PolygonExtended> fieldPolygonList = new ConcurrentDictionary<int, PolygonExtended>();

        void DrawSoccerField()
        {
            fieldPolygonList = new ConcurrentDictionary<int, PolygonExtended>();

            
            int fieldLineWidth = 2;

            /// Fond du terrain
            PolygonExtended p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-stadiumLength/2, -stadiumWidth / 2));
            p.polygon.Points.Add(new PointD(stadiumLength / 2, -stadiumWidth / 2));
            p.polygon.Points.Add(new PointD(stadiumLength / 2, stadiumWidth / 2));
            p.polygon.Points.Add(new PointD(-stadiumLength / 2, stadiumWidth / 2));
            p.polygon.Points.Add(new PointD(-stadiumLength / 2, -stadiumWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x22, 0x22, 0x22);
            fieldPolygonList.AddOrUpdate((int)Terrain.ZoneProtegee, p, (key, value) => p);

            /// Bords du terrain
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(fieldLength / 2, -fieldWidth / 2));
            p.polygon.Points.Add(new PointD(0, -fieldWidth / 2));
            p.polygon.Points.Add(new PointD(0, fieldWidth / 2));
            p.polygon.Points.Add(new PointD(fieldLength / 2, fieldWidth / 2));
            p.polygon.Points.Add(new PointD(fieldLength / 2, -fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x66, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.DemiTerrainDroit, p, (key, value) => p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -fieldWidth / 2));
            p.polygon.Points.Add(new PointD(0, -fieldWidth / 2));
            p.polygon.Points.Add(new PointD(0, fieldWidth / 2));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, fieldWidth / 2));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x66, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.DemiTerrainGauche, p, (key, value) => p);

            ///Surface but gauche
            double percentLengthSurface = 0.7;
            double percentWidthSurface = 0.7;
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -(fieldWidth / 2) * percentWidthSurface));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2) * percentLengthSurface, -(fieldWidth / 2) * percentWidthSurface));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2) * percentLengthSurface, (fieldWidth / 2) * percentWidthSurface));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, (fieldWidth / 2) * percentWidthSurface));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -(fieldWidth / 2) * percentWidthSurface));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.SurfaceButGauche, p, (key, value) => p);

            ///Surface but droit
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(fieldLength / 2, -(fieldWidth / 2)* percentWidthSurface));
            p.polygon.Points.Add(new PointD((fieldLength / 2) * percentLengthSurface, -(fieldWidth / 2)* percentWidthSurface));
            p.polygon.Points.Add(new PointD((fieldLength / 2) * percentLengthSurface, (fieldWidth / 2)* percentWidthSurface));
            p.polygon.Points.Add(new PointD(fieldLength / 2, (fieldWidth / 2)* percentWidthSurface));
            p.polygon.Points.Add(new PointD(fieldLength / 2, -(fieldWidth / 2) * percentWidthSurface));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.SurfaceButDroit, p, (key, value) => p);

            ///Surface gardien Gauche
            double LongueurSurfacecGardien = 0.8;
            double LargeurSurfaceGardien = 2.5;
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -LargeurSurfaceGardien/2));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2) + LongueurSurfacecGardien, -LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2) + LongueurSurfacecGardien, LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -LargeurSurfaceGardien / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.SurfaceGardienGauche, p, (key, value) => p);

            ///Surface but droit
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(fieldLength / 2, -LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD((fieldLength / 2) - LongueurSurfacecGardien, -LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD((fieldLength / 2) - LongueurSurfacecGardien, LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD(fieldLength / 2, LargeurSurfaceGardien / 2));
            p.polygon.Points.Add(new PointD(fieldLength / 2, -LargeurSurfaceGardien / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.SurfaceGardienDroit, p, (key, value) => p);

            ///But Gauche
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -1.20));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, 1.20));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2)-0.5, 1.20));
            p.polygon.Points.Add(new PointD(-(fieldLength / 2)-0.50, -1.20));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -1.20));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.ButGauche, p, (key, value) => p);

            ///But Droit
            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(fieldLength / 2, -1.20));
            p.polygon.Points.Add(new PointD(fieldLength / 2, 1.20));
            p.polygon.Points.Add(new PointD((fieldLength / 2)+0.50, 1.20));
            p.polygon.Points.Add(new PointD((fieldLength / 2)+0.5, -1.20));
            p.polygon.Points.Add(new PointD(fieldLength / 2, -1.20));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.ButDroit, p, (key, value) => p);
            //PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.ButDroit, p);


            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-fieldLength / 2 , -(fieldWidth/2)));
            p.polygon.Points.Add(new PointD(-fieldLength / 2 , -(fieldWidth / 2)-1));
            p.polygon.Points.Add(new PointD(-4.00, -(fieldWidth / 2)-1));
            p.polygon.Points.Add(new PointD(-4.00, -(fieldWidth / 2)));
            p.polygon.Points.Add(new PointD(-fieldLength / 2 - 1.00, -(fieldWidth / 2)));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
            fieldPolygonList.AddOrUpdate((int)Terrain.ZoneTechniqueGauche, p, (key, value) => p);
            //PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.ZoneTechniqueGauche, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(+fieldLength / 2 , -(fieldWidth / 2)));
            p.polygon.Points.Add(new PointD(+fieldLength / 2 , -(fieldWidth / 2) - 1));
            p.polygon.Points.Add(new PointD(+4.00, -(fieldWidth / 2) - 1));
            p.polygon.Points.Add(new PointD(+4.00, -(fieldWidth / 2)));
            p.polygon.Points.Add(new PointD(+fieldLength / 2 + 1.00, -(fieldWidth / 2)));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0x00, 0x00);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0xFF);
            fieldPolygonList.AddOrUpdate((int)Terrain.ZoneTechniqueDroite, p, (key, value) => p);
            //PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.ZoneTechniqueDroite, p);

            p = new PolygonExtended();
            int nbSteps = 30;
            for (int i = 0; i < nbSteps + 1; i++)
                p.polygon.Points.Add(new PointD(1.0f * Math.Cos((double)i * (2 * Math.PI / nbSteps)), 1.0f * Math.Sin((double)i * (2 * Math.PI / nbSteps))));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.RondCentral, p, (key, value) => p);
            //PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.RondCentral, p);

            ///Quarts de cercle corner
            double cornerRadius = 0.75 * fieldLength / 22;
            p = new PolygonExtended();
            for (int i = 0; i < nbSteps + 1; i++)
                p.polygon.Points.Add(new PointD(-fieldLength / 2 + cornerRadius * Math.Cos((double)i * (Math.PI / 2 / nbSteps) ), -fieldWidth / 2 + cornerRadius * Math.Sin((double)i * (Math.PI /2 / nbSteps))));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, -fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.CornerBasGauche, p, (key, value) => p);

            p = new PolygonExtended();
            for (int i = 0; i < nbSteps + 1; i++)
                p.polygon.Points.Add(new PointD(fieldLength / 2 + cornerRadius * Math.Cos((double)i * (Math.PI / 2 / nbSteps) + Math.PI/2), -fieldWidth / 2 + cornerRadius * Math.Sin((double)i * (Math.PI / 2 / nbSteps) + Math.PI/2)));
            p.polygon.Points.Add(new PointD(fieldLength / 2, -fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.CornerBasDroite, p, (key, value) => p);

            p = new PolygonExtended();
            for (int i = 0; i < nbSteps + 1; i++)
                p.polygon.Points.Add(new PointD(fieldLength / 2 + cornerRadius * Math.Cos((double)i * (Math.PI / 2 / nbSteps) + Math.PI), fieldWidth / 2 + cornerRadius * Math.Sin((double)i * (Math.PI / 2 / nbSteps) + Math.PI)));
            p.polygon.Points.Add(new PointD(fieldLength / 2, fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.CornerHautDroite, p, (key, value) => p);

            p = new PolygonExtended();
            for (int i = 0; i < nbSteps + 1; i++)
                p.polygon.Points.Add(new PointD(-fieldLength / 2 + cornerRadius * Math.Cos((double)i * (Math.PI / 2 / nbSteps) + 3 * Math.PI / 2), fieldWidth / 2 + cornerRadius * Math.Sin((double)i * (Math.PI / 2 / nbSteps) + 3 * Math.PI / 2)));
            p.polygon.Points.Add(new PointD(-fieldLength / 2, fieldWidth / 2));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.CornerHautGauche, p, (key, value) => p);

            ///Point Avant Surface Gauche
            p = new PolygonExtended();
            for (int i = 0; i < (int)(nbSteps) + 1; i++)
                p.polygon.Points.Add(new PointD(-7.4 + 0.075 * Math.Cos((double)i * (2 * Math.PI / nbSteps)), 0.075 * Math.Sin((double)i * (2 * Math.PI / nbSteps))));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.PtAvantSurfaceGauche, p, (key, value) => p);

            ///Point Avant Surface Droite
            p = new PolygonExtended();
            for (int i = 0; i < (int)(nbSteps) + 1; i++)
                p.polygon.Points.Add(new PointD(7.4 + 0.075 * Math.Cos((double)i * (2 * Math.PI / nbSteps)), 0.075 * Math.Sin((double)i * (2 * Math.PI / nbSteps))));
            p.borderWidth = fieldLineWidth;
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0x00, 0xFF, 0x00);
            fieldPolygonList.AddOrUpdate((int)Terrain.PtAvantSurfaceDroit, p, (key, value) => p);

            PolygonTerrainSeries.UpdatePolygonExtendedList(fieldPolygonList);

        }
        
        private void UpdateField(Location mapCenter)
        {
            PopulateFieldPolygonList();

            ConcurrentDictionary<int, PolygonExtended> fieldPolygonRefMapCenterList = new ConcurrentDictionary<int, PolygonExtended>();
            foreach(var elt in fieldPolygonList)
            {
                PolygonExtended polygon = new PolygonExtended();
                polygon.backgroundColor = elt.Value.backgroundColor;
                polygon.borderColor = elt.Value.borderColor;
                polygon.borderDashPattern = elt.Value.borderDashPattern;
                polygon.borderOpacity= elt.Value.borderOpacity;
                polygon.borderWidth = elt.Value.borderWidth;
                polygon.polygon.Points = new List<PointD>();

                for(int i= 0; i< elt.Value.polygon.Points.Count; i++)
                {
                    polygon.polygon.Points.Add(Toolbox.OffsetLocation(elt.Value.polygon.Points[i], mapCenter));
                }

                fieldPolygonRefMapCenterList.AddOrUpdate(elt.Key, polygon, (key, value) => polygon);
            }
            PolygonTerrainSeries.UpdatePolygonExtendedList(fieldPolygonRefMapCenterList);
        }

        void DrawEurobotField()
        {
            //double TerrainLowerX = -fieldLength / 2 - 0.2;
            //double TerrainUpperX = fieldLength / 2 + 0.2;
            //double TerrainLowerY = -fieldWidth / 2 - 0.2;
            //double TerrainUpperY = fieldWidth / 2 + 0.2;

            int fieldLineWidth = 1;
            PolygonExtended p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-1.5, -1));
            p.polygon.Points.Add(new PointD(1.5, -1));
            p.polygon.Points.Add(new PointD(1.5, 1));
            p.polygon.Points.Add(new PointD(-1.5, 1));
            p.polygon.Points.Add(new PointD(-1.5, -1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.TerrainComplet, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, 1));
            p.polygon.Points.Add(new PointD(-1.5, 1));
            p.polygon.Points.Add(new PointD(-1.5, 1 - 0.1));
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, 1 - 0.1));
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, 1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheHaut, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(+1.5, -0.1));
            p.polygon.Points.Add(new PointD(+1.5 + 0.1, -0.1));
            p.polygon.Points.Add(new PointD(+1.5 + 0.1, 0.1));
            p.polygon.Points.Add(new PointD(+1.5, 0.1));
            p.polygon.Points.Add(new PointD(+1.5, -0.1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheCentre, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, -1));
            p.polygon.Points.Add(new PointD(-1.5, -1));
            p.polygon.Points.Add(new PointD(-1.5, -1 + 0.1));
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, -1 + 0.1));
            p.polygon.Points.Add(new PointD(-1.5 - 0.1, -1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheBas, p);

        }

        void DrawCachanField()
        {
            double TerrainLowerX = -fieldLength / 2;
            double TerrainUpperX = fieldLength / 2;
            double TerrainLowerY = -fieldWidth / 2;
            double TerrainUpperY = fieldWidth / 2;

            int fieldLineWidth = 1;
            PolygonExtended p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-4, -2));
            p.polygon.Points.Add(new PointD(4, -2));
            p.polygon.Points.Add(new PointD(4, 2));
            p.polygon.Points.Add(new PointD(-4, 2));
            p.polygon.Points.Add(new PointD(-4, -2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 46, 49, 146);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.TerrainComplet, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-4, 1.5));
            p.polygon.Points.Add(new PointD(-0.03, 1.5));
            p.polygon.Points.Add(new PointD(-2.15, 1.5));
            p.polygon.Points.Add(new PointD(-2.15, 0));
            p.polygon.Points.Add(new PointD(-0.03, 0));
            p.polygon.Points.Add(new PointD(-2.15, 0));
            p.polygon.Points.Add(new PointD(-2.15, -1.5));
            p.polygon.Points.Add(new PointD(-0.03, -1.5));
            p.polygon.Points.Add(new PointD(-4, -1.5));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0, 0, 0x00);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneTerrainGauche, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(4, 1.5));
            p.polygon.Points.Add(new PointD(0.03, 1.5));
            p.polygon.Points.Add(new PointD(2.15, 1.5));
            p.polygon.Points.Add(new PointD(2.15, 0));
            p.polygon.Points.Add(new PointD(0.03, 0));
            p.polygon.Points.Add(new PointD(2.15, 0));
            p.polygon.Points.Add(new PointD(2.15, -1.5));
            p.polygon.Points.Add(new PointD(0.03, -1.5));
            p.polygon.Points.Add(new PointD(4, -1.5));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0x00, 0, 0, 0x00);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneTerrainDroite, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-0.335, -2));
            p.polygon.Points.Add(new PointD(0.335, -2));
            p.polygon.Points.Add(new PointD(0.335, 2));
            p.polygon.Points.Add(new PointD(-0.335, 2));
            p.polygon.Points.Add(new PointD(-0.335, -2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneCentraleEpaisse, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-0.0095, -2));
            p.polygon.Points.Add(new PointD(0.0095, -2));
            p.polygon.Points.Add(new PointD(0.0095, 2));
            p.polygon.Points.Add(new PointD(-0.0095, 2));
            p.polygon.Points.Add(new PointD(-0.0095, -2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.LigneCentraleFine, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-1 - 0.1, 2));
            p.polygon.Points.Add(new PointD(-1 + 0.1, 2));
            p.polygon.Points.Add(new PointD(-1 + 0.1, 2 + 0.2));
            p.polygon.Points.Add(new PointD(-1 - 0.1, 2 + 0.2));
            p.polygon.Points.Add(new PointD(-1 - 0.1, 2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheHaut, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-4.2, -0.1));
            p.polygon.Points.Add(new PointD(-4, -0.1));
            p.polygon.Points.Add(new PointD(-4, 0.1));
            p.polygon.Points.Add(new PointD(-4.2, 0.1));
            p.polygon.Points.Add(new PointD(-4.2, -0.1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheCentre, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(-1 - 0.1, -2));
            p.polygon.Points.Add(new PointD(-1 + 0.1, -2));
            p.polygon.Points.Add(new PointD(-1 + 0.1, -2 - 0.2));
            p.polygon.Points.Add(new PointD(-1 - 0.1, -2 - 0.2));
            p.polygon.Points.Add(new PointD(-1 - 0.1, -2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseGaucheBas, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(1 - 0.1, 2));
            p.polygon.Points.Add(new PointD(1 + 0.1, 2));
            p.polygon.Points.Add(new PointD(1 + 0.1, 2 + 0.2));
            p.polygon.Points.Add(new PointD(1 - 0.1, 2 + 0.2));
            p.polygon.Points.Add(new PointD(1 - 0.1, 2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroiteHaut, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(4.2, -0.1));
            p.polygon.Points.Add(new PointD(4, -0.1));
            p.polygon.Points.Add(new PointD(4, 0.1));
            p.polygon.Points.Add(new PointD(4.2, 0.1));
            p.polygon.Points.Add(new PointD(4.2, -0.1));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroiteCentre, p);

            p = new PolygonExtended();
            p.polygon.Points.Add(new PointD(1 - 0.1, -2));
            p.polygon.Points.Add(new PointD(1 + 0.1, -2));
            p.polygon.Points.Add(new PointD(1 + 0.1, -2 - 0.2));
            p.polygon.Points.Add(new PointD(1 - 0.1, -2 - 0.2));
            p.polygon.Points.Add(new PointD(1 - 0.1, -2));
            p.borderWidth = fieldLineWidth;
            p.borderColor = System.Drawing.Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF);
            p.backgroundColor = System.Drawing.Color.FromArgb(0xFF, 0xA0, 0xA0, 0xA0);
            PolygonTerrainSeries.AddOrUpdatePolygonExtended((int)Terrain.BaliseDroiteBas, p);
        }


        //Récupération de la position cliquée sur la heatmap
        private void sciChart_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Console.WriteLine("CTRL+Click Gauche");
                // Perform the hit test relative to the GridLinesPanel
                var hitTestPoint = e.GetPosition(sciChartSurface.GridLinesPanel as UIElement);

                double xmin = (double)sciChartSurface.XAxes[0].VisibleRange.Min;
                double xmax = (double)sciChartSurface.XAxes[0].VisibleRange.Max;
                double ymin = (double)sciChartSurface.YAxes[0].VisibleRange.Min;
                double ymax = (double)sciChartSurface.YAxes[0].VisibleRange.Max;

                var width = sciChartSurface.ModifierSurface.ActualWidth;
                var height = sciChartSurface.ModifierSurface.ActualHeight;

                var xMap = xmin + (xmax - xmin) * hitTestPoint.X / width;
                var yMap = -(ymin + (ymax - ymin) * hitTestPoint.Y / height);

                Console.WriteLine("Click on : x=" + xMap + " - y=" + yMap);
                OnCtrlClickLeftOnHeatMap(xMap, yMap);

                //foreach (var serie in sciChartSurface.RenderableSeries)
                //{
                //    if (serie.GetType().Name == "FastUniformHeatmapRenderableSeries")
                //    {
                        
                //    }
                //}
            }
        }

        private void sciChart_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            this.sciChartSurface.XAxis.VisibleRange.SetMinMax(-stadiumLength / 2, stadiumLength / 2);
            this.sciChartSurface.YAxis.VisibleRange.SetMinMax(-stadiumWidth / 2, stadiumWidth / 2);
        }

        //Event en cas de CTRL+click dans une map
        public event EventHandler<PositionEventArgs> OnCtrlClickLeftOnHeatMapEvent;
        public virtual void OnCtrlClickLeftOnHeatMap(double x, double y)
        {
            var handler = OnCtrlClickLeftOnHeatMapEvent;
            if (handler != null)
            {
                handler(this, new PositionEventArgs { X = x, Y = y });
            }
        }

        public event EventHandler<PositionEventArgs> OnCtrlClickRightOnHeatMapEvent;
        public virtual void OnCtrlClickRightOnHeatMap(double x, double y)
        {
            var handler = OnCtrlClickRightOnHeatMapEvent;
            if (handler != null)
            {
                handler(this, new PositionEventArgs { X = x, Y = y });
            }
        }


        public XyDataSeries<double, double> GetXYDataSeriesFromPoints(List<PointD> ptList)
        {
            var dataSeries = new XyDataSeries<double, double>();
            var listX = ptList.Select(e => e.X);
            var listY = ptList.Select(e => e.Y);
            dataSeries.AcceptsUnsortedData = true;
            dataSeries.Append(listX, listY);
            return dataSeries;
        }

        private void sciChartSurface_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                Console.WriteLine("CTRL+Click Droit");

                // Perform the hit test relative to the GridLinesPanel
                var hitTestPoint = e.GetPosition(sciChartSurface.GridLinesPanel as UIElement);

                double xmin = (double)sciChartSurface.XAxes[0].VisibleRange.Min;
                double xmax = (double)sciChartSurface.XAxes[0].VisibleRange.Max;
                double ymin = (double)sciChartSurface.YAxes[0].VisibleRange.Min;
                double ymax = (double)sciChartSurface.YAxes[0].VisibleRange.Max;

                var width = sciChartSurface.ModifierSurface.ActualWidth;
                var height = sciChartSurface.ModifierSurface.ActualHeight;

                var xMap = xmin + (xmax - xmin) * hitTestPoint.X / width;
                var yMap = -(ymin + (ymax - ymin) * hitTestPoint.Y / height);

                OnCtrlClickRightOnHeatMap(xMap, yMap);
            }
        }
    }

    public class CustomAnnotationViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private string _logo;
        private string _text;

        // Provides a text for the watermark
        public string Text
        {
            get { return _text; }
            set
            {
                if (value != _text)
                {
                    _text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        // Provides the path to the image
        public string Logo
        {
            get { return _logo; }
            set
            {
                if (value != _logo)
                {
                    _logo = value;
                    OnPropertyChanged("Logo");
                }
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}


