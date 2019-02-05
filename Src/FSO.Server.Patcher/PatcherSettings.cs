using System;
using System.Collections.Generic;
using System.Text;
using Nett;

namespace FSO.Server.Patcher
{
    class PatcherSettings
    {
        public bool GlacierMode { get; set; } = false;
        public string GlacierId { get; set; }
        public string Branch { get; set; } = "master";
        public string ServerUrl { get; set; } = "http://localhost";
    }
}
