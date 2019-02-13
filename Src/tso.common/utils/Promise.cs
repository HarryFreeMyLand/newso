/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;

namespace FSO.Common.Utils
{
    public class Promise<T>
    {
        Func<object, T> _getter;
        T _value;
        bool _hasRun = false;


        public Promise(Func<object, T> getter)
        {
            _getter = getter;
        }

        public T Values
        {
            set
            {
                _hasRun = true;
                _value = value;
            }
            get
            {
                if (_hasRun == false)
                {
                    _value = _getter(null);
                    _hasRun = true;
                }

                return _value;
            }
        }

        [Obsolete("Use Values property")]
        public void SetValue(T value)
        {
            _hasRun = true;
            _value = value;
        }

        [Obsolete("Use Values property")]
        public T Get()
        {
            if (_hasRun == false)
            {
                _value = _getter(null);
                _hasRun = true;
            }

            return _value;
        }
    }
}
