using System;
using System.Reflection;

namespace Python.Runtime.Codecs
{
    //converts python functions to C# actions
    public class FunctionCodec : IPyObjectDecoder
    {
        private static int GetNumArgs(PyObject pyCallable)
        {
            var locals = new PyDict();
            locals.SetItem("f", pyCallable);
            using (Py.GIL())
                PythonEngine.Exec(@"
from inspect import signature
try:
    x = len(signature(f).parameters)
except:
    x = 0
", null, locals.Handle);

            var x = locals.GetItem("x");
            return new PyInt(x).ToInt32();
        }

        private static int GetNumArgs(Type targetType)
        {
            MethodInfo invokeMethod = targetType.GetMethod("Invoke");
            return invokeMethod.GetParameters().Length;
        }

        private static bool IsUnaryAction(Type targetType)
        {
            return targetType == typeof(Action);
        }

        private static bool IsVariadicObjectAction(Type targetType)
        {
            return targetType == typeof(Action<object[]>);
        }

        private static bool IsUnaryFunc(Type targetType)
        {
            return targetType == typeof(Func<object>);
        }

        private static bool IsVariadicObjectFunc(Type targetType)
        {
            return targetType == typeof(Func<object[], object>);
        }

        private static bool IsAction(Type targetType)
        {
            return IsUnaryAction(targetType) || IsVariadicObjectAction(targetType);
        }

        private static bool IsFunc(Type targetType)
        {
            return IsUnaryFunc(targetType) || IsVariadicObjectFunc(targetType);
        }

        private static bool IsCallable(Type targetType)
        {
            //Python.Runtime.ClassManager dtype
            return targetType.IsSubclassOf(typeof(MulticastDelegate));
        }

        public static FunctionCodec Instance { get; } = new FunctionCodec();
        public bool CanDecode(PyObject objectType, Type targetType)
        {
            //python object must be callable
            if (!objectType.IsCallable()) return false;

            //C# object must be callable
            if (!IsCallable(targetType))
                return false;

            return true;
        }

        private static object ConvertUnaryAction(PyObject pyObj)
        {
            Func<object> func = (Func<object>)ConvertUnaryFunc(pyObj);
            Action action = () => { func(); };
            return (object)action;
        }

        private static object ConvertVariadicObjectAction(PyObject pyObj, int numArgs)
        {
            Func<object[], object> func = (Func<object[], object>)ConvertVariadicObjectFunc(pyObj, numArgs);
            Action<object[]> action = (object[] args) => { func(args); };
            return (object)action;
        }

        //TODO share code between ConvertUnaryFunc and ConvertVariadicObjectFunc
        private static object ConvertUnaryFunc(PyObject pyObj)
        {
            var pyAction = new PyObject(pyObj.Handle);
            Func<object> func = () =>
            {
                var pyArgs = new PyObject[0];
                using (Py.GIL())
                {
                    var pyResult = pyAction.Invoke(pyArgs);
                    return pyResult.As<object>();
                }
            };
            return (object)func;
        }

        private static object ConvertVariadicObjectFunc(PyObject pyObj, int numArgs)
        {
            var pyAction = new PyObject(pyObj.Handle);
            Func<object[], object> func = (object[] o) =>
            {
                var pyArgs = new PyObject[numArgs];
                int i = 0;
                foreach (object obj in o)
                {
                    pyArgs[i++] = obj.ToPython();
                }

                using (Py.GIL())
                {
                    var pyResult = pyAction.Invoke(pyArgs);
                    return pyResult.As<object>();
                }
            };
            return (object)func;
        }

        public bool TryDecode<T>(PyObject pyObj, out T value)
        {
            value = default(T);
            var tT = typeof(T);
            if (!IsCallable(tT))
                return false;

            var numArgs = GetNumArgs(pyObj);
            if (numArgs != GetNumArgs(tT))
                return false;

            if (IsAction(tT))
            {
                object actionObj = null;
                if (numArgs == 0)
                {
                    actionObj = ConvertUnaryAction(pyObj);
                }
                else
                {
                    actionObj = ConvertVariadicObjectAction(pyObj, numArgs);
                }

                value = (T)actionObj;
                return true;
            }
            else if (IsFunc(tT))
            {

                object funcObj = null;
                if (numArgs == 0)
                {
                    funcObj = ConvertUnaryFunc(pyObj);
                }
                else
                {
                    funcObj = ConvertVariadicObjectFunc(pyObj, numArgs);
                }

                value = (T)funcObj;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void Register()
        {
            PyObjectConversions.RegisterDecoder(Instance);
        }
    }
}
