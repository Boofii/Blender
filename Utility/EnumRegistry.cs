using System;
using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Utility {
    public class EnumRegistry(Type type) {

        private readonly Dictionary<string, int> namesAndIds = [];
        private readonly Dictionary<int, string> idsAndNames = [];

        public readonly Type type = type;

        public void Register(string name) {
            int id = GetHighestId() + 1;
            if (!namesAndIds.ContainsKey(name)) {
                namesAndIds[name] = id;
                idsAndNames[id] = name;
            }
        }

        public string GetName(int id) {
            if (idsAndNames.ContainsKey(id))
                return idsAndNames[id];
            return null;
        }

        public int GetId(string name) {
            if (namesAndIds.ContainsKey(name))
                return namesAndIds[name];
            return 0;
        }

        public bool ContainsName(string name) {
            return namesAndIds.ContainsKey(name);
        }

        public bool ContainsId(int value) {
            return idsAndNames.ContainsKey(value);
        }

        public List<string> GetNames() {
            return namesAndIds.Keys.ToList();
        }

        public List<int> GetIds() {
            return namesAndIds.Values.ToList();
        }

        private int GetHighestId() {
            int result = 0;
            foreach (int intValue in Enum.GetValues(type)) {
                if (intValue > result)
                    result = intValue;
            }
            return result;
        }
    }
}