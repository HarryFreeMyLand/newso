/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */


namespace FSO.SimAntics.Engine
{
    public abstract class VMPrimitiveHandler
    {
        protected void Trace(string message){
            System.Diagnostics.Debug.WriteLine(message);
        }

        public abstract VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand operand);
    }
}
