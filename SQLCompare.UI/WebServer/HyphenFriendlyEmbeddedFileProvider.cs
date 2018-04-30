using System.Text;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;

namespace SQLCompare.UI.WebServer
{
    /// <summary>
    /// I have written this to workaround a present issue: https://github.com/aspnet/FileSystem/issues/184
    /// </summary>
    public class HyphenFriendlyEmbeddedFileProvider : IFileProvider
    {
        private readonly EmbeddedFileProvider _embeddedFileProvider;

        public HyphenFriendlyEmbeddedFileProvider(EmbeddedFileProvider embeddedFileProvider)
        {
            _embeddedFileProvider = embeddedFileProvider;
        }

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

            for (int i = indexOfFirstHyphen; i < indexOfLastSeperator; i++)
            {
                if (builder[i] == '-')
                {
                    builder[i] = '_';
                }
            }

            var normalisedPath = builder.ToString();
            return _embeddedFileProvider.GetFileInfo(normalisedPath);
        }

        public IDirectoryContents GetDirectoryContents(string subpath)
        {
            var contents = _embeddedFileProvider.GetDirectoryContents(subpath);
            return contents;
        }

        public IChangeToken Watch(string filter)
        {
            return _embeddedFileProvider.Watch(filter);
        }
    }
}
