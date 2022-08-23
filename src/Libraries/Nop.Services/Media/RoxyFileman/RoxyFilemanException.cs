using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;

namespace Nop.Services.Media.RoxyFileman
{
    public class RoxyFilemanException : Exception
    {
        private static Dictionary<string, string> _languageResources;

        private RoxyFilemanException() : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">Language resource key</param>
        /// <returns></returns>
        public RoxyFilemanException(string key) : base(GetLocalizedMessage(key))
        {

        }

        public RoxyFilemanException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Get the language resource value
        /// </summary>
        /// <param name="key">Language resource key</param>
        /// <returns>
        /// The language resource value
        /// </returns>
        protected static string GetLocalizedMessage(string key)
        {
            var fileProvider = EngineContext.Current.Resolve<INopFileProvider>();

            var roxyConfig = Singleton<RoxyFilemanConfig>.Instance;
            var languageFile = fileProvider.GetAbsolutePath($"{NopRoxyFilemanDefaults.LanguageDirectory}/{roxyConfig.LANG}.json");

            if (!fileProvider.FileExists(languageFile))
                languageFile = fileProvider.GetAbsolutePath($"{NopRoxyFilemanDefaults.LanguageDirectory}/en.json");

            if (_languageResources is null)
            {
                var json = fileProvider.ReadAllTextAsync(languageFile, Encoding.UTF8).Result;
                _languageResources = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            }

            if (_languageResources is null)
                return key;

            if (_languageResources.TryGetValue(key, out var value))
                return value;

            return key;
        }
    }
}