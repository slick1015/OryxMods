using MelonLoader;
using MelonLoader.TinyJSON;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using UnityEngine;


namespace LocalPlayerPrefs
{
    /*
     * Code adapted from the illustrious knah (https://github.com/knah)
     * Original: https://github.com/knah/ML-UniversalMods
     */
    public class LocalPlayerPrefs : MelonMod
    {
        private const string FileName = "PlayerPrefs.json";

        private static readonly List<Delegate> myPinnedDelegates = new List<Delegate>();
        private static readonly ConcurrentDictionary<string, object> myPrefs = new ConcurrentDictionary<string, object>();

        private static bool myHadChanges = false;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool TrySetFloatDelegate(IntPtr keyPtr, float value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool TrySetIntDelegate(IntPtr keyPtr, int value);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool TrySetStringDelegate(IntPtr keyPtr, IntPtr valuePtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate int GetIntDelegate(IntPtr keyPtr, int defaultValue);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate float GetFloatDelegate(IntPtr keyPtr, float defaultValue);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr GetStringDelegate(IntPtr keyPtr, IntPtr defaultValuePtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate bool HasKeyDelegate(IntPtr keyPtr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void DeleteKeyDelegate(IntPtr keyPtr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void VoidDelegate();

        public override void OnApplicationStart()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    var dict = (ProxyObject)JSON.Load(File.ReadAllText(FileName));
                    foreach (var keyValuePair in dict) myPrefs[keyValuePair.Key] = ToObject(keyValuePair.Key, keyValuePair.Value);
                    MelonLogger.Msg($"Loaded {dict.Count} prefs from PlayerPrefs.json");
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Unable to load PlayerPrefs.json: {ex}");
            }

            HookICall<TrySetFloatDelegate>(nameof(PlayerPrefs.TrySetFloat), TrySetFloat);
            HookICall<TrySetIntDelegate>(nameof(PlayerPrefs.TrySetInt), TrySetInt);
            HookICall<TrySetStringDelegate>(nameof(PlayerPrefs.TrySetSetString), TrySetString);

            HookICall<GetFloatDelegate>(nameof(PlayerPrefs.GetFloat), GetFloat);
            HookICall<GetIntDelegate>(nameof(PlayerPrefs.GetInt), GetInt);
            HookICall<GetStringDelegate>(nameof(PlayerPrefs.GetString), GetString);

            HookICall<HasKeyDelegate>(nameof(PlayerPrefs.HasKey), HasKey);
            HookICall<DeleteKeyDelegate>(nameof(PlayerPrefs.DeleteKey), DeleteKey);

            HookICall<VoidDelegate>(nameof(PlayerPrefs.DeleteAll), DeleteAll);
            HookICall<VoidDelegate>(nameof(PlayerPrefs.Save), Save);
        }

        private static object ToObject(string key, Variant value)
        {
            if (value is null) return null;

            if (value is ProxyString proxyString)
                return proxyString.ToString();

            if (value is ProxyNumber number)
            {
                var numDouble = number.ToDouble(NumberFormatInfo.InvariantInfo);
                if ((double)(int)numDouble == numDouble)
                    return (int)numDouble;

                return (float)numDouble;
            }

            throw new ArgumentException($"Unknown value in prefs: {key} = {value?.GetType()} / {value}");
        }

        private static bool HasKey(IntPtr keyPtr)
        {
            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            return myPrefs.ContainsKey(key);
        }

        private static void DeleteKey(IntPtr keyPtr)
        {
            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            myPrefs.TryRemove(key, out _);
        }

        private static void DeleteAll()
        {
            myPrefs.Clear();
        }

        private static readonly object mySaveLock = new object();
        public static void Save()
        {
            if (!myHadChanges)
                return;

            myHadChanges = false;

            try
            {
                lock (mySaveLock)
                {
                    File.WriteAllText(FileName, JSON.Dump(myPrefs, EncodeOptions.PrettyPrint));
                }
            }
            catch (IOException ex)
            {
                MelonLogger.Warning($"Exception while saving PlayerPrefs: {ex}");
            }
        }

        private static float GetFloat(IntPtr keyPtr, float defaultValue)
        {
            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            if (myPrefs.TryGetValue(key, out var result))
            {
                switch (result)
                {
                    case float resultFloat:
                        return resultFloat;
                    case int resultInt:
                        return resultInt;
                }
            }

            return defaultValue;
        }

        private static int GetInt(IntPtr keyPtr, int defaultValue)
        {
            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            if (myPrefs.TryGetValue(key, out var result))
            {
                switch (result)
                {
                    case float resultFloat:
                        return (int)resultFloat;
                    case int resultInt:
                        return resultInt;
                }
            }

            return defaultValue;
        }

        private static IntPtr GetString(IntPtr keyPtr, IntPtr defaultValuePtr)
        {
            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            if (myPrefs.TryGetValue(key, out var result))
            {
                if (result is string resultString)
                    return IL2CPP.ManagedStringToIl2Cpp(resultString);
            }

            return defaultValuePtr;
        }

        private static bool TrySetFloat(IntPtr keyPtr, float value)
        {
            myHadChanges = true;

            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            myPrefs[key] = value;
            return true;
        }

        private static bool TrySetInt(IntPtr keyPtr, int value)
        {
            myHadChanges = true;

            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            myPrefs[key] = value;
            return true;
        }

        private static bool TrySetString(IntPtr keyPtr, IntPtr valuePtr)
        {
            myHadChanges = true;

            var key = IL2CPP.Il2CppStringToManaged(keyPtr);
            myPrefs[key] = IL2CPP.Il2CppStringToManaged(valuePtr);
            return true;
        }

        private static unsafe void HookICall<T>(string name, T target) where T : Delegate
        {
            var originalPointer = IL2CPP.il2cpp_resolve_icall("UnityEngine.PlayerPrefs::" + name);
            if (originalPointer == IntPtr.Zero)
            {
                MelonLogger.Warning($"ICall {name} was not found, not patching");
                return;
            }

            myPinnedDelegates.Add(target);
            MelonUtils.NativeHookAttach((IntPtr)(&originalPointer), Marshal.GetFunctionPointerForDelegate(target));
        }
    }
}
