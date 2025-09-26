using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using IO = System.IO;

namespace StatusBar {
  public class Config {

    #region Construction

    private const string FOLDER_NAME = "StatusBar";
    private const string FILE_NAME = "config.txt";

    internal const string COMMENT = "//";

    private const string DEFAULT_POSITION = "bottom";
    internal const string DEFAULT_FONT = "Consolas";
    internal const string DEFAULT_COLOR = "Silver";
    internal const string DEFAULT_BACKGROUND = "#000000";
    private const string DEFAULT_CONTENT_LEFT = " {User} @ {Host} -> {IP}";
    private const string DEFAULT_CONTENT_CENTER = "t-{T}   u-{U}   @{S}";
    private const string DEFAULT_CONTENT_RIGHT = "{W}   KW-{CW}   {D}   ⚡ {Battery}% ";

    private const int LINES_INDEX_POSITION = 0;
    private const int LINES_INDEX_FONT = 1;
    private const int LINES_INDEX_COLOR = 2;
    private const int LINES_INDEX_BACKGROUND = 3;
    private const int LINES_INDEX_CONTENT_START = 4;

    internal static string FolderName() {
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (!IO.Directory.Exists(path))
        IO.Directory.CreateDirectory(path);

      path = IO.Path.Combine(path, FOLDER_NAME);
      if (!IO.Directory.Exists(path))
        IO.Directory.CreateDirectory(path);

      return path;
    }

    internal static string FileName() {
      string path = FolderName();

      path = IO.Path.Combine(path, FILE_NAME);

      return path;
    }

    private static Config myself;

    static Config() {
      Reload();
    }

    public static void Reload() {
      string filename = FileName();
      if (!IO.File.Exists(filename)) {
        Save();
      } else {
        string[] lines = IO.File.ReadAllLines(filename, Encoding.UTF8);
        myself = FromLines(lines);
      }
    }

    public static void Save() {
      if (myself == null) {
        myself = new Config();
      }
      string filename = FileName();
      string[] lines = myself.ToLines();
      IO.File.WriteAllLines(filename, lines, Encoding.UTF8);
    }

    private string[] ToLines() {
      return lines;
    }

    private static Config FromLines(string[] lines) {
      Config newConf = new Config();
      newConf.lines = lines;
      return newConf;
    }
    #endregion

    #region Properties
    private string[] lines = NewLines();

    public static bool IsTopPosition {
      get {
        return (myself.CleanLines?.Skip(LINES_INDEX_POSITION)?.First() ?? DEFAULT_POSITION).ToLower() == "top";
      }
    }

    public static string Font {
      get {
        return myself.CleanLines?.Skip(LINES_INDEX_FONT)?.First() ?? DEFAULT_FONT;
      }
    }

    public static string TextColor {
      get {
        return myself.CleanLines?.Skip(LINES_INDEX_COLOR)?.First() ?? DEFAULT_COLOR;
      }
    }

    public static string BackgroundColor {
      get {
        return myself.CleanLines?.Skip(LINES_INDEX_BACKGROUND)?.First() ?? DEFAULT_BACKGROUND;
      }
    }

    public static IEnumerable<string> Lines {
      get {
        return from line in myself.CleanLines.Skip(LINES_INDEX_CONTENT_START) select line;
      }
    }
    #endregion

    #region Internals
    private IEnumerable<string> CleanLines {
      get {
        return from line in lines where (!line.Trim().StartsWith(COMMENT)) && line.Trim().Length > 0 select line;
      }
    }

    private static string[] NewLines() {
      Tools.ContentProvider contentProvider = new Tools.ContentProvider();
      
      int max_key_len = 0;
      foreach(int len in contentProvider.Infos.Select(kvp => kvp.Key.Length)) {
        if (len > max_key_len) {
          max_key_len = len;
        }
      }

      List<string> lines = new List<string>() {
        COMMENT + " The Config is structured in lines.",
        COMMENT + " The first line defines the position of the bar. (top or bottom)",
        COMMENT + " The second line defines the font.",
        COMMENT + " The third line defines the text color.",
        COMMENT + " The fourth line defines the background color.",
        COMMENT + " The following lines define the content.",
        COMMENT + " ",
        COMMENT + " A change of the Position requires a restart of the statusbar.",
        COMMENT + " ",
        COMMENT + " Use " + COMMENT + " to comment out a line.",
        COMMENT + " Each content line becomes a column starting left to right.",
        COMMENT + " Comments (" + COMMENT + " Line) or empty lines are ignored.",
        COMMENT + " ",
        COMMENT + " All columns are centered except the first and last one.",
        COMMENT + " The first column is left aligned.",
        COMMENT + " The last column is right aligned.",
        COMMENT + " ",
        COMMENT + " Add more lines for more columns.",
        COMMENT + " ",
        COMMENT + " Dynamic content can also be used for the font, text color and background.",
        COMMENT + " You can use the following keys for dynamic content:"
      };

      foreach (KeyValuePair<string,string> kvp in contentProvider.Infos) {
        string line = COMMENT + " - " + kvp.Key.PadRight(max_key_len, ' ');
        line += " -> ";
        line += kvp.Value;
        lines.Add(line);
      }

      lines.Add("");
      lines.Add(COMMENT + " POSITION:");
      lines.Add(DEFAULT_POSITION);

      lines.Add("");
      lines.Add(COMMENT + " FONT:");
      lines.Add(DEFAULT_FONT);

      lines.Add("");
      lines.Add(COMMENT + " COLOR:");
      lines.Add(DEFAULT_COLOR);

      lines.Add("");
      lines.Add(COMMENT + " BACKGROUND:");
      lines.Add(DEFAULT_BACKGROUND);

      lines.Add("");
      lines.Add(COMMENT + " CONTENT_LEFT:");
      lines.Add(DEFAULT_CONTENT_LEFT);

      lines.Add("");
      lines.Add(COMMENT + " CONTENT_CENTER:");
      lines.Add(DEFAULT_CONTENT_CENTER);

      lines.Add("");
      lines.Add(COMMENT + " CONTENT_RIGHT:");
      lines.Add(DEFAULT_CONTENT_RIGHT);

      lines.Add("");
      lines.Add(COMMENT + " EXAMPLES:");
      lines.Add(COMMENT + " $(env:OS)");
      lines.Add(COMMENT + " $(url:https://raw.githubusercontent.com/derDere/ExampleData/refs/heads/main/short_text.txt)");
      lines.Add(COMMENT + " $(run:python -V)");
      lines.Add(COMMENT + " {Rainbow}");
      lines.Add(COMMENT + " {Rnd}");
      lines.Add(COMMENT + " {Guid}");
      lines.Add(COMMENT + " {Credits}");

      return lines.ToArray();
    }
    #endregion
  }
}
