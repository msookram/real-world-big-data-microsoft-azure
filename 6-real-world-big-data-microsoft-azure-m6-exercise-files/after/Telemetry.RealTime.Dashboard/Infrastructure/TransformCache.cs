using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;

namespace dashing.net.Infrastructure
{
    public static class TransformCache
    {
        private static Dictionary<string, string> _ContentCache = new Dictionary<string, string>();

        public static string Get(FileInfo sourceFile, Func<string> transformRunner)
        {
            var stopwatch = Stopwatch.StartNew();
            var key = GetKey(sourceFile);

            if (!_ContentCache.ContainsKey(key))
            {
                if (bool.Parse(ConfigurationManager.AppSettings["Dashing.CacheTransformsOnDisk"]))
                {
                    var outputDirectory = Path.Combine(Path.GetTempPath(), "transformcache");
                    Directory.CreateDirectory(outputDirectory);                    

                    var path = Path.Combine(outputDirectory, key);
                    if (!File.Exists(path))
                    {
                        var output = transformRunner();
                        File.WriteAllText(path, output);
                        _ContentCache[key] = output;
                    }
                    else
                    {
                        _ContentCache[key] = File.ReadAllText(path);
                    }
                }
                else
                {
                    var output = transformRunner();
                    _ContentCache[key] = output;
                }
            }

            return _ContentCache[key];
        }

        private static string GetKey(FileInfo sourceFile)
        {
            return string.Format("{0}-{1}.cache", sourceFile.Name, sourceFile.LastWriteTimeUtc.Ticks);
        } 
    }
}