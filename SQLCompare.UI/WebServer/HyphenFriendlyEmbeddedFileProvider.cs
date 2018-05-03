using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;

namespace SQLCompare.UI.WebServer
{
    /// <summary>
    /// EmbeddedFileProvider wrapper that properly handles the hyphen characters.
    /// See: https://github.com/aspnet/FileSystem/issues/184
    /// </summary>
    public class HyphenFriendlyEmbeddedFileProvider : IFileProvider
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HyphenFriendlyEmbeddedFileProvider"/> class.
        /// </summary>
        /// <param name="embeddedFileProvider">The <see cref="EmbeddedFileProvider"/> to wrap</param>
        public HyphenFriendlyEmbeddedFileProvider(EmbeddedFileProvider embeddedFileProvider)
        {
            _embeddedFileProvider = embeddedFileProvider;
        }

        /// <inheritdoc/>
        public IFileInfo GetFileInfo(string subpath)
        {
            if (string.IsNullOrEmpty(subpath))
            {
                return new NotFoundFileInfo(subpath);
            }

            // does the subpath contain directory?
            var indexOfLastSeperator = subpath.LastIndexOf('/');
            if (indexOfLastSeperator == -1)
            {
                return _embeddedFileProvider.GetFileInfo(subpath);
            }

            // Does it contain a hyphen?
            var indexOfFirstHyphen = subpath.IndexOf('-');
            if (indexOfFirstHyphen == -1)
            {
                // no hyphens.
                return _embeddedFileProvider.GetFileInfo(subpath);
            }

            // is hyphen within the directory portion?
            if (indexOfFirstHyphen > indexOfLastSeperator)
            {
                // nope
                return _embeddedFileProvider.GetFileInfo(subpath);
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
            return _embeddedFileProvider.GetFileInfo(normalisedPath);
        }

        /// <inheritdoc/>
        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var contents = _embeddedFileProvider.GetDirectoryContents(subpath);
            return contents;
        }

        /// <inheritdoc/>
        public IChangeToken Watch(string filter)
        {
            return _embeddedFileProvider.Watch(filter);
        }
    }
}
