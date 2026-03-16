using Gigasoft.ProEssentials.Enums;
using System;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media;

namespace GigaPrime2D_WPF
{
    /// <summary>
    /// GigaPrime2D WPF — 100 Million Point GPU Compute Shader Demo
    ///
    /// Demonstrates ProEssentials v10 GPU compute shader rendering of
    /// 100 million data points completely re-passed per timer tick.
    ///
    /// 5 subsets x 20,000,000 points = 100M data points per update.
    ///
    /// Note: WPF renders to texture and updates max at ~25ms.
    /// For maximum real-time performance (~15ms), see the WinForms version:
    /// https://github.com/GigasoftInc/winforms-chart-100million-points-proessentials
    /// </summary>
    public partial class MainWindow : Window
    {
        // --- Data arrays ---
        public static float[] wavedata  = new float[10000];
        public static float[] wavedata2 = new float[10000];
        public static float[] wavedata3 = new float[10000];
        public static float[] wavedata4 = new float[10000];
        public static float[] wavedata5 = new float[10000];

        public static float[] fYDataPool    = new float[120010000]; // One time prep buffer
        public static float[] fXData        = new float[20000000];  // Shared X data for all subsets
        public static float[] fYDataToChart = new float[100000000]; // 100M buffer — pointer passed to chart
        // Chart uses fYDataToChart pointer directly via UseDataAtLocation.
        // Changing contents of fYDataToChart is all that is needed to update the chart.

        public static Random Rand_Num = new Random(unchecked((int)DateTime.Now.Ticks));

        private int _frameCount = 0;
        private DateTime _lastFpsTime = DateTime.Now;

        private DispatcherTimer _timer;
        private bool _updatingSlider = false;

        public MainWindow()
        {
            InitializeComponent();
            Pesgo1.PeZoomIn  += Pesgo1_PeZoomIn;
            Pesgo1.PeZoomOut += Pesgo1_PeZoomOut;
        }

        private void Pesgo1_Loaded(object sender, RoutedEventArgs e)
        {
            // --- Initialize timer ---
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(25); // WPF max ~25ms vs WinForms ~15ms
            _timer.Tick += Timer1_Tick;

            // --- Prepare X data ---
            for (int j = 0; j < 20000000; j++)
                fXData[j] = (j + 1);

            // --- Prepare waveforms ---
            for (int j = 0; j <= 9999; j++)
            {
                wavedata[j]  = (((float)Math.Sin(3.1415 * 0.0002F * j)) * 10.0F) + 10.0F;
                wavedata2[j] = (((float)Math.Sin(3.1415 * 0.0001F * j)) * 20.0F);
                wavedata3[j] = (((float)Math.Sin(3.1415 * 0.0006F * j)) * 10.0F) + 10.0F;
                wavedata4[j] = ((float)Math.Sin(3.1415 * 0.00005F * j)) * 20.0F;
                wavedata5[j] = ((float)Math.Sin(2.5415 * 0.0004F * j) * (float)Math.Sin(3.1415 * 0.0001F * j) * 10.0F) + 10.0F;
            }

            // --- Fill large data pool repeating waveform data ---
            int nShift;

            nShift = (int)((float)(Rand_Num.NextDouble()) * 9000.0F);
            for (int j = 0; j < 24000000; j += 10000)
                Array.Copy(wavedata, 0, fYDataPool, j, 10000);

            nShift = (int)((float)(Rand_Num.NextDouble()) * 9000.0F);
            Array.Copy(wavedata2, nShift, fYDataPool, 24000000, 10000 - nShift);
            for (int j = nShift; j < 24000000; j += 10000)
                Array.Copy(wavedata2, 0, fYDataPool, j + 24000000, 10000);

            nShift = (int)((float)(Rand_Num.NextDouble()) * 9000.0F);
            Array.Copy(wavedata3, nShift, fYDataPool, 48000000, 10000 - nShift);
            for (int j = nShift; j < 24000000; j += 10000)
                Array.Copy(wavedata3, 0, fYDataPool, j + 48000000, 10000);

            nShift = (int)((float)(Rand_Num.NextDouble()) * 9000.0F);
            Array.Copy(wavedata4, nShift, fYDataPool, 72000000, 10000 - nShift);
            for (int j = nShift; j < 24000000; j += 10000)
                Array.Copy(wavedata4, 0, fYDataPool, j + 72000000, 10000);

            nShift = (int)((float)(Rand_Num.NextDouble()) * 9000.0F);
            Array.Copy(wavedata5, nShift, fYDataPool, 96000000, 10000 - nShift);
            for (int j = nShift; j < 24000000; j += 10000)
                Array.Copy(wavedata5, 0, fYDataPool, j + 96000000, 10000);

            // --- Initialize ProEssentials PesgoWpf ---

            Pesgo1.PeFont.SizeGlobalCntl = 1.05F;

            Pesgo1.PeData.Subsets = 5;
            Pesgo1.PeData.Points  = 20000000; // 20M points x 5 subsets = 100M per update

            // Define 5 axes, 1 subset per axis
            Pesgo1.PeGrid.MultiAxesSubsets[0] = 1;
            Pesgo1.PeGrid.MultiAxesSubsets[1] = 1;
            Pesgo1.PeGrid.MultiAxesSubsets[2] = 1;
            Pesgo1.PeGrid.MultiAxesSubsets[3] = 1;
            Pesgo1.PeGrid.MultiAxesSubsets[4] = 1;

            // X axis
            Pesgo1.PeGrid.Configure.ManualScaleControlX = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinX = 0;
            Pesgo1.PeGrid.Configure.ManualMaxX = 20000000;
            Pesgo1.PeString.XAxisLabel = "Sample";

            // Y axis per WorkingAxis
            Pesgo1.PeGrid.WorkingAxis = 0;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = 0;
            Pesgo1.PeGrid.Configure.ManualMaxY = 21;
            Pesgo1.PeString.YAxisLabel = "uV";

            Pesgo1.PeGrid.WorkingAxis = 1;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = 0;
            Pesgo1.PeGrid.Configure.ManualMaxY = 21;
            Pesgo1.PeString.YAxisLabel = "uV";

            Pesgo1.PeGrid.WorkingAxis = 2;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = 0;
            Pesgo1.PeGrid.Configure.ManualMaxY = 21;
            Pesgo1.PeString.YAxisLabel = "mV";

            Pesgo1.PeGrid.WorkingAxis = 3;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = 0;
            Pesgo1.PeGrid.Configure.ManualMaxY = 21;
            Pesgo1.PeString.YAxisLabel = "mV";

            Pesgo1.PeGrid.WorkingAxis = 4;
            Pesgo1.PeGrid.Configure.ManualScaleControlY = ManualScaleControl.MinMax;
            Pesgo1.PeGrid.Configure.ManualMinY = 0;
            Pesgo1.PeGrid.Configure.ManualMaxY = 21;
            Pesgo1.PeString.YAxisLabel = "uV";
            Pesgo1.PeGrid.WorkingAxis = 0; // always reset WorkingAxis when done

            // Reset default data points
            Pesgo1.PeData.Y[0, 0] = 0; Pesgo1.PeData.Y[0, 1] = 0;
            Pesgo1.PeData.Y[0, 2] = 0; Pesgo1.PeData.Y[0, 3] = 0;
            Pesgo1.PeData.X[0, 0] = 1.0F; Pesgo1.PeData.X[0, 1] = 2.0F;
            Pesgo1.PeData.X[0, 2] = 3.0F; Pesgo1.PeData.X[0, 3] = 4.0F;

            Pesgo1.PeData.NullDataValue  = -9999999;
            Pesgo1.PeData.NullDataValueX = -9999999;

            Pesgo1.PeString.MainTitle = "";
            Pesgo1.PeString.SubTitle  = "";

            // Disable built-in UI elements managed by our custom controls
            Pesgo1.PeUserInterface.Allow.FocalRect             = false;
            Pesgo1.PeUserInterface.Dialog.PlotCustomization    = false;
            Pesgo1.PeUserInterface.Dialog.Page2                = true;
            Pesgo1.PeUserInterface.Dialog.Axis                 = false;
            Pesgo1.PeUserInterface.Dialog.Subsets              = false;
            Pesgo1.PeUserInterface.Dialog.RandomPointsToExport = false;
            Pesgo1.PeUserInterface.Allow.Customization         = false;
            Pesgo1.PeUserInterface.Allow.Maximization          = false;
            Pesgo1.PeUserInterface.Allow.Popup                 = true;
            Pesgo1.PeUserInterface.Menu.BorderType             = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.BitmapGradient         = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.QuickStyle             = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.ViewingStyle           = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.ShowLegend             = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.PlotMethod             = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.MarkDataPoints         = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.CustomizeDialog        = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.DataShadow             = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.DataPrecision          = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.LegendLocation         = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.ShowAnnotations        = MenuControl.Hide;
            Pesgo1.PeUserInterface.Menu.AnnotationControl      = false;
            Pesgo1.PeUserInterface.Dialog.AllowEmfExport       = false;
            Pesgo1.PeUserInterface.Dialog.AllowSvgExport       = false;
            Pesgo1.PeUserInterface.Dialog.AllowWmfExport       = false;
            Pesgo1.PeUserInterface.Allow.TextExport            = false;
            Pesgo1.PeUserInterface.Dialog.HideExportImageDpi   = true;
            Pesgo1.PeUserInterface.Dialog.HidePrintDpi         = true;

            // Zoom and scrollbar settings
            Pesgo1.PeUserInterface.Scrollbar.ScrollingHorzZoom       = true;
            Pesgo1.PeUserInterface.Scrollbar.MouseWheelFunction       = MouseWheelFunction.HorizontalZoom;
            Pesgo1.PeUserInterface.Scrollbar.MouseWheelZoomFactor     = 1.05F;
            Pesgo1.PeUserInterface.Scrollbar.MouseWheelZoomEvents     = true;
            Pesgo1.PeUserInterface.Allow.Zooming                      = AllowZooming.None;

            // Subset labels
            Pesgo1.PeString.SubsetLabels[0] = "Signal 1";
            Pesgo1.PeString.SubsetLabels[1] = "Signal 2";
            Pesgo1.PeString.SubsetLabels[2] = "Signal 3";
            Pesgo1.PeString.SubsetLabels[3] = "Signal 4";
            Pesgo1.PeString.SubsetLabels[4] = "Signal 5";

            // Subset colors — matching WinForms version
            Pesgo1.PeColor.SubsetColors[0] = Color.FromArgb(255, 255, 0,   69);
            Pesgo1.PeColor.SubsetColors[1] = Color.FromArgb(255, 63,  255, 0);
            Pesgo1.PeColor.SubsetColors[2] = Color.FromArgb(255, 255, 168, 0);
            Pesgo1.PeColor.SubsetColors[3] = Color.FromArgb(255, 255, 20,  255);
            Pesgo1.PeColor.SubsetColors[4] = Color.FromArgb(255, 26,  255, 255);
            Pesgo1.PeColor.SubsetShades[0] = Color.FromArgb(255, 80,  80,  80);
            Pesgo1.PeColor.SubsetShades[1] = Color.FromArgb(255, 100, 100, 100);
            Pesgo1.PeColor.SubsetShades[2] = Color.FromArgb(255, 60,  60,  60);
            Pesgo1.PeColor.SubsetShades[3] = Color.FromArgb(255, 120, 120, 120);
            Pesgo1.PeColor.SubsetShades[4] = Color.FromArgb(255, 50,  50,  50);

            Pesgo1.PePlot.DataShadows          = DataShadows.None;
            Pesgo1.PePlot.SubsetLineTypes[0]   = LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[1]   = LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[2]   = LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[3]   = LineType.ThinSolid;
            Pesgo1.PePlot.SubsetLineTypes[4]   = LineType.ThinSolid;

            Pesgo1.PeSpecial.DpiX = 600;
            Pesgo1.PeSpecial.DpiY = 600;

            Pesgo1.PeUserInterface.Cursor.HourGlassThreshold = 2000000000;
            Pesgo1.PeFont.FontSize = Gigasoft.ProEssentials.Enums.FontSize.Large;    //  = FontSize.Large;
            Pesgo1.PeFont.Fixed     = true;
            Pesgo1.PeLegend.Show    = false;

            Pesgo1.PeConfigure.ImageAdjustTop  = 100;
            Pesgo1.PeConfigure.ImageAdjustLeft = 100;

            Pesgo1.PeSpecial.AutoImageReset = false; // important for D3D, call Reinitialize explicitly

            Pesgo1.PeConfigure.Composite2D3D = Composite2D3D.Background;

            // Color theme — dark teal matching WinForms version
            Pesgo1.PeColor.BitmapGradientMode = false;
            Pesgo1.PeConfigure.BorderTypes    = TABorder.NoBorder;
            Pesgo1.PeColor.GraphBmpStyle      = BitmapStyle.NoBmp;
            Pesgo1.PeColor.GraphBackground    = Color.FromArgb(0xff, 0x00, 0x2B, 0x35);
            Pesgo1.PeColor.Desk               = Color.FromArgb(0xff, 0x00, 0x2B, 0x35);
            Pesgo1.PeColor.GraphForeground    = Colors.White;
            Pesgo1.PeColor.Text               = Colors.White;
            Pesgo1.PeGrid.GridBands           = false;
            Pesgo1.PeColor.GridBold           = false;

            Pesgo1.PeConfigure.CacheBmp      = true;
            Pesgo1.PeConfigure.PrepareImages = true;
            Pesgo1.PeConfigure.RenderEngine  = RenderEngine.Direct3D;

            // Share X data across all subsets — avoids duplicating 20M x data points
            Pesgo1.PeData.DuplicateDataX = DuplicateData.PointIncrement;

            // Pass pointers to data arrays — no copy, chart uses app memory directly
            Pesgo1.PeData.X.UseDataAtLocation(fXData,        20000000);
            Pesgo1.PeData.Y.UseDataAtLocation(fYDataToChart, 100000000);

            // v10 GPU compute shader settings
            Pesgo1.PeData.ComputeShader   = true;  // GPU-side chart construction
            Pesgo1.PeData.Filter2D3D      = true;  // only set with ComputeShader + Line plotting
            Pesgo1.PeData.StagingBufferY  = true;  // always set for ComputeShader
            Pesgo1.PeData.StagingBufferX  = true;  // always set for ComputeShader

            // Set axis Y colors to match subset colors
            for (int i = 0; i < 5; i++)
            {
                Pesgo1.PeGrid.WorkingAxis = i;
                Pesgo1.PeColor.YAxis = Pesgo1.PeColor.SubsetColors[i];
            }
            Pesgo1.PeGrid.WorkingAxis = 0;

            // Final render
            Pesgo1.PeFunction.Force3dxNewColors      = true;
            Pesgo1.PeFunction.Force3dxVerticeRebuild = true;
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();

            // Initialize zoom slider
            SampleViewToZoomAmount(175);
        }

        // -----------------------------------------------------------------------
        // Timer tick — re-passes 100M data points from pool to chart buffer
        // -----------------------------------------------------------------------
        private void Timer1_Tick(object sender, EventArgs e)
        {
            _timer.Stop();

            // FPS counter
            _frameCount++;
            var elapsed = (DateTime.Now - _lastFpsTime).TotalSeconds;
            if (elapsed >= 1.0)
            {
                Title = $"GigaPrime2D WPF — 100M Points — {_frameCount} FPS";
                _frameCount = 0;
                _lastFpsTime = DateTime.Now;
            }

            Random rn = new Random();
            int iRandomOffset = rn.Next(600000);

            // Copy 100M points from random offset in pool to chart buffer
            Array.Copy(fYDataPool, iRandomOffset,            fYDataToChart, 0,        20000000);
            Array.Copy(fYDataPool, iRandomOffset + 24000000, fYDataToChart, 20000000, 20000000);
            Array.Copy(fYDataPool, iRandomOffset + 48000000, fYDataToChart, 40000000, 20000000);
            Array.Copy(fYDataPool, iRandomOffset + 72000000, fYDataToChart, 60000000, 20000000);
            Array.Copy(fYDataPool, iRandomOffset + 96000000, fYDataToChart, 80000000, 20000000);

            Pesgo1.PeData.ReuseDataX             = true; // X data unchanged, reuse buffer
            Pesgo1.PeFunction.Force3dxVerticeRebuild = true; // process new Y data
            Pesgo1.Invalidate();
            _timer.Start();
        }

        // -----------------------------------------------------------------------
        // Timer checkbox
        // -----------------------------------------------------------------------
        private void TimerControl_Changed(object sender, RoutedEventArgs e)
        {
            if (timerControl.IsChecked == true)
            {
                _timer.Interval = TimeSpan.FromMilliseconds(15);
                _timer.Start();
            }
            else
            {
                _timer.Stop();
            }
        }

        // -----------------------------------------------------------------------
        // Zoom slider
        // -----------------------------------------------------------------------
        private void SliderSampleView_ValueChanged(object sender,
            System.Windows.RoutedPropertyChangedEventArgs<double> e)
        {
            if (Pesgo1 != null)
            {
                if (Pesgo1.Chart == null) return;
                if (_updatingSlider) return;
                SampleViewToZoomAmount((int)sliderSampleView.Value);
                Pesgo1.PeGrid.Zoom.Mode = true;
                Pesgo1.PeFunction.Reinitialize();
                Pesgo1.Invalidate();
            }
        }

        private void SampleViewToZoomAmount(int nSliderValue)
        {
            double dValue     = nSliderValue / 1000.0F;
            double dZoomRange = 20000000.0F * dValue;
            double dHalfRange = dZoomRange / 2.0F;

            Pesgo1.PeGrid.Zoom.MinX = 10000000.0F - dHalfRange;
            Pesgo1.PeGrid.Zoom.MaxX = 10000000.0F + dHalfRange;

            for (int i = 0; i < 5; i++)
            {
                Pesgo1.PeGrid.WorkingAxis = i;
                Pesgo1.PeGrid.Zoom.MinY   = Pesgo1.PeGrid.Configure.ManualMinY;
                Pesgo1.PeGrid.Zoom.MaxY   = Pesgo1.PeGrid.Configure.ManualMaxY;
            }
            Pesgo1.PeGrid.WorkingAxis  = 0;
            Pesgo1.PeGrid.Zoom.Mode    = true;
        }

        // -----------------------------------------------------------------------
        // Zoom events — sync slider with chart zoom state
        // -----------------------------------------------------------------------
        private void Pesgo1_PeZoomIn(object sender, EventArgs e)
        {
            if (Pesgo1.PeGrid.Zoom.Mode)
            {
                double dZoom        = Pesgo1.PeGrid.Zoom.MaxX - Pesgo1.PeGrid.Zoom.MinX;
                double dZoomPercent = (dZoom / 20000000.0F) * 100;
                int nNewValue       = (int)dZoomPercent * 10;
                if (nNewValue < 1)    nNewValue = 1;
                if (nNewValue > 1000) nNewValue = 1000;
                _updatingSlider = true;
                sliderSampleView.Value = nNewValue;
                _updatingSlider = false;
            }
            else
            {
                _updatingSlider = true;
                sliderSampleView.Value = 1000;
                _updatingSlider = false;
            }
        }

        private void Pesgo1_PeZoomOut(object sender, EventArgs e)
        {
            _updatingSlider = true;
            sliderSampleView.Value = 1000;
            _updatingSlider = false;
        }

        // -----------------------------------------------------------------------
        // Combine Axes
        // -----------------------------------------------------------------------
        private void CombineAxes_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (combineAxes.IsChecked == true)
            {
                Pesgo1.PeGrid.OverlapMultiAxes[0] = 5;
                hideAxes.IsEnabled = true;

                highlightAxis1.IsChecked = false;
                highlightAxis2.IsChecked = false;
                highlightAxis3.IsChecked = false;
                highlightAxis4.IsChecked = false;
                highlightAxis5.IsChecked = false;

                if (hideAxes.IsChecked == true) Hide4Axes();
                else Show4Axes();
            }
            else
            {
                Pesgo1.PeGrid.OverlapMultiAxes.Clear();
                hideAxes.IsEnabled = false;
                Show4Axes();
            }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Hide/Show Axes
        // -----------------------------------------------------------------------
        private void HideAxes_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (hideAxes.IsChecked == true) Hide4Axes();
            else Show4Axes();
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        private void Hide4Axes()
        {
            for (int i = 1; i < 5; i++)
            {
                Pesgo1.PeGrid.WorkingAxis = i;
                Pesgo1.PeGrid.Option.ShowYAxis = ShowAxis.Empty;
            }
            Pesgo1.PeGrid.WorkingAxis  = 0;
            Pesgo1.PeString.YAxisLabel = "Combined Axes";
        }

        private void Show4Axes()
        {
            for (int i = 1; i < 5; i++)
            {
                Pesgo1.PeGrid.WorkingAxis = i;
                Pesgo1.PeGrid.Option.ShowYAxis = ShowAxis.All;
            }
            Pesgo1.PeGrid.WorkingAxis  = 0;
            Pesgo1.PeString.YAxisLabel = "uV";
        }

        // -----------------------------------------------------------------------
        // Show Legend
        // -----------------------------------------------------------------------
        private void ShowLegend_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (showLegend.IsChecked == true)
            {
                Pesgo1.PeLegend.Show  = true;
                Pesgo1.PeLegend.Style = LegendStyle.OneLineTopOfAxis;
            }
            else
            {
                Pesgo1.PeLegend.Show  = false;
                Pesgo1.PeLegend.Style = LegendStyle.TwoLine;
            }
            Pesgo1.PeLegend.SimpleLine = true;
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Highlight Axis handlers
        // -----------------------------------------------------------------------
        private void HighlightAxis1_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (highlightAxis1.IsChecked == true)
            {
                highlightAxis2.IsChecked = false;
                highlightAxis3.IsChecked = false;
                highlightAxis4.IsChecked = false;
                highlightAxis5.IsChecked = false;
                Pesgo1.PeGrid.MultiAxesProportions[0] = .80F;
                Pesgo1.PeGrid.MultiAxesProportions[1] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[2] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[3] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[4] = .05F;
            }
            else { Pesgo1.PeGrid.MultiAxesProportions.Clear(); }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        private void HighlightAxis2_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (highlightAxis2.IsChecked == true)
            {
                highlightAxis1.IsChecked = false;
                highlightAxis3.IsChecked = false;
                highlightAxis4.IsChecked = false;
                highlightAxis5.IsChecked = false;
                Pesgo1.PeGrid.MultiAxesProportions[0] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[1] = .80F;
                Pesgo1.PeGrid.MultiAxesProportions[2] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[3] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[4] = .05F;
            }
            else { Pesgo1.PeGrid.MultiAxesProportions.Clear(); }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        private void HighlightAxis3_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (highlightAxis3.IsChecked == true)
            {
                highlightAxis1.IsChecked = false;
                highlightAxis2.IsChecked = false;
                highlightAxis4.IsChecked = false;
                highlightAxis5.IsChecked = false;
                Pesgo1.PeGrid.MultiAxesProportions[0] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[1] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[2] = .80F;
                Pesgo1.PeGrid.MultiAxesProportions[3] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[4] = .05F;
            }
            else { Pesgo1.PeGrid.MultiAxesProportions.Clear(); }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        private void HighlightAxis4_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (highlightAxis4.IsChecked == true)
            {
                highlightAxis1.IsChecked = false;
                highlightAxis2.IsChecked = false;
                highlightAxis3.IsChecked = false;
                highlightAxis5.IsChecked = false;
                Pesgo1.PeGrid.MultiAxesProportions[0] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[1] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[2] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[3] = .80F;
                Pesgo1.PeGrid.MultiAxesProportions[4] = .05F;
            }
            else { Pesgo1.PeGrid.MultiAxesProportions.Clear(); }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        private void HighlightAxis5_Changed(object sender, RoutedEventArgs e)
        {
            if (Pesgo1.Chart == null) return;
            if (highlightAxis5.IsChecked == true)
            {
                highlightAxis1.IsChecked = false;
                highlightAxis2.IsChecked = false;
                highlightAxis3.IsChecked = false;
                highlightAxis4.IsChecked = false;
                Pesgo1.PeGrid.MultiAxesProportions[0] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[1] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[2] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[3] = .05F;
                Pesgo1.PeGrid.MultiAxesProportions[4] = .80F;
            }
            else { Pesgo1.PeGrid.MultiAxesProportions.Clear(); }
            Pesgo1.PeFunction.ReinitializeResetImage();
            Pesgo1.Invalidate();
        }

        // -----------------------------------------------------------------------
        // Help
        // -----------------------------------------------------------------------
        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            string hs = "GigaPrime2D WPF — 100 Million Point Demo\n\n";
            hs += "This demo demonstrates ProEssentials v10 GPU compute shader rendering — ";
            hs += "100 million data points completely re-passed and rendered per timer tick.\n\n";
            hs += "The title bar displays live FPS. GPU compute shader render time is ~15ms. ";
            hs += "End-to-end frame rate including 100M point data transfer is typically 17-20 FPS ";
            hs += "on a modern development workstation with dedicated GPU.\n\n";
            hs += "Controls:\n";
            hs += "1. Mouse Wheel — zooms X axis range.\n";
            hs += "2. Right-click — shows popup menu.\n";
            hs += "3. Right-click → Undo Zoom — resets chart zoom.\n";
            hs += "4. Zoom X Axes slider — programmatic zoom control.\n";
            hs += "5. Highlight Signal checkboxes — expand individual axis to 80% height.\n";
            hs += "6. Combine Axes — overlaps all 5 signals into one shared graph area.\n\n";
            hs += "WPF vs WinForms Performance:\n";
            hs += "WinForms has a slight performance edge as Direct3D is directly coupled to the ";
            hs += "window device context. WPF renders to texture, adding a compositing step. ";
            hs += "Both versions use identical GPU compute shaders and achieve comparable frame rates. ";
            hs += "For maximum real-time throughput see the WinForms version.\n\n";
            hs += "Performance is dependent on CPU/GPU. Systems without a dedicated GPU or with ";
            hs += "poor integrated graphics may see reduced frame rates. ";
            hs += "ProEssentials renders this data faster than any other known .NET charting library.";
            MessageBox.Show(hs, "GigaPrime2D WPF Help");
        }

        // -----------------------------------------------------------------------
        // Window closing — stop timer cleanly
        // -----------------------------------------------------------------------
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _timer?.Stop();
        }
    }
}
