using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace Community.PowerToys.Run.Plugin.JetbrainsProjects
{
    public class JetBrainsProductInfo
    {
        public string InstallDir { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("productCode")]
        public string ProductCode { get; set; }

        [JsonPropertyName("dataDirectoryName")]
        public string DataDirectoryName { get; set; }

        [JsonPropertyName("productVendor")]
        public string ProductVendor { get; set; }

        public string FirstLaunchOption { get; set; }

        [JsonPropertyName("launch")]
        public LaunchInfo[] Launch { get; set; }

        public string IconPath { get; set; }
    }

    public class LaunchInfo
    {
        [JsonPropertyName("launcherPath")]
        public string LauncherPath { get; set; }
    }

    public class JetBrainsProgram
    {
        public static JetBrainsProductInfo ParseProductInfo(string dir)
        {

            var productInfoPath = Path.Combine(dir, "product-info.json");

            if (!File.Exists(productInfoPath))
            {
                return null;
            }

            var json = File.ReadAllText(productInfoPath);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            try
            {
                var productInfo = JsonSerializer.Deserialize<JetBrainsProductInfo>(json, options);
                productInfo.InstallDir = dir;

                if (productInfo.Launch.Length > 0)
                {
                    productInfo.FirstLaunchOption = productInfo.Launch[0].LauncherPath;
                    var binPath = Path.GetDirectoryName(Path.Combine(dir, productInfo.FirstLaunchOption));

                    var iconPath = Directory.GetFiles(binPath, "*.ico").FirstOrDefault();

                    if (iconPath != null)
                    {
                        productInfo.IconPath = iconPath;
                    }
                    return productInfo;


                }
                else
                {
                    return null;
                }




            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error deserializing JSON: {ex.Message}");
                return null;
            }
        }
    }
}
