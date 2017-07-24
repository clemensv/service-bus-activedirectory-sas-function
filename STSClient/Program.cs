
namespace STSClient
{
    using System;
    using System.Globalization;
    using System.Configuration;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Microsoft.ServiceBus.ActiveDirectorySasTokenFunction;

    class Program
    {
        static string aadInstance = "https://login.microsoftonline.com/{0}";
        static string tenant = "[YourAADTenant].onmicrosoft.com"; // AAD tenant address
        static string clientId = "111111111-2222-3333-4444-555555555555"; // client app-id from portal
        static string appKey = "N9SwrNVViVGhHAUjMSCC9s+tHavFZNGA+uE5uYM39wI="; // client app-key from portal
        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        static string serviceBusStsAppId = "111111111-2222-3333-4444-555555555555"; // STS app-id from portal
        static string serviceBusSts = "https://[SB-NAMESPACE-NAME]sts.azurewebsites.net"; // STS endpoint
        
        static void Main(string[] args)
        {

            var tokenProvider = new FederatedTokenProvider(authority, serviceBusSts, serviceBusStsAppId, clientId, appKey);
            var factory = MessagingFactory.Create("sb://[SB-NAMESPACE-NAME].servicebus.windows.net", tokenProvider );
            
            // you have to create this queue first
            var qc = factory.CreateQueueClient("myqueue");
            qc.Send(new BrokeredMessage());
            qc.Close();

            Console.WriteLine("Message sent");
            
        }

    }     
}
