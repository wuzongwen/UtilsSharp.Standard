﻿using Castle.DynamicProxy;
using LoggerHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using static UtilsSharp.Standard.BaseMsg;

namespace AspNetCore.Interceptor
{
    /// <summary>
    /// 日志拦截器
    /// </summary>
    public class LoggerInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                var returnType = invocation.Method.ReturnType;
                var setErrorMethod = returnType.GetMethod("SetError", new Type[] { typeof(string), typeof(int) });
                var result = Activator.CreateInstance(returnType, true);
                string methodName = invocation.InvocationTarget.ToString() + "." + invocation.Method.Name;
                List<string> args = new List<string>();
                if (invocation.Arguments != null && invocation.Arguments.Length > 0)
                {
                    foreach (object item in invocation.Arguments)
                    {
                        if (item != null)
                        {
                            args.Add(JsonConvert.SerializeObject(item));
                        }
                    }
                }
                var param = string.Join(",", args);
                if (setErrorMethod == null)
                {
                    Logger.Error($"{methodName} 异常", ex, param: $"{param}", func: $"{methodName}");
                    invocation.ReturnValue = result;
                    return;
                }
                var errorCode = Logger.Error($"{methodName} 异常", ex, param: $"{param}", func: $"{methodName}");
                setErrorMethod.Invoke(result, new object[] { errorCode.ToMsgException(), 5000 });
                invocation.ReturnValue = result;
            }
        }
    }
}
