using CommandLine;
using CommandLine.Text;

namespace Signature
{
    internal class Options
    {
        [Option('i', "input", Required = true, HelpText = "Input file.")]
        public string InputFileName
        { get; set; }

        [Option('o', "output", Required = true, HelpText = "Output file.")]
        public string OutputFileName
        { get; set; }

        [Option('s', "size", HelpText = "Size of block in bytes.", DefaultValue = 1048576)]
        public int sizeBlock
        { get; set; }

        [Option('t', "time", HelpText = "Print time of work program.", DefaultValue = false)]
        public bool PrintTime
        { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this, current => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}
