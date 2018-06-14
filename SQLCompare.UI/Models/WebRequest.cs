using System;
using System.Net.Http;

namespace SQLCompare.UI.Models
{
    /// <summary>
    /// Represent a request performed with Ajax
    /// </summary>
    public class WebRequest
    {
        /// <summary>
        /// Gets or sets the URL of the request
        /// </summary>
        public Uri Url { get; set; }

        /// <summary>
        /// Gets or sets the method of the request
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the data to be sent if the method is POST
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Gets or sets the div name to serialize the data from
        /// </summary>
        public string SerializeDataFromDiv { get; set; }

        /// <summary>
        /// Gets or sets the target of the operation
        /// </summary>
        public string Target { get; set; }
    }
}
