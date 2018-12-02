using System.Collections.Generic;
using System.ComponentModel;
using BepInEx;
using PluginLibrary;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardOrganizerKK
{
    [BepInPlugin("keelhauled.cardorganizerkk", "CardOrganizerKK", "1.0.0")]
    class CardOrganizerKK : BaseUnityPlugin
    {
        [DisplayName("Disable ingame card lists")]
        [Description("These lists drain a lot of performance and are useless with this plugin so they should be disabled.\n\n" +
                     "Changes take effect after game restart.")]
        ConfigWrapper<bool> DisableLists { get; }

        [Browsable(true)]
        [DisplayName("Reconnect to server")]
        [CustomSettingDraw(nameof(ReconnectDrawer))]
        string Reconnect { get; set; } = "";

        [DisplayName("Play sounds")]
        public static ConfigWrapper<bool> PlaySounds { get; private set; }

        CardOrganizerKK()
        {
            DisableLists = new ConfigWrapper<bool>("DisableLists", this, true);
            PlaySounds = new ConfigWrapper<bool>("PlaySounds", this, true);
        }

        void ReconnectDrawer()
        {
            var text = RPCClient_Plugin.Status() ? "Connected" : "Reconnect";

            if(GUILayout.Button(text, GUILayout.ExpandWidth(true)))
            {
                RPCClient_Plugin.StartServer();
            }
        }

        void Awake()
        {
            SceneManager.sceneLoaded += SceneLoaded;
            if(DisableLists.Value) DisableCharaList.Patch();

            var gameobject = new GameObject(nameof(CardOrganizerKK));
            gameobject.transform.SetParent(gameObject.transform);

            var scenes = new Dictionary<string, CardHandler>
            {
                { "Maker", gameobject.AddComponent<Methods_Maker>() },
                { "Studio", gameobject.AddComponent<Methods_CharaStudio>() },
                { "FreeH", gameobject.AddComponent<Methods_HScene>() },
                { "FreeHSelect", gameobject.AddComponent<Methods_FreeHSelect>() }
            };

            RPCClient_Plugin.Init("CardOrganizerServer", 9125, "KK", (message, id) => {
                gameobject.AddComponent<UnityMainThreadDispatcher>().Enqueue(() => scenes[id].UseCard(message));
            });
        }

        void OnDestroy()
        {
            RPCClient_Plugin.StopServer();
            DisableCharaList.RemovePatches();
            SceneManager.sceneLoaded -= SceneLoaded;
        }

        void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if(FindObjectOfType<StudioScene>())
            {
                RPCClient_Plugin.ChangeId("Studio");
            }
            else if(FindObjectOfType<FreeHScene>() && !FindObjectOfType<FreeHCharaSelect>())
            {
                RPCClient_Plugin.ChangeId("FreeHSelect");
            }
            else if(FindObjectOfType<CustomScene>())
            {
                RPCClient_Plugin.ChangeId("Maker");
            }
            else if(FindObjectOfType<HSceneProc>())
            {
                RPCClient_Plugin.ChangeId("FreeH");
            }
            else
            {
                RPCClient_Plugin.ChangeId("");
            }
        }
    }
}
