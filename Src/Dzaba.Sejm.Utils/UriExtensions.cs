using System;
using System.Collections.Generic;
using System.IO;

namespace Dzaba.Sejm.Utils
{
    public static class UriExtensions
    {
        public static Uri GetHostUri(this Uri uri)
        {
            Require.NotNull(uri, nameof(uri));

            return new Uri($"{uri.Scheme}://{uri.Host}");
        }

        public static IEnumerable<string> SplitAbsolutePath(this Uri uri)
        {
            Require.NotNull(uri, nameof(uri));

            return uri.AbsolutePath.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string ToLocalRelativePath(this Uri uri)
        {
            Require.NotNull(uri, nameof(uri));

            return uri.PathAndQuery.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
