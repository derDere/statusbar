using StatusBar.Control;
using StatusBar.Tools;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace StatusBar.Window {
  /// <summary>
  /// Interaktionslogik für AppMenu.xaml
  /// </summary>
  public partial class AppMenu : System.Windows.Window {

    private IEnumerable<IconConfig> icons;
    private Bar bar;

    private static bool _IsMenuOpen = false;
    public static bool IsMenuOpen {
      get {
        return _IsMenuOpen;
      }
    }

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
        _IsMenuOpen = _isOpen;
      }
    }

    public AppMenu(Bar parent) {
      InitializeComponent();

      Tile.GlobalTileProcessStarting += Tile_GlobalTileProcessStarting;

      Brush bru;
      try {
        bru = (Brush)(new BrushConverter().ConvertFromString(BarContent.contentProvider.ApplyTo(Config.BackgroundColor)));
      }
      catch (Exception) {
        bru = (Brush)(new BrushConverter().ConvertFromString(Config.DEFAULT_BACKGROUND));
      }
      bru.Opacity = 0.8;
      Border.Background = bru;

      bar = parent;
      ReloadIcons();
    }

    private void Tile_GlobalTileProcessStarting(EventArgs e) {
      CloseMenu();
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
      this.BringIntoView();
      this.Activate();
      this.Focus();
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
      int maxColCount = pos.Column + pos.GetValidColSpan();
      int maxRowCount = pos.Row + pos.GetValidRowSpan();

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
      Grid.SetColumnSpan(tile, tile.MenuPosition.GetValidColSpan());
      Grid.SetRowSpan(tile, tile.MenuPosition.GetValidRowSpan());
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
