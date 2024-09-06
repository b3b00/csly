using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace sly.lexer
{
    public static class CallBacksBuilder
    {

        public static void BuildCallbacks<IN>(GenericLexer<IN> lexer) where IN : struct
        {
            var callbacks = GetCallbacks(lexer);
            foreach (var callback in callbacks)
            {
                lexer.AddCallBack(callback.tokenId, callback.callback);
            }
        }

        public static List<(IN tokenId,Func<Token<IN>, Token<IN>> callback)> GetCallbacks<IN>(GenericLexer<IN> lexer) where IN : struct
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