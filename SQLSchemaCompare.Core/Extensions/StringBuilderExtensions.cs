using System;
using System.Text;

namespace TiCodeX.SQLSchemaCompare.Core.Extensions
{
    /// <summary>
    /// The StringBuilder extensions
    /// </summary>
    public static class StringBuilderExtensions
    {
        /// <summary>
        /// Appends the default line terminator if it's not empty.
        /// </summary>
        /// <param name="sb">The StringBuilder.</param>
        /// <returns>A reference to this instance</returns>
        public static StringBuilder AppendLineIfNotEmpty(this StringBuilder sb)
        {
            if (sb == null)
            {
                throw new ArgumentNullException(nameof(sb));
            }

            return sb.Length > 0 ? sb.AppendLine() : sb;
        }
    }
}
