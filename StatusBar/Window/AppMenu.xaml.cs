using StatusBar.Control;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StatusBar.Window {
  /// <summary>
  /// Interaktionslogik für AppMenu.xaml
  /// </summary>
  public partial class AppMenu : System.Windows.Window {

    private IEnumerable<IconConfig> icons;
    private Bar bar;

    private bool _isOpen = false;
    public bool IsOpen {
      get {
        return _isOpen;
      }
      set {
        if (value) {
          OpenMenu();
        }
        else {
          CloseMenu();
        }
      }
    }

    public AppMenu(Bar parent) {
      InitializeComponent();
      bar = parent;
      ReloadIcons();
    }

    public void ReloadIcons() {
      icons = IconConfig.Load();
    }

    private void OpenMenu() {
      if (Config.IsTopPosition) {
        this.Top = bar.Top + bar.Height;
        Border.CornerRadius = new CornerRadius(0, 0, 6, 0);
      }
      else {
        this.Top = bar.Top - this.ActualHeight;
        Border.CornerRadius = new CornerRadius(0, 6, 0, 0);
      }
      this.Left = 0;
      if (ContentGrid.Children.Count <= 0) {
        GenerateIcons();
      }
      this.Show();
      this.Activate();
      _isOpen = true;
    }

    private void CloseMenu() {
      this.Hide();
      _isOpen = false;
    }

    private void Win_Deactivated(object sender, EventArgs e) {
      CloseMenu();
    }

    private void CheckColsRows(Tile.TilePosition pos) {
      int maxColCount = pos.Column + pos.ColSpan;
      int maxRowCount = pos.Row + pos.RowSpan;

      while (maxColCount > ContentGrid.ColumnDefinitions.Count) {
        ColumnDefinition col = new ColumnDefinition();
        col.Width = new GridLength(64, GridUnitType.Pixel);
        ContentGrid.ColumnDefinitions.Add(col);
      }

      while (maxRowCount > ContentGrid.RowDefinitions.Count) {
        RowDefinition row = new RowDefinition();
        row.Height = new GridLength(64, GridUnitType.Pixel);
        ContentGrid.RowDefinitions.Add(row);
      }
    }

    private void AddTile(IconConfig icon) {
      Tile tile = new Tile(icon);
      CheckColsRows(tile.MenuPosition);
      ContentGrid.Children.Add(tile);
      Grid.SetColumn(tile, tile.MenuPosition.Column);
      Grid.SetRow(tile, tile.MenuPosition.Row);
      Grid.SetColumnSpan(tile, tile.MenuPosition.ColSpan);
      Grid.SetRowSpan(tile, tile.MenuPosition.RowSpan);
    }

    private void GenerateIcons() {
      IEnumerable<IconConfig> categories = from icon in icons where icon.IsCategory select icon;
      IEnumerable<IconConfig> widgets = from icon in icons where icon.IsWidget select icon;
      IEnumerable<IconConfig> launchers = from icon in icons where !(icon.IsCategory || icon.IsWidget) select icon;

      foreach (IconConfig icon in categories) {
        AddTile(icon);
      }
      foreach (IconConfig icon in widgets) {
        AddTile(icon);
      }
      foreach (IconConfig icon in launchers) {
        AddTile(icon);
      }
    }

    private void ReloadMI_Click(object sender, RoutedEventArgs e) {
      ContentGrid.Children.Clear();
      ContentGrid.ColumnDefinitions.Clear();
      ContentGrid.RowDefinitions.Clear();
      ReloadIcons();
      GenerateIcons();
    }
  }
}
