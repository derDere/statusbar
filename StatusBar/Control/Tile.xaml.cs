using StatusBar.Tools;
using StatusBar.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using IO = System.IO;

namespace StatusBar.Control {
  /// <summary>
  /// Interaktionslogik für Tile.xaml
  /// </summary>
  public partial class Tile : System.Windows.Controls.UserControl {

    // -------------------------------------------------------------------
    // TileProcessStarting Event
    public delegate void TileProcessStartingHandler(object sender, EventArgs e);
    public event TileProcessStartingHandler TileProcessStarting;
    public delegate void GlobalTileProcessStartingHandler(EventArgs e);
    public static event GlobalTileProcessStartingHandler GlobalTileProcessStarting;
    [System.Diagnostics.DebuggerHidden]
    protected virtual void OnTileProcessStarting(EventArgs e) { }
    [System.Diagnostics.DebuggerHidden]
    private void RaiseTileProcessStarting(EventArgs e) {
      OnTileProcessStarting(e);
      TileProcessStarting?.Invoke(this, e);
      GlobalTileProcessStarting?.Invoke(e);
    }

    private static readonly Random RND = new Random();
    private static readonly string[] RND_COLORS = {
      "#FF2d6281",
      "#FFFDC675",
      "#FFFD9D75",
      "#FF59a100",
      "#FF96E3A5",
      "#FF41BEC2",
      "#FF4D8BFD",
      "#FFA77BFD",
      "#FF83618a",
    };

    public class TilePosition {
      public int Column { get; set; }
      public int Row { get; set; }
      public int ColSpan { get; set; }
      public int RowSpan { get; set; }

      public TilePosition(string posStr) {
        IEnumerable<int> values = from part in posStr.Split(',') select int.Parse(part.Trim());
        Column = values.Skip(0)?.First() ?? 0;
        Row = values.Skip(1)?.First() ?? 0;
        ColSpan = values.Skip(2)?.First() ?? 1;
        RowSpan = values.Skip(3)?.First() ?? 1;

        if (Column < 0)
          Column = 0;
        if (Row < 0)
          Row = 0;
      }

      public int GetValidColSpan() {
        return ColSpan < 1 ? 1 : ColSpan;
      }

      public int GetValidRowSpan() {
        return RowSpan < 1 ? 1 : RowSpan;
      }
    }

    public TilePosition MenuPosition { get; set; }

    private IconConfig myConfig;

    private DispatcherTimer widgetTicket;
    private BackgroundWorker bgw;

    public Tile(IconConfig iconConfig) {
      InitializeComponent();

      widgetTicket = new DispatcherTimer() {
        Interval = TimeSpan.FromSeconds(1),
        IsEnabled = false
      };
      widgetTicket.Tick += WidgetTicket_Tick;

      bgw = new BackgroundWorker();
      bgw.DoWork += this.Bgw_DoWork;
      bgw.RunWorkerCompleted += this.Bgw_RunWorkerCompleted;

      myConfig = iconConfig;
      MenuPosition = new TilePosition(myConfig.Position);

      UpdateDisplay();
    }

    private void Bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      Exception ex = e.Result as Exception;
      string result = e.Result as string;
      if (ex != null) {
        Debugger.Break();
      }
      else if (result != null) {
        IconCC.Content = result.Trim();
        widgetTicket.Start();
      }
    }

    private void Bgw_DoWork(object sender, DoWorkEventArgs e) {
      ProcessStartInfo psi = e.Argument as ProcessStartInfo;
      if (psi != null) {
        try {
          Process p = Process.Start(psi);
          string result = p.StandardOutput.ReadToEnd();
          e.Result = result;
        }
        catch (Exception ex) {
          if (Debugger.IsAttached) {
            Debugger.Break();
          }
          e.Result = ex;
        }
      }
      else {
        e.Result = new Exception("PSI was null");
      }
    }

    private Brush ToColor(string val) {
      if (val == IconConfig.RANDOM_COLOR) {
        val = RND_COLORS[RND.Next(1000, 9999) % RND_COLORS.Length];
      }
      if (val.Trim().Length <= 0) {
        return Brushes.Transparent;
      }
      try {
        byte a = (byte)int.Parse(val.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
        byte r = (byte)int.Parse(val.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = (byte)int.Parse(val.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = (byte)int.Parse(val.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);

        return new SolidColorBrush(Color.FromArgb(a, r, g, b));
      }
      catch (Exception ex) {
        if (Debugger.IsAttached) {
          Debugger.Break();
        }
        return Brushes.Red;
      }
    }

    private static BitmapImage LoadImage(string path) {
      if (path.StartsWith("\"")) {
        path = path.Substring(1);
      }
      if (path.EndsWith("\"")) {
        path = path.Substring(0, path.Length - 1);
      }
      if (IO.File.Exists(path)) {
        try {
          BitmapImage source = new BitmapImage();
          using (IO.FileStream file = new IO.FileStream(path, IO.FileMode.Open, IO.FileAccess.Read)) {
            source.BeginInit();
            source.CacheOption = BitmapCacheOption.OnLoad;
            source.StreamSource = file;
            source.EndInit();
          }
          return source;
        }
        catch (Exception ex) {
          if (Debugger.IsAttached) {
            Debugger.Break();
          }
        }
      }
      return new BitmapImage(new Uri("pack://application:,,,/StatusBar;component/Resources/cross.png"));
    }

    private void UpdateDisplay() {
      widgetTicket.Stop();

      FontFamily font;
      try {
        font = new FontFamily(BarContent.contentProvider.ApplyTo(Config.Font));
      }
      catch (Exception) {
        font = new FontFamily(Config.DEFAULT_FONT);
      }
      TitleTxb.FontFamily = font;
      IconCC.FontFamily = font;

      if (myConfig.IsCategory) {
        Border.Margin = new Thickness(2);
        Border.Background = null;
        Border.BorderBrush = ToColor(myConfig.Background);
        Border.BorderThickness = new Thickness(1);
        Border.CornerRadius = new CornerRadius(8);

        TitleTxb.VerticalAlignment = VerticalAlignment.Top;
        TitleTxb.Margin = new Thickness(10, 20, 10, 0);

        TriggerBtn.IsEnabled = false;
      }
      else {
        Border.Margin = new Thickness(5);
        Border.Background = ToColor(myConfig.Background);
        Border.BorderBrush = null;
        Border.BorderThickness = new Thickness(0);
        Border.CornerRadius = new CornerRadius(6);

        TitleTxb.VerticalAlignment = VerticalAlignment.Bottom;
        TitleTxb.Margin = new Thickness(10, 0, 10, 10);

        if (myConfig.IsWidget) {
          TriggerBtn.IsEnabled = false;
        }
        else {
          TriggerBtn.IsEnabled = true;
        }
      }

      TitleTxb.Foreground = ToColor(myConfig.Foreground);
      IconCC.Foreground = TitleTxb.Foreground;

      if (MenuPosition.ColSpan <= 1 || MenuPosition.RowSpan <= 1) {
        int span = Math.Min(MenuPosition.ColSpan, MenuPosition.RowSpan);
        double m = (span == 1) ? 2 : 0;
        if (span < 0) {
          m += Math.Abs(span) * 5;
        }
        ViewB.Margin = new Thickness(m);
        Border.ToolTip = myConfig.DisplayName;
        TitleTxb.Text = "";
      }
      else {
        ViewB.Margin = new Thickness(10, 10, 10, 30);
        Border.ToolTip = null;

        if (myConfig.DisplayName.Trim().Length <= 0) {
          ViewB.Margin = new Thickness(10, 10, 10, 10);
          TitleTxb.Text = "";
        }
        else {
          TitleTxb.Text = myConfig.DisplayName;
        }
      }

      if (myConfig.ImagePath.Trim().Length > 0) {
        int imgW = (int)(64 - Border.Margin.Left - Border.Margin.Right - IconCC.Margin.Left - IconCC.Margin.Right);
        int imgH = (int)(64 - Border.Margin.Top - Border.Margin.Bottom - IconCC.Margin.Top - IconCC.Margin.Bottom);
        Image img = new Image();
        BitmapImage source = LoadImage(myConfig.ImagePath);
        img.Source = source;
        if ((source.Width < imgW) && (source.Height < imgH)) {
          RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);
        }
        else {
          RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
        }
        IconCC.Content = img;
      }
      else if (myConfig.Emoji.Trim().Length > 0) {
        IconCC.Content = myConfig.Emoji.Trim();
      }
      else {
        IconCC.Content = null;
      }

      if (myConfig.IsWidget) {
        TitleTxb.Text = "";
        IconCC.Content = null;
        ViewB.Margin = new Thickness(10, 2, 10, 2);
        WidgetTicket_Tick(null, null);
      }
    }

    private void WidgetTicket_Tick(object sender, EventArgs e) {
      widgetTicket.Stop();
      if (myConfig.IsWidget) {
        if (AppMenu.IsMenuOpen) {
          ProcessStartInfo psi = GetPSI();
          if (psi == null) {
            return;
          }
          psi.RedirectStandardOutput = true;
          psi.UseShellExecute = false;
          psi.CreateNoWindow = true;
          psi.StandardOutputEncoding = System.Text.Encoding.UTF8;
          bgw.RunWorkerAsync(psi);
        }
        else {
          widgetTicket.Start();
        }
      }
    }

    private ProcessStartInfo GetPSI() {
      try {
        string[] parts = myConfig.Exe.Split('|');
        string exePath = parts.First();
        string args = "";
        if (parts.Length > 1) {
          args = string.Join(" ", parts.Skip(1));
        }
        IO.FileInfo exe = new IO.FileInfo(exePath);
        string workDir = myConfig.WorkingDirectory;
        if (workDir.Trim().Length <= 0) {
          workDir = exe.Directory.FullName;
        }
        ProcessStartInfo psi = new ProcessStartInfo() {
          FileName = exe.FullName,
          WorkingDirectory = workDir,
          Arguments = args
        };
        return psi;
      }
      catch (Exception ex) {
        if (Debugger.IsAttached) {
          Debugger.Break();
        }
        return null;
      }
    }

    private void TriggerBtn_Click(object sender, RoutedEventArgs e) {
      try {
        RaiseTileProcessStarting(new EventArgs());
        ProcessStartInfo psi = GetPSI();
        if (psi == null) {
          System.Windows.Forms.MessageBox.Show("Invalid Process Start Infos!", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        Process.Start(psi);
      }
      catch (Exception ex) {
        if (Debugger.IsAttached) {
          Debugger.Break();
        }
      }
    }
  }
}
