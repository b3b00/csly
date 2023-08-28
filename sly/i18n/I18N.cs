using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace sly.i18n
{
    public class I18N
    {
        public readonly IDictionary<string, IDictionary<I18NMessage, string>> Translations;

        private static I18N _instance;

        public static I18N Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new I18N();
                }

                return _instance;
            }
        }

        protected I18N()
        {
            Translations = new Dictionary<string, IDictionary<I18NMessage, string>>();
        }
        
        public string GetText(string lang, I18NMessage key, params string[] args)
        {
            lang = lang ?? CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            IDictionary<I18NMessage, string> translation = new Dictionary<I18NMessage, string>();
            if (!Translations.TryGetValue(lang, out translation))
            {
                translation = Load(lang);
            }

            string pattern = null;
            if (translation.TryGetValue(key, out pattern))
            {
                return string.Format(pattern, args);
            }

            return "";
        }


        private IDictionary<I18NMessage,string> Load(string lang)
        {
            var translation = new Dictionary<I18NMessage, string>();
            Assembly assembly = GetType().Assembly;
            var res = assembly.GetManifestResourceNames();
            using (var stream = assembly.GetManifestResourceStream($"sly.i18n.translations.{lang}.txt"))
            {
                if (stream != null)
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            if (!line.StartsWith("#"))
                            {
                                var items = line.Split(new[] {'='});
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