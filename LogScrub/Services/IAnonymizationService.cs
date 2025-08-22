using LogScrub.Gui;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Service for anonymizing sensitive data in log files
    /// </summary>
    public interface IAnonymizationService
    {
        /// <summary>
        /// Creates an anonymizer instance with the given secret and settings
        /// </summary>
        /// <param name="secret">Secret key for tokenization</param>
        /// <param name="settings">Anonymization settings</param>
        /// <returns>Configured anonymizer instance</returns>
        Anonymizer CreateAnonymizer(string secret, Settings settings);

        /// <summary>
        /// Anonymizes a single line of text
        /// </summary>
        /// <param name="anonymizer">Configured anonymizer instance</param>
        /// <param name="line">Line to anonymize</param>
        /// <returns>Anonymization result with statistics</returns>
        (string Line, int IpMatches, int FqdnMatches, int UserMatches, int ServerMatches) AnonymizeLine(
            Anonymizer anonymizer, string line);
    }
}