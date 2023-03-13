using CommandLine;

namespace TiCodeX.SQLSchemaCompare.CLI
{
    /// <summary>
    /// The CLI options
    /// </summary>
    internal class Options
    {
        /// <summary>
        /// Gets or sets the project file
        /// </summary>
        [Option(
            shortName: 'p',
            longName: "project",
            Required = true,
            HelpText = "The project file")]
        public string ProjectFile { get; set; }

        /// <summary>
        /// Gets or sets the output file
        /// </summary>
        [Option(
            shortName: 'o',
            longName: "output",
            Required = true,
            HelpText = "The output file")]
        public string OutputFile { get; set; }
    }
}
