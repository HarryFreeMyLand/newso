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
        List<T> _vertexes = new List<T>();
        List<int> _indexes = new List<int>();
        int _indexOffset = 0;

        public void AddQuad(T tl, T tr, T br, T bl)
        {
            _vertexes.Add(tl);
            _vertexes.Add(tr);
            _vertexes.Add(br);
            _vertexes.Add(bl);

            _indexes.Add(_indexOffset);
            _indexes.Add(_indexOffset + 1);
            _indexes.Add(_indexOffset + 2);
            _indexes.Add(_indexOffset + 2);
            _indexes.Add(_indexOffset + 3);
            _indexes.Add(_indexOffset);

            _indexOffset += 4;
            PrimitiveCount += 2;
        }

        public T[] GetVertexes()
        {
            return _vertexes.ToArray();
        }

        public int[] GetIndexes()
        {
            return _indexes.ToArray();
        }

        public int PrimitiveCount { get; private set; } = 0;
    }
}
