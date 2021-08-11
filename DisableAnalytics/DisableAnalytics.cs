using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeltaDNA;
using HarmonyLib;

namespace DisableAnalytics
{
    public class DisableAnalytics : MelonMod
    {
        [HarmonyPatch(typeof(DDNA))]
        class DDNA_Patches
        {
            [HarmonyPatch("Awake")]
            [HarmonyPrefix]
            static bool Awake_Prefix() => false;
        }
    }
}
