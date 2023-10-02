using System;

namespace OrbitPOInts.Utils
{
    public static class Reflection
    {
        public static object AccessProp<T>(T instance, string propertyName)
        {
            return AccessProp<object, T>(instance, propertyName);
        }

        public static V AccessProp<V, T>(T instance, string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if(propertyInfo == null)
            {
                throw new NullReferenceException($"{propertyName} is not a property of {typeof(T)}");
            }
            return (V)propertyInfo.GetValue(instance);
        }
    }
}
