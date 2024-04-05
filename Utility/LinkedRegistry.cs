using System;
using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Utility {
    public class LinkedRegistry<TEnum, TValue> where TEnum : Enum {

        public readonly EnumRegistry<TEnum> EnumRegistry;
        private readonly Dictionary<string, TValue> namesAndValues = [];

        public LinkedRegistry() {
            EnumRegistry = EnumManager.Register<TEnum>();
        }

        public void Register(string name, TValue value) {
            if (!namesAndValues.ContainsKey(name) && EnumRegistry != null) {
                EnumRegistry.Register(name);
                namesAndValues[name] = value;
            }
        }

        public TValue Get(string name) {
            if (namesAndValues.ContainsKey(name))
                return namesAndValues[name];
            return default;
        }

        public bool ContainsName(string name) {
            return namesAndValues.ContainsKey(name);
        }

        public List<string> GetNames() {
            return namesAndValues.Keys.ToList();
        }

        public List<TValue> GetValues() {
            return namesAndValues.Values.ToList();
        }
    }
}