using System;
using System.Collections.Generic;

namespace OrbitPOInts.Data.POI
{
    class PoiCustomEqualityComparer : IEqualityComparer<POI>
    {
        public static readonly PoiCustomEqualityComparer FilterByType = new PoiCustomEqualityComparer(
            (a, b) => a.Type == b.Type,
            poi => poi.Type.GetHashCode()
        );

        private readonly Func<POI, POI, bool> _compareFn;
        private readonly Func<POI, int> _hashCodeFn;
        public PoiCustomEqualityComparer(Func<POI, POI, bool> compareFn, Func<POI, int> hashCodeFn)
        {
            _compareFn = compareFn;
            _hashCodeFn = hashCodeFn;
        }
        public bool Equals(POI x, POI y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null) || ReferenceEquals(y, null)) return false;
            // return x.Type == y.Type;
            return _compareFn.Invoke(x, y);
        }

        public int GetHashCode(POI obj)
        {
            if (ReferenceEquals(obj, null)) return 0;
            // return obj.Type.GetHashCode();
            return _hashCodeFn.Invoke(obj);
        }
    }
}