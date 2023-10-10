using System.Collections.Generic;

#if TEST
using System.Linq;
using UnityEngineMock;
using CanBeNullAttribute = UnityEngineMock.JetBrains.Annotations.CanBeNullAttribute;
using NotNullAttribute = UnityEngineMock.JetBrains.Annotations.NotNullAttribute;
#else
using UniLinq;
using UnityEngine;
using CanBeNullAttribute = JetBrains.Annotations.CanBeNullAttribute;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;
#endif

namespace OrbitPOInts.Data.POI
{
    public class PoiIdBodyComparer : IEqualityComparer<POI>
    {
        public bool Equals(POI x, POI y)
        {
            return StaticEquals(x, y);
        }

        public int GetHashCode(POI obj)
        {
            return StaticGetHashCode(obj);
        }

        public static bool StaticEquals([CanBeNull] POI x, [CanBeNull] POI y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            // we probably dont want to worry about derived vs base class comparisons
            // if (x.GetType() != y.GetType()) return false;
            return x.Id.Equals(y.Id) && ReferenceEquals(x.Body, y.Body);
        }

        public static int StaticGetHashCode([NotNull] POI obj)
        {
            unchecked
            {
                // ReSharper disable once Unity.NoNullPropagation
                var bodyHashCode = obj.Body?.GetHashCode() ?? 0;
                return (obj.Id.GetHashCode() * 397) ^ bodyHashCode;
            }
        }
    }
}
