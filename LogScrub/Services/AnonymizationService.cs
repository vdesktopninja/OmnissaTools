using LogScrub.Gui;

namespace LogScrub.Gui.Services
{
    /// <summary>
    /// Implementation of anonymization service
    /// </summary>
    public class AnonymizationService : IAnonymizationService
    {
        public Anonymizer CreateAnonymizer(string secret, Settings settings)
        {
            return new Anonymizer(secret, settings);
        }

        public (string Line, int IpMatches, int FqdnMatches, int UserMatches, int ServerMatches) AnonymizeLine(
            Anonymizer anonymizer, string line)
        {
            return anonymizer.AnonymizeLine(line);
        }
    }
}