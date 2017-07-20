using System;
using System.Threading.Tasks;

// The following using statements were added for this sample.
using System.Globalization;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Configuration;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace STSClient
{
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
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(clientId, appKey);

            string token = GetServiceBusToken(authContext, clientCredential, "/", "send").GetAwaiter().GetResult();
            if (token != null)
            {
                var sbc = ServiceBusConnectionStringBuilder.CreateUsingSharedAccessSignature(new Uri("sb://clemensv8.servicebus.windows.net"), "myqueue", "me", token);
                var qc = QueueClient.CreateFromConnectionString(sbc);
                qc.Send(new BrokeredMessage());
                qc.Close();

                Console.WriteLine("Message sent");
            }
        }

        static async Task<string> GetServiceBusToken(AuthenticationContext authContext,ClientCredential clientCredential, string path, string permission)
        {
            //
            // Get an access token from Azure AD using client credentials.
            // If the attempt to get a token fails because the server is unavailable, retry twice after 3 seconds each.
            //

            HttpClient httpClient = new HttpClient();
            AuthenticationResult aadAuthenticationResult = null;
            int retryCount = 0;
            bool retry = false;

            do
            {
                retry = false;
                try
                {
                    // ADAL includes an in memory cache, so this call will only send a message to the server if the cached token is expired.
                    aadAuthenticationResult = await authContext.AcquireTokenAsync(serviceBusStsId, clientCredential);
                }
                catch (AdalException ex)
                {
                    if (ex.ErrorCode == "temporarily_unavailable")
                    {
                        retry = true;
                        retryCount++;
                        await Task.Delay(3000);
                    }
                }
            }
            while ((retry == true) && (retryCount < 3));

            if (aadAuthenticationResult != null)
            {
                // Add the access token to the authorization header of the request.
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", aadAuthenticationResult.AccessToken);

                // Forms encode To Do item and POST to the todo list web api.
                HttpContent content = new StringContent(string.Format("{{\"Path\": \"{0}\",\"Permission\":\"{1}\" }}", path, permission));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceBusSts + "/api/gettoken", content);

                if (response.IsSuccessStatusCode == true)
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            return null;
        }

    }
}
