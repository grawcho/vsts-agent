#if OS_WINDOWS
using System;
using System.Security.Cryptography;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
    public class RSACspKeyManager : IRSAKeyManager
    {
        public RSA CreateKey(String keyContainerName)
        {
            var cspParameters = new CspParameters
            {
                Flags = CspProviderFlags.UseMachineKeyStore | CspProviderFlags.UseNonExportableKey,
                KeyContainerName = keyContainerName,
            };

            return new RSACryptoServiceProvider(2048, cspParameters);
        }

        public void DeleteKey(String keyContainerName)
        {
            var cspParameters = new CspParameters
            {
                Flags = CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = keyContainerName,
            };

            using (var rsa = new RSACryptoServiceProvider(cspParameters))
            {
                rsa.PersistKeyInCsp = false;
            }
        }

        public RSA GetKey(String keyContainerName)
        {
            var cspParameters = new CspParameters
            {
                Flags = CspProviderFlags.UseExistingKey | CspProviderFlags.UseMachineKeyStore,
                KeyContainerName = keyContainerName,
            };

            return new RSACryptoServiceProvider(cspParameters);
        }

        void IAgentService.Initialize(IHostContext context)
        { 
            // Nothing to do for the CSP implementation
        }
    }
}
#endif
