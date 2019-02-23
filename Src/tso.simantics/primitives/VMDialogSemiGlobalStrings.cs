/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using FSO.SimAntics.Engine;
using FSO.Files.Formats.IFF.Chunks;

namespace FSO.SimAntics.Primitives
{
    public class VMDialogSemiGlobalStrings : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand args)
        {
            return VMDialogPrivateStrings.ExecuteGeneric(context, args, context.ScopeResource.SemiGlobal.Get<STR>(301));
        }
    }
}
