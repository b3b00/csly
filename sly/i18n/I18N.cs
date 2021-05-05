using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace sly.i18n
{

    public enum Message
    {
        UnexpectedTokenExpecting,
        UnexpectedEosExpecting,
        UnexpectedToken,
        UnexpectedEos,
        UnexpectedChar,
        
        
        MissingOperand,
        ReferenceNotFound,
        MixedChoices,
        NonTerminalChoiceCannotBeDiscarded,
        IncorrectVisitorReturnType,
        IncorrectVisitorParameterType,
        IncorrectVisitorParameterNumber,
        LeftRecursion,
        NonTerminalNeverUsed
    }
    
    public class I18N
    {
        public static IDictionary<string, IDictionary<Message, string>> Translations;

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
            Translations = new Dictionary<string, IDictionary<Message, string>>();
        }
        
        
        
        public string GetText(Message key, params string[] args)
        {
            var lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return GetText(lang,key,args);
        }
        
        public string GetText(string lang, Message key, params string[] args)
        {
            IDictionary<Message, string> translation = new Dictionary<Message, string>();
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


        private IDictionary<Message,string> Load(string lang)
        {
            var translation = new Dictionary<Message, string>();
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
                                    var key = EnumConverter.ConvertStringToEnum<Message>(items[0]);
                                    translation[key] = items[1];
                                }
                            }
                            line = reader.ReadLine();
                        }
                    }
                }
            }

            Translations[lang] = translation;
            return translation;
        }
        
        
    }
}