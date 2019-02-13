using FSO.Common.DatabaseService.Framework;
using FSO.Common.DataService.Framework;
using FSO.Common.Serialization;
using FSO.Server.Protocol.Voltron.DataService;
using Ninject.Activation;
using Ninject.Modules;
using System;

namespace FSO.Server.DataService
{
    /// <summary>
    /// Data service classes that can be shared between multiple shards when multi-tenanting
    /// </summary>
    public class GlobalDataServiceModule : NinjectModule
    {
        public override void Load()
        {
            Bind<cTSOSerializer>().ToProvider<cTSOSerializerProvider>().InSingletonScope();
            Bind<IModelSerializer>().ToProvider<ModelSerializerProvider>().InSingletonScope();
            Bind<ISerializationContext>().To<SerializationContext>();
        }
    }

    class ModelSerializerProvider : IProvider<IModelSerializer>
    {
        private Content.GameContent Content;

        public ModelSerializerProvider(Content.GameContent content)
        {
            Content = content;
        }

        public Type Type
        {
            get
            {
                return typeof(IModelSerializer);
            }
        }

        public object Create(IContext context)
        {
            var serializer = new ModelSerializer();
            serializer.AddTypeSerializer(new DatabaseTypeSerializer());
            serializer.AddTypeSerializer(new DataServiceModelTypeSerializer(Content.DataDefinition));
            serializer.AddTypeSerializer(new DataServiceModelVectorTypeSerializer(Content.DataDefinition));
            return serializer;
        }
    }

    class cTSOSerializerProvider : IProvider<cTSOSerializer>
    {
        private Content.GameContent Content;

        public cTSOSerializerProvider(Content.GameContent content)
        {
            Content = content;
        }

        public Type Type
        {
            get
            {
                return typeof(cTSOSerializer);
            }
        }

        public object Create(IContext context){
            return new cTSOSerializer(Content.DataDefinition);
        }
    }
}
