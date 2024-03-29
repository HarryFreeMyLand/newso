﻿using System.Linq;
using FSO.Server.Framework.Aries;
using FSO.Common.DataService;
using FSO.Files.Formats.tsodata;
using System.Reflection;
using FSO.Common.DataService.Framework.Attributes;
using FSO.Server.Protocol.Voltron.Packets;

namespace FSO.Server.DataService
{
    public class DataServiceSyncFactory : IDataServiceSyncFactory
    {
        IDataService DataService;

        public DataServiceSyncFactory(IDataService ds)
        {
            DataService = ds;
        }

        public IDataServiceSync<T> Get<T>(params string[] fields)
        {
            return new DataServiceSync<T>(DataService, fields);
        }
    }

    public class DataServiceSync<T> : IDataServiceSync<T>
    {
        IDataService DataService;
        StructField[] Fields;
        PropertyInfo KeyField;

        public DataServiceSync(IDataService ds, string[] fields)
        {
            DataService = ds;
            Fields = ds.GetFieldsByName(typeof(T), fields);
            KeyField = typeof(T).GetProperties().First(x => x.GetCustomAttribute<Key>() != null);
        }

        public void Sync(IAriesSession target, T item)
        {
            var asObject = (object)item;
            var updates = DataService.SerializeUpdate(Fields, asObject, (uint)KeyField.GetValue(asObject));

            if (updates.Count == 0) { return; }
            var packets = new DataServiceWrapperPDU[updates.Count];

            for(int i=0; i < updates.Count; i++)
            {
                var update = updates[i];
                packets[i] = new DataServiceWrapperPDU() {
                    Body = update,
                    RequestTypeID = 0,
                    SendingAvatarID = 0
                };
            }

            target.Write(packets);
        }
    }
}
