using System;

namespace StGeorgeContactus
{
    public interface IConfigurationManager
    {
        string GetSnsTopic();
    }
    public class ConfigurationManager : IConfigurationManager
    {
        public string GetSnsTopic()
        {
            return Environment.GetEnvironmentVariable("SnsTopic");
        }
    }
}
