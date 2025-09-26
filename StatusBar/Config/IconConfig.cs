using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Linq;
using IO = System.IO;

namespace StatusBar {
  public class IconConfig {


    #region Construction
    private const string FOLDER_NAME = "Apps";
    private const string FILE_NAME_PATTERN = "*_ICON.txt";

    internal const string RANDOM_COLOR = "random";

    private const int LINES_INDEX_DISPLAY_NAME = 0;
    private const int LINES_INDEX_IMAGE_PATH = 1;
    private const int LINES_INDEX_EMOJI_STR = 2;
    private const int LINES_INDEX_EXE_PATH = 3;
    private const int LINES_INDEX_WORK_DIR = 4;
    private const int LINES_INDEX_IS_WIDGET = 5;
    private const int LINES_INDEX_POSITION = 6;
    private const int LINES_INDEX_FOREGROUND = 7;
    private const int LINES_INDEX_BACKGROUND = 8;
    private const int LINES_INDEX_IS_CATEGORY = 9;

    private static string FolderName() {
      string path = Config.FolderName();
      path = IO.Path.Combine(path, FOLDER_NAME);

      if (!IO.Directory.Exists(path)) {
        IO.Directory.CreateDirectory(path);
        IO.File.WriteAllLines(IO.Path.Combine(path, FILE_NAME_PATTERN.Replace("*", "Demo")), NewLines());
        IO.File.WriteAllLines(IO.Path.Combine(path, FILE_NAME_PATTERN.Replace("*", "DemoCategory")), NewLinesCategory());
      }

      return path;
    }

    public static IEnumerable<IconConfig> Load() {
      string path = FolderName();

      string[] files = IO.Directory.GetFiles(path, FILE_NAME_PATTERN, IO.SearchOption.TopDirectoryOnly);

      foreach (string file in files) {
        foreach (IconConfig iconConfig in FromFile(file)) {
          yield return iconConfig;
        }
      }
    }

    private static bool IsHl(string s) {
      if (s.Trim().Length <= 0)
        return false;
      s = s.Trim().Replace("-","");
      return s.Length <= 0;
    }

    private static IEnumerable<IconConfig> FromFile(string path) {
      string[] lines = IO.File.ReadAllLines(path);
      List<string> lineBlock = new List<string>();
      foreach (string line in lines) {
        if (IsHl(line)) {
          IconConfig newConf = new IconConfig(path, lineBlock.ToArray());
          lineBlock.Clear();
          yield return newConf;
        } else {
          lineBlock.Add(line);
        }
      }
      if (lineBlock.Count > 0) {
        IconConfig newConf = new IconConfig(path, lineBlock.ToArray());
        lineBlock.Clear();
        yield return newConf;
      }
    }

    private IconConfig(string file, string[] lines) {
      this.file = file;
      this.lines = lines;
    }
    #endregion

    #region Internals
    private static string[] NewLines() {
      return new string[] {
        Config.COMMENT + " The Config is structured in lines.",
        Config.COMMENT + " The first line defines the displayname of the icon",
        Config.COMMENT + " The second line defines the icons image file",
        Config.COMMENT + " The third line defines the icon emoji if no images is given (Text also works)",
        Config.COMMENT + " The fourth line defines the programm executable path",
        Config.COMMENT + "     (You can seperate the cli args from the exe with a | )",
        Config.COMMENT + " The fifts line defines the working direktory (If none is foven the executables directory will be used.)",
        Config.COMMENT + " The sixts line defines if the icon is a widget (0|1)",
        Config.COMMENT + "     (Widgets are icons that display the output stream of the given executable.)",
        Config.COMMENT + " The sevents line defines the position <column,row,colspan,rowspan>",
        Config.COMMENT + $" The eigths line defines the icons foreground color <#AARRGGBB|{RANDOM_COLOR}>",
        Config.COMMENT + $" The nines line defines the icons background/border color <#AARRGGBB|{RANDOM_COLOR}>",
        Config.COMMENT + " The tenth line defines if the icon is a category (0|1)",
        Config.COMMENT + "     (A category shows just the title at the Top and displays a border.)",
        Config.COMMENT + " ",
        Config.COMMENT + " Use // to comment out a line",
        Config.COMMENT + " Comments (// Line) or empty lines are ignores",
        Config.COMMENT + " If you want to set a value to empty or default use a ? instead",
        "",
        Config.COMMENT + " Display Name",
        "Demo Icon (Calc)",
        "",
        Config.COMMENT + " Image File",
        "?",
        "",
        Config.COMMENT + " Emoji as Icon",
        "🖩",
        "",
        Config.COMMENT + " Executable",
        "C:\\WINDOWS\\system32\\calc.exe",
        "",
        Config.COMMENT + " Working Directory",
        "?",
        "",
        Config.COMMENT + " Is Widget",
        "0",
        "",
        Config.COMMENT + " Icon Position",
        "0,1,2,2",
        "",
        Config.COMMENT + " Foreground Color",
        "#FFFFFFFF",
        "",
        Config.COMMENT + " Background Color",
        RANDOM_COLOR,
        "",
        Config.COMMENT + " Is a category",
        "0",
        "",
        "",
        "----------------------------------------------------------",
        Config.COMMENT + " Using horizontal lines just made of - and spaces you can place multiple tiles in one file",
        "",
        Config.COMMENT + " Display Name",
        "Demo Widget",
        Config.COMMENT + " Image File",
        "?",
        Config.COMMENT + " Emoji as Icon",
        "?",
        Config.COMMENT + " Executable",
        "C:\\WINDOWS\\System32\\WindowsPowerShell\\v1.0\\powershell.exe | Get-Date -Format \"HH:mm\"",
        Config.COMMENT + " Working Directory",
        "?",
        Config.COMMENT + " Is Widget",
        "1",
        Config.COMMENT + " Icon Position",
        "3,3,2,2",
        Config.COMMENT + " Foreground Color",
        "#FFFFFFFF",
        Config.COMMENT + " Background Color",
        RANDOM_COLOR,
        Config.COMMENT + " Is a category",
        "0",
      };
    }

    private static string[] NewLinesCategory() {
      return new string[] {
        Config.COMMENT + " Read the Demo_ICON.txt for more Informations",
        "",
        Config.COMMENT + " Display Name",
        "Mathematics",
        "",
        Config.COMMENT + " Image File",
        "?",
        "",
        Config.COMMENT + " Emoji as Icon",
        "?",
        "",
        Config.COMMENT + " Executable",
        "?",
        "",
        Config.COMMENT + " Working Directory",
        "?",
        "",
        Config.COMMENT + " Is Widget",
        "0",
        "",
        Config.COMMENT + " Icon Position",
        "0,0,2,3",
        "",
        Config.COMMENT + " Foreground Color",
        "#FF007FFF",
        "",
        Config.COMMENT + " Background Color",
        "#FF007FFF",
        "",
        Config.COMMENT + " Is a category",
        "1"
      };
    }

    private IEnumerable<string> CleanLines {
      get {
        return from line in lines where (!line.Trim().StartsWith(Config.COMMENT)) && line.Trim().Length > 0 select line == "?" ? "" : line;
      }
    }
    #endregion

    #region Properties
    private string file;
    private string[] lines;

    public string DisplayName {
      get {
        return CleanLines?.Skip(LINES_INDEX_DISPLAY_NAME)?.First() ?? "";
      }
    }

    public string ImagePath {
      get {
        return CleanLines?.Skip(LINES_INDEX_IMAGE_PATH)?.First() ?? "";
      }
    }

    public string Emoji {
      get {
        return CleanLines?.Skip(LINES_INDEX_EMOJI_STR)?.First() ?? "";
      }
    }

    public string Exe {
      get {
        return CleanLines?.Skip(LINES_INDEX_EXE_PATH)?.First() ?? "";
      }
    }

    public string WorkingDirectory {
      get {
        return CleanLines?.Skip(LINES_INDEX_WORK_DIR)?.First() ?? "";
      }
    }

    public bool IsWidget {
      get {
        return (CleanLines?.Skip(LINES_INDEX_IS_WIDGET)?.First() ?? "0").ToLower() == "1";
      }
    }

    public string Position {
      get {
        return CleanLines?.Skip(LINES_INDEX_POSITION)?.First() ?? "0,0,1,1";
      }
    }

    public string Foreground {
      get {
        return CleanLines?.Skip(LINES_INDEX_FOREGROUND)?.First() ?? "#FFFFFFFF";
      }
    }

    public string Background {
      get {
        return CleanLines?.Skip(LINES_INDEX_BACKGROUND)?.First() ?? RANDOM_COLOR;
      }
    }

    public bool IsCategory {
      get {
        return (CleanLines?.Skip(LINES_INDEX_IS_CATEGORY)?.First() ?? "0").ToLower() == "1";
      }
    }
    #endregion
  }
}
