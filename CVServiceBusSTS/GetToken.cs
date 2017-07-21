
// Request body format:
// - `Path` - *required*. Name of container in storage account
// - `Permission` - *optional*. Default value is read permissions. The format matches the enum values of SharedAccessBlobPermissions. 
//    Possible values are "Send", "Listen", "Manage", "SendListen". Comma-separate multiple permissions, such as "Read, Write, Create".

namespace Microsoft.ServiceBus.ActiveDirectorySasTokenFunction
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.ServiceBus;
    using System.Threading.Tasks;
    using System.Security.Claims;


    public static class GetToken
    {
        [FunctionName("GetToken")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] TokenRequestData input, TraceWriter log)
        {
            // pick up config
            var serviceBusNamespace = GetSetting("ServiceBusNamespace");
            if (serviceBusNamespace == null)
            {
                log.Error(@"ServiceBusNamespace not configured");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Bad internal configuration") };
            }
            int serviceBusTokenTimeout;
            if (!int.TryParse(GetSetting("ServiceBusTokenTimeoutSecs"), out serviceBusTokenTimeout))
            {
                 serviceBusTokenTimeout = (int)TimeSpan.FromHours(8).TotalSeconds;
            }

            // check the given inputs
            var path = input.Path;
            if (string.IsNullOrWhiteSpace(input.Permission))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest) { Content = new StringContent("Missing permission and/or path") };
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                path = "/";
            }
            // get the key config for the desired permission
            string requestedPermission = input.Permission.ToLowerInvariant();

            if (await IsCallerAuthorizedAsync(serviceBusNamespace, path, requestedPermission, ClaimsPrincipal.Current))
            {
                var permissionRule = GetPermissionRule(requestedPermission);
                if (permissionRule == null)
                {
                    log.Error($"Permission rule {requestedPermission} is invalid");
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError) { Content = new StringContent("Bad internal configuration") };
                }

                // issue the token
                var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(permissionRule.Item1, permissionRule.Item2);
                UriBuilder entityUri = new UriBuilder("http", serviceBusNamespace, -1, input.Path);
                var token = await tokenProvider.GetWebTokenAsync(entityUri.ToString(), requestedPermission, false, TimeSpan.FromMinutes(10));

                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(token) };
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized);
            }
        }

        static async Task<bool> IsCallerAuthorizedAsync(string serviceBusNamespace, string path, string requestedPermission, ClaimsPrincipal principal)
        {
            return principal != null;
        }

        private static Tuple<string, string> GetPermissionRule(string requestedPermission)
        {
            string permissionRule = null;
            switch (requestedPermission)
            {
                case "send": permissionRule = GetSetting("ServiceBusSend"); break;
                case "listen": permissionRule = GetSetting("ServiceBusListen"); break;
                case "sendlisten": permissionRule = GetSetting("ServiceBusSendListen"); break;
                case "manage": permissionRule = GetSetting("ServiceBusManage"); break;
            }
            var permissionRuleKeyValue = permissionRule.Split(':');
            if (permissionRuleKeyValue.Length != 2)
            {
                return null;
            }
            return new Tuple<string, string>(permissionRuleKeyValue[0].Trim(), permissionRuleKeyValue[1].Trim());
        }

        public static string GetSetting(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }
        public class TokenRequestData
        {
            public string Path { get; set; }
            public string Permission { get; set; }
        }
    }
}