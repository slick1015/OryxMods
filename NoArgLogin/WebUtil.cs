using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace NoArgLogin
{
    class WebUtil
    {
        private const string AppEngineUrl = "https://www.realmofthemadgod.com";

        public static string Request(string endpoint, Dictionary<string, string> data)
        {
            var form = new WWWForm();
            foreach (var kvp in data)
            {
                form.AddField(kvp.Key, kvp.Value);
            }
            form.AddField("game_net", "Unity");
            form.AddField("play_platform", "Unity");
            form.AddField("game_net_user_id", "");

            var uwr = UnityWebRequest.Post(AppEngineUrl + endpoint, form);

            var asyncOp = uwr.SendWebRequest();
            while (!asyncOp.isDone)
                Thread.Sleep(100);

            return uwr.downloadHandler.text;
        }
    }
}
