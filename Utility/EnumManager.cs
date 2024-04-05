using System;
using System.Collections.Generic;

namespace CupAPI.Utility {
    public static class EnumManager {

        private static readonly Dictionary<Type, EnumRegistry> enumRegistries = [];
        private static readonly Dictionary<Type, object> registries = [];

        public static void Register<TEnum>() where TEnum : Enum {
            Type type = typeof(TEnum);
            if (!enumRegistries.ContainsKey(type))
                enumRegistries[type] = new EnumRegistry(type);
        }

        public static void Register<TEnum, TValue>() where TEnum : Enum {
            Register<TEnum>();
            Type type = typeof(TEnum);
            if (!registries.ContainsKey(type))
                registries[type] = new Registry<TValue>();
        }

        public static bool TryGetRegistry<TEnum>(out EnumRegistry registry) where TEnum : Enum {
            Type type = typeof(TEnum);
            return enumRegistries.TryGetValue(type, out registry);
        }

        public static bool TryGetRegistry<TEnum, TValue>(out Registry<TValue> registry) where TEnum : Enum {
            registry = null;
            if (TryGetRegistry<TEnum>(out _)) {
                Type type = typeof(TEnum);
                if (registries.TryGetValue(type, out object registryObj)) {
                    if (registryObj is Registry<TValue> reg) {
                        registry = reg;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}