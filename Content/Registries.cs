using CupAPI.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CupAPI.Content {
    public static class Registries {

        public static readonly LinkedRegistry<Charm, ICharm> Charms = new LinkedRegistry<Charm, ICharm>(
            name => {
                int count = 1;
                foreach (var pageCharms in CharmPages.Values)
                    count += pageCharms.Where(charm => charm != Charm.None).Count();
                int page = (int)Math.Ceiling((double)count / 9);

                if (!CharmPages.ContainsKey(page)) {
                    Charm[] charms = new Charm[9];
                    for (int i = 0; i < charms.Length; i++)
                        charms[i] = Charm.None;

                    CharmPages[page] = charms;
                }

                Charm[] array = CharmPages[page];
                int index = array.Where(charm => charm != Charm.None).Count();
                array[index] = (Charm)Enum.Parse(typeof(Charm), name);
            });

        internal static readonly Dictionary<int, Charm[]> CharmPages = new Dictionary<int, Charm[]>
        {
            {1, new Charm[]
                {
                    Charm.charm_health_up_1,
                    Charm.charm_super_builder,
                    Charm.charm_smoke_dash,
                    Charm.charm_parry_plus,
                    Charm.charm_health_up_2,
                    Charm.charm_parry_attack,
                    Charm.charm_chalice,
                    Charm.charm_curse,
                    Charm.charm_healer
                }
            }
        };
    }
}