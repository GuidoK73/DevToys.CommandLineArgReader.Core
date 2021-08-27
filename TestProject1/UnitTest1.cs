using DevToys.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace TestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var _args = new string[] { "/silent", "/server", "Server1001", "/db", "MyDataBase", "/files", "\"file1\"", "\"file2\"",  "\"file3\"", "/fastmode", "On" };

            var _commandLineArgReader = new CommandLineArgReader<CommandLineArgsObject>(_args); // Build reader for args

            CommandLineArgsObject _commandLineArgsObject = _commandLineArgReader.GetObject(); // translate the args into the desired object.

            if (_commandLineArgReader.HelpRequested || _commandLineArgReader.NoArgs)
            {
                Console.WriteLine(_commandLineArgReader.Help); // check for /? and show help if requested.
                return;
            }

            Assert.AreEqual(true, _commandLineArgsObject.Silent);
            Assert.AreEqual("Server1001", _commandLineArgsObject.Server);
            Assert.AreEqual("MyDataBase", _commandLineArgsObject.DataBase);
            Assert.AreEqual("\"file1\"", _commandLineArgsObject.FileNames[0]);
            Assert.AreEqual("\"file2\"", _commandLineArgsObject.FileNames[1]);
            Assert.AreEqual("\"file3\"", _commandLineArgsObject.FileNames[2]);
            Assert.AreEqual(EnumTest.On, _commandLineArgsObject.FastMode);
        }
    }
}
