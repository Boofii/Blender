using System;
using System.Collections.Generic;
using System.Globalization;

namespace CupAPI.Utility {
    public static class EnumManager {

        private static readonly Dictionary<Type, IEnumRegistry> registries = [];

        public static EnumRegistry<TEnum> Register<TEnum>() where TEnum : Enum {
            Type type = typeof(TEnum);
            if (!registries.ContainsKey(type)) {
                var registry = new EnumRegistry<TEnum>();
                registries[type] = registry;
                return registry;
            }
            return null;
        }

        public static bool TryGetRegistry<TEnum>(out EnumRegistry<TEnum> registry) where TEnum : Enum {
            Type type = typeof(TEnum);
            registry = null;
            if (registries.TryGetValue(type, out var reg) && reg is EnumRegistry<TEnum> reg1) {
                registry = reg1;
                return true;
            }
            return false;
        }

        public static bool TryGetRegistry(Type type, out IEnumRegistry registry) {
            registry = null;
            if (type.IsEnum)
                return registries.TryGetValue(type, out registry);
            return false;
        }
    }
}