using Dapper;
using System.Collections.Generic;

namespace FSO.Server.Database.DA.Tuning
{
    public class SqlTuning : AbstractSqlDA, ITuning
    {
        public SqlTuning(ISqlContext context) : base(context)
        {
        }

        public IEnumerable<DbTuning> All()
        {
            return Context.Connection.Query<DbTuning>("SELECT * FROM fso_tuning");
        }

        public IEnumerable<DbTuning> AllCategory(string type, int table)
        {
            return Context.Connection.Query<DbTuning>("SELECT * FROM fso_tuning WHERE tuning_type = @type AND tuning_table = @table", new { type = type, table = table });
        }
    }
}
