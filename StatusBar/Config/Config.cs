using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Shapes;
using IO = System.IO;

namespace StatusBar {
  public class Config {

    #region Construction

    private const string FOLDER_NAME = "StatusBar";
    private const string FILE_NAME = "config.txt";

    private const string CONTENT_LEFT = " {User} @ {Host} -> {IP}";
    private const string CONTENT_CENTER = "t-{T}   u-{U}   @{S}";
    private const string CONTENT_RIGHT = "{W}   KW-{CW}   {D}   ⚡ {Battery}% ";

    private static string FileName() {
      string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
      if (!IO.Directory.Exists(path))
        IO.Directory.CreateDirectory(path);

      path = IO.Path.Combine(path, FOLDER_NAME);
      if (!IO.Directory.Exists(path))
        IO.Directory.CreateDirectory(path);

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
    public static IEnumerable<string> Lines {
      get {
        return from line in myself.lines where !line.Trim().StartsWith("#") && line.Trim().Length > 0 select line;
      }
    }
    #endregion

    #region Internals
    private static string[] NewLines() {
      Tools.ContentProvider contentProvider = new Tools.ContentProvider();
      
      int max_key_len = 0;
      foreach(int len in contentProvider.Infos.Select(kvp => kvp.Key.Length)) {
        if (len > max_key_len) {
          max_key_len = len;
        }
      }

      List<string> lines = new List<string>() {
        "# A sharp comments out a line",
        "# Each line becomes a column starting left to right",
        "# ",
        "# All columns are centered except the first and last one",
        "# The first column is left aligned",
        "# The last column is right aligned",
        "# ",
        "# Add more lines for more columns",
        "# Empty lines are ignored",
        "#",
        "# You can use the following keys for dynamic content:"
      };

      foreach (KeyValuePair<string,string> kvp in contentProvider.Infos) {
        string line = "# - " + kvp.Key.PadRight(max_key_len, ' ');
        line += " -> ";
        line += kvp.Value;
        lines.Add(line);
      }

      lines.Add("");
      lines.Add("# CONTENT_LEFT");
      lines.Add(CONTENT_LEFT);

      lines.Add("");
      lines.Add("# CONTENT_CENTER");
      lines.Add(CONTENT_CENTER);

      lines.Add("");
      lines.Add("# CONTENT_RIGHT");
      lines.Add(CONTENT_RIGHT);

      lines.Add("");
      lines.Add("# EXAMPLES:");
      lines.Add("# $(env:OS)");
      lines.Add("# $(url:https://raw.githubusercontent.com/derDere/ExampleData/refs/heads/main/short_text.txt)");
      lines.Add("# $(run:python -V)");

      return lines.ToArray();
    }
    #endregion
  }
}
