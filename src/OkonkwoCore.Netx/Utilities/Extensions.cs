using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OkonkwoCore.Netx.Utilities
{
    public static class Extensions
    {
        /// <summary>
        /// Divides an lists of an entity into smaller lists per the given chunkSize
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static List<List<TEntity>> ChunkBy<TEntity>(this IList<TEntity> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        /// <summary>
        /// Returns the full directory path of the given type
        /// </summary>
        /// <param name="type">The type to return the full directory path for.</param>
        /// <returns></returns>
        public static string GetAssemblyPath(this Type type)
        {
            var codeBase = type.Assembly.CodeBase;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);

            var assemblyDirectory = Path.GetDirectoryName(path);

            return assemblyDirectory;
        }

        /// <summary>
        /// Removes trailing occurrence(s) of a given string from the current System.String object.
        /// </summary>
        /// <param name="input">The inupt string to trim.</param> 
        /// <param name="trimSuffix">A string to remove from the end of the current System.String object.</param>
        /// <param name="removeAll">If true, removes all trailing occurrences of the given suffix; 
        /// otherwise, just removes the outermost one.</param>
        /// <returns>The string that remains after removal of suffix occurrence(s) of the string in the 
        /// trimSuffix parameter.</returns>
        public static string TrimEnd(this string input, string trimSuffix, bool removeAll = true)
        {
            while (input != null && trimSuffix != null && input.EndsWith(trimSuffix))
            {
                input = input.Substring(0, input.Length - trimSuffix.Length);

                if (!removeAll)
                {
                    return input;
                }
            }

            return input;
        }
    }
}
