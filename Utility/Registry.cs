using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Utility {
    public class Registry<TValue> {

        private readonly Dictionary<string, TValue> namesAndValues;

        public void Register(string name, TValue value) {
            if (!namesAndValues.ContainsKey(name))
                namesAndValues[name] = value;
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