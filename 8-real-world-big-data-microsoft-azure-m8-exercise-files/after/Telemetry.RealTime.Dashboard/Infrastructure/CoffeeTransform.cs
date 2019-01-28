using CoffeeSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace dashing.net.Infrastructure
{
    public class CoffeeTransform : IBundleTransform
    {
        private static Dictionary<string, string> _ContentCache = new Dictionary<string, string>();
        private static CoffeeScriptEngine _Engine = new CoffeeScriptEngine();

        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "text/javascript";
            response.Content = string.Empty;

            foreach (var fileInfo in response.Files)
            {
                if (fileInfo.Extension.Equals(".coffee", StringComparison.Ordinal))
                {
                    response.Content += TransformCache.Get(fileInfo, () => _Engine.Compile(File.ReadAllText(fileInfo.FullName)));
                }
                else if (fileInfo.Extension.Equals(".js", StringComparison.Ordinal))
                {
                    response.Content += TransformCache.Get(fileInfo, () => File.ReadAllText(fileInfo.FullName));
                }
            }
        }
    }
}