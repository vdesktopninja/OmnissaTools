using System.Globalization;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for handling application localization
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets the current culture
        /// </summary>
        CultureInfo CurrentCulture { get; }

        /// <summary>
        /// Gets a localized string by key
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <returns>Localized string</returns>
        string GetString(string key);

        /// <summary>
        /// Gets a localized string by key with format arguments
        /// </summary>
        /// <param name="key">Resource key</param>
        /// <param name="args">Format arguments</param>
        /// <returns>Formatted localized string</returns>
        string GetString(string key, params object[] args);

        /// <summary>
        /// Sets the current culture
        /// </summary>
        /// <param name="culture">Culture to set</param>
        void SetCulture(CultureInfo culture);

        /// <summary>
        /// Sets the current culture by name
        /// </summary>
        /// <param name="cultureName">Culture name (e.g., "en-US", "pl-PL")</param>
        void SetCulture(string cultureName);

        /// <summary>
        /// Gets available cultures
        /// </summary>
        /// <returns>List of available cultures</returns>
        IEnumerable<CultureInfo> GetAvailableCultures();
    }
}