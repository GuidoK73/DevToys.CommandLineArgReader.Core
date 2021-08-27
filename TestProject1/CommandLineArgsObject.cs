using System.ComponentModel;
using System.Runtime.Serialization;

namespace TestProject1
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
        public EnumTest FastMode { get; set; }
    }
}
