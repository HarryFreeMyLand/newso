using FSO.Common.Serialization;
using FSO.Content;
using Ninject;

namespace FSO.Common.DataService
{
    public class NullDataService : DataService
    {
        public NullDataService(IModelSerializer serializer,
                                GameContent content,
                                IKernel kernel) : base(serializer, content)
        {
        }
    }
}
