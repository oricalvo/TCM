using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Optimization;

namespace TCM.Web.Bundling.Core
{
    public class BundleOrderer3 : IBundleOrderer
    {
        private int maxLinesToParseForScriptReferences;

        public BundleOrderer3(int maxLinesToParseForScriptReferences = 5)
        {
            this.maxLinesToParseForScriptReferences = maxLinesToParseForScriptReferences;
        }

        private class Node
        {
            public string ID;
            public List<Node> Children;
            public BundleFile FileInfo;
            public bool IsRoot;
            public int Level;

            public Node(string id)
            {
                this.ID = id;
                this.Children = new List<Node>();
                this.IsRoot = true;
                this.Level = 0;
            }
        }

        public IEnumerable<BundleFile> OrderFiles(BundleContext context, IEnumerable<BundleFile> filesIn)
        {
            BundleFile[] files = filesIn.ToArray();
            Dictionary<string, Node> nodes = new Dictionary<string, Node>();

            //
            //  Build a node and collect dependency information for each file
            //
            foreach (BundleFile bundleFile in files)
            {
                FileInfo file = new FileInfo(context.HttpContext.Server.MapPath(bundleFile.VirtualFile.VirtualPath));

                //
                //  Create a node for the current file (the node may be already created)
                //
                string fileId = GetFileID(file.FullName);
                Node node = GetCreateNode(nodes, fileId);
                node.FileInfo = bundleFile;

                //
                //  Extract dependencies from script header
                //
                string[] dependencies = ParseDepenedencies(context, bundleFile, file);
                foreach (string dependencyId in dependencies)
                {
                    Node dependencyNode = GetCreateNode(nodes, dependencyId);
                    dependencyNode.Children.Add(node);
                }

                if (dependencies.Length > 0)
                {
                    //
                    //  Since this node has dependency it is no longer considered a root
                    //
                    node.IsRoot = false;
                }
            }

            //
            //  Orgnaize nodes by level. Level 0 is a root node which has no dependency.
            //
            foreach (Node rootNode in (from n in nodes.Values where n.IsRoot select n))
            {
                WalkBFS(rootNode, 0);
            }

            Dictionary<int, List<Node>> nodesByLevel = new Dictionary<int, List<Node>>();
            foreach (Node node in nodes.Values)
            {
                List<Node> levelNodes;
                if (!nodesByLevel.TryGetValue(node.Level, out levelNodes))
                {
                    levelNodes = new List<Node>();
                    nodesByLevel.Add(node.Level, levelNodes);
                }

                levelNodes.Add(node);
            }

            //
            //  Merge all nodes into flat list. First are level 0 and then level 1 and so on
            //
            List<Node> flat = new List<Node>();
            int level = 0;
            while (nodesByLevel.ContainsKey(level))
            {
                flat.AddRange(nodesByLevel[level]);

                ++level;
            }

            //
            //  res now contains the final flat list organized in the right order
            //
            BundleFile[] res = (from node in flat where node.FileInfo != null select node.FileInfo).ToArray();
            return res;
        }

        private string[] ParseDepenedencies(BundleContext context, BundleFile bundleFile, FileInfo file)
        {
            List<string> dependencies = new List<string>();

            using (StreamReader reader = File.OpenText(context.HttpContext.Server.MapPath(bundleFile.VirtualFile.VirtualPath)))
            {
                int lineCount = 0;

                while (true)
                {
                    if (lineCount > this.maxLinesToParseForScriptReferences)
                    {
                        //
                        //  Only scan the first 100 lines
                        //  js file generated from ts contains generated method like __extends.
                        //  Therefore we cannot assume that script reference are always at the top of the file
                        //
                        break;
                    }

                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        //
                        //  End of file
                        //
                        break;
                    }
                    ++lineCount;

                    line = line.Trim();

                    if (!line.StartsWith("/// <reference"))
                    {
                        continue;
                    }

                    int begin = line.IndexOf('"');
                    int end = line.IndexOf('"', begin + 1);
                    if (begin == -1 || end == -1)
                    {
                        //
                        //  Bad reference syntax. Ignore this line
                        //
                        continue;
                    }

                    string dependencyRelativePath = line.Substring(begin + 1, end - begin - 1);
                    string dependencyFullPath = GetFilePath(Path.GetFullPath(Path.Combine(file.DirectoryName, dependencyRelativePath)));

                    if (!File.Exists(dependencyFullPath))
                    {
                        throw new Exception("Script " + file.Name + " contains invalid script reference " + dependencyRelativePath);
                    }

                    string dependencyFileName = Path.GetFileName(dependencyFullPath);
                    string dependencyId = GetFileID(dependencyFullPath);

                    dependencies.Add(dependencyId);
                }
            }

            return dependencies.ToArray();
        }

        private Node GetCreateNode(Dictionary<string, Node> nodes, string fileId)
        {
            Node node;

            if (!nodes.TryGetValue(fileId, out node))
            {
                node = new Node(fileId);
                nodes.Add(fileId, node);
            }

            return node;
        }

        private string GetFilePath(string filePath)
        {
            string res = filePath.Replace(".ts", ".js");
            return res;
        }

        private string GetFileID(string filePath)
        {
            string res = filePath.ToLower().Replace(".ts", ".js");
            return res;
        }

        private void WalkBFS(Node root, int level)
        {
            if (root.Level < level)
            {
                //
                //  Only update if level is higher than calculated up until now
                //
                root.Level = level;
            }

            foreach (Node child in root.Children)
            {
                WalkBFS(child, level + 1);
            }
        }
    }
}
