using System;
using System.Collections.Generic;

namespace CupAPI.Utility {
    public static class EnumManager {

        private static readonly Dictionary<Type, EnumRegistry> enumRegistries = [];

        public static void Register<TEnum>() where TEnum : Enum {
            Type type = typeof(TEnum);
            Register(type);
        }

        public static bool TryGetRegistry<T>(out EnumRegistry registry) {
            Type type = typeof(T);
            return TryGetRegistry(type, out registry);
        }

        internal static void Register(Type type) {
            if (!enumRegistries.ContainsKey(type)) {
                enumRegistries[type] = new EnumRegistry(type);
            }
        }

        internal static bool TryGetRegistry(Type type, out EnumRegistry registry) {
            return enumRegistries.TryGetValue(type, out registry);
        }
    }
}