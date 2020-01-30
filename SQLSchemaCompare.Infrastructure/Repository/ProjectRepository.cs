using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;
using TiCodeX.SQLSchemaCompare.Core.Entities.Project;
using TiCodeX.SQLSchemaCompare.Core.Interfaces.Repository;

namespace TiCodeX.SQLSchemaCompare.Infrastructure.Repository
{
    /// <summary>
    /// Implementation that provides the mechanism to store and retrieve the project configuration
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectRepository"/> class.
        /// </summary>
        /// <param name="loggerFactory">The injected logger factory</param>
        public ProjectRepository(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new ArgumentNullException(nameof(loggerFactory));
            }

            this.logger = loggerFactory.CreateLogger(nameof(ProjectRepository));
        }

        /// <inheritdoc />
        public CompareProject Read(string filename)
        {
            this.logger.LogInformation($"Reading project file: {filename}");
            var xml = new XmlSerializer(typeof(CompareProject));
            using var f = File.OpenRead(filename);
            using var r = XmlReader.Create(f);
            return xml.Deserialize(r) as CompareProject;
        }

        /// <inheritdoc />
        public void Write(CompareProject project, string filename)
        {
            this.logger.LogInformation($"Writing project file: {filename}");

            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                this.logger.LogInformation($"Creating folder: {Path.GetDirectoryName(filename)}");
                Directory.CreateDirectory(Path.GetDirectoryName(filename) ?? throw new InvalidOperationException());
            }

            var xml = new XmlSerializer(typeof(CompareProject));
            using (var f = File.Create(filename))
            {
                xml.Serialize(f, project);
            }
        }
    }
}
