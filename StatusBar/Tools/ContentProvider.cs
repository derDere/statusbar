using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StatusBar.Tools {
  public class ContentProvider {

    #region Constants
    private const long MILLISECONDS_PER_DAY = 24 * 60 * 60 * 1000;
    private const int CULTURE_ID = 7;
    private const double SWATCH_TICKS_PER_DAY = 1000D;
    private const int SWATCH_DECIMAL_COUNT = 2;
    private const string SWATCH_FORMAT = "000.00";
    private const string DATE_FORMAT = "yyyy-MM-dd";
    private const string TIME_FORMAT = "HH:mm:ss";
    #endregion

    #region Private Generators
    private static long SecondsSinceMidnight(DateTime t) {
      long e = 0;
      e += t.Hour * 60 * 60;
      e += t.Minute * 60;
      e += t.Second;
      return e;
    }
    private static long MilliSecondsSinceMidnight(DateTime t) {
      long e = SecondsSinceMidnight(t) * 1000;
      e += t.Millisecond;
      return e;
    }

    private static string FindAppPath(string app) {
      List<string> dirs = new List<string>();

      if (Environment.GetEnvironmentVariable("PATH") is string path1) {
        dirs.AddRange(path1.Split(';'));
      }
      if (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process) is string path2) {
        dirs.AddRange(path2.Split(';'));
      }
      if (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) is string path3) {
        dirs.AddRange(path3.Split(';'));
      }
      if (Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) is string path4) {
        dirs.AddRange(path4.Split(';'));
      }

      foreach (string dir in dirs) {
        string fullPath = System.IO.Path.Combine(dir, app);
        if (System.IO.File.Exists(fullPath)) {
          return fullPath;
        }
      }

      return app;
    }
    #endregion

    #region Generators
    public static string Kw(string _) {
      CultureInfo ci = CultureInfo.GetCultureInfo(CULTURE_ID);
      return (ci.Calendar.GetWeekOfYear(DateTime.Now, ci.DateTimeFormat.CalendarWeekRule, ci.DateTimeFormat.FirstDayOfWeek).ToString().PadLeft(2, '0'));
    }

    public static string WeekDay(string _) {
      return DateTime.Now.ToString("dddd", CultureInfo.GetCultureInfo(CULTURE_ID)).Substring(0, 2);
    }

    public static string LocalTime(string _) {
      return DateTime.Now.ToString(TIME_FORMAT);
    }

    public static string UtcTime(string _) {
      return DateTime.UtcNow.ToString(TIME_FORMAT);
    }

    public static string SwatchTime(string _) {
      DateTime currDate = DateTime.Now;

      double p = (0D + MilliSecondsSinceMidnight(currDate.ToUniversalTime().AddHours(1))) / MILLISECONDS_PER_DAY;
      p *= SWATCH_TICKS_PER_DAY;

      double decimal_count = Math.Pow(10D, SWATCH_DECIMAL_COUNT);
      return (Math.Round(p * decimal_count) / decimal_count).ToString(SWATCH_FORMAT).Replace(",", ".");
    }

    public static string Date(string _) {
      return DateTime.Now.ToString(DATE_FORMAT);
    }

    public static string Username(string _) {
      return Environment.UserName;
    }

    public static string MachineName(string _) {
      return Environment.MachineName;
    }

    public static string IpAddress(string _) {
      // Get all local IP addresses and use the one starting with 192
      return string.Join(", ", System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Where(ip => ip.ToString().StartsWith("192")).Select(ip => ip.ToString()));
    }

    public static string Battery(string _) {
      PowerStatus p = SystemInformation.PowerStatus;
      int a = (int)Math.Round(p.BatteryLifePercent * 100D);
      return a.ToString();
    }

    public static string EnvVar(string match) {
      Match m = Regex.Match(match, ENV_PATTERN);
      string envVarName = m.Groups[3].Value;
      if (Environment.GetEnvironmentVariable(envVarName) is string envVarValue1) {
        return envVarValue1;
      }
      else if (Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.Process) is string envVarValue2) {
        return envVarValue2;
      }
      else if (Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User) is string envVarValue3) {
        return envVarValue3;
      }
      else if (Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.Machine) is string envVarValue4) {
        return envVarValue4;
      }
      return "$(" + envVarName + ")";
    }

    public static string RunApp(string match) {
      Match m = Regex.Match(match, RUN_PATTERN);
      string appName = m.Groups[3].Value;
      string[] appNameParts = appName.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
      appName = appNameParts[0];
      appName = FindAppPath(appName);
      string arguments = string.Join(" ", appNameParts.Skip(1));
      string output = string.Empty;
      try {
        System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo(appName) {
          RedirectStandardOutput = true,
          UseShellExecute = false,
          CreateNoWindow = true,
          Arguments = arguments
        };
        System.Diagnostics.Process p = new System.Diagnostics.Process {
          StartInfo = psi
        };
        p.Start();
        output = p.StandardOutput.ReadToEnd();
        p.WaitForExit();
      }
      catch (Exception ex) {
        output = "!!!" + ex.Message + "!!!";
      }
      return output.Replace("\r", "").Replace("\n", "");
    }

    public static string UrlContent(string match) {
      Match m = Regex.Match(match, URL_PATTERN);
      string url = m.Groups[3].Value;
      string content = string.Empty;
      try {
        System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
        req.Method = "GET";
        using (System.Net.HttpWebResponse resp = (System.Net.HttpWebResponse)req.GetResponse()) {
          using (System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream())) {
            content = sr.ReadToEnd();
          }
        }
      }
      catch (Exception ex) {
        content = "!!!" + ex.Message + "!!!";
      }
      return content.Replace("\r", "").Replace("\n", "");
    }

    public static string About(string _) {
      string[] lines = new string[] {
        "About StatusBar",
        "Developed by: github.com/derDere",
        "License: GNU GENERAL PUBLIC LICENSE V3",
        "Version: " + Application.ProductVersion,
        "Language: C#.Net"
      };
      List<string> doubleLines = new List<string>();
      foreach (string line in lines) {
        doubleLines.Add(line);
        doubleLines.Add(line);
      }
      long sec = SecondsSinceMidnight(DateTime.Now);
      int index = ((int)sec % doubleLines.Count);
      return doubleLines[index];
    }

    public static string Rainbow(string _) {
      const int STEP = 255 / 10;
      const int MAX = 255 / STEP;

      const int R2Y = 1 * MAX; // RED to YELLOW 0 - MAX
      const int Y2G = 2 * MAX; // YELLOW to GREEN MAX - 2*MAX
      const int G2C = 3 * MAX; // GREEN to CYAN 2*MAX - 3*MAX
      const int C2B = 4 * MAX; // CYAN to BLUE 3*MAX - 4*MAX
      const int B2M = 5 * MAX; // BLUE to MAGENTA 4*MAX - 5*MAX
      const int M2R = 6 * MAX; // MAGENTA to RED 5*MAX - 6*MAX

      int v = (int)SecondsSinceMidnight(DateTime.Now) % (M2R);
      int r = 0, g = 0, b = 0;

      if (v < R2Y) {
        r = 255;
        g = (v % MAX) * STEP;
      }
      else if (v < Y2G) {
        r = 255 - (v % MAX) * STEP;
        g = 255;
      }
      else if (v < G2C) {
        g = 255;
        b = (v % MAX) * STEP;
      }
      else if (v < C2B) {
        g = 255 - (v % MAX) * STEP;
        b = 255;
      }
      else if (v < B2M) {
        b = 255;
        r = (v % MAX) * STEP;
      }
      else if (v < M2R) {
        b = 255 - (v % MAX) * STEP;
        r = 255;
      }

      return $"#{r:X2}{g:X2}{b:X2}";
    }

    public static string Guid(string _) {
      return System.Guid.NewGuid().ToString();
    }

    public static string Rnd(string _) {
      return RND_GEN.Next().ToString();
    }
    #endregion

    #region Content Keys
    public const string KW = "CW";
    public const string WEEKDAY = "W";
    public const string LOCALTIME = "T";
    public const string UTCTIME = "U";
    public const string SWATCHTIME = "S";
    public const string DATE = "D";
    public const string USERNAME = "User";
    public const string HOST = "Host";
    public const string IP = "IP";
    public const string BATTERY = "Battery";
    public const string ABOUT = "About";
    public const string RAINBOW = "Rainbow";
    public const string GUID = "Guid";
    public const string RND = "Rnd";

    public const string ENV_PATTERN = @"(\$\()(env:)(.*?)(\))";
    public const string RUN_PATTERN = @"(\$\()(run:)(.*?)(\))";
    public const string URL_PATTERN = @"(\$\()(url:)(.*?)(\))";
    #endregion

    #region Internals
    private void AddFixedContent(string key, string description, Func<string, string> generator) {
      string sequence = "{" + key + "}";
      string pattern = Regex.Escape(sequence);
      generators.Add(pattern, generator);
      _infos.Add(sequence, description);
    }
    #endregion

    #region Privates
    private readonly static Random RND_GEN = new Random();
    private readonly Dictionary<string, Func<string, string>> generators;
    private readonly Dictionary<string, string> _infos;
    #endregion

    #region Properties
    public IEnumerable<KeyValuePair<string, string>> Infos {
      get {
        foreach (string key in _infos.Keys) {
          KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(key, _infos[key]);
          yield return kvp;
        }
      }
    }
    #endregion

    #region Constructors
    public ContentProvider() {
      // Construct dictionaries
      generators = new Dictionary<string, Func<string, string>>();
      _infos = new Dictionary<string, string>();

      // Add fixed content
      AddFixedContent(KW, "Calendar week", Kw);
      AddFixedContent(WEEKDAY, "Weekday", WeekDay);
      AddFixedContent(LOCALTIME, "Local time", LocalTime);
      AddFixedContent(UTCTIME, "UTC time", UtcTime);
      AddFixedContent(SWATCHTIME, "Swatch time", SwatchTime);
      AddFixedContent(DATE, "Date", Date);
      AddFixedContent(USERNAME, "Username", Username);
      AddFixedContent(HOST, "Machine name", MachineName);
      AddFixedContent(IP, "IP address", IpAddress);
      AddFixedContent(BATTERY, "Battery level in %", Battery);
      AddFixedContent(ABOUT, "About Infos", About);
      AddFixedContent(RAINBOW, "Rainbow", Rainbow);
      AddFixedContent(GUID, "GUID", Guid);
      AddFixedContent(RND, "Random number", Rnd);

      // Add environment variable pattern
      string env_sequence = "$(env:VAR)";
      generators.Add(ENV_PATTERN, EnvVar);
      _infos.Add(env_sequence, "Environment.VAR content");

      // Add run pattern
      string run_sequence = "$(run:APP)";
      generators.Add(RUN_PATTERN, RunApp);
      _infos.Add(run_sequence, "Run APP and display its output");

      // Add URL pattern
      string url_sequence = "$(url:URL)";
      generators.Add(URL_PATTERN, UrlContent);
      _infos.Add(url_sequence, "Get content from URL");
    }
    #endregion

    #region Actions
    [DebuggerHidden]
    public string ApplyTo(string content) {
      string result = content;
      foreach (string pattern in generators.Keys) {
        MatchEvaluator me = new MatchEvaluator(m => generators[pattern](m.Value));
        result = Regex.Replace(result, pattern, me, RegexOptions.Singleline | RegexOptions.IgnoreCase);
      }
      return result;
    }
    #endregion
  }
}
