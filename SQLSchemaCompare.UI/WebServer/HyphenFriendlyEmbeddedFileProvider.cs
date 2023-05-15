namespace TiCodeX.SQLSchemaCompare.UI.WebServer
{
    using System;
    using System.Text;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;

    /// <summary>
    /// EmbeddedFileProvider wrapper that properly handles the hyphen characters.
    /// See: https://github.com/aspnet/FileSystem/issues/184
    /// </summary>
    public class HyphenFriendlyEmbeddedFileProvider : IFileProvider
    {
        /// <summary>
        /// The embedded file provider
        /// </summary>
        private readonly EmbeddedFileProvider embeddedFileProvider;

        /// <summary>
        /// The logger
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyphenFriendlyEmbeddedFileProvider"/> class.
        /// </summary>
        /// <param name="embeddedFileProvider">The <see cref="EmbeddedFileProvider"/> to wrap</param>
        /// <param name="logger">The logger</param>
        public HyphenFriendlyEmbeddedFileProvider(EmbeddedFileProvider embeddedFileProvider, ILogger logger)
        {
            this.embeddedFileProvider = embeddedFileProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public IFileInfo GetFileInfo(string subpath)
        {
            this.logger.LogTrace($"Requested embedded file: {subpath}");

            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            // does the subpath contain directory?
            var indexOfLastSeperator = subpath.LastIndexOf('/');
            if (indexOfLastSeperator == -1)
            {
                return this.embeddedFileProvider.GetFileInfo(subpath);
            }

            // Does it contain a hyphen?
            var indexOfFirstHyphen = subpath.IndexOf('-', StringComparison.Ordinal);
            if (indexOfFirstHyphen == -1)
            {
                // no hyphens.
                return this.embeddedFileProvider.GetFileInfo(subpath);
            }

            // is hyphen within the directory portion?
            if (indexOfFirstHyphen > indexOfLastSeperator)
            {
                // nope
                return this.embeddedFileProvider.GetFileInfo(subpath);
            }

            // Ok, re-write directory portion, from the first hyphen, replacing hyphens!
            var builder = new StringBuilder(subpath.Length);
            builder.Append(subpath);

            for (var i = indexOfFirstHyphen; i < indexOfLastSeperator; i++)
            {
                if (builder[i] == '-')
                {
                    builder[i] = '_';
                }
            }

            var normalisedPath = builder.ToString();
            return this.embeddedFileProvider.GetFileInfo(normalisedPath);
        }

        /// <inheritdoc/>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var contents = this.embeddedFileProvider.GetDirectoryContents(subpath);
            return contents;
        }

        /// <inheritdoc/>
        public IChangeToken Watch(string filter)
        {
            return this.embeddedFileProvider.Watch(filter);
        }
    }
}
