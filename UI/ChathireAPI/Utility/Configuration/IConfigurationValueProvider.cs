using Microsoft.Extensions.Configuration;
using System;

namespace Utility.Configuration
{

    public interface IConfigurationValueProvider
    {
        String GetValue(String Key);

        bool TryGetValue(String Key, out String Value);
    }

    public class AppConfigValueProvider : IConfigurationValueProvider
    {
        private IConfigurationSection _configurationSection;

        public AppConfigValueProvider(IConfigurationSection configurationSection)
        {
            _configurationSection = configurationSection;
        }
        public string GetValue(string Key)
        {
            return _configurationSection[Key];
        }

        public bool TryGetValue(string Key, out string Value)
        {
            Value = null;
            try
            {
                Value = _configurationSection[Key];
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
    public class AzureConfigValueProvider : IConfigurationValueProvider
    {
        public string GetValue(string Key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string Key, out string Value)
        {
            throw new NotImplementedException();
        }
    }
    public class CustomConfigurationProvider
    {
        public static IConfigurationValueProvider GetConfigurationSection(IConfiguration appConfig, string SectionName = null)
        {
            if (!String.IsNullOrWhiteSpace(SectionName)
                || !String.IsNullOrWhiteSpace(appConfig[SectionName])
                )
            {
                if(String.IsNullOrWhiteSpace(appConfig[SectionName + ":Source"]))
                {
                    return new AppConfigValueProvider(appConfig.GetSection(SectionName));
                }
                if (appConfig[SectionName + ":Source"].Equals("appConfig", StringComparison.OrdinalIgnoreCase))
                    return new AppConfigValueProvider(appConfig.GetSection(appConfig[SectionName]));
                else if (appConfig[SectionName + ":Source"].Equals("Azure", StringComparison.OrdinalIgnoreCase))
                {
                    return new AzureConfigValueProvider();
                }
            }
            
            return null;
        }


    }
}
