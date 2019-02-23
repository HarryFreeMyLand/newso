using System;

namespace FSO.Common.DataService.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = true)]
    public class Demand : Attribute
    {
        string Pattern;

        public Demand(string pattern)
        {
            this.Pattern = pattern;
        }
    }
}
