using System;
using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Utility {
    public class LinkedRegistry<TEnum, TValue> where TEnum : Enum {

        private readonly EnumRegistry<TEnum> enumRegistry;
        private readonly Dictionary<string, TValue> namesAndValues = [];
        private readonly Dictionary<TValue, string> valuesAndNames = [];
        private readonly Action<string> RegisteredEvent;

        public LinkedRegistry() {
            enumRegistry = EnumManager.Register<TEnum>();
        }

        public LinkedRegistry(Action<string> registeredEvent) {
            enumRegistry = EnumManager.Register<TEnum>();
            this.RegisteredEvent = registeredEvent;
        }

        public void Register(string name, TValue value) {
            if (!namesAndValues.ContainsKey(name) && enumRegistry != null) {
                enumRegistry.Register(name);
                namesAndValues[name] = value;
                valuesAndNames[value] = name;

                RegisteredEvent?.Invoke(name);
            }
        }

        public TValue Get(string name) {
            if (namesAndValues.ContainsKey(name))
                return namesAndValues[name];

            return default;
        }

        public string GetName(TValue value) {
            if (valuesAndNames.ContainsKey(value))
                return valuesAndNames[value];

            return null;
        }

        public List<TValue> GetValues() {
            return namesAndValues.Values.ToList();
        }

        public List<string> GetNames() {
            return namesAndValues.Keys.ToList();
        }
    }
}