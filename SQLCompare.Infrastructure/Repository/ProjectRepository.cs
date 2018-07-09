using System;
using System.IO;
using System.Xml.Serialization;
using SQLCompare.Core.Entities.Project;
using SQLCompare.Core.Interfaces.Repository;

namespace SQLCompare.Infrastructure.Repository
{
    /// <summary>
    /// Implementation that provides the mechanism to store and retrieve the project configuration
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        /// <inheritdoc />
        public CompareProject Read(string filename)
        {
            var xml = new XmlSerializer(typeof(CompareProject));
            using (var f = File.OpenRead(filename))
            {
                return xml.Deserialize(f) as CompareProject;
            }
        }

        /// <inheritdoc />
        public void Write(CompareProject project, string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
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
