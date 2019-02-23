using FSO.Common.Serialization;
using FSO.Common.Serialization.TypeSerializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using FSO.Common.DatabaseService.Model;

namespace FSO.Common.DatabaseService.Framework
{
    public class DatabaseTypeSerializer : cTSOValueDecorated
    {
        protected override void ScanAssembly(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                Attribute[] attributes = Attribute.GetCustomAttributes(type);

                foreach (var attribute in attributes)
                {
                    if (attribute is DatabaseRequest)
                    {
                        var request = (DatabaseRequest)attribute;
                        uint requestId = DBRequestTypeUtils.GetRequestID(request.Type);

                        ClsIdToType.Add(requestId, type);
                        if (!TypeToClsId.ContainsKey(type))
                        {
                            TypeToClsId.Add(type, requestId);
                        }
                    }else if (attribute is DatabaseResponse)
                    {
                        var response = (DatabaseResponse)attribute;
                        uint responseId = DBResponseTypeUtils.GetResponseID(response.Type);

                        ClsIdToType.Add(responseId, type);
                        if (!TypeToClsId.ContainsKey(type)){
                            TypeToClsId.Add(type, responseId);
                        }
                        
                    }
                }
            }
        }
    }
}
