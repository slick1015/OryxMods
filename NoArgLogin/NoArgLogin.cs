using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DecaGames.RotMG.Managers;
using HarmonyLib;
using MelonLoader;
using MelonLoader.TinyJSON;
using UnityEngine;

namespace NoArgLogin
{
    public class NoArgLogin : MelonMod
    {
        [DllImport("user32.dll")]
        private static extern int MessageBoxA(IntPtr hWnd,
            string lpText,
            string lpCaption,
            uint uType);

        private const string FileName = "Login.json";
        private const string DefaultGuid = "REPLACE WITH EMAIL";
        private const string DefaultPassword = "REPLACE WITH PASSWORD";

        public static bool NeedsDefaultsChange { get; private set; }
        public static bool LoginArgsProvided { get; private set; }

        private class LoginData
        {
            public string guid;
            public string password;
        }

        private static LoginData _data;

        private static bool _retrievedTokenData;
        private static string _token;
        private static string _timestamp;
        private static string _expiration;

        public override void OnApplicationStart()
        {
            try
            {
                if (!File.Exists(FileName))
                {
                    File.WriteAllText(FileName, JSON.Dump(new LoginData()
                    {
                        guid = DefaultGuid,
                        password = DefaultPassword,
                    }, EncodeOptions.NoTypeHints | EncodeOptions.PrettyPrint));
                }

                _data = JSON.Load(File.ReadAllText(FileName)).Make<LoginData>();

                NeedsDefaultsChange = _data.guid == DefaultGuid || _data.password == DefaultPassword;
                LoginArgsProvided = Environment.GetCommandLineArgs()
                    .Any(arg => arg.IndexOf("data:", StringComparison.Ordinal) != -1);

                if (NeedsDefaultsChange && !LoginArgsProvided)
                {
                    MessageBoxA(IntPtr.Zero, $"Put your account information in {FileName} then relaunch the game.", "NoArgLogin", 0);
                    Environment.Exit(1);
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Unable to handle {FileName}: {ex}");
            }
        }

        private static Tuple<string, string, string> ParseFromVerify(string verify)
        {
            var token = verify.Split(new[] { "<AccessToken>" }, StringSplitOptions.None)[1].Split(new[] { "</AccessToken>" }, StringSplitOptions.None)[0];
            var timestamp = verify.Split(new[] { "<AccessTokenTimestamp>" }, StringSplitOptions.None)[1].Split(new[] { "</AccessTokenTimestamp>" }, StringSplitOptions.None)[0];
            var expiration = verify.Split(new[] { "<AccessTokenExpiration>" }, StringSplitOptions.None)[1].Split(new[] { "</AccessTokenExpiration>" }, StringSplitOptions.None)[0];
            return new Tuple<string, string, string>(token, timestamp, expiration);
        }

        [HarmonyPatch(typeof(ApplicationManager), "PGPBOBNNEPG")]
        class ApplicationManager_PGPBOBNNEPG_Patches
        {
            [HarmonyPrefix]
            static bool Prefix(ApplicationManager __instance)
            {
                if (!LoginArgsProvided)
                {
                    if (!_retrievedTokenData)
                    {
                        MelonLogger.Msg("Starting to load account information for login...");
                        var verify = WebUtil.Request("/account/verify", new Dictionary<string, string>
                        {
                            { "guid", _data.guid },
                            { "password", _data.password },
                            { "clientToken", SystemInfo.deviceUniqueIdentifier }
                        });

                        if (verify.StartsWith("<Error") || !verify.Contains("<AccessToken>") ||
                            !verify.Contains("</AccessToken>"))
                        {
                            MessageBoxA(IntPtr.Zero, "Failed to verify account and retrieve access token.",
                                "NoArgLogin: VERIFY ERROR", 0);
                            Environment.Exit(1);
                        }

                        (_token, _timestamp, _expiration) = ParseFromVerify(verify);

                        var verifyAccessToken = WebUtil.Request("/account/verifyAccessTokenClient", new Dictionary<string, string>
                        {
                            { "clientToken", SystemInfo.deviceUniqueIdentifier },
                            { "accessToken", _token }
                        });

                        if (!verifyAccessToken.Contains("Success"))
                        {
                            MessageBoxA(IntPtr.Zero, "Failed to verify access token.",
                                "NoArgLogin: VERIFY TOKEN ERROR", 0);
                            Environment.Exit(1);
                        }

                        MelonLogger.Msg("Successfully finished loading account information!");

                        _retrievedTokenData = true;
                    }

                    var dataBank = __instance.ELOOGFOEGPJ;
                    dataBank.PGIDOACDONN._NMGDJEDLAGK_k__BackingField = _token; // token
                    dataBank.PGIDOACDONN._CLAPFHFJMJK_k__BackingField = _timestamp; // token timestamp
                    dataBank.PGIDOACDONN._FFIJPPMKHJC_k__BackingField = _expiration; // token expiration
                    dataBank.PGIDOACDONN._FPIKBBOPAGD_k__BackingField = _data.guid; // guid
                    dataBank.PGIDOACDONN._MMGODEBMBML_k__BackingField = ""; // platform token
                    dataBank.PGIDOACDONN._LBDCDELDPAE_k__BackingField = ""; // steam id

                    dataBank.KDPIHNMEHHC = GLABHOJGCDK.Deca;

                    __instance.HLFELIKNFJO = 4;
                    __instance.PDDCLOPLJHH = true;

                    return false;
                }

                return true;
            }
        }
    }
}
