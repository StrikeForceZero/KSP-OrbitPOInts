using System;

namespace UnityEngineMock
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RequireComponent : Attribute
    {
        public Type Type1;
        public Type Type2;
        public Type Type3;

        public RequireComponent(Type requiredComponent) => this.Type1 = requiredComponent;

        public RequireComponent(Type requiredComponent, Type requiredComponent2)
        {
            this.Type1 = requiredComponent;
            this.Type2 = requiredComponent2;
        }

        public RequireComponent(
            Type requiredComponent,
            Type requiredComponent2,
            Type requiredComponent3
        )
        {
            this.Type1 = requiredComponent;
            this.Type2 = requiredComponent2;
            this.Type3 = requiredComponent3;
        }
    }
}
