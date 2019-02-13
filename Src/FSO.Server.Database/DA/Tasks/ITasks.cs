using FSO.Server.Database.DA.Utils;

namespace FSO.Server.Database.DA.Tasks
{
    public interface ITasks
    {
        int Create(DbTask task);
        void CompleteTask(int task_id, DbTaskStatus status);
        void SetStatus(int task_id, DbTaskStatus status);
        PagedList<DbTask> All(int offset = 1, int limit = 20);
    }
}
