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
            string[] paths = Environment.GetEnvironmentVariable("PATH")
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string pathDir in paths)
            {
                string dll = Path.Combine(pathDir, Runtime.PythonDLL);
                if (File.Exists(dll))
                {
                    Runtime.PythonDLL = dll;
                    if (string.IsNullOrEmpty(pyHome))
                    {
                        pyHome = Path.GetDirectoryName(dll);
                    }
                    break;
                }
            }


            if (!string.IsNullOrEmpty(pyHome))
            {
                PythonEngine.PythonHome = pyHome;
            }
        }
    }
}
