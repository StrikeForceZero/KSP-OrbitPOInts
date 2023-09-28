using System;
using System.Linq.Expressions;
using System.Reflection;

#if TEST
using UnityEngineMock;
using KSP_ConfigNode = KSPMock.ConfigNode;
using System.Linq;
#else
using UniLinq;
using UnityEngine;
using KSP_ConfigNode = ConfigNode;
#endif

namespace OrbitPOInts.Data
{
    using ConfigNode = KSP_ConfigNode;

    [AttributeUsage(AttributeTargets.Property)]
    public class DeserializationMethodAttribute : Attribute
    {
        public string MethodName { get; }

        public DeserializationMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }

    public abstract class ConfigNodeDto<T> where T : ConfigNodeDto<T>, new()
    {
        protected static T Empty => new();

        protected ConfigNodeDto() { }

        protected ConfigNodeDto(ConfigNode node)
        {
            // (T)Activator.CreateInstance(typeof(T), node); should prevent odd behavior
            // ReSharper disable once VirtualMemberCallInConstructor
            Hydrate(node);
        }
        public abstract ConfigNode Save();

        // since we want load to be static we need to make the implementable member with the loading logic protected
        protected abstract void Hydrate(ConfigNode node);

        public static T Load(ConfigNode node)
        {
            // var obj = Empty;
            // obj.Hydrate(node);
            // return obj;
            return (T)Activator.CreateInstance(typeof(T), node);
        }

        // this can be expensive but its convenient, avoids reflection, and less prone to copy paste errors
        protected void AddValue<TValue>(ConfigNode node, Expression<Func<TValue>> propertyExpression, Func<TValue, string> serializer = null)
        {
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException("The provided expression did not point to a valid property.", nameof(propertyExpression));
            }
            var propertyName = member.Member.Name;
            var value = propertyExpression.Compile().Invoke();

            // Use the provided serializer if available, otherwise, use ToString().
            serializer ??= v => v.ToString();
            var serializedValue = serializer(value);
            node.AddValue(propertyName, serializedValue);
        }

        // TODO: awkward that we spent time making this but just end up using ConfigNodeValueExtractor.LoadNamedValueFromNode instead
        protected void LoadValue<TValue>(ConfigNode node, Expression<Func<TValue>> propertyExpression, Func<string, TValue> deserializer)
        {
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException("The provided expression did not point to a valid property.", nameof(propertyExpression));
            }

            var declaringType = member.Member.DeclaringType;
            var propertyName = member.Member.Name;

            var stringValue = "";
            if (!node.TryGetValue(propertyName, ref stringValue)) return;
            var property = member.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException($"The member {member.Member.Name} in the provided expression is not a property.", nameof(propertyExpression));
            }

            var value = deserializer(stringValue);
            property.SetValue(this, value);
        }

    }
}
