
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

        //
        // The Client ID is used by the application to uniquely identify itself to Azure AD.
        // The App Key is a credential used by the application to authenticate to Azure AD.
        // The Tenant is the name of the Azure AD tenant in which this application is registered.
        // The AAD Instance is the instance of Azure, for example public Azure or Azure China.
        // The Authority is the sign-in URL of the tenant.
        //
        private static string aadInstance = ConfigurationManager.AppSettings["ida:AADInstance"];
        private static string tenant = ConfigurationManager.AppSettings["ida:Tenant"];
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string appKey = ConfigurationManager.AppSettings["ida:AppKey"];

        static string authority = String.Format(CultureInfo.InvariantCulture, aadInstance, tenant);

        //
        // To authenticate to the To Do list service, the client needs to know the service's App ID URI.
        // To contact the To Do list service we need it's URL as well.
        //
        private static string serviceBusStsId = ConfigurationManager.AppSettings["ServiceBusStsId"];
        private static string serviceBusSts = ConfigurationManager.AppSettings["ServiceBusSts"];

        static void Main(string[] args)
        {

            var tsc = new TokenServiceClient(authority, serviceBusSts, serviceBusStsId, clientId, appKey);
            string token = tsc.GetServiceBusToken("/", "send").GetAwaiter().GetResult();
            if (token != null)
            {
                var sbc = ServiceBusConnectionStringBuilder.CreateUsingSharedAccessSignature(new Uri("sb://clemensv102.servicebus.windows.net"), "myqueue", "me", token);
                var qc = QueueClient.CreateFromConnectionString(sbc);
                qc.Send(new BrokeredMessage());
                qc.Close();

                Console.WriteLine("Message sent");
            }
        }

    }     
}
