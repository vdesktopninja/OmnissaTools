namespace LogScrub.Gui
{
    /// <summary>
    /// Statistical report of the anonymization process
    /// </summary>
    public class Report
    {
        /// <summary>
        /// Number of IP addresses that were anonymized
        /// </summary>
        public long IpCount;

        /// <summary>
        /// Number of FQDNs (domain names) that were anonymized
        /// </summary>
        public long FqdnCount;

        /// <summary>
        /// Number of user names/emails that were anonymized
        /// </summary>
        public long UserCount;

        /// <summary>
        /// Number of server/host names that were anonymized
        /// </summary>
        public long ServerCount;

        /// <summary>
        /// Number of files that were processed successfully
        /// </summary>
        public long FilesProcessed;

        /// <summary>
        /// Number of files that were skipped because they were not text files
        /// </summary>
        public long SkippedNonText;

        /// <summary>
        /// Number of errors encountered during processing
        /// </summary>
        public long Errors;
    }
}
