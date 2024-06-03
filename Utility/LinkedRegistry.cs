using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Blender.Utility;

public class LinkedRegistry<TEnum, TValue> where TEnum : Enum
{

    private readonly EnumRegistry<TEnum> enumRegistry;
    private readonly Dictionary<string, TValue> namesAndValues = [];
    private readonly Dictionary<TValue, string> valuesAndNames = [];
    private readonly Action<string, string, TValue> registeredEvent;

    public LinkedRegistry()
    {
        this.enumRegistry = EnumManager.Register<TEnum>();
    }

    public LinkedRegistry(Action<string, string, TValue> registeredEvent)
    {
        this.enumRegistry = EnumManager.Register<TEnum>();
        this.registeredEvent = registeredEvent;
    }

    public void Register(string name, TValue value)
    {
        if (!namesAndValues.ContainsKey(name) && enumRegistry != null)
        {
            enumRegistry.Register(name);
            namesAndValues[name] = value;
            valuesAndNames[value] = name;

            string modName = Path.GetFileName
                (Path.GetDirectoryName(Assembly.GetCallingAssembly().Location));
            registeredEvent?.Invoke(name, modName, value);
        }
    }

    public TValue GetValue(string name)
    {
        if (namesAndValues.ContainsKey(name))
            return namesAndValues[name];
        return default;
    }

    public string GetName(TValue value)
    {
        if (valuesAndNames.ContainsKey(value))
            return valuesAndNames[value];
        return null;
    }

    public bool ContainsName(string name)
    {
        return namesAndValues.ContainsKey(name);
    }

    public List<TValue> GetValues()
    {
        return namesAndValues.Values.ToList();
    }

    public List<string> GetNames()
    {
        return namesAndValues.Keys.ToList();
    }
}