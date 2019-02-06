﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.SimAntics.Engine;
using FSO.Files.Utils;
using FSO.SimAntics.Engine.Utils;
using FSO.SimAntics.Engine.Scopes;
using FSO.SimAntics.Model;
using System.IO;

namespace FSO.SimAntics.Primitives
{
    public class VMIdleForInput : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand args) //TODO: Behaviour for being notified out of idle and interaction canceling
        {
            var operand = (VMIdleForInputOperand)args;

            //if we're main, attempt to run a queued interaction. We just idle if this fails.
            if (operand.AllowPush == 1 && !context.ActionTree && context.Thread.AttemptPush())
            {
                return VMPrimitiveExitCode.CONTINUE; //control handover
                //TODO: does this forcefully end the rest of the idle? (force a true return, must loop back to run again)
            }

            if (context.ActionTree && context.Thread.Queue[0].Cancelled)
            {
                context.Caller.SetFlag(VMEntityFlags.NotifiedByIdleForInput, true);
                return VMPrimitiveExitCode.GOTO_TRUE;
            }

            if (context.Thread.Interrupt)
            {
                context.Thread.Interrupt = false;
                return VMPrimitiveExitCode.GOTO_TRUE;
            }

            var ticks = VMMemory.GetVariable(context, FSO.SimAntics.Engine.Scopes.VMVariableScope.Parameters, operand.StackVarToDec);
            ticks--;

            if (ticks < 0)
            {
                return VMPrimitiveExitCode.GOTO_TRUE;
            }
            else
            {
                VMMemory.SetVariable(context, FSO.SimAntics.Engine.Scopes.VMVariableScope.Parameters, operand.StackVarToDec, ticks);
                return VMPrimitiveExitCode.CONTINUE_NEXT_TICK;
            }
        }
    }

    public class VMIdleForInputOperand : VMPrimitiveOperand
    {
        public short StackVarToDec { get; set; }
        public ushort AllowPush { get; set; }

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
                StackVarToDec = io.ReadInt16();
                AllowPush = io.ReadUInt16();
            }
        }

        public void Write(byte[] bytes)
        {
            using (var io = new BinaryWriter(new MemoryStream(bytes)))
            {
                io.Write(StackVarToDec);
                io.Write(AllowPush);
            }
        }
        #endregion
    }
}
