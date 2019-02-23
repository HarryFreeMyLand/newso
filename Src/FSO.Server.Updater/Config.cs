using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nett;

namespace FSO.Server.Watchdog
{
    public class Config : IniConfig
    {
        private static Config _defaultInstance;

        public static Config Default
        {
            get
            {
                if (_defaultInstance == null)
                    _defaultInstance = new Config("watchdog.ini");
                return _defaultInstance;
            }
        }

        public Config(string path) : base(path) { }

        private Dictionary<string, string> _defaultValues = new Dictionary<string, string>()
        {
            { "UseTeamCity", "False" },
            { "TeamCityUrl", "http://servo.freeso.org" },
            { "TeamCityProject", "FreeSO_TsoClient" },
            { "Branch", "feature/server-rebuild" },

            { "NormalUpdateUrl", "https://dl.dropboxusercontent.com/u/12239448/FreeSO/devserver.zip" },
        };
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
