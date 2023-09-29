using System;

namespace UnityEngineMock.JetBrains.Annotations
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Delegate | AttributeTargets.Field, AllowMultiple = false)]
    public class CanBeNullAttribute : Attribute { }
}
