using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Python.Runtime.Codecs
{
    partial class TestsRuntimeConfig
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
                // work around Python.NET unstable issue with dll suffixes on *nix
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

            string pyHome = Environment.GetEnvironmentVariable("PYTHON_HOME")
                // defined in GitHub action setup-python
                ?? Environment.GetEnvironmentVariable("pythonLocation");
            if (!string.IsNullOrEmpty(pyHome))
            {
                string dll = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? Path.Combine(pyHome, Runtime.PythonDLL)
                    : Path.Combine(pyHome, "lib", Runtime.PythonDLL);
                if (File.Exists(dll))
                {
                    Runtime.PythonDLL = dll;
                }
            }

            if (!Path.IsPathFullyQualified(Runtime.PythonDLL))
            {
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
                            pyHome = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                                // on Windows, paths is PYTHON_HOME/dll
                                ? Path.GetDirectoryName(dll)
                                // on *nix the path is HOME/lib/dll
                                : Path.GetDirectoryName(Path.GetDirectoryName(dll));
                        }

                        break;
                    }
                }
            }

            if (!string.IsNullOrEmpty(pyHome))
            {
                PythonEngine.PythonHome = pyHome;
            }
        }
    }
}
