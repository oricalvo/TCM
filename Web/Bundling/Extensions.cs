using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Optimization;

namespace TCM.Web.Bundling.Core
{
    public static class BundleCollectionExtensions
    {
        private static IBundleOrderer defaultOrderer;

        static BundleCollectionExtensions()
        {
            defaultOrderer = new BundleOrderer2();
        }

        public static void IncludeDirectory(this BundleCollection bundles, string bundleVirtualPath, params string[] directories)
        {
            ScriptBundle bundle = new ScriptBundle(bundleVirtualPath);
            foreach (string dir in directories)
            {
                DoIncludeDirectory(bundle, dir);
            }

            bundle.Orderer = defaultOrderer;
            bundles.Add(bundle);
        }

        private static void DoIncludeDirectory(ScriptBundle bundle, string virtualDir)
        {
            string dirPath = HttpContext.Current.Server.MapPath(virtualDir);
            foreach (string filePath in Directory.GetFiles(dirPath, "*.js"))
            {
                if (ExcludeScript(filePath))
                {
                    continue;
                }

                bundle.Include(virtualDir + "/" + Path.GetFileName(filePath));
            }

            foreach (string subDirPath in Directory.GetDirectories(dirPath))
            {
                string dirName = Path.GetFileName(subDirPath);
                string virtualSubDir = virtualDir + "/" + dirName;

                DoIncludeDirectory(bundle, virtualSubDir);
            }
        }

        private static bool ExcludeScript(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (string.Compare(fileName, "_references.js", true) == 0)
            {
                return true;
            }

            return false;
        }

        public static void SetDefaultBundleOrderer(this BundleCollection bundles, IBundleOrderer defaultOrderer)
        {
            BundleCollectionExtensions.defaultOrderer = defaultOrderer;
        }
    }
}
