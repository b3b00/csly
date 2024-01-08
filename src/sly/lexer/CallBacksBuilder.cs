using System;
using System.Linq;
using System.Reflection;

namespace sly.lexer
{
    public static class CallBacksBuilder
    {

        public static void BuildCallbacks<IN>(GenericLexer<IN> lexer) where IN : struct
        {
            var attributes =
                (CallBacksAttribute[]) typeof(IN).GetCustomAttributes(typeof(CallBacksAttribute), true);
            Type callbackClass = attributes[0].CallBacksClass;
            ExtractCallBacks<IN>(callbackClass,lexer);

        }

        public static void ExtractCallBacks<IN>(Type callbackClass, GenericLexer<IN> lexer) where IN : struct
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

        public static void AddCallback<IN>(GenericLexer<IN> lexer, MethodInfo method, IN token) where IN : struct
        {
            var callbackDelegate = (Func<Token<IN>,Token<IN>>)Delegate.CreateDelegate(typeof(Func<Token<IN>,Token<IN>>), method);
            lexer.AddCallBack(token,callbackDelegate);
        }
    }
}