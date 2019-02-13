using System.Collections.Generic;

namespace FSO.Server.Database.DA.Tuning
{
    public interface ITuning
    {
        IEnumerable<DbTuning> All();
        IEnumerable<DbTuning> AllCategory(string type, int table);
    }
}
