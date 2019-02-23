/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.IO;

namespace FSO.SimAntics.NetPlay.Model.Commands
{
    public class VMNetInteractionCancelCmd : VMNetCommandBodyAbstract
    {
        public ushort ActionUID;
        public override bool Execute(VM vm, VMAvatar caller)
        {
            if (caller == null) return false;

            caller.Thread.CancelAction(ActionUID);

            return true;
        }

        #region VMSerializable Members

        public override void SerializeInto(BinaryWriter writer)
        {
            base.SerializeInto(writer);
            writer.Write(ActionUID);
        }

        public override void Deserialize(BinaryReader reader)
        {
            base.Deserialize(reader);
            ActionUID = reader.ReadUInt16();
        }

        #endregion
    }
}
