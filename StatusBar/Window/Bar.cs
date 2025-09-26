using StatusBar.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StatusBar.Window {
  public class Bar : System.Windows.Forms.Form {

    private BarContent content;
    private AppMenu menu = null;
    private WindowsKeyInterceptor winKeys;

    public Bar() : base() {
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.ShowInTaskbar = false;
      this.TopMost = true;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.BackColor = System.Drawing.Color.Black;

      System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost();
      host.Dock = System.Windows.Forms.DockStyle.Fill;
      content = new Control.BarContent();
      content.MenuButtonClicked += BarContent_MenuButton_Clicked;
      host.Child = content;
      this.Controls.Add(host);

      menu = new AppMenu(this);

      if (Config.UseWindowsKey) {
        winKeys = new WindowsKeyInterceptor();
        winKeys.SuppressSystemHandling = true;
        winKeys.WindowsKeyEvent += this.WinKeys_WindowsKeyEvent;
        winKeys.Start();
      }
    }

    private void WinKeys_WindowsKeyEvent(object sender, WindowsKeyEventArgs e) {
      menu.IsOpen = true;
    }

    private void BarContent_MenuButton_Clicked(object sender, EventArgs e) {
      menu.IsOpen = true;
    }
  }
}
