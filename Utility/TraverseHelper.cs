using HarmonyLib;

namespace CupAPI.Utility {
    public static class TraverseHelper {

        public static T GetField<T>(object instance, string name) {
            Traverse<T> field = Traverse.Create(instance).Field<T>(name);
            return field.Value;
        }

        public static void SetField<T>(object instance, string name, T value) {
            Traverse<T> field = Traverse.Create(instance).Field<T>(name);
            field.Value = value;
        }
    }
}