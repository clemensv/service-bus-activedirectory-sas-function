namespace Microsoft.ServiceBus.ActiveDirectorySasTokenFunction
{
    using System;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class TokenServiceClient
    {
        AuthenticationContext authContext;
        ClientCredential clientCredential;
        readonly string serviceBusSts;
        readonly string serviceBusStsId;

        public TokenServiceClient(string authority, string serviceBusSts, string serviceBusStsId, string clientId, string appKey)
        {
            authContext = new AuthenticationContext(authority);
            clientCredential = new ClientCredential(clientId, appKey);
            this.serviceBusSts = serviceBusSts;
            this.serviceBusStsId = serviceBusStsId;
        }

        

        public async Task<string> GetServiceBusToken(string path, string permission)
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
