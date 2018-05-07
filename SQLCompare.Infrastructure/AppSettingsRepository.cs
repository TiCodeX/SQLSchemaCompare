using SQLCompare.Core;
using SQLCompare.Core.Entities;
using SQLCompare.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SQLCompare.Infrastructure
{
    /// <summary>
    /// Defines the application settings repository
    /// </summary>
    public class AppSettingsRepository : IAppSettingsRepository
    {
        /// <inheritdoc/>
        public void Write(AppSettings appSettings)
        {
            if (!Directory.Exists(Path.GetDirectoryName(AppGlobal.AppSettingsFullFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(AppGlobal.AppSettingsFullFilename));
            }

            XmlSerializer xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.OpenWrite(AppGlobal.AppSettingsFullFilename))
            {
                xml.Serialize(f, appSettings);
            }
        }

        /// <inheritdoc/>
        public AppSettings Read()
        {
            XmlSerializer xml = new XmlSerializer(typeof(AppSettings));
            using (var f = File.OpenRead(AppGlobal.AppSettingsFullFilename))
            {
                return xml.Deserialize(f) as AppSettings;
            }
        }
    }
}
