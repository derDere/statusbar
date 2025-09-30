using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace StatusBar {
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application {

    public const int HEIGHT = 18;

    DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };

    internal Tools.ScreenMarginManager screenMarginManager = new Tools.ScreenMarginManager(() => new Window.Bar());

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      if (Config.IsTopPosition) {
        screenMarginManager.AddTopMargin(HEIGHT);
      }
      else {
        screenMarginManager.AddBottomMargin(HEIGHT);
      }
      screenMarginManager.Update();

      Timer.Tick += (s, ev) => screenMarginManager.Update();
      Timer.Start();
    }

    protected override void OnExit(ExitEventArgs e) {
      base.OnExit(e);
      screenMarginManager.Dispose();
    }
  }
}
