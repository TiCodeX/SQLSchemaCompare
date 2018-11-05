using System;

namespace TiCodeX.SQLSchemaCompare.Core.Entities.Api
{
    /// <summary>
    /// Represent the VerifySession request
    /// </summary>
    public class VerifySessionRequest
    {
        /// <summary>
        /// Gets or sets the session token
        /// </summary>
        public string SessionToken { get; set; }

        /// <summary>
        /// Gets or sets the application version
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Gets or sets the product code
        /// </summary>
        public string ProductCode { get; set; }
    }
}
