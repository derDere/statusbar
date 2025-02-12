using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatusBar.Window {
  public class Bar : System.Windows.Forms.Form {

    public Bar() : base() {
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
      this.ShowInTaskbar = false;
      this.TopMost = true;
      this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      this.BackColor = System.Drawing.Color.Black;

      System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost();
      host.Dock = System.Windows.Forms.DockStyle.Fill;
      host.Child = new Control.BarContent();
      this.Controls.Add(host);
    }
  }
}
