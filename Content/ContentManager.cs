using CupAPI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Content {
    public static class ContentManager {

        public static readonly LinkedRegistry<Charm, IEquipInfo> Charms = new(
            name => HandlePages(name, CharmPages,
                [
                    Charm.charm_health_up_1,
                    Charm.charm_super_builder,
                    Charm.charm_smoke_dash,
                    Charm.charm_parry_plus,
                    Charm.charm_health_up_2,
                    Charm.charm_parry_attack,
                    Charm.charm_chalice,
                    Charm.charm_curse,
                    Charm.charm_healer
                ]
                , Charm.None, 9));

        private static void HandlePages<TEnum>(string name, Dictionary<int, TEnum[]> pagesDict, TEnum[] firstPageValues, TEnum noneValue, int pageAmount) where TEnum : Enum {
            if (!pagesDict.ContainsKey(1))
                pagesDict[1] = firstPageValues;
            
            int count = 1;
            foreach (var pageValues in pagesDict.Values)
                count += pageValues.Where(value => !value.Equals(noneValue)).Count();
            int page = (int)Math.Ceiling((double)count / pageAmount);

            if (!pagesDict.ContainsKey(page)) {
                TEnum[] values = new TEnum[pageAmount];
                for (int i = 0; i < values.Length; i++)
                    values[i] = noneValue;

                pagesDict[page] = values;
            }

            TEnum[] array = pagesDict[page];
            int index = array.Where(value => !value.Equals(noneValue)).Count();
            array[index] = (TEnum)Enum.Parse(typeof(TEnum), name);
        }

        internal static readonly Dictionary<int, Charm[]> CharmPages = [];
    }
}