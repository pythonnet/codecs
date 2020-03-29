using System;
using NUnit.Framework;

namespace Python.Runtime.Codecs
{
    class FunctionCodecTests
    {
        [SetUp]
        public void SetUp()
        {
            PythonEngine.Initialize();
        }


        [TearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        [Test]
        public void FunctionAction()
        {
            var codec = FunctionCodec.Instance;

            PyInt x = new PyInt(1);
            PyDict y = new PyDict();
            //non-callables can't be decoded into Action
            Assert.IsFalse(codec.CanDecode(x, typeof(Action)));
            Assert.IsFalse(codec.CanDecode(y, typeof(Action)));

            var locals = new PyDict();
            PythonEngine.Exec(@"
def foo():
    return 1
def bar(a):
    return 2
", null, locals.Handle);

            //foo, the function with no arguments
            var fooFunc = locals.GetItem("foo");
            Assert.IsFalse(codec.CanDecode(fooFunc, typeof(bool)));

            //CanDecode does not work for variadic actions
            //Assert.IsFalse(codec.CanDecode(fooFunc, typeof(Action<object[]>)));
            Assert.IsTrue(codec.CanDecode(fooFunc, typeof(Action)));

            Action fooAction;
            Assert.IsTrue(codec.TryDecode(fooFunc, out fooAction));
            Assert.DoesNotThrow(() => fooAction());

            //bar, the function with an argument
            var barFunc = locals.GetItem("bar");
            Assert.IsFalse(codec.CanDecode(barFunc, typeof(bool)));
            //Assert.IsFalse(codec.CanDecode(barFunc, typeof(Action)));
            Assert.IsTrue(codec.CanDecode(barFunc, typeof(Action<object[]>)));

            Action<object[]> barAction;
            Assert.IsTrue(codec.TryDecode(barFunc, out barAction));
            Assert.DoesNotThrow(() => barAction(new[] { (object)true }));
        }

        [Test]
        public void FunctionFunc()
        {
            var codec = FunctionCodec.Instance;

            PyInt x = new PyInt(1);
            PyDict y = new PyDict();
            //non-callables can't be decoded into Func
            Assert.IsFalse(codec.CanDecode(x, typeof(Func<object>)));
            Assert.IsFalse(codec.CanDecode(y, typeof(Func<object>)));

            var locals = new PyDict();
            PythonEngine.Exec(@"
def foo():
    return 1
def bar(a):
    return 2
", null, locals.Handle);

            //foo, the function with no arguments
            var fooFunc = locals.GetItem("foo");
            Assert.IsFalse(codec.CanDecode(fooFunc, typeof(bool)));

            //CanDecode does not work for variadic actions
            //Assert.IsFalse(codec.CanDecode(fooFunc, typeof(Func<object[], object>)));
            Assert.IsTrue(codec.CanDecode(fooFunc, typeof(Func<object>)));

            Func<object> foo;
            Assert.IsTrue(codec.TryDecode(fooFunc, out foo));
            object res1 = null;
            Assert.DoesNotThrow(() => res1 = foo());
            Assert.AreEqual(res1, 1);

            //bar, the function with an argument
            var barFunc = locals.GetItem("bar");
            Assert.IsFalse(codec.CanDecode(barFunc, typeof(bool)));
            //Assert.IsFalse(codec.CanDecode(barFunc, typeof(Func<object>)));
            Assert.IsTrue(codec.CanDecode(barFunc, typeof(Func<object[], object>)));

            Func<object[], object> bar;
            Assert.IsTrue(codec.TryDecode(barFunc, out bar));
            object res2 = null;
            Assert.DoesNotThrow(() => res2 = bar(new[] { (object)true }));
            Assert.AreEqual(res2, 2);
        }

    }
}
