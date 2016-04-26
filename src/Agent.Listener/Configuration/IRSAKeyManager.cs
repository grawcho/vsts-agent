using System;
using System.Security.Cryptography;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
#if OS_WINDOWS
    [ServiceLocator(Default = typeof(RSACspKeyManager))]
#else
    [ServiceLocator(Default = typeof(RSAFileKeyManager))]
#endif
    public interface IRSAKeyManager : IAgentService
    {
        RSA CreateKey(String keyContainerName);

        void DeleteKey(String keyContainerName);

        RSA GetKey(String keyContainerName);
    }
}
