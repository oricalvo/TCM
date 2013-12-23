using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;

namespace TCM.Web.Resources
{
    public class ResxJS
    {
        public static ClientResourcesGroup[] GetAllResourcesForNamespace(string namespaceName)
        {
            List<ClientResourcesGroup> groups = new List<ClientResourcesGroup>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.Namespace == null || !type.Namespace.StartsWith(namespaceName))
                {
                    continue;
                }

                string clientNamespace = type.Namespace.Substring(namespaceName.Length);
                if (clientNamespace != "")
                {
                    clientNamespace = clientNamespace + ".";
                }

                string clientTypeName = clientNamespace + type.Name;

                ClientResourcesGroup group = new ClientResourcesGroup()
                {
                    Name = clientTypeName,
                    Resources = new List<ClientResource>()
                };
                groups.Add(group);

                foreach (PropertyInfo propInfo in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (propInfo.PropertyType.IsPrimitive || propInfo.PropertyType == typeof(string))
                    {
                        group.Resources.Add(new ClientResource()
                        {
                            Name = propInfo.Name,
                            Value = propInfo.GetValue(null, null).ToString()
                        });
                    }
                }
            }

            return groups.ToArray();
        }

        public static void CreateResxJSFile(string namespaceName, string clientVarName)
        {
            ClientResourcesGroup[] groups = GetAllResourcesForNamespace(namespaceName);

            using (StreamWriter writer = File.CreateText(HttpContext.Current.Server.MapPath(("~/Scripts/ResX.js"))))
            {
                writer.WriteLine("var " + clientVarName + " = (function(){");

                string functions = "    function ensureObjectPathExists(parent, path) {var parts = path.split(\".\");if (!parts.length) {return;};for (var i = 0; i < parts.length; i++) {var part = parts[i];if (!parent[part]) {parent[part] = {};}parent = parent[part];}}";
                writer.WriteLine(functions);
                writer.WriteLine();

                writer.WriteLine("    var Resources = {};");
                writer.WriteLine();

                foreach (ClientResourcesGroup group in groups)
                {
                    writer.WriteLine("    ensureObjectPathExists(Resources, \"" + group.Name + "\");");

                    foreach (ClientResource res in group.Resources)
                    {
                        writer.WriteLine("        Resources." + group.Name + "." + res.Name + " = \"" + res.Value + "\";");
                    }

                    writer.WriteLine();
                }

                writer.WriteLine("    return Resources;");

                writer.WriteLine("})();");
            }
        }
    }

    public class ClientResourcesGroup
    {
        public string Name { get; set; }
        public List<ClientResource> Resources { get; set; }
    }

    public class ClientResource
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
