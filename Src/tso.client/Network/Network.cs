using FSO.Client.Model;
using FSO.Client.Regulators;
using FSO.Common.Domain.Shards;
using FSO.Server.Clients;
using FSO.Server.Protocol.CitySelector;
using System.Linq;

namespace FSO.Client.Network
{
    public class Network
    {
        CityConnectionRegulator CityRegulator;
        LotConnectionRegulator LotRegulator;
        LoginRegulator LoginRegulator;
        IShardsDomain Shards;

        public Network(LoginRegulator loginReg, CityConnectionRegulator cityReg, LotConnectionRegulator lotReg, IShardsDomain shards)
        {
            Shards = shards;
            CityRegulator = cityReg;
            LoginRegulator = loginReg;
            LotRegulator = lotReg;
        }

        public AriesClient CityClient
        {
            get
            {
                return CityRegulator.Client;
            }
        }

        public AriesClient LotClient
        {
            get
            {
                return LotRegulator.Client;   
            }
        }

        public UserReference MyCharacterRef
        {
            get
            {
                return UserReference.Of(Common.Enum.UserReferenceType.AVATAR, MyCharacter);
            }
        }

        public uint MyCharacter
        {
            get
            {
                return uint.Parse(CityRegulator.CurrentShard.AvatarID);
            }
        }

        public ShardStatusItem MyShard
        {
            get
            {
                return Shards.All.First(x => x.Name == CityRegulator.CurrentShard.ShardName);
            }
        }
    }
}
