using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.Windows.Threading;

namespace StatusBar.Control {
  /// <summary>
  /// Interaktionslogik für BarContent.xaml
  /// </summary>
  public partial class BarContent : UserControl {

    private static Tools.ContentProvider contentProvider = null;

    private DispatcherTimer timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };

    private List<TextBox> textBoxes = new List<TextBox>();

    public BarContent() {
      InitializeComponent();

      if (contentProvider == null) {
        contentProvider = new Tools.ContentProvider();
      }

      GenerateContent();

      timer.Tick += Timer_Tick;
      timer.Start();

      Timer_Tick();
    }

    private static Tuple<FontFamily, Brush, Brush> GetStyle() {
      FontFamily font;
      try {
        font = new FontFamily(contentProvider.ApplyTo(Config.Font));
      }
      catch (Exception) {
        font = new FontFamily(Config.DEFAULT_FONT);
      }

      Brush color;
      try {
        color = (Brush)(new BrushConverter().ConvertFromString(contentProvider.ApplyTo(Config.TextColor)));
      }
      catch (Exception) {
        color = (Brush)(new BrushConverter().ConvertFromString(Config.DEFAULT_COLOR));
      }

      Brush background;
      try {
        background = (Brush)(new BrushConverter().ConvertFromString(contentProvider.ApplyTo(Config.BackgroundColor)));
      }
      catch (Exception) {
        background = (Brush)(new BrushConverter().ConvertFromString(Config.DEFAULT_BACKGROUND));
      }

      return new Tuple<FontFamily, Brush, Brush>(font, color, background);
    }

    private void ReGenerateContent() {
      ClearContent();
      GenerateContent();
    }

    private void ClearContent() {
      Grid.Children.Clear();
      Grid.ColumnDefinitions.Clear();
      textBoxes.Clear();
    }

    private void GenerateContent() {
      string[] lines = Config.Lines.ToArray();

      (FontFamily font, Brush color, Brush background) = GetStyle();
      UC.FontFamily = font;
      UC.Foreground = color;
      UC.Background = background;

      foreach (string line in lines) {
        Grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
        Viewbox vb = new Viewbox() {
          Margin = new Thickness(0),
          VerticalAlignment = VerticalAlignment.Center,
          HorizontalAlignment = HorizontalAlignment.Center
        };
        if (textBoxes.Count <= 0) {
          vb.HorizontalAlignment = HorizontalAlignment.Left;
        }
        else if (textBoxes.Count >= lines.Length - 1) {
          vb.HorizontalAlignment = HorizontalAlignment.Right;
        }
        TextBox txb = new TextBox() {
          FontFamily = font,
          Foreground = color,
          Background = Brushes.Transparent,
          Text = contentProvider.ApplyTo(line),
          BorderThickness = new Thickness(0),
          IsReadOnly = true,
          Tag = line,
          Margin = new Thickness(0),
          Padding = new Thickness(0),
          ContextMenu = Grid.ContextMenu,
          Cursor = Cursors.Arrow,
        };
        vb.Child = txb;
        Grid.Children.Add(vb);
        Grid.SetColumn(vb, Grid.ColumnDefinitions.Count - 1);
        textBoxes.Add(txb);
      }
    }

    private void Timer_Tick(object sender = null, EventArgs e = null) {
      (FontFamily font, Brush color, Brush background) = GetStyle();

      UC.FontFamily = font;
      UC.Foreground = color;
      UC.Background = background;

      foreach (TextBox txb in textBoxes) {
        if (txb.Tag is string content) {
          int ss = txb.SelectionStart;
          int sl = txb.SelectionLength;
          txb.Text = contentProvider.ApplyTo(content);
          txb.SelectionStart = ss;
          txb.SelectionLength = sl;
          txb.FontFamily = font;
          txb.Foreground = color;
        }
      }
    }

    private void ReloadMI_Click(object sender, RoutedEventArgs e) {
      Config.Reload();
      ReGenerateContent();
    }

    private void UpdateMI_Click(object sender, RoutedEventArgs e) {
      Config.Reload();
      ReGenerateContent();
      ((App)App.Current).screenMarginManager.Update(true);
    }

    private void CloseMI_Click(object sender, RoutedEventArgs e) {
      App.Current.Shutdown();
    }

    private void ShowConfigMI_Click(object sender, RoutedEventArgs e) {
      // Open Explorer and show the file in a folder window
      string path = Config.FileName();
      if (
        !string.IsNullOrEmpty(path) &&
        System.IO.File.Exists(path)
      ) {
        System.Diagnostics.Process.Start("explorer.exe", "/select, \"" + path + "\"");
      }
    }
  }
}
