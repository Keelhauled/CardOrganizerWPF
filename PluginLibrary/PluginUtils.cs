using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PluginLibrary
{
    public static class PluginUtils
    {
        public static object InvokeStaticMethod(string typeName, string methodName, object[] parameters = null, Type[] paramTypes = null)
        {
            return InvokeStaticMethod<object>(typeName, methodName, parameters, paramTypes);
        }

        public static T InvokeStaticMethod<T>(string typeName, string methodName, object[] parameters, Type[] paramTypes = null) where T : class
        {
            if(parameters.Contains(null) && paramTypes == null)
            {
                Console.WriteLine("A parameter can't be null if paramTypes is not defined");
                return null;
            }

            var type = FindType(typeName);

            if(type != null)
            {
                parameters = parameters ?? new object[0];
                if(paramTypes == null) parameters.Select(x => x.GetType()).ToArray();

                var bindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
                var methodInfo = type.GetMethod(methodName, bindingFlags, null, paramTypes, null);

                if(methodInfo != null)
                {
                    if(methodInfo.GetParameters().Length == 0)
                        return methodInfo.Invoke(null, null) as T;
                    else
                        return methodInfo.Invoke(null, parameters) as T;
                }
                else
                {
                    Console.WriteLine($"Method {typeName}.{methodName} not found");
                }
            }
            else
            {
                Console.WriteLine($"Type {typeName} not found");
            }

            return null;
        }

        public static object InvokePluginMethod(object instance, string methodName, params object[] parameters)
        {
            if(instance != null)
            {
                parameters = parameters ?? new object[0];
                var paramTypes = parameters.Select(x => x.GetType()).ToArray();
                var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                var methodInfo = instance.GetType().GetMethod(methodName, bindingFlags, null, paramTypes, null);

                if(methodInfo != null)
                {
                    if(methodInfo.GetParameters().Length == 0)
                        return methodInfo.Invoke(instance, null);
                    else
                        return methodInfo.Invoke(instance, parameters);
                }
                else
                {
                    Console.WriteLine($"Method {instance.GetType()}.{methodName} not found");
                }
            }
            else
            {
                Console.WriteLine($"Instance of type {instance.GetType()} is null");
            }

            return null;
        }

        public static object InvokePluginMethod(string typeName, string methodName, params object[] parameters)
        {
            var type = FindType(typeName);

            if(type != null)
            {
                var instance = GameObject.FindObjectOfType(type);
                if(instance)
                {
                    parameters = parameters ?? new object[0];
                    var paramTypes = parameters.Select(x => x.GetType()).ToArray();
                    var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
                    var methodInfo = type.GetMethod(methodName, bindingFlags, null, paramTypes, null);

                    if(methodInfo != null)
                    {
                        if(methodInfo.GetParameters().Length == 0)
                            return methodInfo.Invoke(instance, null);
                        else
                            return methodInfo.Invoke(instance, parameters);
                    }
                    else
                    {
                        Console.WriteLine($"Method {typeName}.{methodName} not found");
                    }
                }
                else
                {
                    Console.WriteLine($"Instance of {typeName} not found");
                }
            }
            else
            {
                Console.WriteLine($"Type {typeName} not found");
            }

            return null;
        }

        public static Type FindType(string qualifiedTypeName)
        {
            var t = Type.GetType(qualifiedTypeName);

            if(t != null)
            {
                return t;
            }
            else
            {
                foreach(var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = ass.GetType(qualifiedTypeName);
                    if(t != null)
                    {
                        return t;
                    }
                }

                return null;
            }
        }

        public static Type FindTypeIPlugin(string qualifiedTypeName)
        {
            var t = Type.GetType(qualifiedTypeName);

            if(t != null)
            {
                return t;
            }
            else
            {
                // Requires IPA
                //foreach(Assembly asm in PluginManager.Plugins.Select(x => x.GetType().Assembly))
                //{
                //    t = asm.GetType(qualifiedTypeName);
                //    if(t != null)
                //    {
                //        //Console.WriteLine("{0} belongs to an IPlugin", qualifiedTypeName);
                //        return t;
                //    }
                //}

                foreach(var ass in AppDomain.CurrentDomain.GetAssemblies())
                {
                    t = ass.GetType(qualifiedTypeName);
                    if(t != null)
                    {
                        return t;
                    }
                }

                return null;
            }
        }
    }
}
