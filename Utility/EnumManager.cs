using System;
using System.Collections.Generic;

namespace CupAPI.Utility {
    public static class EnumManager {

        private static readonly Dictionary<Type, IEnumRegistry> Registries = [];

        public static EnumRegistry<TEnum> Register<TEnum>() where TEnum : Enum {
            Type type = typeof(TEnum);
            if (!Registries.ContainsKey(type)) {
                var registry = new EnumRegistry<TEnum>();
                Registries[type] = registry;
                return registry;
            }
            return null;
        }

        public static bool TryGetRegistry<TEnum>(out EnumRegistry<TEnum> registry) where TEnum : Enum {
            Type type = typeof(TEnum);
            registry = null;
            if (Registries.TryGetValue(type, out var reg) && reg is EnumRegistry<TEnum> reg1) {
                registry = reg1;
                return true;
            }
            return false;
        }

        internal static bool TryGetRegistry(Type type, out IEnumRegistry registry) {
            registry = null;
            if (type.IsEnum)
                return Registries.TryGetValue(type, out registry);
            return false;
        }
    }
}