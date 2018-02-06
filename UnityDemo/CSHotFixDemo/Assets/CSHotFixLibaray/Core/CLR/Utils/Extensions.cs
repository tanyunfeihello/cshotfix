﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSHotFix.CLR.TypeSystem;
using CSHotFix.CLR.Method;
using CSHotFix.Other;
using Mono.Cecil;
using CSHotFix.Runtime.Intepreter;
namespace CSHotFix.CLR.Utils
{
    public delegate TResult Func<T1, T2, T3, T4, T5, TResult>(T1 a1, T2 a2, T3 a3, T4 a4, T5 a5);

    public static class Extensions
    {
        public static List<IType> EmptyParamList = new List<IType>();
        public static List<IType> GetParamList(this MethodReference def, CSHotFix.Runtime.Enviorment.AppDomain appdomain, IType contextType, IMethod contextMethod, IType[] genericArguments)
        {
            if (def.HasParameters)
            {
                List<IType> param = new List<IType>();
                var dt = appdomain.GetType(def.DeclaringType, contextType, contextMethod);
                foreach (var i in def.Parameters)
                {
                    IType t = null;
                    t = appdomain.GetType(i.ParameterType, dt, null);
                    if (t == null && def.IsGenericInstance)
                    {
                        GenericInstanceMethod gim = (GenericInstanceMethod)def;
                        string name = i.ParameterType.IsByReference ? i.ParameterType.GetElementType().FullName : i.ParameterType.FullName;
                        
                        for (int j = 0; j < gim.GenericArguments.Count; j++)
                        {
                            var gp = gim.ElementMethod.GenericParameters[j];
                            var ga = gim.GenericArguments[j];
                            if (name == gp.Name)
                            {
                                t = appdomain.GetType(ga, contextType, contextMethod);
                                if (t == null && genericArguments != null)
                                    t = genericArguments[j];
                                break;
                            }
                            else if (name.Contains(gp.Name))
                            {
                                if (name == gp.Name)
                                {
                                    name = ga.FullName;
                                }
                                else if (name == gp.Name + "[]")
                                {
                                    name = ga.FullName + "[]";
                                }
                                else
                                {
                                    /*name = name.Replace("<" + gp.Name + ">", "<" + ga.FullName + ">");
                                    name = name.Replace("<" + gp.Name + "[", "<" + ga.FullName + "[");
                                    name = name.Replace("<" + gp.Name + ",", "<" + ga.FullName + ",");
                                    name = name.Replace("," + gp.Name + ">", "," + ga.FullName + ">");
                                    name = name.Replace("," + gp.Name + "[", "," + ga.FullName + "[");
                                    name = name.Replace("," + gp.Name + ",", "," + ga.FullName + ",");
                                    name = name.Replace("," + gp.Name + "[", "," + ga.FullName + "[");*/
                                    name = ReplaceGenericArgument(name, gp.Name, ga.FullName);
                                }
                            }
                        }
                        if (t == null)
                            t = appdomain.GetType(name);
                    }

                    param.Add(t);
                }
                return param;
            }
            else
                return EmptyParamList;
        }

        static string ReplaceGenericArgument(string typename, string argumentName, string argumentType)
        {
            string baseType;
            StringBuilder sb = new StringBuilder();
            List<string> ga;
            bool isArray;
            Runtime.Enviorment.AppDomain.ParseGenericType(typename, out baseType, out ga, out isArray);
            if (baseType == argumentName)
                sb.Append(argumentType);
            else
                sb.Append(baseType);
            if (ga != null && ga.Count > 0)
            {
                sb.Append("<");
                bool isFirst = true;
                foreach(var i in ga)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        sb.Append(",");

                    sb.Append(ReplaceGenericArgument(i, argumentName, argumentType));
                }
                sb.Append(">");
            }
            if (isArray)
                sb.Append("[]");
            return sb.ToString();
        }

        [Flags]
        public enum TypeFlags
        {
            Default = 0,
            IsPrimitive = 0x1,
            IsByRef = 0x2,
            IsEnum = 0x4,
            IsDelegate = 0x8,
            IsValueType = 0x10,
        }

        private static readonly Dictionary<Type, TypeFlags> typeFlags = new Dictionary<Type, TypeFlags>(new ByReferenceKeyComparer<Type>());

        public static bool FastIsEnum(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsEnum) != 0;
        }

        public static bool FastIsByRef(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsByRef) != 0;
        }

        public static bool FastIsPrimitive(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsPrimitive) != 0;
        }

        public static bool FastIsValueType(this Type pt)
        {
            return (pt.GetTypeFlags() & TypeFlags.IsValueType) != 0;
        }

        public static TypeFlags GetTypeFlags(this Type pt)
        {
            var result = TypeFlags.Default;

            if (!typeFlags.TryGetValue(pt, out result))
            {
                if (pt.IsPrimitive)
                {
                    result |= TypeFlags.IsPrimitive;
                }

                if (pt == typeof(Delegate) || pt.IsSubclassOf(typeof(Delegate)))
                {
                    result |= TypeFlags.IsDelegate;
                }

                if (pt.IsByRef)
                {
                    result |= TypeFlags.IsByRef;
                }

                if (pt.IsEnum)
                {
                    result |= TypeFlags.IsEnum;
                }

                if (pt.IsValueType)
                {
                    result |= TypeFlags.IsValueType;
                }

                typeFlags[pt] = result;
            }

            return result;
        }

        public static object CheckCLRTypes(this Type pt, object obj)
        {
            if (obj == null)
                return null;

            var typeFlags = GetTypeFlags(pt);

            if ((typeFlags & TypeFlags.IsPrimitive) != 0 && pt != typeof(int))
            {
                if (pt == typeof(bool) && !(obj is bool))
                {
                    obj = (int)obj == 1;
                }
                else if (pt == typeof(byte) && !(obj is byte))
                    obj = (byte)(int)obj;
                else if (pt == typeof(short) && !(obj is short))
                    obj = (short)(int)obj;
                else if (pt == typeof(char) && !(obj is char))
                    obj = (char)(int)obj;
                else if (pt == typeof(ushort) && !(obj is ushort))
                    obj = (ushort)(int)obj;
                else if (pt == typeof(uint) && !(obj is uint))
                    obj = (uint)(int)obj;
                else if (pt == typeof(sbyte) && !(obj is sbyte))
                    obj = (sbyte)(int)obj;
                else if (pt == typeof(ulong) && !(obj is ulong))
                {
                    obj = (ulong)(long)obj;
                }
            }
            else if (obj is CSHotFix.Reflection.CSHotFixWrapperType)
            {
                obj = ((CSHotFix.Reflection.CSHotFixWrapperType)obj).RealType;
            }
            else if ((typeFlags & TypeFlags.IsDelegate) != 0)
            {
                if (obj is Delegate)
                    return obj;
                if (pt == typeof(Delegate))
                    return ((IDelegateAdapter)obj).Delegate;
                return ((IDelegateAdapter)obj).GetConvertor(pt);
            }
            else if ((typeFlags & TypeFlags.IsByRef) != 0)
            {
                return CheckCLRTypes(pt.GetElementType(), obj);
            }
            else if ((typeFlags & TypeFlags.IsEnum) != 0)
            {
                return Enum.ToObject(pt, obj);
            }
            else if (obj is ILTypeInstance)
            {
                var adapter = obj as IDelegateAdapter;

                if (adapter != null && pt != typeof(ILTypeInstance))
                {
                    return adapter.Delegate;
                }

                if (!(obj is ILEnumTypeInstance))
                {
                    var ins = (ILTypeInstance)obj;
                    if (ins.IsValueType)
                        ins = ins.Clone();
                    return ins.CLRInstance;
                }
            }
            return obj;
        }
    }
}
