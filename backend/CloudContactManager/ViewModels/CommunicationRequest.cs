namespace CloudContactManager.ViewModels
{
    /// <summary>
    /// ViewModel for sending bulk communications (SMS/Email) to selected customers.
    /// </summary>
    public class CommunicationRequest
    {
        /// <summary>
        /// List of selected customer IDs to send communication to.
        /// </summary>
        public List<int> CustomerIds { get; set; } = new List<int>();

        /// <summary>
        /// The message content to send.
        /// </summary>
        public string MessageContent { get; set; } = string.Empty;

        /// <summary>
        /// Communication type: "SMS" or "Email".
        /// </summary>
        public string Type { get; set; } = string.Empty;
    }
}
