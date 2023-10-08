using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OrbitPOInts.Utils
{
    public class PropChangeMapper<TSource, TTarget>
    {
        private IReadOnlyDictionary<string, (string, Action)> PropChangeMap { get; }

        public PropChangeMapper(params PropChangeMapping<TSource, TTarget>[] mappings)
        {
            PropChangeMap = new ReadOnlyDictionary<string, (string, Action)>(CreateDictionary(mappings));
        }

        public bool Process(TSource sourceInstance, TTarget targetInstance, PropertyChangedEventArgs eventArgs)
        {
            if (!PropChangeMap.TryGetValue(eventArgs.PropertyName, out var targetActionTuple)) return false;
            if (targetActionTuple.Item1 != null)
            {
                var value = Reflection.AccessProp(sourceInstance, eventArgs.PropertyName);
                Reflection.SetMemberValue(targetInstance, targetActionTuple.PropertyName, value);
            }
            targetActionTuple.Item2?.Invoke();
            return true;
        }

        public static IDictionary<string, (string, Action)> CreateDictionary(
            params PropChangeMapping<TSource, TTarget>[] mappings
        )
        {
            var dictionary = new Dictionary<string, (string, Action)>();

            foreach (var mapping in mappings)
            {
                var sourcePropName = GetPropertyName(mapping.SourceProperty);
                var targetPropertySpecified = mapping.TargetProperty != null;
                var visualizerPropName = targetPropertySpecified ? GetPropertyName(mapping.TargetProperty) : null;
                if (!targetPropertySpecified && mapping.TargetHasSameProperty) visualizerPropName = sourcePropName;

                dictionary[sourcePropName] = (visualizerPropName, mapping.Action);
            }

            return dictionary;
        }

        private static string GetPropertyName<T, TProp>(Expression<Func<T, TProp>> propertyExpr)
        {
            return propertyExpr.Body switch
            {
                MemberExpression member => member.Member.Name,
                UnaryExpression { Operand: MemberExpression unaryMember } => unaryMember.Member.Name,
                _ => throw new ArgumentException("Expression is not a property.")
            };
        }
    }

    public class PropChangeMapping<TSource, TTarget>
    {
        public Expression<Func<TSource, object>> SourceProperty { get; private set; }
        public Expression<Func<TTarget, object>> TargetProperty { get; private set; }
        public Action Action { get; private set; }
        public bool TargetHasSameProperty { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public static PropChangeMapping<TSource, TTarget> From(
            Expression<Func<TSource, object>> sourceProperty,
            Expression<Func<TTarget, object>> targetProperty,
            Action action,
            bool targetHasSameProperty = false
        )
        {
            return new PropChangeMapping<TSource, TTarget>()
            {
                SourceProperty = sourceProperty,
                TargetProperty = targetProperty,
                Action = action,
                TargetHasSameProperty = targetHasSameProperty,
            };
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static PropChangeMapping<TSource, TTarget> From(
            Expression<Func<TSource, object>> sourceProperty,
            Action action,
            bool targetHasSameProperty = false
        )
        {
            return From(sourceProperty, null, action, targetHasSameProperty);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static PropChangeMapping<TSource, TTarget> From(
            Expression<Func<TSource, object>> sourceProperty,
            Expression<Func<TTarget, object>> targetProperty,
            bool targetHasSameProperty = false
        )
        {
            return From(sourceProperty, targetProperty, null, targetHasSameProperty);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static PropChangeMapping<TSource, TTarget> From(
            Expression<Func<TSource, object>> sourceProperty,
            bool targetHasSameProperty = true
        )
        {
            return From(sourceProperty, null, targetHasSameProperty);
        }
    }
}
