using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq.Expressions;

namespace OrbitPOInts.Utils
{
    public class PropChangeEventArgs<TSource>
    {
        public TSource Source { get; set; }
        public PropertyChangedEventArgs PropertyArgs { get; set; }
        public object Value { get; set; }
    }

    public class PropChangeActionMapper<TSource>
    {
        public delegate void OnPropChangeAction(PropChangeEventArgs<TSource> args);
        private IReadOnlyDictionary<string, OnPropChangeAction> PropChangeActionMap { get; }

        public PropChangeActionMapper(params PropChangeActionMapping<TSource>[] mappings)
        {
            PropChangeActionMap = new ReadOnlyDictionary<string, OnPropChangeAction>(CreateDictionary(mappings));
        }

        public bool Process(TSource sourceInstance, PropertyChangedEventArgs eventArgs)
        {
            if (!PropChangeActionMap.TryGetValue(eventArgs.PropertyName, out var action)) return false;
            var value = Reflection.GetMemberValue(sourceInstance, eventArgs.PropertyName);
            action.Invoke(new PropChangeEventArgs<TSource>() { Source = sourceInstance, PropertyArgs = eventArgs, Value = value });
            return true;
        }

        public static IDictionary<string, OnPropChangeAction> CreateDictionary(
            params PropChangeActionMapping<TSource>[] mappings
        )
        {
            var dictionary = new Dictionary<string, OnPropChangeAction>();

            foreach (var mapping in mappings)
            {
                var sourcePropName = GetPropertyName(mapping.SourceProperty);

                dictionary[sourcePropName] = mapping.Action;
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

    public class PropChangeActionMapping<TSource>
    {
        public Expression<Func<TSource, object>> SourceProperty { get; private set; }
        public PropChangeActionMapper<TSource>.OnPropChangeAction Action { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public static PropChangeActionMapping<TSource> From(
            Expression<Func<TSource, object>> sourceProperty,
            PropChangeActionMapper<TSource>.OnPropChangeAction action
        )
        {
            return new PropChangeActionMapping<TSource>()
            {
                SourceProperty = sourceProperty,
                Action = action,
            };
        }
    }
}
