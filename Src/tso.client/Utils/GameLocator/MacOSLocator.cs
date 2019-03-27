using System;

namespace FSO.Client.Utils.GameLocator
{
    public class UnixLocator : ILocator
    {
        public string FindTheSimsOnline => string.Format("{0}/Documents/The Sims Online/TSOClient/", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
    }
}
