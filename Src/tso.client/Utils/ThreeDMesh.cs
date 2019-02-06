/*
This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
If a copy of the MPL was not distributed with this file, You can obtain one at
http://mozilla.org/MPL/2.0/.
*/

using System.Collections.Generic;

namespace FSO.Client.Utils
{
    public class ThreeDMesh<T>
    {
        List<T> Vertexes = new List<T>();
        List<int> Indexes = new List<int>();
        int IndexOffset = 0;
        int _PrimitiveCount = 0;

        public void AddQuad(T tl, T tr, T br, T bl)
        {
            Vertexes.Add(tl);
            Vertexes.Add(tr);
            Vertexes.Add(br);
            Vertexes.Add(bl);

            Indexes.Add(IndexOffset);
            Indexes.Add(IndexOffset + 1);
            Indexes.Add(IndexOffset + 2);
            Indexes.Add(IndexOffset + 2);
            Indexes.Add(IndexOffset + 3);
            Indexes.Add(IndexOffset);

            IndexOffset += 4;
            _PrimitiveCount += 2;
        }

        public T[] GetVertexes()
        {
            return Vertexes.ToArray();
        }

        public int[] GetIndexes()
        {
            return Indexes.ToArray();
        }

        public int PrimitiveCount
        {
            get
            {
                return _PrimitiveCount;
            }
        }
    }
}
