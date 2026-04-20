using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Calculis.Core.Calculation
{
    internal static class FunctionManager
    {
        private static readonly IDictionary<string, Assembly> _assemblies = new Dictionary<string, Assembly>();
        internal static IDictionary<string, Type> Functions { get; } = new Dictionary<string, Type>();

        static FunctionManager()
        {
            Register("Calculis.Functions.dll");
        }

        internal static void Register(string assemblyName)
        {
            if (assemblyName == null) throw new ArgumentNullException(nameof(assemblyName));
            if (_assemblies.ContainsKey(assemblyName)) throw new ArgumentException($"Assembly {assemblyName} has already been registered!");

            _assemblies.Add(assemblyName, Assembly.LoadFrom(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, assemblyName)) ??
                        throw new InvalidOperationException($"Assembly {assemblyName} not found! Make sure that it is located in the same directory with the project."));

            var assemblyTypes = _assemblies[assemblyName].GetTypes();
            foreach (var type in assemblyTypes)
            {
                if (Functions.ContainsKey(type.Name.Replace("Function", "")))
                    throw new InvalidOperationException($"Names conflict in function {type.Name}");
            }

            foreach (var type in assemblyTypes)
            {
                if (type.Name.Contains("Function"))
                    Functions.Add(type.Name.Replace("Function", "").ToUpper(), type);
            }
        }

        internal static FunctionBase Create(string name, IList<IValueItem> args)
        {
            if (!Functions.ContainsKey(name))
                throw new ArgumentException($"Function \"{name}\" has not been detected in plugged assemblies!", nameof(name));

            return (FunctionBase)Activator.CreateInstance(Functions[name], args);
        }
    }
}
