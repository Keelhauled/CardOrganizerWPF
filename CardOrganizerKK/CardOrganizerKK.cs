using System;
using System.ComponentModel;
using BepInEx;
using PluginLibrary;
using UnityEngine;

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
            if(DisableLists.Value) DisableCharaList.Patch();

            var gameobject = new GameObject(nameof(CardOrganizerKK));
            gameobject.transform.SetParent(gameObject.transform);
            var dispatcher = gameobject.AddComponent<UnityMainThreadDispatcher>();
            var studio = gameobject.AddComponent<Methods_CharaStudio>();
            var freeh = gameobject.AddComponent<Methods_FreeHSelect>();
            var maker = gameobject.AddComponent<Methods_Maker>();
            var hscene = gameobject.AddComponent<Methods_HScene>();

            Action<MsgObject> action = (message) =>
            {
                dispatcher.Enqueue(() =>
                {
                    if(FindObjectOfType<StudioScene>())
                    {
                        studio.UseCard(message);
                    }
                    else if(FindObjectOfType<FreeHScene>() && !FindObjectOfType<FreeHCharaSelect>())
                    {
                        freeh.UseCard(message);
                    }
                    else if(FindObjectOfType<CustomScene>())
                    {
                        maker.UseCard(message);
                    }
                    else if(FindObjectOfType<HSceneProc>())
                    {
                        hscene.UseCard(message);
                    }
                });
            };

            RPCClient_Plugin.Init("CardOrganizerServer.KK", 9125, action);
        }

        void OnDestroy()
        {
            RPCClient_Plugin.StopServer();
        }
    }
}
