﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System.Collections.Generic;

namespace FSO.Common.Content
{
    public interface IContentProvider <T>
    {
        T Get(ulong id);
        T Get(string name);
        T Get(uint type, uint fileID);
        T Get(ContentID id);
        List<IContentReference<T>> List();
    }
}
