using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace FastStart
{
    public class FastStart : MelonMod
    {
        [HarmonyPatch(typeof(SplashScreenScript))]
        class SplashScreenScript_Patches
        {
            [HarmonyPatch("Start")]
            [HarmonyPrefix]
            static bool Start_Prefix()
            {
                SceneManager.LoadScene("Bootstrap");
                return false;
            }
        }
    }
}
