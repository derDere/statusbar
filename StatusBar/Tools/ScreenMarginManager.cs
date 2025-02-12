using StatusBar.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Forms = System.Windows.Forms;

namespace StatusBar.Tools {
  public class ScreenMarginManager : IDisposable {

    #region Declarations
    [System.Runtime.InteropServices.DllImport("Shell32")]
    private static extern int SHAppBarMessage(ABM_ dwMessage, ref AppBarData pData);

    private struct AppBarPlacement {
      public int Left; // Position if the Rectangles Left side
      public int Top; // Position if the Rectangles Top side
      public int Right; // Position if the Rectangles Right side
      public int Bottom; // Position if the Rectangles Bottom side
    }

    private struct AppBarData {
      #pragma warning disable S1144, S4487
      public int cbSize;
      public IntPtr hWnd;
      public int uCallbackMessage;
      public int uEdge;
      public AppBarPlacement rc;
      public int lParam;
      #pragma warning restore S1144, S4487
    }

    private struct AppBar {
      public Forms.Form bar;
      public AppBarData data;
    }

    private struct MarginData {
      public int Size;
      public Side Dock;
    }

    public enum Side {
      LEFT = 0x0,
      TOP = 0x1,
      RIGHT = 0x2,
      BOTTOM = 0x3,
      NONE = -1
    }

    private enum ABM_ {
      _NEW = 0x0,
      _REMOVE = 0x1,
      _SETPOS = 0x3,
      _GETTASKBARPOS = 0x5
    }
    #endregion

    #region Privates
    private string currentLayout;
    private readonly List<AppBar> currentAppBars;
    private readonly List<MarginData> _bars;
    private readonly Func<Forms.Form> BarFactory;
    #endregion

    #region Construction
    public ScreenMarginManager(Func<Forms.Form> BarFactory) {
      this._bars = new List<MarginData>();
      this.currentLayout = "<None>";
      this.currentAppBars = new List<AppBar>();
      this.BarFactory = BarFactory;
    }
    #endregion

    #region Actions
    public ScreenMarginManager AddMargin(int size, Side dock) {
      MarginData data = new MarginData() {
        Size = size,
        Dock = dock
      };
      this._bars.Add(data);
      return this;
    }

    public ScreenMarginManager AddTopMargin(int size) {
      return AddMargin(size, Side.TOP);
    }

    public ScreenMarginManager AddBottomMargin(int size) {
      return AddMargin(size, Side.BOTTOM);
    }

    public ScreenMarginManager AddLeftMargin(int size) {
      return AddMargin(size, Side.LEFT);
    }

    public ScreenMarginManager AddRightMargin(int size) {
      return AddMargin(size, Side.RIGHT);
    }

    public void Update(bool force = false) {
      string actualLayout = ScreenLayout();
      if (actualLayout != currentLayout || force) {
        ApplyMargins();
        currentLayout = actualLayout;
      }
    }
    #endregion

    #region Internals
    [DebuggerHidden]
    private static string ScreenLayout() {
      List<string> layout = new List<string>();

      foreach (Forms.Screen screeen in Forms.Screen.AllScreens) {
        layout.Add($"{(screeen.Primary ? "1" : "0")},{screeen.Bounds.X},{screeen.Bounds.Y},{screeen.Bounds.Width},{screeen.Bounds.Height}");
      }

      return string.Join(";", layout);
    }

    [DebuggerHidden]
    private static void ApplyMarginToData(MarginData md, ref AppBarData data, System.Drawing.Rectangle r) {
      data.uEdge = (int)md.Dock;
      if (md.Dock == Side.TOP) {
        data.rc.Top = r.Top;
        data.rc.Bottom = r.Top + md.Size;
      }
      else if (md.Dock == Side.BOTTOM) {
        data.rc.Top = r.Bottom - md.Size;
        data.rc.Bottom = r.Bottom;
      }
      else {
        data.rc.Top = r.Top;
        data.rc.Bottom = r.Bottom;
      }
      if (md.Dock == Side.LEFT) {
        data.rc.Left = r.Left;
        data.rc.Right = r.Left + md.Size;
      }
      else if (md.Dock == Side.RIGHT) {
        data.rc.Left = r.Right - md.Size;
        data.rc.Right = r.Right;
      }
      else {
        data.rc.Left = r.Left;
        data.rc.Right = r.Right;
      }
    }

    public void ApplyMargins() {
      RemoveMargins();
      foreach (System.Drawing.Rectangle r in from screen in Forms.Screen.AllScreens select screen.WorkingArea) {
        foreach (MarginData md in this._bars) {
          AppBarData data = new AppBarData();
          Forms.Form bar = BarFactory();
          data.hWnd = bar.Handle;
          SHAppBarMessage(ABM_._NEW, ref data);
          ApplyMarginToData(md, ref data, r);
          SHAppBarMessage(ABM_._SETPOS, ref data);
          this.currentAppBars.Add(new AppBar { bar = bar, data = data });
        }
      }

      foreach (AppBar ab in this.currentAppBars) {
        ab.bar.Top = ab.data.rc.Top;
        ab.bar.Left = ab.data.rc.Left;
        ab.bar.Width = ab.data.rc.Right - ab.data.rc.Left;
        ab.bar.Height = ab.data.rc.Bottom - ab.data.rc.Top;
        ab.bar.Show();
      }
    }

    public void RemoveMargins() {
      while (this.currentAppBars.Count > 0) {
        int index = this.currentAppBars.Count - 1;
        AppBar ab = this.currentAppBars[index];
        AppBarData data = ab.data;
        SHAppBarMessage(ABM_._REMOVE, ref data);
        this.currentAppBars.RemoveAt(index);
        ab.bar.Close();
      }
    }
    #endregion

    #region IDisposable Support
    private bool disposedValue;

    protected virtual void Dispose(bool disposing) {
      if (!this.disposedValue) {
        if (disposing) {
          RemoveMargins();
        }
        this._bars.Clear();
        this.currentLayout = string.Empty;
      }
      this.disposedValue = true;
    }

    public void Dispose() {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    #endregion
  }
}
