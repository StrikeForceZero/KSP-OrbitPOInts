using System;

namespace OrbitPOInts.Utils
{
    public static class Reflection
    {
        [Obsolete]
        public static object AccessProp<T>(T instance, string propertyName)
        {
            return AccessProp<object, T>(instance, propertyName);
        }

        [Obsolete]
        public static V AccessProp<V, T>(T instance, string propertyName)
        {
            var propertyInfo = typeof(T).GetProperty(propertyName);
            if(propertyInfo == null)
            {
                throw new NullReferenceException($"{propertyName} is not a property of {typeof(T)}");
            }
            return (V)propertyInfo.GetValue(instance);
        }

        public static object GetMemberValue<T>(T instance, string propertyName)
        {
            return GetMemberValue<object, T>(instance, propertyName);
        }

        public static V GetMemberValue<V, T>(T instance, string memberName)
        {
            var type = typeof(T);

            // Try to get as property
            var propertyInfo = type.GetProperty(memberName);
            if (propertyInfo != null)
            {
                return (V)propertyInfo.GetValue(instance);
            }

            // Try to get as field
            var fieldInfo = type.GetField(memberName);
            if (fieldInfo != null)
            {
                return (V)fieldInfo.GetValue(instance);
            }

            throw new ArgumentException($"'{memberName}' is neither a property nor a field of {type.FullName}");
        }

        public static void SetMemberValue(object obj, string memberName, object value)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var type = obj.GetType();

            // Try to set as property
            var prop = type.GetProperty(memberName);
            if (prop != null)
            {
                if (!prop.CanWrite)
                    throw new ArgumentException($"Property '{memberName}' on type {type.FullName} does not have a set accessor.");

                prop.SetValue(obj, value);
                return;
            }

            // Try to set as field
            var field = type.GetField(memberName);
            if (field != null)
            {
                field.SetValue(obj, value);
                return;
            }

            throw new ArgumentException($"Member '{memberName}' not found on type {type.FullName}", nameof(memberName));
        }
    }
}
