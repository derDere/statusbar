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
          Text = contentProvider.ApplyTo(line),
          Background = Brushes.Black,
          Foreground = Brushes.Silver,
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
      foreach (TextBox txb in textBoxes) {
        if (txb.Tag is string content) {
          int ss = txb.SelectionStart;
          int sl = txb.SelectionLength;
          txb.Text = contentProvider.ApplyTo(content);
          txb.SelectionStart = ss;
          txb.SelectionLength = sl;
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
  }
}
