using System.Collections.Generic;
using System.IO;
using Nett;

namespace FSO.Server.Watchdog
{
    public class Config : IniConfig
    {
        const string INI_FILE = "watchdog.ini";
        const string TOML_FILE = "watchdog.toml";

        static Config _defaultInstance;

        Dictionary<string, string> _defaultValues = new Dictionary<string, string>()
        {
            { "UseTeamCity", "False" },
            { "TeamCityUrl", "http://servo.freeso.org" },
            { "TeamCityProject", "FreeSO_TsoClient" },
            { "Branch", "feature/server-rebuild" },

            { "NormalUpdateUrl", "https://dl.dropboxusercontent.com/u/12239448/FreeSO/devserver.zip" },
        };

        public static Config Default
        {
            get
            {
                try
                {
                    if (File.Exists(TOML_FILE))
                    {
                        if (_defaultInstance == null)
                            _defaultInstance = Toml.ReadFile<Config>(TOML_FILE);
                    }
                    else if (File.Exists(INI_FILE))
                    {
                        if (_defaultInstance == null)
                            _defaultInstance = new Config(INI_FILE);
                    }
                }
                catch
                {
                    throw new FileNotFoundException("Could not find configuration file. Please ensure it is valid and present in the same folder as this executable.");
                }

                return _defaultInstance;
            }
        }

        public Config(string path) : base(path) { }

        public override Dictionary<string, string> DefaultValues
        {
            get { return _defaultValues; }
            set { _defaultValues = value; }
        }

        public bool UseTeamCity { get; set; } = false;
        public string TeamCityUrl { get; set; } = "http://servo.freeso.org";
        public string TeamCityProject { get; set; } = "FreeSO_TsoClient";
        public string Branch { get; set; } = "feature/server-rebuild";

        public string NormalUpdateUrl { get; set; } = "https://dl.dropboxusercontent.com/u/12239448/FreeSO/devserver.zip";
    }
}
