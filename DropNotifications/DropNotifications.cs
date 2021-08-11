using System.IO;
using DecaGames.RotMG.Managers;
using DecaGames.RotMG.Managers.Game.MapObjects;
using HarmonyLib;
using Il2CppSystem;
using MelonLoader;
using UnityEngine;

namespace DropNotifications
{
    public class DropNotificationsConfig
    {
        public static DropNotificationsConfig Instance => MelonPreferences.GetCategory<DropNotificationsConfig>("DropNotifications");

        public bool NotifyWhiteBags = true;
        public bool NotifyOrangeBags = true;
    }

    public class DropNotifications : MelonMod
    {
        public override void OnApplicationStart()
        {
            var category = MelonPreferences.CreateCategory<DropNotificationsConfig>("DropNotifications");

            if (!Directory.Exists("OryxMods"))
                Directory.CreateDirectory("OryxMods");
            category.SetFilePath("OryxMods/DropNotifications.toml");
            category.SaveToFile(false);
        }

        private static void ShowNotification(string message, Color color)
        {
            var map = ApplicationManager.IGKPDHHANDN.FJMBDKMJFOI;
            var player = ApplicationManager.IGKPDHHANDN.FJMBDKMJFOI.HFLGDFNPKPM;

            if (player == null) return;

            // show floating text over player
            Color32 color32 = color;
            var nullableColor = Nullable<Color32>.Unbox(color32.BoxIl2CppObject());
            nullableColor.has_value = true;
            player.GOHBCMBOHOC.Cast<MapObjectUIManager>().NMHOHNMKLIF();
            player.GOHBCMBOHOC.ShowFloatingText(CLEPBEMBHAJ.LevelUp, message, nullableColor);

            // show levelup particle effect around player
            var levelUpEffect = FKJCKLMDPEL.GLOGBDBEMHC.ADOMOLEGLCC();
            levelUpEffect.KAKOKPJOJHL(player, color, 20);
            map.HFOIFHEOJPA(levelUpEffect);
        }

        [HarmonyPatch(typeof(DKMLMKFGPCC))]
        class DKMLMKFGPCC_Patches
        {
            [HarmonyPatch("HFOIFHEOJPA", typeof(MIFLGAPPNJI))]
            [HarmonyPostfix]
            static void HFOIFHEOJPA_Postfix(MIFLGAPPNJI GOLADMMICPF)
            {
                switch (GOLADMMICPF.LDGGOBCHABO)
                {
                    case 0x050C:
                    case 0x0510:
                        if (DropNotificationsConfig.Instance.NotifyWhiteBags)
                            ShowNotification("White Bag!", new Color(1.0f, 1.0f, 1.0f));
                        break;

                    case 0x050F:
                    case 0x06BF:
                        if (DropNotificationsConfig.Instance.NotifyOrangeBags)
                            ShowNotification("Orange Bag!", new Color32(0xff, 0x81, 0x20, 0xff));
                        break;
                }
            }
        }
    }
}
