#if !OS_WINDOWS
using System;
using System.IO;
using System.Security.Cryptography;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;

namespace Microsoft.VisualStudio.Services.Agent.Listener.Configuration
{
    public class RSAFileKeyManager : IRSAKeyManager, IAgentService
    {
        private string _keyFile;
        private JsonSerializer _serializer;

        public RSA CreateKey(String keyContainerName)
        {
            RSA rsa = null;
            if (!File.Exists(_keyFile))
            {
                rsa = RSA.Create();
                rsa.KeySize = 2048;
                File.WriteAllBytes(_keyFile, JsonUtility.Serialize(rsa.ExportParameters(true), false));
            }
            else
            {
                rsa = RSA.Create();
                rsa.ImportParameters(JsonUtility.Deserialize<RSAParameters>(File.ReadAllBytes(_keyFile)));
            }

            return rsa;
        }

        public void DeleteKey(String keyContainerName)
        {
            if (File.Exists(_keyFile))
            {
                File.Delete(_keyFile);
            }
        }

        public RSA GetKey(String keyContainerName)
        {
            if (!File.Exists(_keyFile))
            {
                throw new CryptographicException("Key file was not found");
            }

            var parameters = JsonUtility.Deserialize<RSAParameters>(File.ReadAllBytes(_keyFile));
            var rsa = RSA.Create();
            rsa.ImportParameters(parameters);
            return rsa;
        }

        void IAgentService.Initialize(IHostContext context)
        {
            _keyFile = Path.Combine(IOUtil.GetRootPath(), ".privatekey");
            _serializer = new VssJsonMediaTypeFormatter().CreateJsonSerializer();
        }
    }
}
#endif
