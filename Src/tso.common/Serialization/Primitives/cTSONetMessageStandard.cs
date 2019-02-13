using System;
using Mina.Core.Buffer;
using System.ComponentModel;
using FSO.Common.Serialization.TypeSerializers;

namespace FSO.Common.Serialization.Primitives
{
    [cTSOValue(0x125194E5)]
    public class cTSONetMessageStandard : IoBufferSerializable, IoBufferDeserializable
    {
        public uint Unknown_1 { get; set; }
        public uint SendingAvatarID { get; set; }
        public cTSOParameterizedEntityFlags Flags { get; set; }
        public uint MessageID { get; set; }

        public uint? DatabaseType { get; set; }
        public uint? DataServiceType { get; set; }

        public uint? Parameter { get; set; }
        public uint RequestResponseID { get; set; }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public object ComplexParameter { get; set; }

        public uint Unknown_2 { get; set; }

        public cTSONetMessageStandard(){
        }

        public void Deserialize(IoBuffer input, ISerializationContext context)
        {
            Unknown_1 = input.GetUInt32();
            SendingAvatarID = input.GetUInt32();
            var flagsByte = input.Get();
            Flags = (cTSOParameterizedEntityFlags)flagsByte;
            MessageID = input.GetUInt32();

            if ((Flags & cTSOParameterizedEntityFlags.HAS_DS_TYPE) == cTSOParameterizedEntityFlags.HAS_DS_TYPE)
            {
                DataServiceType = input.GetUInt32();
            }else if ((Flags & cTSOParameterizedEntityFlags.HAS_DB_TYPE) == cTSOParameterizedEntityFlags.HAS_DB_TYPE){
                DatabaseType = input.GetUInt32();
            }

            if ((Flags & cTSOParameterizedEntityFlags.HAS_BASIC_PARAMETER) == cTSOParameterizedEntityFlags.HAS_BASIC_PARAMETER)
            {
                Parameter = input.GetUInt32();
            }

            if ((Flags & cTSOParameterizedEntityFlags.UNKNOWN) == cTSOParameterizedEntityFlags.UNKNOWN)
            {
                Unknown_2 = input.GetUInt32();
            }

            if ((Flags & cTSOParameterizedEntityFlags.HAS_COMPLEX_PARAMETER) == cTSOParameterizedEntityFlags.HAS_COMPLEX_PARAMETER)
            {
                uint typeId = DatabaseType.HasValue ? DatabaseType.Value : DataServiceType.Value;
                ComplexParameter = context.ModelSerializer.Deserialize(typeId, input, context);
            }
        }

        public void Serialize(IoBuffer output, ISerializationContext context)
        {
            output.PutUInt32(Unknown_1);
            output.PutUInt32(SendingAvatarID);

            byte flags = 0;
            if (DatabaseType.HasValue){
                flags |= (byte)cTSOParameterizedEntityFlags.HAS_DB_TYPE;
            }

            if (DataServiceType.HasValue){
                flags |= (byte)cTSOParameterizedEntityFlags.HAS_DB_TYPE;
                flags |= (byte)cTSOParameterizedEntityFlags.HAS_DS_TYPE;
            }

            if (Parameter != null){
                flags |= (byte)cTSOParameterizedEntityFlags.HAS_BASIC_PARAMETER;
            }

            if(ComplexParameter != null){
                flags |= (byte)cTSOParameterizedEntityFlags.HAS_COMPLEX_PARAMETER;
            }

            output.Put(flags);
            output.PutUInt32(MessageID);

            if (DataServiceType.HasValue)
            {
                output.PutUInt32(DataServiceType.Value);
            }else if (DatabaseType.HasValue){
                output.PutUInt32(DatabaseType.Value);
            }

            if (Parameter.HasValue){
                output.PutUInt32(Parameter.Value);
            }

            if (ComplexParameter != null){
                context.ModelSerializer.Serialize(output, ComplexParameter, context, false);
            }
        }
    }

    [Flags]
    public enum cTSOParameterizedEntityFlags
    {
        HAS_DB_TYPE = 1,
        HAS_DS_TYPE = 2,
        HAS_BASIC_PARAMETER = 4,
        UNKNOWN = 8,
        HAS_COMPLEX_PARAMETER = 32
    }
}
