#region copyright
// Copyright 2013 Matthew S. Gerber (gerber.matthew@gmail.com)
// 
// This file is part of the Asymmetric Threat Tracker (ATT).
// 
// The ATT is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// The ATT is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with the ATT.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using LAIR.Collections.Generic;
using System.IO;
using PTL.ATT.Models;
using LAIR.Extensions;
using PTL.ATT.Evaluation;
using LAIR.Misc;
using PostGIS = LAIR.ResourceAPIs.PostGIS;
using Npgsql;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Drawing.Imaging;
using PTL.ATT.Smoothers;
using PTL.ATT.GUI.Properties;

namespace PTL.ATT.GUI.Visualization
{
    public partial class ThreatMap : Visualizer
    {
        #region static members
        private static Pen _pen;
        private static SolidBrush _brush;

        private static PointF ConvertMetersPointToDrawingPoint(PointF pointInMeters, PointF regionBottomLeftInMeters, float pixelsPerMeter, Rectangle drawingRectangle)
        {
            float drawingPointX = (pointInMeters.X - regionBottomLeftInMeters.X) * pixelsPerMeter + drawingRectangle.Left;
            float drawingPointY = drawingRectangle.Height - ((pointInMeters.Y - regionBottomLeftInMeters.Y) * pixelsPerMeter) + drawingRectangle.Top;

            return new PointF(drawingPointX, drawingPointY);
        }
        #endregion

        private PointF _regionBottomLeftInMeters;
        private SizeF _regionSizeInMeters;
        private Dictionary<string, Color> _incidentColor;
        private Dictionary<long, Dictionary<string, List<Tuple<PointF, double>>>> _sliceIncidentPointScores;
        private Dictionary<long, Bitmap> _sliceThreatSurface;

        private bool _dragging;
        private System.Drawing.Point _draggingStart;
        private Size _panOffset;
        private int _panIncrement;

        private float _zoomedImageWidth;
        private float _zoomIncrement;
        private System.Windows.Forms.Timer _clarifyZoomTimer;

        private Rectangle _highlightedThreatRectangle;
        private int _highlightedThreatRectangleRow;
        private int _highlightedThreatRectangleCol;

        private Rectangle _topPanelRectangle;

        private Bitmap CurrentThreatSurface
        {
            get
            {
                Bitmap currentThreatSurface = null;

                if (_sliceThreatSurface != null)
                    _sliceThreatSurface.TryGetValue((int)timeSlice.Value, out currentThreatSurface);

                return currentThreatSurface;
            }
        }

        private float AspectRatio
        {
            get { return CurrentThreatSurface.Height / (float)CurrentThreatSurface.Width; }
        }

        private float ZoomedImageHeight
        {
            get { return AspectRatio * _zoomedImageWidth; }
        }

        private float ZoomFactor
        {
            get { return _zoomedImageWidth / CurrentThreatSurface.Width; }
        }

        public override Color BackColor
        {
            get { return base.BackColor; }
            set
            {
                base.BackColor = value;

                if (CurrentThreatSurface != null)
                    GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);
            }
        }

        public ThreatMap()
        {
            InitializeComponent();

            _pen = new Pen(BackColor, 1);
            _brush = new SolidBrush(BackColor);
            _zoomIncrement = 0.1f;
            _clarifyZoomTimer = new System.Windows.Forms.Timer();
            _clarifyZoomTimer.Interval = 1000;
            _clarifyZoomTimer.Tick += new EventHandler(ClarifyZoom);

            MouseWheel += new MouseEventHandler(ThreatMap_MouseWheel);

            _highlightedThreatRectangle = Rectangle.Empty;
            _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;

            _topPanelRectangle = topPanel.Bounds;
        }

        public override void Display(Prediction prediction, IEnumerable<Overlay> overlays)
        {
            base.Display(prediction, overlays);

            _dragging = false;
            _draggingStart = System.Drawing.Point.Empty;
            _panOffset = new Size(0, 0);
            _panIncrement = 50;

            DiscreteChoiceModel model = prediction.Model;

            Dictionary<int, Point> idPoint = new Dictionary<int, Point>();
            foreach (Point p in prediction.Points)
                idPoint.Add(p.Id, p);

            _incidentColor = new Dictionary<string, Color>();
            _sliceIncidentPointScores = new Dictionary<long, Dictionary<string, List<Tuple<PointF, double>>>>();
            float minPointX = float.MaxValue;
            float minPointY = float.MaxValue;
            float maxPointX = float.MinValue;
            float maxPointY = float.MinValue;
            foreach (PointPrediction pointPrediction in prediction.PointPredictions)
            {
                long slice = -1;
                if (model is TimeSliceDCM)
                    slice = (long)(pointPrediction.Time.Ticks / (model as TimeSliceDCM).TimeSliceTicks);

                _sliceIncidentPointScores.EnsureContainsKey(slice, typeof(Dictionary<string, List<Tuple<PointF, double>>>));

                Point point = idPoint[pointPrediction.PointId];

                foreach (string incident in pointPrediction.IncidentScore.Keys)
                {
                    Color color;
                    if (!_incidentColor.TryGetValue(incident, out color))
                    {
                        color = ColorPalette.GetColor();
                        _incidentColor.Add(incident, color);
                    }

                    double score = pointPrediction.IncidentScore[incident];

                    _sliceIncidentPointScores[slice].EnsureContainsKey(incident, typeof(List<Tuple<PointF, double>>));
                    _sliceIncidentPointScores[slice][incident].Add(new Tuple<PointF, double>(new PointF((float)point.Location.X, (float)point.Location.Y), score));
                }

                float x = (float)point.Location.X;
                float y = (float)point.Location.Y;
                if (x < minPointX) minPointX = x;
                if (x > maxPointX) maxPointX = x;
                if (y < minPointY) minPointY = y;
                if (y > maxPointY) maxPointY = y;
            }

            Invoke(new Action(delegate()
                {
                    incidentTypeCheckBoxes.Controls.Clear();
                    bool first = true;
                    foreach (string incidentType in _incidentColor.Keys)
                    {
                        ColoredCheckBox cb = new ColoredCheckBox(true, first ? CheckState.Checked : CheckState.Unchecked, incidentType, _incidentColor[incidentType]);
                        cb.CheckBoxCheckStateChanged += new EventHandler(IncidentCheckBox_CheckStateChanged);
                        cb.LabelClicked += new EventHandler(IncidentCheckBox_LabelClicked);
                        incidentTypeCheckBoxes.Controls.Add(cb);
                        first = false;
                    }

                    overlayCheckBoxes.Controls.Clear();
                    foreach (Overlay overlay in Overlays)
                    {
                        ColoredCheckBox cb = new ColoredCheckBox(false, overlay.Displayed ? CheckState.Checked : CheckState.Unchecked, overlay.Name, overlay.Color);
                        cb.CheckBoxCheckedChanged += new EventHandler(OverlayCheckBox_CheckedChanged);
                        cb.LabelClicked += new EventHandler(OverlayCheckBox_LabelClicked);
                        overlayCheckBoxes.Controls.Add(cb);

                        IEnumerable<float> xs = overlay.Points.SelectMany(points => points).Select(point => point.X);
                        IEnumerable<float> ys = overlay.Points.SelectMany(points => points).Select(point => point.Y);
                        float minX = xs.Min();
                        float maxX = xs.Max();
                        float minY = ys.Min();
                        float maxY = ys.Max();
                        if (minX < minPointX) minPointX = minX;
                        if (maxX > maxPointX) maxPointX = maxX;
                        if (minY < minPointY) minPointY = minY;
                        if (maxY > maxPointY) maxPointY = maxY;
                    }

                    _regionBottomLeftInMeters = new PointF(minPointX, minPointY);
                    _regionSizeInMeters = new SizeF(maxPointX - minPointX, maxPointY - minPointY);

                    bool newThreatSurface = threatResolution.Value != prediction.Model.PointSpacing;
                    threatResolution.Value = threatResolution.Minimum = prediction.Model.PointSpacing;
                    if (!newThreatSurface)
                    {
                        Rectangle bitmapDimensions;
                        if (_regionSizeInMeters.Height >= _regionSizeInMeters.Width)
                            bitmapDimensions = new Rectangle(ClientRectangle.Location, new Size((int)(ClientRectangle.Height * (_regionSizeInMeters.Width / (float)_regionSizeInMeters.Height)), ClientRectangle.Height));
                        else
                            bitmapDimensions = new Rectangle(ClientRectangle.Location, new Size(ClientRectangle.Width, (int)(ClientRectangle.Width * (_regionSizeInMeters.Height / (float)_regionSizeInMeters.Width))));

                        GetThreatSurfaces(bitmapDimensions, true);
                    }

                    GetSliceTimeText();
                }));
        }

        private void GetThreatSurfaces(Rectangle bitmapDimensions, bool displayFirstSlice)
        {
            if (_sliceIncidentPointScores == null)
                return;

            Set<string> selectedIncidents = new Set<string>(incidentTypeCheckBoxes.Controls.Cast<ColoredCheckBox>().Where(c => c.CheckState != CheckState.Unchecked).Select(c => c.Text).ToArray());

            float pixelsPerMeter;
            float threatRectanglePixelWidth;
            GetDrawingParameters(bitmapDimensions, out pixelsPerMeter, out threatRectanglePixelWidth);

            if (_sliceThreatSurface == null)
                _sliceThreatSurface = new Dictionary<long, Bitmap>(_sliceIncidentPointScores.Count);
            else
            {
                foreach (Bitmap threatSurface in _sliceThreatSurface.Values)
                    threatSurface.Dispose();

                _sliceThreatSurface.Clear();
            }

            Dictionary<long, Dictionary<int, Dictionary<int, Dictionary<string, List<double>>>>> sliceRowColIncidentScores = new Dictionary<long, Dictionary<int, Dictionary<int, Dictionary<string, List<double>>>>>();
            foreach (long slice in _sliceIncidentPointScores.Keys)
            {
                try { _sliceThreatSurface.Add(slice, new Bitmap(bitmapDimensions.Width, bitmapDimensions.Height, PixelFormat.Format16bppRgb565)); }
                catch (ArgumentException)
                {
                    Console.Out.WriteLine("Maximum zoom exceeded.");
                    return;
                }

                sliceRowColIncidentScores.EnsureContainsKey(slice, typeof(Dictionary<int, Dictionary<int, Dictionary<string, List<double>>>>));

                foreach (string incident in _sliceIncidentPointScores[slice].Keys)
                    if (selectedIncidents.Contains(incident))
                        foreach (Tuple<PointF, double> pointScore in _sliceIncidentPointScores[slice][incident])
                        {
                            PointF drawingPoint = ConvertMetersPointToDrawingPoint(pointScore.Item1, _regionBottomLeftInMeters, pixelsPerMeter, bitmapDimensions);

                            int row, col;
                            GetThreatRectangleRowColumn(drawingPoint, threatRectanglePixelWidth, out row, out col);

                            sliceRowColIncidentScores[slice].EnsureContainsKey(row, typeof(Dictionary<int, Dictionary<string, List<double>>>));
                            sliceRowColIncidentScores[slice][row].EnsureContainsKey(col, typeof(Dictionary<string, List<double>>));
                            sliceRowColIncidentScores[slice][row][col].EnsureContainsKey(incident, typeof(List<double>));
                            sliceRowColIncidentScores[slice][row][col][incident].Add(pointScore.Item2);
                        }
            }

            Dictionary<long, Dictionary<int, Dictionary<int, Tuple<double, Color>>>> sliceRowColScoreColor = new Dictionary<long, Dictionary<int, Dictionary<int, Tuple<double, Color>>>>();
            double minScore = double.MaxValue;
            double maxScore = double.MinValue;
            foreach (long slice in sliceRowColIncidentScores.Keys)
            {
                Dictionary<int, Dictionary<int, Tuple<double, Color>>> rowColScoreColor = new Dictionary<int, Dictionary<int, Tuple<double, Color>>>();

                foreach (int row in sliceRowColIncidentScores[slice].Keys)
                    foreach (int col in sliceRowColIncidentScores[slice][row].Keys)
                    {
                        string mostLikelyIncident = null;
                        double max = -1;
                        foreach (string incident in sliceRowColIncidentScores[slice][row][col].Keys)
                        {
                            double score = sliceRowColIncidentScores[slice][row][col][incident].Average();
                            if (score > max)
                            {
                                mostLikelyIncident = incident;
                                max = score;
                            }
                        }

                        if (max < minScore) minScore = max;
                        if (max > maxScore) maxScore = max;

                        rowColScoreColor.EnsureContainsKey(row, typeof(Dictionary<int, Tuple<double, Color>>));
                        rowColScoreColor[row].Add(col, new Tuple<double, Color>(max, _incidentColor[mostLikelyIncident]));
                    }

                sliceRowColScoreColor.Add(slice, rowColScoreColor);
            }

            double scoreRange = maxScore - minScore;
            if (scoreRange == 0)
                scoreRange = float.Epsilon;

            foreach (long slice in sliceRowColScoreColor.Keys)
            {
                Graphics g = Graphics.FromImage(_sliceThreatSurface[slice]);
                g.Clear(BackColor);

                foreach (int row in sliceRowColScoreColor[slice].Keys)
                    foreach (int col in sliceRowColScoreColor[slice][row].Keys)
                    {
                        Tuple<double, Color> scoreColor = sliceRowColScoreColor[slice][row][col];
                        double scaledScore = (scoreColor.Item1 - minScore) / scoreRange;
                        double portionBackground = 1 - scaledScore;
                        Color color = scoreColor.Item2;

                        byte red = (byte)(scaledScore * color.R + portionBackground * BackColor.R);
                        byte green = (byte)(scaledScore * color.G + portionBackground * BackColor.G);
                        byte blue = (byte)(scaledScore * color.B + portionBackground * BackColor.B);
                        _brush.Color = Color.FromArgb(red, green, blue);

                        g.FillRectangle(_brush, col * threatRectanglePixelWidth, row * threatRectanglePixelWidth, threatRectanglePixelWidth, threatRectanglePixelWidth);
                    }

                foreach (Overlay overlay in Overlays)
                    if (overlay.Displayed)
                    {
                        _pen.Color = overlay.Color;
                        _brush.Color = overlay.Color;
                        foreach (List<PointF> points in overlay.Points)
                            if (points.Count == 1)
                            {
                                PointF drawingPoint = ConvertMetersPointToDrawingPoint(points[0], _regionBottomLeftInMeters, pixelsPerMeter, bitmapDimensions);
                                RectangleF circle = GetCircleBoundingBox(drawingPoint, 5);
                                g.FillEllipse(_brush, circle);
                                g.DrawEllipse(_pen, circle);
                            }
                            else
                                for (int i = 1; i < points.Count; ++i)
                                    g.DrawLine(_pen, ConvertMetersPointToDrawingPoint(points[i - 1], _regionBottomLeftInMeters, pixelsPerMeter, bitmapDimensions), ConvertMetersPointToDrawingPoint(points[i], _regionBottomLeftInMeters, pixelsPerMeter, bitmapDimensions));
                    }

                Set<string> selectedTrueIncidentOverlays = new Set<string>(incidentTypeCheckBoxes.Controls.Cast<ColoredCheckBox>().Where(c => c.CheckState == CheckState.Checked).Select(c => c.Text).ToArray());
                DateTime sliceStart = DisplayedPrediction.PredictionStartTime;
                DateTime sliceEnd = DisplayedPrediction.PredictionEndTime;
                if (slice != -1)
                {
                    if (!(DisplayedPrediction.Model is TimeSliceDCM))
                        throw new Exception("Expected TimeSliceDCM since slice != 1");

                    long sliceTicks = (DisplayedPrediction.Model as TimeSliceDCM).TimeSliceTicks;
                    sliceStart = new DateTime(slice * sliceTicks);
                    sliceEnd = sliceStart + new TimeSpan(sliceTicks);
                }

                foreach (string trueIncidentOverlay in selectedTrueIncidentOverlays)
                {
                    _brush.Color = _incidentColor[trueIncidentOverlay];
                    _pen.Color = Color.Black;
                    foreach (Incident incident in Incident.Get(sliceStart, sliceEnd, DisplayedPrediction.PredictionArea, trueIncidentOverlay))
                    {
                        PointF drawingPoint = ConvertMetersPointToDrawingPoint(new PointF((float)incident.Location.X, (float)incident.Location.Y), _regionBottomLeftInMeters, pixelsPerMeter, bitmapDimensions);
                        RectangleF circle = GetCircleBoundingBox(drawingPoint, 5);
                        g.FillEllipse(_brush, circle);
                        g.DrawEllipse(_pen, circle);
                    }
                }
            }

            timeSlice.ValueChanged -= new EventHandler(timeSlice_ValueChanged);
            timeSlice.Minimum = (int)_sliceThreatSurface.Keys.Min();
            timeSlice.Maximum = (int)_sliceThreatSurface.Keys.Max();
            if (displayFirstSlice)
                timeSlice.Value = timeSlice.Minimum;
            timeSlice.ValueChanged += new EventHandler(timeSlice_ValueChanged);

            _zoomedImageWidth = CurrentThreatSurface.Width;

            Invalidate();
        }

        private RectangleF GetCircleBoundingBox(PointF center, int diameter)
        {
            return new RectangleF(new PointF(center.X - diameter / 2f, center.Y - diameter / 2f), new SizeF(diameter, diameter));
        }

        private void GetDrawingParameters(Rectangle bitmapDimensions, out float pixelsPerMeter, out float threatRectanglePixelWidth)
        {
            pixelsPerMeter = Math.Min(bitmapDimensions.Width / _regionSizeInMeters.Width, bitmapDimensions.Height / _regionSizeInMeters.Height); // make sure entire threat map fits in the bitmap
            threatRectanglePixelWidth = (float)threatResolution.Value * pixelsPerMeter;
        }

        private void GetThreatRectangleRowColumn(PointF threatMapPoint, float threatRectanglePixelWidth, out int row, out int col)
        {
            row = (int)(threatMapPoint.Y / threatRectanglePixelWidth);
            col = (int)(threatMapPoint.X / threatRectanglePixelWidth);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (CurrentThreatSurface == null)
                return;

            float sourceWidth = e.ClipRectangle.Width / ZoomFactor;
            float sourceHeight = e.ClipRectangle.Height / ZoomFactor;

            PointF sourceUpperLeft = new PointF(e.ClipRectangle.Left + (e.ClipRectangle.Width - sourceWidth) / 2f + _panOffset.Width,
                                                e.ClipRectangle.Top + (e.ClipRectangle.Height - sourceHeight) / 2f + _panOffset.Height);

            RectangleF sourceRectangle = new RectangleF(sourceUpperLeft, new SizeF(sourceWidth, sourceHeight));

            e.Graphics.DrawImage(CurrentThreatSurface, e.ClipRectangle, sourceRectangle, GraphicsUnit.Pixel);

            if (_highlightedThreatRectangle != Rectangle.Empty && e.ClipRectangle.IntersectsWith(_highlightedThreatRectangle))
            {
                _pen.Color = Color.Yellow;
                e.Graphics.DrawRectangle(_pen, _highlightedThreatRectangle);
            }
        }

        #region mouse events
        private void ThreatMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _dragging = true;
                _draggingStart = new System.Drawing.Point(e.X, e.Y);

                UpdateHighlightedThreatRectangle(e.Location);
            }
        }

        private void ThreatMap_MouseUp(object sender, MouseEventArgs e)
        {
            _dragging = false;
            Cursor = Cursors.Default;
        }

        private void ThreatMap_MouseMove(object sender, MouseEventArgs e)
        {
            if (CurrentThreatSurface == null)
                return;

            if (_dragging)
            {
                ChangeCursor(Resources.ClosedHand);

                _panOffset.Width += (int)((_draggingStart.X - e.X) / ZoomFactor);
                _panOffset.Height += (int)((_draggingStart.Y - e.Y) / ZoomFactor);

                Invalidate();

                _draggingStart = e.Location;

                if (_highlightedThreatRectangle != Rectangle.Empty)
                {
                    Rectangle toInvalidate = _highlightedThreatRectangle;
                    toInvalidate.Inflate(10, 10);
                    _highlightedThreatRectangle = Rectangle.Empty;
                    _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;
                    Invalidate(toInvalidate);
                }
            }

            topPanel.Visible = _topPanelRectangle.Contains(e.Location);
        }

        private void UpdateHighlightedThreatRectangle(System.Drawing.Point mouseLocation)
        {
            if (DisplayedPrediction == null)
                return;

            float pixelsPerMeter;
            float threatRectanglePixelWidth;
            GetDrawingParameters(new Rectangle(new System.Drawing.Point(0, 0), CurrentThreatSurface.Size), out pixelsPerMeter, out threatRectanglePixelWidth);

            int row, col;
            int rowColX = (int)(mouseLocation.X + (_panOffset.Width % threatRectanglePixelWidth));
            int rowColY = (int)(mouseLocation.Y + (_panOffset.Height % threatRectanglePixelWidth));
            System.Drawing.Point threatMapPoint = new System.Drawing.Point(rowColX, rowColY);
            GetThreatRectangleRowColumn(threatMapPoint, threatRectanglePixelWidth, out row, out col);

            if (row != _highlightedThreatRectangleRow || col != _highlightedThreatRectangleCol)
            {
                Rectangle toInvalidate;

                if (_highlightedThreatRectangle != Rectangle.Empty)
                {
                    toInvalidate = _highlightedThreatRectangle;
                    toInvalidate.Inflate(10, 10);
                    _highlightedThreatRectangle = Rectangle.Empty;
                    _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;
                    Invalidate(toInvalidate);
                }

                _highlightedThreatRectangle = new Rectangle((int)(col * threatRectanglePixelWidth - (_panOffset.Width % threatRectanglePixelWidth)),
                                                            (int)(row * threatRectanglePixelWidth - (_panOffset.Height % threatRectanglePixelWidth)),
                                                            (int)threatRectanglePixelWidth, (int)threatRectanglePixelWidth);
                _highlightedThreatRectangleRow = row;
                _highlightedThreatRectangleCol = col;

                toInvalidate = _highlightedThreatRectangle;
                toInvalidate.Inflate(10, 10);
                Invalidate(toInvalidate);
            }
        }

        void ThreatMap_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
                zoomInBtn_Click(sender, e);
            else
                zoomOutBtn_Click(sender, e);
        }

        private void ThreatMap_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }
        #endregion

        #region panning
        private void panUpBtn_Click(object sender, EventArgs e)
        {
            _panOffset.Height -= _panIncrement;
            Invalidate();
        }

        private void panDownBtn_Click(object sender, EventArgs e)
        {
            _panOffset.Height += _panIncrement;
            Invalidate();
        }

        private void panLeftBtn_Click(object sender, EventArgs e)
        {
            _panOffset.Width -= _panIncrement;
            Invalidate();
        }

        private void panRightBtn_Click(object sender, EventArgs e)
        {
            _panOffset.Width += _panIncrement;
            Invalidate();
        }

        private void resetPan_Click(object sender, EventArgs e)
        {
            _panOffset = new System.Drawing.Size(0, 0);
            Invalidate();
        }
        #endregion

        #region zooming
        private void zoomInBtn_Click(object sender, EventArgs e)
        {
            _highlightedThreatRectangle = Rectangle.Empty;
            _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;

            _zoomedImageWidth *= (1 + _zoomIncrement);
            Invalidate();
            StartClarifyZoomTimer();
        }

        private void zoomOutBtn_Click(object sender, EventArgs e)
        {
            _highlightedThreatRectangle = Rectangle.Empty;
            _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;

            _zoomedImageWidth *= (1 - _zoomIncrement);
            Invalidate();
            StartClarifyZoomTimer();
        }

        private void StartClarifyZoomTimer()
        {
            if (_clarifyZoomTimer.Enabled)
                _clarifyZoomTimer.Stop();

            _clarifyZoomTimer.Start();
        }

        private void ClarifyZoom(object sender, EventArgs args)
        {
            _clarifyZoomTimer.Stop();

            if (DisplayedPrediction == null)
                return;

            Rectangle threatSurfaceBoundingBox = new Rectangle(0, 0, (int)_zoomedImageWidth, (int)ZoomedImageHeight);

            float x = ((ClientSize.Width - (ClientSize.Width / ZoomFactor)) / 2f) * ZoomFactor;
            float y = ((ClientSize.Height - (ClientSize.Height / ZoomFactor)) / 2f) * ZoomFactor;

            _panOffset = new Size((int)(_panOffset.Width * ZoomFactor + x), (int)(_panOffset.Height * ZoomFactor + y));

            _highlightedThreatRectangle = Rectangle.Empty;
            _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;

            GetThreatSurfaces(threatSurfaceBoundingBox, false);
        }

        private void resetZoom_Click(object sender, EventArgs e)
        {
            _panOffset = new System.Drawing.Size(0, 0);
            GetThreatSurfaces(ClientRectangle, false);
        }
        #endregion

        private void ThreatMap_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void IncidentCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);
        }

        private void IncidentCheckBox_LabelClicked(object sender, EventArgs e)
        {
            ColoredCheckBox cb = sender as ColoredCheckBox;
            if (cb == null)
                throw new ArgumentException("Must pass ColoredCheckBox");

            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                ColorPalette.ReturnColor(cb.Label.BackColor);
                cb.Label.BackColor = _incidentColor[cb.Text] = cd.Color;
                GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);
            }
        }

        private void OverlayCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            ColoredCheckBox cb = sender as ColoredCheckBox;
            if (cb == null)
                throw new ArgumentException("Must pass ColoredCheckBox");

            Overlays.Where(o => o.Name == cb.Text).First().Displayed = cb.Checked;
            GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);
        }

        private void OverlayCheckBox_LabelClicked(object sender, EventArgs e)
        {
            ColoredCheckBox cb = sender as ColoredCheckBox;
            if (cb == null)
                throw new ArgumentException("Must pass ColoredCheckBox");

            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
            {
                ColorPalette.ReturnColor(cb.Label.BackColor);

                cb.Label.BackColor = Overlays.Where(o => o.Name == cb.Text).First().Color = cd.Color;
                GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);
            }
        }

        public override void Clear()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(Clear));
                return;
            }

            base.Clear();

            _incidentColor = null;
            _sliceThreatSurface = null;
            _sliceIncidentPointScores = null;
            _regionBottomLeftInMeters = PointF.Empty;
            _regionSizeInMeters = SizeF.Empty;

            foreach (ColoredCheckBox cb in incidentTypeCheckBoxes.Controls)
                ColorPalette.ReturnColor(cb.Label.BackColor);

            incidentTypeCheckBoxes.Controls.Clear();

            foreach (ColoredCheckBox cb in overlayCheckBoxes.Controls)
                ColorPalette.ReturnColor(cb.Label.BackColor);

            overlayCheckBoxes.Controls.Clear();

            sliceTime.Text = "";

            Invalidate();
        }

        private void ChangeCursor(byte[] cursor)
        {
            MemoryStream ms = new MemoryStream(cursor);
            Cursor = new Cursor(ms);
            ms.Close();
        }

        private void threatResolution_ValueChanged(object sender, EventArgs e)
        {
            _highlightedThreatRectangle = Rectangle.Empty;
            _highlightedThreatRectangleCol = _highlightedThreatRectangleRow = -1;

            if (CurrentThreatSurface == null)
                GetThreatSurfaces(ClientRectangle, false);
            else
                GetThreatSurfaces(new Rectangle(0, 0, CurrentThreatSurface.Width, CurrentThreatSurface.Height), false);

            panUpBtn.Focus();
        }

        private void timeSlice_ValueChanged(object sender, EventArgs e)
        {
            Invalidate();

            GetSliceTimeText();
        }

        private void GetSliceTimeText()
        {
            DiscreteChoiceModel model = DisplayedPrediction.Model;
            if (model is TimeSliceDCM)
            {
                TimeSliceDCM ts = model as TimeSliceDCM;
                DateTime start = new DateTime((((long)timeSlice.Value) * ts.TimeSliceTicks));
                DateTime end = start + new TimeSpan(ts.TimeSliceTicks);
                sliceTime.Text = start.ToShortDateString() + " " + start.ToShortTimeString() + " - " + end.ToShortDateString() + " " + end.ToShortTimeString();
            }
        }

        private void topPanel_MouseLeave(object sender, EventArgs e)
        {
            topPanel.Visible = false;
        }

        private void checkBoxes_Scroll(object sender, ScrollEventArgs e)
        {
            FlowLayoutPanel p = sender as FlowLayoutPanel;
            if (p == null)
                throw new ArgumentException("Must pass FlowLayoutPanel to checkBoxes_Scroll");

            p.Invalidate();
        }

        private void exportThreatSurfaceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CurrentThreatSurface == null)
                MessageBox.Show("No threat surface displayed. Nothing to export.");
            else
            {
                StringBuilder filter = new StringBuilder();
                Dictionary<string, ImageFormat> nameFormat = new Dictionary<string, ImageFormat>();
                foreach (ImageFormat format in new ImageFormat[] { ImageFormat.Bmp, ImageFormat.Emf, ImageFormat.Exif, ImageFormat.Gif, ImageFormat.Icon, ImageFormat.Jpeg, ImageFormat.Png, ImageFormat.Tiff, ImageFormat.Wmf })
                {
                    filter.Append((filter.Length == 0 ? "" : "|") + format + " image files (*." + format + ")|*." + format);
                    nameFormat.Add(format.ToString().ToLower(), format);
                }

                string path = LAIR.IO.File.PromptForSavePath("Select export location...", filter.ToString());
                ImageFormat selectedFormat;
                try { selectedFormat = nameFormat[Path.GetExtension(path).Trim('.').ToLower()]; }
                catch (Exception)
                {
                    MessageBox.Show("Invalid file extension. Must be one of:  " + nameFormat.Keys.Concatenate(",") + ".");
                    return;
                }

                if (path != null)
                {
                    if (File.Exists(path))
                        File.Delete(path);

                    CurrentThreatSurface.Save(path, selectedFormat);
                }
            }
        }

        private void examinePredictionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DisplayedPrediction == null)
                MessageBox.Show("No prediction displayed. Cannot examine regions.");
            else if (!File.Exists(DisplayedPrediction.PointPredictionLogPath))
                MessageBox.Show("No point prediction information is available.");
            else if (_highlightedThreatRectangle == Rectangle.Empty || _highlightedThreatRectangleCol == -1 || _highlightedThreatRectangleRow == -1)
                MessageBox.Show("Must select a region to examine.");
            else
            {
                float pixelsPerMeter;
                float threatRectanglePixelWidth;
                GetDrawingParameters(new Rectangle(new System.Drawing.Point(0, 0), CurrentThreatSurface.Size), out pixelsPerMeter, out threatRectanglePixelWidth);

                int rowAbsoluteThreatRectangle = _highlightedThreatRectangleRow + (int)(_panOffset.Height / threatRectanglePixelWidth);
                int colAbsoluteThreatRectangle = _highlightedThreatRectangleCol + (int)(_panOffset.Width / threatRectanglePixelWidth);

                if (rowAbsoluteThreatRectangle < 0 || colAbsoluteThreatRectangle < 0)
                    MessageBox.Show("No information at that location.");
                else
                {
                    float widthMeters = threatRectanglePixelWidth / pixelsPerMeter;
                    float bottomMeters = _regionBottomLeftInMeters.Y + _regionSizeInMeters.Height - (rowAbsoluteThreatRectangle + 1) * widthMeters;
                    float leftMeters = _regionBottomLeftInMeters.X + colAbsoluteThreatRectangle * widthMeters;

                    PostGIS.Polygon threatRectangle = new PostGIS.Polygon(new PostGIS.Point[]{
                                                                          new PostGIS.Point(leftMeters, bottomMeters, DisplayedPrediction.PredictionArea.SRID),
                                                                          new PostGIS.Point(leftMeters, bottomMeters + widthMeters, DisplayedPrediction.PredictionArea.SRID),
                                                                          new PostGIS.Point(leftMeters + widthMeters, bottomMeters + widthMeters, DisplayedPrediction.PredictionArea.SRID),
                                                                          new PostGIS.Point(leftMeters + widthMeters, bottomMeters, DisplayedPrediction.PredictionArea.SRID),
                                                                          new PostGIS.Point(leftMeters, bottomMeters, DisplayedPrediction.PredictionArea.SRID)}, DisplayedPrediction.PredictionArea.SRID);

                    DiscreteChoiceModel model = DisplayedPrediction.Model;

                    PointPrediction[] pointPredictions = PointPrediction.GetWithin(threatRectangle, DisplayedPrediction.Id).ToArray();

                    // only get point predictions in current slice if we've got a timeslice model
                    if (model is TimeSliceDCM)
                        pointPredictions = pointPredictions.Where(p => (p.Time.Ticks / (model as TimeSliceDCM).TimeSliceTicks) == timeSlice.Value).ToArray();

                    if (pointPredictions.Length > 0)
                    {
                        DataGridView dataView = new DataGridView();
                        dataView.ReadOnly = true;
                        dataView.AllowUserToAddRows = false;

                        int predictionIdCol = dataView.Columns.Add("prediction_id", "Prediction ID");

                        Dictionary<string, int> incidentProbCol = new Dictionary<string, int>();
                        Set<int> incidentCols = new Set<int>();
                        foreach (string incident in pointPredictions[0].IncidentScore.Keys)
                        {
                            string colName = "Threat:  " + incident;
                            int incidentCol = dataView.Columns.Add(colName, colName);
                            incidentProbCol.Add(incident, incidentCol);
                            incidentCols.Add(incidentCol);
                        }

                        Dictionary<int, int> featureIdCol = new Dictionary<int, int>();
                        Set<int> featureCols = new Set<int>();
                        if (model is IFeatureBasedDCM)
                        {
                            IFeatureBasedDCM featureBasedModel = DisplayedPrediction.Model as IFeatureBasedDCM;
                            foreach (Feature feature in featureBasedModel.Features.OrderBy(f => f.ToString()))
                            {
                                string colName = "Feature:  " + feature.ToString();
                                int featureCol = dataView.Columns.Add(colName, colName);
                                featureIdCol.Add(feature.Id, featureCol);
                                featureCols.Add(featureCol);
                            }
                        }

                        dataView.Rows.Add(pointPredictions.Length);

                        try
                        {
                            Set<string> logPointIdsToGet = new Set<string>(pointPredictions.Select(p => model.GetPointIdForLog(p.PointId, p.Time)).ToArray());
                            Dictionary<string, Tuple<List<Tuple<string, double>>, List<Tuple<int, double>>>> pointPredictionLog = model.ReadPointPredictionLog(DisplayedPrediction.PointPredictionLogPath, logPointIdsToGet);
                            for (int i = 0; i < pointPredictions.Length; ++i)
                            {
                                PointPrediction pointPrediction = pointPredictions[i];

                                dataView[predictionIdCol, i].Value = pointPrediction.PointId;

                                string logPointId = model.GetPointIdForLog(pointPrediction.PointId, pointPrediction.Time);

                                foreach (Tuple<string, double> labelConfidence in pointPredictionLog[logPointId].Item1)
                                    if (labelConfidence.Item1 != PointPrediction.NullLabel)
                                        dataView[incidentProbCol[labelConfidence.Item1], i].Value = Math.Round(labelConfidence.Item2, 3);

                                foreach (Tuple<int, double> featureIdValue in pointPredictionLog[logPointId].Item2)
                                    dataView[featureIdCol[featureIdValue.Item1], i].Value = Math.Round(featureIdValue.Item2, 3);
                            }
                        }
                        catch (Exception ex) { Console.Out.WriteLine("Error while reading prediction log:  " + ex.Message); }

                        dataView.SortCompare += new DataGridViewSortCompareEventHandler(delegate(object o, DataGridViewSortCompareEventArgs args)
                            {
                                int sortedColumn = args.Column.DisplayIndex;

                                if (args.CellValue1 == null && args.CellValue2 == null)
                                    args.SortResult = args.RowIndex1.CompareTo(args.RowIndex2);
                                else if (args.CellValue1 == null || args.CellValue2 == null)
                                    args.SortResult = args.CellValue1 == null ? -1 : 1;
                                else if (sortedColumn == predictionIdCol)
                                    args.SortResult = ((int)args.CellValue1).CompareTo((int)args.CellValue2);
                                else if (incidentCols.Contains(sortedColumn))
                                    args.SortResult = ((double)args.CellValue1).CompareTo((double)args.CellValue2);
                                else if (featureCols.Contains(sortedColumn))
                                    args.SortResult = Math.Abs((double)args.CellValue1).CompareTo(Math.Abs((double)args.CellValue2));
                                else
                                    throw new Exception("Unknown column type");

                                args.Handled = true;
                            });

                        PredictionDataGridViewForm form = new PredictionDataGridViewForm();
                        form.MaximumSize = Screen.FromControl(this).Bounds.Size;
                        form.Controls.Add(dataView);
                        form.Show();
                        dataView.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
                        form.ClientSize = dataView.Size = dataView.PreferredSize;
                        form.Location = new System.Drawing.Point(0, 0);
                    }
                    else
                        MessageBox.Show("No predictions were made at that location.");
                }
            }
        }

        private void setBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ColorDialog cd = new ColorDialog();
            if (cd.ShowDialog() == DialogResult.OK)
                BackColor = cd.Color;
        }
    }
}
