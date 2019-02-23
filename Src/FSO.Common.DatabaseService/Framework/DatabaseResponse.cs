using FSO.Common.DatabaseService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Common.DatabaseService.Framework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class DatabaseResponse : Attribute
    {
        public DBResponseType Type;

        public DatabaseResponse(DBResponseType type)
        {
            Type = type;
        }
    }
}
