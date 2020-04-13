Extra marshaling codecs for Python.NET
======================================

Running tests manually
----------------------

Set PYTHON_HOME and PYTHON_VERSION environment variables. If using Visual Studio,
this has to be done before launching Visual Studio.

Alternatively,
create LAUNCH.cs file in Codecs.Tests project, and add the following code there:

.. code-block:: csharp

    partial class TestsRuntimeConfig
    {
        static TestsRuntimeConfig()
        {
            Environment.SetEnvironmentVariable("PYTHON_HOME", @"PATH/TO/PYHOME");
            Environment.SetEnvironmentVariable("PYTHON_VERSION", "3.5");
        }
    }

Resources
---------
Mailing list: https://mail.python.org/mailman/listinfo/pythondotnet
Chat: https://gitter.im/pythonnet/pythonnet
