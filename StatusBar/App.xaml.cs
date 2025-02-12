using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatusBar {
  /// <summary>
  /// Interaktionslogik für "App.xaml"
  /// </summary>
  public partial class App : Application {

    DispatcherTimer Timer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(10) };

    internal Tools.ScreenMarginManager screenMarginManager = new Tools.ScreenMarginManager(() => new Window.Bar());

    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      screenMarginManager.AddBottomMargin(18);
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
