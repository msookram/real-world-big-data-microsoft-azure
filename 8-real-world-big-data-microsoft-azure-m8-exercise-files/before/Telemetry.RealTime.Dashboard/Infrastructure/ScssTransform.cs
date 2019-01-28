using SassAndCoffee.Ruby.Sass;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Optimization;

namespace dashing.net.Infrastructure
{
    public class ScssTransform : IBundleTransform
    {
        private static Dictionary<string, string> _ContentCache = new Dictionary<string, string>();
        private static SassCompiler _Engine = new SassCompiler();

        public void Process(BundleContext context, BundleResponse response)
        {
            response.ContentType = "text/css";
            response.Content = string.Empty;

            foreach (var fileInfo in response.Files)
            {
                if (fileInfo.Extension.Equals(".sass", StringComparison.Ordinal) || fileInfo.Extension.Equals(".scss", StringComparison.Ordinal))
                {
                    response.Content += TransformCache.Get(fileInfo, () => _Engine.Compile(fileInfo.FullName, false, new List<string>()));
                }
                else if (fileInfo.Extension.Equals(".css", StringComparison.Ordinal))
                {
                    response.Content += TransformCache.Get(fileInfo, () => File.ReadAllText(fileInfo.FullName));
                }
            }
        }
    }
}