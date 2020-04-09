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
        }
    }
}
