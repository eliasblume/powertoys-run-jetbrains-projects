using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Wox.Plugin;

namespace Community.PowerToys.Run.Plugin.JetbrainsProjects.UnitTests
{

    [TestClass]
    public class JetbarinsProjectTests
    {
        private JetbrainsProjects jetbrainsProjects;

        [TestInitialize]
        public void TestInitialize()
        {
            jetbrainsProjects = new JetbrainsProjects();
        }

        [TestMethod]
        public void GetProjects_should_return_results()
        {
            var results  = jetbrainsProjects.GetInstalledProducts();

            foreach (var result in results)
            {
                System.Diagnostics.Debug.WriteLine(result.Name);
                System.Diagnostics.Debug.WriteLine(result.InstallDir);
                System.Diagnostics.Debug.WriteLine(result.DataDirectoryName);
                System.Diagnostics.Debug.WriteLine(result.ProductVendor);
                System.Diagnostics.Debug.WriteLine(result.ProductCode);
                System.Diagnostics.Debug.WriteLine(result.FirstLaunchOption);
                System.Diagnostics.Debug.WriteLine(result.IconPath);
            }

            Assert.IsNotNull(results.First());
        }

        [TestMethod]
        public void GetProducts_should_return_results()
        {
            var results = jetbrainsProjects.GetProducts();

            // log the results
            foreach (var result in results)
            {
                System.Diagnostics.Debug.WriteLine("-------");
                System.Diagnostics.Debug.WriteLine(result.ProductInfo.Name);
                System.Diagnostics.Debug.WriteLine("-------");


                var projects = jetbrainsProjects.GetProjectsFromProduct(result);

                foreach (var project in projects)
                {
                    System.Diagnostics.Debug.WriteLine("-------");
                    System.Diagnostics.Debug.WriteLine(project.Name);
                    System.Diagnostics.Debug.WriteLine(project.Path);
                    System.Diagnostics.Debug.WriteLine(project.LastOpened);
                    System.Diagnostics.Debug.WriteLine(project.CurrentFile);
                }
            }

            Assert.IsNotNull(results.First());
        }
    }

    [TestClass]
    public class MainTests
    {
        private Main main;

        [TestInitialize]
        public void TestInitialize()
        {
            main = new Main();
        }

        [TestMethod]
        public void Query_should_return_results()
        {
            var results = main.Query(new("search"));

            Assert.IsNotNull(results.First());
        }

        [TestMethod]
        public void LoadContextMenus_should_return_results()
        {
            var results = main.LoadContextMenus(new Result { ContextData = "search" });

            Assert.IsNotNull(results.First());
        }
    }
}
