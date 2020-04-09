using System.IO;
using System.Runtime.InteropServices;

namespace Python.Runtime.Codecs
{
    class TestsRuntimeConfig
    {
        public static void Ensure()
        {
            if (PythonEngine.IsInitialized)
                return;

            if (Path.IsPathFullyQualified(Runtime.PythonDLL)) return;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !Runtime.PythonDLL.StartsWith("lib"))
            {
                string dllExtension = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";
                Runtime.PythonDLL = "lib" + Runtime.PythonDLL + dllExtension;
            }
        }
    }
}
