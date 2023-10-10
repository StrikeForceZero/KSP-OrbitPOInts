using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using OrbitPOInts.Utils;

namespace OrbitPOInts.Tests.Utils
{
#if TEST
    class TestSource : INotifyPropertyChanged
    {
        private string _foo;
        private string _fizz;

        public string Foo
        {
            get => _foo;
            set => SetField(ref _foo, value);
        }

        public string Fizz
        {
            get => _fizz;
            set => SetField(ref _fizz, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public void ClearPropertyChanged()
        {
            PropertyChanged = null;
        }
    }

    class TestTarget
    {
        public string Foo { get; set; }
        public string Buzz { get; set; }
    }

    [TestFixture]
    public class PropChangeMapperTests
    {
        private TestSource _testSource;
        private TestTarget _testTarget;
        private PropChangeMapper<TestSource, TestTarget> _propChangeMapper;

        private const string InitialValue = "initial";

        [SetUp]
        public void Setup()
        {
            _testSource = new TestSource { Foo = InitialValue, Fizz = InitialValue };
            _testTarget = new TestTarget { Foo = InitialValue, Buzz = InitialValue };
        }

        private void UpdateAndProcess<TValue>(Expression<Func<TValue>> propertyExpression, TValue value, bool assertThrows = false)
        {
            if (propertyExpression.Body is not MemberExpression member)
            {
                throw new ArgumentException("The provided expression did not point to a valid property.", nameof(propertyExpression));
            }
            if (member.Member is not PropertyInfo property)
            {
                throw new ArgumentException($"The member {member.Member.Name} in the provided expression is not a property.", nameof(propertyExpression));
            }

            _testSource.PropertyChanged += (sender, args) =>
            {
                TestDelegate process = () => _propChangeMapper.Process(_testSource, _testTarget, args);
                if (assertThrows)
                {
                    Assert.Throws<ArgumentException>(process);
                    return;
                }
                process.Invoke();
            };
            property.SetValue(_testSource, value);
            _testSource.ClearPropertyChanged();
        }

        private static string RandomString => RandomStringGenerator.Generate(10);

        [Test]
        public void PropChangeMapper_ManualMappedWithAction()
        {
            var fooActionCalled = false;
            var fizzActionCalled = false;
            _propChangeMapper = new PropChangeMapper<TestSource, TestTarget>(
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Foo, t => t.Foo, () => fooActionCalled = true),
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Fizz, t => t.Buzz, () => fizzActionCalled = true)
            );

            var randomStringFoo = RandomString;
            UpdateAndProcess(() => _testSource.Foo, randomStringFoo);
            Assert.IsTrue(fooActionCalled);
            Assert.IsFalse(fizzActionCalled);
            Assert.AreEqual(randomStringFoo, _testTarget.Foo);

            var randomStringFizz = RandomString;
            UpdateAndProcess(() => _testSource.Fizz, randomStringFizz);
            Assert.IsTrue(fizzActionCalled);
            Assert.AreEqual(randomStringFizz, _testTarget.Buzz);
        }

        [Test]
        public void PropChangeMapper_AutoMappedTrueWithAction()
        {
            var fooActionCalled = false;
            var fizzActionCalled = false;
            _propChangeMapper = new PropChangeMapper<TestSource, TestTarget>(
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Foo, () => fooActionCalled = true, true),
                // should throw when prop changes
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Fizz, () => fizzActionCalled = true, true)
            );

            var randomStringFoo = RandomString;
            UpdateAndProcess(() => _testSource.Foo, randomStringFoo);
            Assert.IsTrue(fooActionCalled);
            Assert.IsFalse(fizzActionCalled);
            Assert.AreEqual(randomStringFoo, _testTarget.Foo);

            var randomStringFizz = RandomString;
            UpdateAndProcess(() => _testSource.Fizz, randomStringFizz, true);
            Assert.IsFalse(fizzActionCalled);
            Assert.AreEqual(InitialValue, _testTarget.Buzz);
        }

        [Test]
        public void PropChangeMapper_AutoMappedFalseWithAction()
        {
            var fooActionCalled = false;
            var fizzActionCalled = false;
            _propChangeMapper = new PropChangeMapper<TestSource, TestTarget>(
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Foo, () => fooActionCalled = true, false),
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Fizz, () => fizzActionCalled = true, false)
            );

            var randomStringFoo = RandomString;
            UpdateAndProcess(() => _testSource.Foo, randomStringFoo);
            Assert.IsTrue(fooActionCalled);
            Assert.IsFalse(fizzActionCalled);
            Assert.AreEqual(InitialValue, _testTarget.Foo);

            var randomStringFizz = RandomString;
            UpdateAndProcess(() => _testSource.Fizz, randomStringFizz);
            Assert.IsTrue(fizzActionCalled);
            Assert.AreEqual(InitialValue, _testTarget.Buzz);
        }

        [Test]
        public void PropChangeMapper_AutoMappedTrue()
        {
            _propChangeMapper = new PropChangeMapper<TestSource, TestTarget>(
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Foo, true),
                // should throw when prop changes
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Fizz, true)
            );

            var randomStringFoo = RandomString;
            UpdateAndProcess(() => _testSource.Foo, randomStringFoo);
            Assert.AreEqual(randomStringFoo, _testTarget.Foo);

            var randomStringFizz = RandomString;
            UpdateAndProcess(() => _testSource.Fizz, randomStringFizz, true);
            Assert.AreEqual(InitialValue, _testTarget.Buzz);
        }

        [Test]
        public void PropChangeMapper_AutoMappedFalse()
        {
            _propChangeMapper = new PropChangeMapper<TestSource, TestTarget>(
                // no op
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Foo, false),
                // no op
                PropChangeMapping<TestSource, TestTarget>.From(s => s.Fizz, false)
            );

            UpdateAndProcess(() => _testSource.Foo, RandomString);
            Assert.AreEqual(_testTarget.Foo, InitialValue);

            UpdateAndProcess(() => _testSource.Fizz, RandomString);
            Assert.AreEqual(_testTarget.Buzz, InitialValue);
        }
    }
#endif
}
