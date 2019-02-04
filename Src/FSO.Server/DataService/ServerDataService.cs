﻿
using FSO.Common.Serialization;
using FSO.Server.DataService.Providers;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSO.Server.DataService
{
    public class ServerDataService : FSO.Common.DataService.DataService
    {
        public ServerDataService(IModelSerializer serializer, 
                                FSO.Content.GameContent content,
                                IKernel kernel) : base(serializer, content)
        {
            AddProvider(kernel.Get<ServerAvatarProvider>());
            var lots = kernel.Get<ServerLotProvider>();
            AddProvider(lots);
            var city = kernel.Get<ServerCityProvider>();
            AddProvider(city);
            city.BindLots(lots);
        }
    }
}
