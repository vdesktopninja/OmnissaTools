using System.Globalization;
using System.Resources;
using System.Reflection;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of localization service using resource files
    /// </summary>
    public class LocalizationService : ILocalizationService
    {
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public LocalizationService()
        {
            _resourceManager = new ResourceManager("LogScrub.Gui.Resources.Strings", Assembly.GetExecutingAssembly());
            _currentCulture = CultureInfo.CurrentUICulture;
        }

        public CultureInfo CurrentCulture => _currentCulture;

        public string GetString(string key)
        {
            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                return value ?? key; // Return key if not found
            }
            catch
            {
                return key; // Return key if resource loading fails
            }
        }

        public string GetString(string key, params object[] args)
        {
            try
            {
                var format = GetString(key);
                return string.Format(format, args);
            }
            catch
            {
                return key; // Return key if formatting fails
            }
        }

        public void SetCulture(CultureInfo culture)
        {
            _currentCulture = culture ?? CultureInfo.InvariantCulture;
            
            // Set culture for current thread
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
            
            // Set culture for all future threads
            CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;
        }

        public void SetCulture(string cultureName)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(cultureName);
                SetCulture(culture);
            }
            catch (CultureNotFoundException)
            {
                // Fall back to invariant culture if culture not found
                SetCulture(CultureInfo.InvariantCulture);
            }
        }

        public IEnumerable<CultureInfo> GetAvailableCultures()
        {
            var cultures = new List<CultureInfo>
            {
                CultureInfo.GetCultureInfo("en-US"), // English
                CultureInfo.GetCultureInfo("pl-PL")  // Polish
            };

            return cultures;
        }
    }
}