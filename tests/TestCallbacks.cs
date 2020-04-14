using System;
using NUnit.Framework;

namespace Python.Runtime.Codecs
{
    public class TestCallbacks
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            TestsRuntimeConfig.Ensure();
            PythonEngine.Initialize();
        }

        [OneTimeTearDown]
        public void Dispose()
        {
            PythonEngine.Shutdown();
        }

        private class Callables
        {
            internal object CallFunction0(Func<object> func)
            {
                return func();
            }

            internal object CallFunction1(Func<object[], object> func, object arg)
            {
                return func(new[] { arg });
            }

            internal void CallAction0(Action func)
            {
                func();
            }

            internal void CallAction1(Action<object[]> func, object arg)
            {
                func(new[] { arg });
            }
        }

        [Test]
        public void TestPythonFunctionPassedIntoCLRMethod()
        {
            var locals = new PyDict();
            PythonEngine.Exec(@"
def ret_1():
    return 1
def str_len(a):
    return len(a)
", null, locals.Handle);

            var ret1 = locals.GetItem("ret_1");
            var strLen = locals.GetItem("str_len");

            var callables = new Callables();

            FunctionCodec.Register();

            //ret1.  A function with no arguments that returns an integer
            //it must be convertible to Action or Func<object> and not to Func<object, object>
            {
                Action result1 = null;
                Func<object> result2 = null;
                Assert.DoesNotThrow(() => { result1 = ret1.As<Action>(); });
                Assert.DoesNotThrow(() => { result2 = ret1.As<Func<object>>(); });

                Assert.DoesNotThrow(() => { callables.CallAction0((Action)result1); });
                object ret2 = null;
                Assert.DoesNotThrow(() => { ret2 = callables.CallFunction0((Func<object>)result2); });
                Assert.AreEqual(ret2, 1);
            }

            //strLen.  A function that takes something with a __len__ and returns the result of that function
            //It must be convertible to an Action<object[]> and Func<object[], object>) and not to an  Action or Func<object>
            {
                Action<object[]> result3 = null;
                Func<object[], object> result4 = null;
                Assert.DoesNotThrow(() => { result3 = strLen.As<Action<object[]>>(); });
                Assert.DoesNotThrow(() => { result4 = strLen.As<Func<object[], object>>(); });

                //try using both func and action to show you can get __len__ of a string but not an integer
                Assert.Throws<PythonException>(() => { callables.CallAction1((Action<object[]>)result3, 2); });
                Assert.DoesNotThrow(() => { callables.CallAction1((Action<object[]>)result3, "hello"); });
                Assert.Throws<PythonException>(() => { callables.CallFunction1((Func<object[], object>)result4, 2); });

                object ret2 = null;
                Assert.DoesNotThrow(() => { ret2 = callables.CallFunction1((Func<object[], object>)result4, "hello"); });
                Assert.AreEqual(ret2, 5);
            }

            //TODO - this function is internal inside of PythonNet.  It probably should be public.
            //PyObjectConversions.Reset();
        }
    }
}
