using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace sly.lexer
{
    public static class CallBacksBuilder
    {

        public static void BuildCallbacks<IN>(GenericLexer<IN> lexer) where IN : struct, Enum
        {
            var attributes =
                (CallBacksAttribute[]) typeof(IN).GetCustomAttributes(typeof(CallBacksAttribute), true);
            if (attributes.Length > 0)
            {
                Type callbackClass = attributes[0].CallBacksClass;
                ExtractCallBacks<IN>(callbackClass, lexer);
            }

        }

        public static void ExtractCallBacks<IN>(Type callbackClass, GenericLexer<IN> lexer) where IN : struct, Enum
        {
            var methods = callbackClass.GetMethods().ToList();
            methods = methods.Where<MethodInfo>(m =>
            {
                var attributes = m.GetCustomAttributes().ToList();
                var attr = attributes.Find(a => a.GetType() == typeof(TokenCallbackAttribute));
                return m.IsStatic && attr != null;
            }).ToList<MethodInfo>();

            foreach (var method in methods)
            {
                var attributes = method.GetCustomAttributes(typeof(TokenCallbackAttribute), false).Cast<TokenCallbackAttribute>().ToList();
                AddCallback<IN>(lexer, method, EnumConverter.ConvertIntToEnum<IN>(attributes[0].EnumValue));
            }
        }

        public static void AddCallback<IN>(GenericLexer<IN> lexer, MethodInfo method, IN token) where IN : struct, Enum
        {
            var callbackDelegate = (Func<Token<IN>,Token<IN>>)Delegate.CreateDelegate(typeof(Func<Token<IN>,Token<IN>>), method);
            lexer.AddCallBack(token,callbackDelegate);
        }
        
        public static List<(IN tokenId,Func<Token<IN>, Token<IN>> callback)> GetCallbacks<IN>(GenericLexer<IN> lexer) where IN : struct, Enum
        {
            List<(IN,Func<Token<IN>, Token<IN>>)> callbacks = new List<(IN,Func<Token<IN>, Token<IN>>)>();
            var classAttributes =
                (CallBacksAttribute[]) typeof(IN).GetCustomAttributes(typeof(CallBacksAttribute), true);
            if (classAttributes.Any())
            {
                Type callbackClass = classAttributes[0].CallBacksClass;
                if (callbackClass != null)
                {

                    var methods = callbackClass.GetMethods().ToList();
                    methods = methods.Where<MethodInfo>(m =>
                    {
                        var attributes = m.GetCustomAttributes().ToList();
                        var attr = attributes.Find(a => a.GetType() == typeof(TokenCallbackAttribute));
                        return m.IsStatic && attr != null;
                    }).ToList<MethodInfo>();

                    foreach (var method in methods)
                    {
                        var attributes = method.GetCustomAttributes(typeof(TokenCallbackAttribute), false)
                            .Cast<TokenCallbackAttribute>().ToList();

                        foreach (var attr in attributes)
                        {
                            IN tokenId = EnumConverter.ConvertIntToEnum<IN>(attr.EnumValue);
                            var callback = (Func<Token<IN>,Token<IN>>)Delegate.CreateDelegate(typeof(Func<Token<IN>,Token<IN>>), method);
                            callbacks.Add((tokenId,callback));
                        }
                    }
                }
            }

            return callbacks;
        }
    }
}