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
            string _argumentString = "/silent /server Server1001 /db MyDataBase /files \"file1\" \"file2\" \"file3\" /fastmode On /buffersize 256";
            string[] _args = _argumentString.Split(" ");

            var _commandLineArgReader = new CommandLineArgReader<CommandLineArgsObject>(_args); // Build reader for args
            var _commandLineArgsObject = _commandLineArgReader.GetObject(); // translate the args into the desired object.
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
            Assert.AreEqual(256, _commandLineArgsObject.BufferSize);
        }

        [TestMethod]
        public void TestHelpOutput()
        {
            string _argumentString = "";
            string[] _args = _argumentString.Split(" ");

            var _commandLineArgReader = new CommandLineArgReader<CommandLineArgsObject>(_args); // Build reader for args
            var _commandLineArgsObject = _commandLineArgReader.GetObject(); // translate the args into the desired object.

            Assert.AreEqual(true, _commandLineArgReader.HelpRequested);

            string _expecting = @"Usage:

/db | /database [DataBase] (Required) 
Target database name

 /srv | /server [Server] 
Target server name

 /buf | /buffersize [BufferSize] 
Buffersize

 /fs | /files [PARAM] [PARAM] [PARAM] etc. (Array, any following parameter not a keyword is assumed to be an array element.)
Arrays, all items followed by the identifier are considered array elements untill the next identifier.

 /silent  
Switch key example, boolean works as a switch, add it for true, leave it for false.

 /fastmode [On | Off] 
Enum Test

 

Filename commandline argument is mandatory. Bla Bla
";

                Assert.AreEqual(_expecting, _commandLineArgReader.Help);

            

        }
    }
}
