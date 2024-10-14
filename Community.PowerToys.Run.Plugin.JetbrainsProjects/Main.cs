using ManagedCommon;
using Microsoft.PowerToys.Settings.UI.Library;

using System.Windows.Controls;
using System.Windows.Input;

using Wox.Infrastructure;
using Wox.Plugin;
using Wox.Plugin.Logger;

namespace Community.PowerToys.Run.Plugin.JetbrainsProjects
{
    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    public class Main : IPlugin, IContextMenu, IDisposable, ISettingProvider
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "188FA30869F0400DAE569461B2DA6D25";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "JetbrainsProjects";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "JetbrainsProjects Description";

        private PluginInitContext Context { get; set; }

        private readonly JetbrainsProjects _jetbrainsProjects = new JetbrainsProjects();

        private string IconPath { get; set; }

        private bool Disposed { get; set; }


        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            var search = query.Search;

            var products = _jetbrainsProjects.GetProjects();

            var results = new List<Result>();
            foreach (var product in products)
            {
                var matchResult = StringMatcher.FuzzySearch(query.Search, product.Name);
                if (string.IsNullOrWhiteSpace(query.Search) || matchResult.Score > 0)
                {
                    results.Add(new Result
                    {
                        QueryTextDisplay = product.Name,
                        IcoPath = product.Product.IconPath,
                        Title = product.Name + (  product.CurrentFile != null && DisplayLastFileName ? " > " + product.CurrentFile : ""),
                        SubTitle = product.Path,
                        ToolTipData = new ToolTipData("Last Opened", product.LastOpened.ToString()),
                        Action = action =>
                        {
                            _jetbrainsProjects.RunProject(product);
                            return true;
                        },
                        ContextData = product,
                    });
                }
                
            }

            return results;

        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Context.API.ThemeChanged += OnThemeChanged;
            UpdateIconPath(Context.API.GetCurrentTheme());

            _jetbrainsProjects.UpdateProjects();
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is Project project)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Open Project",
                        FontFamily = "Segoe Fluent Icons",
                        Glyph = "\xe838",
                        AcceleratorKey = Key.O,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            _jetbrainsProjects.RunProject(project);
                            return true;
                        },
                    }
                ];
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            if (Context?.API != null)
            {
                Context.API.ThemeChanged -= OnThemeChanged;
            }

            Disposed = true;
        }

        private void UpdateIconPath(Theme theme) => IconPath = theme == Theme.Light || theme == Theme.HighContrastWhite ? "Images/jetbrainsprojects.light.png" : "Images/jetbrainsprojects.dark.png";

        private void OnThemeChanged(Theme currentTheme, Theme newTheme) => UpdateIconPath(newTheme);

        public IEnumerable<PluginAdditionalOption> AdditionalOptions => [
          new()
            {
                Key = nameof(DisplayLastFileName),
                DisplayLabel = "Display last open file",
                DisplayDescription = "Display last open file if information is available",
                PluginOptionType = PluginAdditionalOption.AdditionalOptionType.Checkbox,
                Value = DisplayLastFileName,
            }
      ];

        private bool DisplayLastFileName { get; set; }

        /// <summary>
        /// Creates setting panel.
        /// </summary>
        /// <returns>The control.</returns>
        /// <exception cref="NotImplementedException">method is not implemented.</exception>
        public Control CreateSettingPanel() => throw new NotImplementedException();

        /// <summary>
        /// Updates settings.
        /// </summary>
        /// <param name="settings">The plugin settings.</param>
        public void UpdateSettings(PowerLauncherPluginSettings settings)
        {
            DisplayLastFileName = settings.AdditionalOptions.SingleOrDefault(x => x.Key == nameof(DisplayLastFileName))?.Value ?? false;
        }

    }
}
