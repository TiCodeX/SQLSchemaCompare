namespace TiCodeX.SQLSchemaCompare.Infrastructure.Repository
{
    using System;
    using System.IO;
    using System.Xml;
    using System.Xml.Serialization;
    using TiCodeX.SQLSchemaCompare.Core.Entities;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces;
    using TiCodeX.SQLSchemaCompare.Core.Interfaces.Repository;

    /// <summary>
    /// Implementation that provides the mechanism to store and retrieve application settings
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        /// <summary>
        /// The app globals
        /// </summary>
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
            if (!File.Exists(this.appGlobals.AppSettingsFullFilename))
            {
                var appSettings = new AppSettings();
                this.Write(appSettings);
                return appSettings;
            }

            var xml = new XmlSerializer(typeof(AppSettings));
            using var f = File.OpenRead(this.appGlobals.AppSettingsFullFilename);
            using var r = XmlReader.Create(f);
            return xml.Deserialize(r) as AppSettings;
        }
    }
}
