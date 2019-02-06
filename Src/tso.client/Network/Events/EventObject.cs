﻿/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System;
using System.Collections.Generic;
using System.Text;

namespace FSO.Client.Network.Events
{
    /// <summary>
    /// Codes for various events that can occur.
    /// </summary>
    public enum EventCodes
    {
        BAD_USERNAME = 0x00,
        BAD_PASSWORD = 0x01,

        LOGIN_RESULT = 0x02,
        PROGRESS_UPDATE = 0x03,
        TRANSITION_RESULT = 0x04,

        PACKET_PROCESSING_ERROR = 0x05, //Received a faulty packet that couldn't be processed.
        AUTHENTICATION_FAILURE = 0x06,

        TRANSACTION_RESULT = 0x07,     //Result of a transaction between player(s) and/or server.
        TRANSACTION_PLAYER_OUT_OF_MONEY = 0x08
    }

    /// <summary>
    /// Base class for all events.
    /// </summary>
    public class EventObject
    {
        public EventCodes ECode;

        public EventObject(EventCodes Code)
        {
            ECode = Code;
        }
    }
}
