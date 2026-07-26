// Copyright 2025 Axon Genesis. All rights reserved.
// AxonGenesis.com
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.

using System.Collections.Generic;
using System.Linq;

namespace AxonGenesis
{
    /// <summary>
    /// Global utility methods for working with strings and parsing values
    /// </summary>
    public static class CollectionsUtil
    {
        public static IEnumerable<T> RemoveNulls<T>(this IEnumerable<T> enumerable) where T : class
        {
            //If the generic type inherits from UnityEngine.Object, this needs to be performed to handle the == overload.
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) {
                enumerable = enumerable.Where(e => (e as UnityEngine.Object) != null);
            }
            else {
                enumerable = enumerable.Where(e => e != null);
            }

            return enumerable;
        }
    }

}//AxonGenesis