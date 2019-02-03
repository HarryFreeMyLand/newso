using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Common
{
    public abstract class IniConfig
    {
        private string _activePath;

        public abstract Dictionary<string, string> DefaultValues
        {
            get; set;
        }

        private void SetValue(string key, string value)
        {
            var prop = GetType().GetProperty(key);
            if (prop != null)
            {
                try
                {
                    if (prop.PropertyType != typeof(string))
                        prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType, CultureInfo.InvariantCulture));
                    else
                        prop.SetValue(this, value);
                }
                catch (Exception) { }
            }
        }

        public IniConfig(string path)
        {
            _activePath = path;
            Load();
        }

        public void Load()
        {
            //assume default values for all unset properties
            foreach (var pair in DefaultValues)
            {
                SetValue(pair.Key, pair.Value);
            }

            if (!File.Exists(_activePath))
            {
                Save();
            }
            else
            {
                var lines = File.ReadAllLines(_activePath);
                foreach (var line in lines)
                {
                    var clean = line.Trim();
                    if (clean.Length == 0 || clean[0] == '#' || clean[0] == '[')
                        continue;
                    var split = clean.IndexOf('=');
                    if (split == -1)
                        continue; //?
                    var prop = clean.Substring(0, split).Trim();
                    var value = clean.Substring(split + 1).Trim();

                    SetValue(prop, value);
                }
            }
        }

        public void Save()
        {
            try
            {
                using (var stream = new StreamWriter(File.Open(_activePath, FileMode.Create, FileAccess.Write)))
                {
                    stream.WriteLine($"# {GameConsts.GameName} Settings File. Properties are self explanatory.");
                    var props = GetType().GetProperties();
                    foreach (var prop in props)
                    {
                        if (prop.Name == "Default" || prop.Name == "DefaultValues")
                            continue;
                        stream.WriteLine(prop.Name + "=" + Convert.ToString(prop.GetValue(this), CultureInfo.InvariantCulture));
                    }
                }
            }
            catch (Exception) { }
        }
    }
}
