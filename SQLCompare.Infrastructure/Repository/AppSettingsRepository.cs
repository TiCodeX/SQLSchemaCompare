using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces;
using SQLCompare.Core.Interfaces.Repository;
using System;
using System.IO;
using System.Xml.Serialization;

namespace SQLCompare.Infrastructure.Repository
{
    /// <summary>
    /// Implementation that provides the mechanism to store and retrieve application settings
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        private readonly IAppGlobals appGlobals;

        /// <summary>
        /// Initializes a new instance of the <see cref="AppSettingsRepository"/> class.
        /// </summary>
        /// <param name="appGlobals">Injected application global constants</param>
        public AppSettingsRepository(IAppGlobals appGlobals)
        {
            this.appGlobals = appGlobals;
        }

        /// <inheritdoc/>
        public void Write(AppSettings appSettings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(this.appGlobals.AppSettingsFullFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(this.appGlobals.AppSettingsFullFilename) ?? throw new InvalidOperationException());
            }

            var xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.Create(this.appGlobals.AppSettingsFullFilename))
            {
                xml.Serialize(f, appSettings);
            }
        }

        /// <inheritdoc/>
        public AppSettings Read()
        {
            var xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.OpenRead(this.appGlobals.AppSettingsFullFilename))
            {
                return xml.Deserialize(f) as AppSettings;
            }
        }
    }
}
