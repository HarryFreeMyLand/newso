using FSO.Server.Framework.Gluon;
using FSO.Server.Protocol.Gluon.Packets;
using Newtonsoft.Json;

namespace FSO.Server.Servers.Tasks.Handlers
{
    public class TaskEngineHandler
    {
        TaskEngine TaskEngine;

        public TaskEngineHandler(TaskEngine engine)
        {
            TaskEngine = engine;
        }

        public void Handle(IGluonSession session, RequestTask task)
        {
            var shardId = new int?();
            if(task.ShardId > 0){
                shardId = task.ShardId;
            }

            var id = TaskEngine.Run(new TaskRunOptions() {
                Task = task.TaskType,
                Shard_Id = shardId,
                Parameter = JsonConvert.DeserializeObject(task.ParameterJson)
            });
            session.Write(new RequestTaskResponse() {
                CallId = task.CallId,
                TaskId = id
            });
        }
    }
}
