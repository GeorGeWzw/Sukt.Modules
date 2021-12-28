﻿using Microsoft.Extensions.DependencyModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Sukt.Module.Core.SuktReflection
{
    public static class AssemblyHelper
    {
        private static Assembly[] GetAllAssemblies()
        {
            string[] filters =
             {
                "mscorlib",
                "netstandard",
                "dotnet",
                "api-ms-win-core",
                "runtime.",
                "System",
                "Microsoft",
                "Window",
            };

            DependencyContext context = DependencyContext.Default;
            List<string> names = new List<string>();
            foreach (CompilationLibrary library in context.CompileLibraries)
            {
                string name = library.Name;
                if (filters.Any(name.StartsWith))
                {
                    continue;
                }

                if (!names.Contains(name))
                {
                    names.Add(name);
                }
            }
            return LoadFiles(names);
        }

        private static Assembly[] LoadFiles(IEnumerable<string> files)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (string file in files)
            {
                AssemblyName name = new AssemblyName(file);
                try
                {
                    assemblies.Add(Assembly.Load(name));
                }
                catch (FileNotFoundException)
                { }
            }
            return assemblies.ToArray();
        }
        //private static Assembly[] LoadFiles(IEnumerable<string> files)
        //{
        //    List<Assembly> assemblies = new List<Assembly>();
        //    foreach (string file in files)
        //    {
        //        AssemblyName name = new AssemblyName(file);
        //        try
        //        {
        //            assemblies.Add(Assembly.Load(name));
        //        }
        //        catch (FileNotFoundException)
        //        { }
        //    }
        //    return assemblies.ToArray();
        //}
        /// <summary>
        /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
        /// </summary>
        /// <returns></returns>
        //private static IList<Assembly> GetAllAssemblies()
        //{
        //    string[] filters =
        //    {
        //        "mscorlib",
        //        "netstandard",
        //        "dotnet",
        //        "api-ms-win-core",
        //        "runtime.",
        //        "System",
        //        "Microsoft",
        //        "Window",
        //    };
        //    IEnumerable<Assembly> allAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies().Where(x => !filters.Any(x.Name.StartsWith)).Select(Assembly.Load).ToArray();
        //    List<Assembly> list = new List<Assembly>();
        //    var deps = DependencyContext.Default;
        //    //排除所有的系统程序集、Nuget下载包
        //    var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package" && !filters.Any(lib.Name.StartsWith));
        //    try
        //    {
        //        foreach (var lib in libs)
        //        {
        //            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
        //            list.Add(assembly);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    list.AddRange(allAssemblies);
        //    return list.Distinct().ToList();
        //}

        public static Assembly[] FindAllItems()
        {
            return GetAllAssemblies().ToArray();
        }
        /// <summary>
        /// 根据程序集名字得到程序集
        /// </summary>
        /// <param name="assemblyNames"></param>
        /// <returns></returns>

        public static IEnumerable<Assembly> GetAssembliesByName(params string[] assemblyNames)
        {
            var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath; //获取项目路径
            return assemblyNames.Select(o => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(basePath, $"{o}.dll")));
        }
    }
    //public static class AssemblyHelper
    //{
    //    /// <summary>
    //    /// 获取项目程序集，排除所有的系统程序集(Microsoft.***、System.***等)、Nuget下载包
    //    /// </summary>
    //    /// <returns></returns>
    //    private static IList<Assembly> GetAllAssemblies()
    //    {
    //        string[] filters =
    //        {
    //            "mscorlib",
    //            "netstandard",
    //            "dotnet",
    //            "api-ms-win-core",
    //            "runtime.",
    //            "System",
    //            "Microsoft",
    //            "Window",
    //        };
    //        IEnumerable<Assembly> allAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies().Where(x => !filters.Any(x.Name.StartsWith)).Select(Assembly.Load).ToArray();
    //        List<Assembly> list = new List<Assembly>();
    //        var deps = DependencyContext.Default;
    //        //排除所有的系统程序集、Nuget下载包
    //        var libs = deps.CompileLibraries.Where(lib => !lib.Serviceable && lib.Type != "package" && !filters.Any(lib.Name.StartsWith));
    //        try
    //        {
    //            foreach (var lib in libs)
    //            {
    //                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(lib.Name));
    //                list.Add(assembly);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            throw ex;
    //        }
    //        list.AddRange(allAssemblies);
    //        return list.Distinct().ToList();
    //    }

    //    public static Assembly[] FindAllItems()
    //    {
    //        return GetAllAssemblies().ToArray();
    //    }

    //    /// <summary>
    //    /// 根据程序集名字得到程序集
    //    /// </summary>
    //    /// <param name="assemblyNames"></param>
    //    /// <returns></returns>

    //    public static IEnumerable<Assembly> GetAssembliesByName(params string[] assemblyNames)
    //    {
    //        var basePath = Microsoft.DotNet.PlatformAbstractions.ApplicationEnvironment.ApplicationBasePath; //获取项目路径
    //        return assemblyNames.Select(o => AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(basePath, $"{o}.dll")));
    //    }
    //}
}