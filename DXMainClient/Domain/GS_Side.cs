using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTAClient.Domain
{
    internal static class GS_Side
    {
        public static readonly int FactionSidesCount = 4; // 每个大阵营四个小阵营

        /// <summary>
        /// 阵营，0-3 GDI，4-7 Nod，8-11 Scrin
        /// </summary>
        public static readonly IList<string> Sides = new List<string> {
            "GDI-Offense","GDI-Defense","GDI-Support","ZOCOM",
            "Nod-Offense","Nod-Defense","Nod-Support","MarkedOfKane",
            "Scrin","Reaper17","Traveler59","Destroyer41",
        };

        /// <summary>
        /// 小阵营到大阵营
        /// </summary>
        public static readonly IDictionary<string, string> SideToFaction = new Dictionary<string, string>
        {
            {"GDI-Offense", "GDI"},
            {"GDI-Defense", "GDI"},
            {"GDI-Support", "GDI"},
            {"ZOCOM",       "GDI"},
            {"Nod-Offense", "Nod"},
            {"Nod-Defense", "Nod"},
            {"Nod-Support", "Nod"},
            {"MarkedOfKane","Nod"},
            {"Scrin",       "Scrin"},
            {"Reaper17",    "Scrin"},
            {"Traveler59",  "Scrin"},
            {"Destroyer41", "Scrin"}
        };
    }
}
