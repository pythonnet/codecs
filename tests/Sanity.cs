using NUnit.Framework;

namespace Python.Runtime.Codecs
{
    public class Sanity
    {
        [Test]
        public void Check() { }

        [SetUp]
        public void SetUp()
        {
            TestsRuntimeConfig.Ensure();
            PythonEngine.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            PythonEngine.Shutdown();
        }
    }
}
