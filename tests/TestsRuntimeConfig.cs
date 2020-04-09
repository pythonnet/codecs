using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Python.Runtime.Codecs
{
    class TestsRuntimeConfig
    {
        public static void Ensure()
        {
            if (Path.IsPathFullyQualified(Runtime.PythonDLL)) return;

            string pyVer = Environment.GetEnvironmentVariable("PYTHON_VERSION");
            if (!string.IsNullOrEmpty(pyVer))
            {
                Runtime.PythonVersion = Version.Parse(pyVer);
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!Runtime.PythonDLL.StartsWith("lib"))
                {
                    string dllExtension = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? ".dylib" : ".so";
                    Runtime.PythonDLL = "lib" + Runtime.PythonDLL + dllExtension;
                }
            }
            else
            {
                Runtime.PythonDLL += ".dll";
            }

            string pyHome = Environment.GetEnvironmentVariable("PYTHON_HOME");
            if (!string.IsNullOrEmpty(pyHome))
            {
                PythonEngine.PythonHome = pyHome;
            }
        }
    }
}
