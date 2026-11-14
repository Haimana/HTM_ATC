using System.Reflection;

namespace HTM_PRO.Utils
{
    /// <summary>
    /// Single source of truth for the HTM_PRO plugin version.
    /// Reads the assembly version generated from HTM_PRO.csproj.
    /// </summary>
    public static class PluginVersion
    {
        public static string Current
        {
            get
            {
                try
                {
                    var v = Assembly.GetExecutingAssembly().GetName().Version;
                    if (v != null)
                    {
                        return $"{v.Major}.{v.Minor}.{v.Build}";
                    }
                }
                catch { }
                return "1.0.0";
            }
        }

        public static string DisplayVersion => "v" + Current;

        public static string FullTitle => "HTM_PRO " + DisplayVersion;
    }
}
