﻿using FSO.Files.Formats.IFF.Chunks;
using FSO.SimAntics.Model.Platform;
using System.IO;
using System.Linq;

namespace FSO.SimAntics.Model.TSOPlatform
{
    public class VMTSOObjectState : VMTSOEntityState, VMIObjectState
    {
        public uint OwnerID;

        //repair state
        public ushort Wear { get; set; } = 20 * 4; //times 4. increases by 1 per qtr day.
        public byte QtrDaysSinceLastRepair = 0; //when > 7*4, object can break again.
        public bool Broken
        {
            get
            {
                return QtrDaysSinceLastRepair == 255;
            }
        }

        public VMTSOObjectState() { }

        public VMTSOObjectState(int version) : base(version) { }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            OwnerID = reader.ReadUInt32();

            if (Version > 19)
            {
                Wear = reader.ReadUInt16();
                QtrDaysSinceLastRepair = reader.ReadByte();
            }
        }

        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(OwnerID);

            writer.Write(Wear);
            writer.Write(QtrDaysSinceLastRepair);
        }

        public override void Tick(VM vm, object owner)
        {
            base.Tick(vm, owner);
        }

        public void ProcessQTRDay(VM vm, VMEntity owner) {
            if (((VMGameObject)owner).Disabled > 0) return;
            Wear += 1;
            if (Wear > 90 * 4) Wear = 90 * 4;

            if (QtrDaysSinceLastRepair <= 7 * 4)
            {
                QtrDaysSinceLastRepair++;
            }
            
            //can break if the object has a repair interaction.
            if (QtrDaysSinceLastRepair > 7*4 && Wear > 50*4 && owner.TreeTable?.Interactions?.Any(x => (x.Flags & TTABFlags.TSOIsRepair) > 0) == true)
            {
                //object can break. calculate probability
                var rand = (int)vm.Context.NextRandom(10000);
                //lerp
                //1% at 50%, 4% at 90%
                var prob = 100 + ((Wear - (50 * 4)) * 75) / 40;
                if (rand < prob && owner.MultitileGroup.BaseObject == owner)
                {
                    //break the object
                    QtrDaysSinceLastRepair = 255;
                    //apply the broken object particle to all parts
                    foreach (var item in owner.MultitileGroup.Objects)
                    {
                        ((VMGameObject)item).EnableParticle(256);
                    }
                }
            }
        }
    }
}
