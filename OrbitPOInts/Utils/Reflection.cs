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

        public static void SetProperty(object obj, string propertyName, object value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var prop = obj.GetType().GetProperty(propertyName);
            if (prop == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type {obj.GetType().FullName}", nameof(propertyName));

            if (!prop.CanWrite)
                throw new ArgumentException($"Property '{propertyName}' on type {obj.GetType().FullName} does not have a set accessor.");

            prop.SetValue(obj, value);
        }
    }
}
