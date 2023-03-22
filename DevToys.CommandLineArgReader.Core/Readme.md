# CommandLineArgReader

this class can be used to create standard console applications with an extensive set of parameters.


### Sample Code


~~~cs

using System.ComponentModel;
using System.Runtime.Serialization;

namespace BaseCommandLineArgReaderExample
{
    public enum EnumTest
    {
        On,
        Off
    }
    
    [DefaultProperty("FileName")]
    [Description("Filename commandline argument is mandatory, other settings can be set by using App.config as well.")]
    public class CommandLineArgsObject
    {
        [Description("Target database name")]
        [DataMember(Name = "/db,/database", IsRequired = true)]
        public string DataBase { get; set; }

        [Description("Target server name")]
        [DataMember(Name = "/srv,/server")]
        public string Server { get; set; }

        [Description("Buffersize")]
        [DataMember(Name = "/buf,/buffersize")]
        public int BufferSize { get; set; } = 500;

        [Description("Arrays, all items followed by the identifier are considered array elements untill the next identifier.")]
        [DataMember(Name = "/fs,/files")]
        public string[] FileNames { get; set; }
        
        [Browsable(false)]
        [Description("Hidden property")]
        [DataMember(Name = "/test")]
        public string Test { get; set; }    
    
        [Description("Switch key example, boolean works as a switch, add it for true, leave it for false.")]
        [DataMember(Name = "/silent")]
        public bool Silent { get; set; }  
      
        [Description("Enum Test")]
        [DataMember(Name = "/fastmode")]
        public EnumTest OnOrOff { get; set; }     
    }
}

class Program
{
    static void Main(string[] args)
    {
        var _commandLineArgReader = new CommandLineArgReader<CommandLineArgsObject>(args); // Build reader for args
        
        CommandLineArgsObject _commandLineArgsObject = _commandLineArgReader.GetObject(); // translate the args into the desired object.

        if (_commandLineArgReader.HelpRequested || _commandLineArgReader.NoArgs)
        {
            Console.WriteLine(_commandLineArgReader.Help); // check for /? and show help if requested.
            return;
        }
        
        string _db = _commandLineArgsObject.DataBase;
    }
}

~~~


### Sample Call
 

~~~

Sample call:
    MyApp.exe /silent /server Server1001 /db MyDataBase /files "file1" "file2" "file3" /fastmode On

For help:
    MyApp.exe /?

~~~

-   following parameter is expected to be the value unless it's a keyword.
-   when a keyword does not have a following value it's assumed to be a Switch, in this case the type is expected to be a Boolean or String (string will then contain true when set.).
-   in the case a keyword may be followed by multiple values an Array (string[], int[], DateTime[] etc) can be used as the type.
-   Help, Builds help output based on Datamember / Type / Description Attributes. /? or /help are reserved keywords for help output (HelpRequested will return true)


 
## Attributes

All Attributes are common Attributes.


### DataMemberAttribute
[DataMember(Name = "")]\
Maps name to argument keyword.\
Multiple keywords kan be mapped to the same property by separate names with a comma like: "/f,/filename"

### IsRequired
[DataMember(Name = "", IsRequired = true)]\
IsRequired indicates the argument must be specified (HelpRequested returns true).

### DefaultPropertyAttribute
[DefaultProperty("")]\
Class level attribute, points to the default property, when no keywords are given, the value will be assigned to this property.\
(DefaultProperty does not support arrays)


### DescriptionAttribute
[Description("")]\
is used for the Helptext.

### Browsable
[Browsable(false)]\
With the Browseable Attribute set to false you can hide arguments from the help output.








