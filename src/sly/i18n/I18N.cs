using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace sly.i18n
{
    public class I18N
    {
        private readonly IDictionary<string, IDictionary<I18NMessage, string>> Translations;

        private static I18N _instance;

        public static I18N Instance => _instance ?? (_instance = new I18N());

        private I18N()
        {
            Translations = new Dictionary<string, IDictionary<I18NMessage, string>>();
        }
        
        public string GetText(string lang, I18NMessage key, params string[] args)
        {
            lang = lang ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            if (!Translations.TryGetValue(lang, out var translation))
            {
                translation = Load(lang);
            }

            
            return translation.TryGetValue(key, out var pattern) ? string.Format(pattern, args) : "";
        }


        private IDictionary<I18NMessage,string> Load(string lang)
        {
            var translation = new Dictionary<I18NMessage, string>();
            var assembly = GetType().Assembly;
            using (var stream = assembly.GetManifestResourceStream($"sly.i18n.translations.{lang}.txt"))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var line = reader.ReadLine();
                        while (line != null)
                        {
                            if (!line.StartsWith("#"))
                            {
                                var items = line.Split('=');
                                if (items.Length == 2)
                                {
                                    var key = EnumConverter.ConvertStringToEnum<I18NMessage>(items[0]);
                                    translation[key] = items[1];
                                }
                            }

                            line = reader.ReadLine();
                        }
                    }
                }
                else
                {
                    return Load("en");
                }
            }

            Translations[lang] = translation;
            return translation;
        }
        
        
    }
}