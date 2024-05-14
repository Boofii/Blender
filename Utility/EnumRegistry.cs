using System;
using System.Collections.Generic;
using System.Linq;

namespace Blender.Utility {

    internal interface IEnumRegistry
    {
        void Register(string name);
        string GetName(int id);
        int GetId(string name);
        bool ContainsName(string name);
        bool ContainsId(int value);
        List<string> GetNames();
        List<int> GetIds();
    }

    public class EnumRegistry<TEnum> : IEnumRegistry where TEnum : Enum {

        private readonly Dictionary<string, int> namesAndIds = [];
        private readonly Dictionary<int, string> idsAndNames = [];

        public void Register(string name) {
            if (Enum.IsDefined(typeof(TEnum), name))
                return;
            if (!namesAndIds.ContainsKey(name)) {
                int id = this.LargestId() + 1;
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

        private int LargestId() {
            int result = 0;
            foreach (int intValue in Enum.GetValues(typeof(TEnum)))
                if (intValue > result && intValue != int.MaxValue)
                    result = intValue;
            return result;
        }
    }
}