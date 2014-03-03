using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.CSharp.RuntimeBinder;

namespace ZenXml.Core
{
    /// <summary>
    /// Code borrowed from here: http://stackoverflow.com/questions/5492373/get-generic-type-of-call-to-method-in-dynamic-object
    /// </summary>
    public static class FrameworkTools
    {
// ReSharper disable InconsistentNaming
        private static readonly bool _isMono = Type.GetType("Mono.Runtime") != null;

// ReSharper restore InconsistentNaming

        private static readonly Lazy<Func<InvokeMemberBinder, IList<Type>>> FrameworkTypeArgumentsGetter =
            new Lazy<Func<InvokeMemberBinder, IList<Type>>>(CreateTypeArgumentsGetter);

        /// <summary>Gets a value indicating whether application is running under mono runtime.</summary>
        public static bool IsMono
        {
            get { return _isMono; }
        }

        private static Func<InvokeMemberBinder, IList<Type>> CreateTypeArgumentsGetter()
        {
            if(IsMono)
            {
                var binderType = typeof(RuntimeBinderException).Assembly.GetType("Microsoft.CSharp.RuntimeBinder.CSharpInvokeMemberBinder");

                if(binderType == null)
                    return null;

                var param = Expression.Parameter(typeof(InvokeMemberBinder), "o");

                return
                    Expression.Lambda<Func<InvokeMemberBinder, IList<Type>>>(
                        Expression.TypeAs(Expression.Field(Expression.TypeAs(param, binderType), "typeArguments"), typeof(IList<Type>)), param).Compile();
            }

            var inter = typeof(RuntimeBinderException).Assembly.GetType("Microsoft.CSharp.RuntimeBinder.ICSharpInvokeOrInvokeMemberBinder");

            if(inter == null)
                return null;

            var property = inter.GetProperty("TypeArguments");

            if(!property.CanRead)
                return null;

            var objParm = Expression.Parameter(typeof(InvokeMemberBinder), "o");

            return
                Expression.Lambda<Func<InvokeMemberBinder, IList<Type>>>(
                    Expression.TypeAs(Expression.Property(Expression.TypeAs(objParm, inter), property.Name), typeof(IList<Type>)), objParm).Compile();
        }

        /// <summary>Extension method allowing to easyly extract generic type arguments from <see cref="InvokeMemberBinder"/>.</summary>
        /// <param name="binder">Binder from which get type arguments.</param>
        /// <returns>List of types passed as generic parameters.</returns>
        public static IList<Type> GetGenericTypeArguments(this InvokeMemberBinder binder)
        {
            return FrameworkTypeArgumentsGetter.Value(binder);
        }
    }
}