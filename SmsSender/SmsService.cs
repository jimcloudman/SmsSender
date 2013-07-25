using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mandrill;

namespace SmsSender
{
    public interface ISmsService
    {
        /// <summary>
        /// Sends a message to the specified phone number.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="phoneNumber"></param>
        /// <param name="carrier"></param>
        void Send(string message, long phoneNumber, string carrier);

        /// <summary>
        /// List the possible carrier options.
        /// </summary>
        /// <returns></returns>
        List<string> GetCarrierOptions();
    }

    public class SmsService : ISmsService
    {
        private readonly string _fromAddress;
        private readonly MandrillApi _api;

        // TODO: Replace this with something reading from a JSON file, preferably on GitHub
        private readonly Dictionary<string, string> _templates = new Dictionary<string, string>
                {
                    {"CELLCOM", "{0}@cellcom.quiktxt.com"}
                };

        // TODO: Probably should set this up for multiple email methods - SMTP through Gmail as one option
        /// <summary>
        /// Creates a service to send SMS messages.
        /// </summary>
        /// <param name="mandrillApiKey">API key for the Mandrill emailing service.</param>
        /// <param name="fromAddress">The from address to show in your message.</param>
        public SmsService(string mandrillApiKey, string fromAddress)
        {
            _fromAddress = fromAddress;
            _api = new MandrillApi(mandrillApiKey);
        }

        public void Send(string message, long phoneNumber, string carrier)
        {
            _api.SendMessageAsync(new EmailMessage
                {
                    from_email = _fromAddress,
                    text = message,
                    to = new List<EmailAddress>
                        {
                            new EmailAddress
                                {
                                    email = GetDestination(phoneNumber, carrier)
                                }
                        }});
        }

        public List<string> GetCarrierOptions()
        {
            return _templates.Keys.ToList();
        }

        private string GetDestination(long phoneNumber, string carrier)
        {
            return string.Format(GetCarrierTemplate(carrier), phoneNumber);
        }

        private string GetCarrierTemplate(string carrier)
        {
            var carrierKey = carrier.ToUpperInvariant();
            
            var exists = _templates.ContainsKey(carrierKey);
            if (!exists) throw new SmsServiceException("Carrier does not exist.");

            return _templates[carrierKey];
        }
    }

    [Serializable]
    public class SmsServiceException : Exception
    {
        public SmsServiceException(string message): base(message){}
    }
}
