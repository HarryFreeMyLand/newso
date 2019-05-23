using System.IO;
using Nett;
using Newtonsoft.Json;

namespace FSOFacadeWorker
{

    enum ConfigType
    {
        Json,
        Toml
    }

    class FacadeConfig
    {
        const string JSON_FILE = "facadeconfig.json";
        const string TOML_FILE = "facadeconfig.toml";

        static FacadeConfig _defaultInstance;

        [TomlMember(Key = "ApiUrl")]
        public string Api_Url { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Limit { get; set; } = 2000;

        [TomlMember(Key = "SleepTime")]
        public int Sleep_Time { get; set; } = 30000;


        public static FacadeConfig Default
        {
            get
            {
                try
                {
                    if (File.Exists(TOML_FILE) && !File.Exists(JSON_FILE))
                    {
                        if (_defaultInstance == null)
                            _defaultInstance = Toml.ReadFile<FacadeConfig>(TOML_FILE);
                    }
                    else if (File.Exists(JSON_FILE) && !File.Exists(TOML_FILE))
                    {
                        if (_defaultInstance == null)
                        {
                            var configString = File.ReadAllText(JSON_FILE);
                            _defaultInstance = JsonConvert.DeserializeObject<FacadeConfig>(configString);
                        }
                    }
                }
                catch
                {
                    throw new FileNotFoundException("Could not find configuration file. Please ensure it is valid and present in the same folder as this executable.");
                }

                return _defaultInstance;
            }
        }
    }
}
