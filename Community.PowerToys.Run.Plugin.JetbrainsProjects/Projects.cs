using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using Wox.Infrastructure;
using Wox.Plugin;
using System.Collections.ObjectModel;




namespace Community.PowerToys.Run.Plugin.JetbrainsProjects
{
    public class Product
    {
        public string ProjectsXml { get; set; }
        public JetBrainsProductInfo ProductInfo { get; set; }
    }

    public class Project
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string? CurrentFile { get; set; }
        public DateTime LastOpened { get; set; }
        public JetBrainsProductInfo Product { get; set; }


    }

    public class JetbrainsProjects
    {

        private static readonly string[] ProgrammsDirs = [
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "JetBrains"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "JetBrains"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs")
        ];

        private ReadOnlyCollection<Product>? _products = new ReadOnlyCollection<Product>([]);


        public void UpdateProjects()
        {
            _products = new ReadOnlyCollection<Product>(GetProducts());
        }

        public List<Product> GetProducts()
        {
            var products = new List<Product>();

            var intsalledProducts = GetInstalledProducts();

            foreach (var product in intsalledProducts)
            {
                var xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), product.ProductVendor, product.DataDirectoryName, "options", "recentProjects.xml");
                if (File.Exists(xmlPath))
                {
                    products.Add(new Product
                    {
                        ProjectsXml = xmlPath,
                        ProductInfo = product
                    });
                }

                xmlPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), product.ProductVendor, product.DataDirectoryName, "options", "recentSolutions.xml");
                if (File.Exists(xmlPath))
                {
                    products.Add(new Product
                    {
                        ProjectsXml = xmlPath,
                        ProductInfo = product
                    });
                }


            }

            return products;
        }

        public bool RunProject(Project project)
        {

            var binPath = Path.Combine(project.Product.InstallDir, project.Product.FirstLaunchOption);

            Helper.OpenInShell(binPath, $"\"{project.Path}\"");
            return true;
        }


        public List<JetBrainsProductInfo> GetInstalledProducts()
        {
            var installedProducts = new List<JetBrainsProductInfo>();

            // Mainly the current user or computer installation
            foreach (var programsDir in ProgrammsDirs)
            {
                if (!Directory.Exists(programsDir))
                {
                    continue;
                }

                var dirs = Directory.GetDirectories(programsDir);
                // jetbrains product directory
                foreach (var dir in dirs)
                {

                    var product = JetBrainsProgram.ParseProductInfo(dir);

                    if (product != null)
                    {
                        installedProducts.Add(product);
                    }
                }
            }

            return installedProducts;
        }

        public List<Project> GetProjects()
        {

            var projects = new List<Project>();
            foreach (var product in _products)
            {
                projects.AddRange(GetProjectsFromProduct(product));
            }

            projects.Sort((x, y) => y.LastOpened.CompareTo(x.LastOpened));

            return projects;

        }

        public List<Project> GetProjectsFromProduct(Product product)
        {
            var projects = new List<Project>();

            if (!File.Exists(product.ProjectsXml))
            {
                return projects;
            }

            var xml = new XmlDocument();
            xml.Load(product.ProjectsXml);

            var entryNodes = xml.SelectNodes("//application/component/option[@name='additionalInfo']/map/entry");

            if (entryNodes != null)
            {
                foreach (XmlNode entryNode in entryNodes)
                {
                    var projectPath = entryNode.Attributes?["key"]?.Value;
                    if (string.IsNullOrEmpty(projectPath)) continue;

                    projectPath = ReplaceUserHome(projectPath);

                    // check if file or dir
                    if (!File.Exists(projectPath) && !Directory.Exists(projectPath))
                    {
                        continue;
                    }

                    var metaInfoNode = entryNode.SelectSingleNode(".//RecentProjectMetaInfo");
                    if (metaInfoNode == null) continue;

                    var frameTitle = metaInfoNode.Attributes?["frameTitle"]?.Value ?? Path.GetFileName(projectPath);

                    if (string.IsNullOrEmpty(frameTitle)) continue;

                    string projectName = ExtractProjectName(frameTitle) ?? frameTitle;
                    string? currentFile = ExtractCurrentFile(frameTitle);

                    var lastOpenedTimestamp = metaInfoNode.SelectSingleNode(".//option[@name='projectOpenTimestamp']")?.Attributes?["value"]?.Value;

                    if (!string.IsNullOrEmpty(lastOpenedTimestamp) && long.TryParse(lastOpenedTimestamp, out long timestamp))
                    {
                        var lastOpened = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).DateTime;

                        projects.Add(new Project
                        {
                            Name = projectName,
                            Path = projectPath,
                            CurrentFile = currentFile,
                            LastOpened = lastOpened,
                            Product = product.ProductInfo
                        });
                    }
                }
            }
            return projects;
        }

        private string CleanProductName(string dirName)
        {
            // Remove year and version number
            string pattern = @"^(.*?)(?:\d{4}\.\d+)?$";
            Match match = Regex.Match(dirName, pattern);

            if (match.Success && match.Groups.Count > 1)
            {
                string name = match.Groups[1].Value;

                // Additional cleaning if needed
                name = name.Replace("IDEA", "Idea");  // Adjust "IntelliJIDEA" to "IntelliJIdea"

                return name;
            }

            return dirName;  // Return original if no match
        }

        private string? ExtractProjectName(string? frameTitle)
        {
            if (string.IsNullOrEmpty(frameTitle)) return null;

            var match = Regex.Match(frameTitle, @"^(.*?)\s+–");
            return match.Success ? match.Groups[1].Value.Trim() : frameTitle;
        }
        private string ReplaceUserHome(string path)
        {
            return path.Replace("$USER_HOME$", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
        }

        private string? ExtractCurrentFile(string frameTitle)
        {
            if (string.IsNullOrEmpty(frameTitle)) return null;

            var match = Regex.Match(frameTitle, @"–\s+(.*)$");
            return match.Success ? match.Groups[1].Value.Trim() : null;
        }


        public JetbrainsProjects() { }
    }


}
