using System;
using System.Collections.Generic;
using UnityEngine;
using PluginLibrary;
using UnityEngine.SceneManagement;
using BepInEx;

namespace CardOrganizerHS
{
    [BepInPlugin("keelhauled.cardorganizerhs", "CardOrganizerHS", "1.0.0")]
    class CardOrganizerHS : BaseUnityPlugin
    {
        public void Awake()
        {
            var gameobject = new GameObject(nameof(CardOrganizerHS));
            gameobject.transform.SetParent(gameObject.transform);
            var dispatcher = gameobject.AddComponent<UnityMainThreadDispatcher>();

            var scenes = new Dictionary<string, CardHandler>
            {
                { "StudioNeo", gameobject.AddComponent<Methods_StudioNeo>() },
                { "Maker", gameobject.AddComponent<Methods_Maker>() },
                { "HScene", gameobject.AddComponent<Methods_HScene>() },
                //{ "MapSelect", gameobject.AddComponent<Methods_MapSelect>() },
            };

            RPCClient_Plugin.Init("CardOrganizerServer", 9125, "HS", (message, id) => {
                if(!dispatcher) Console.WriteLine("[CardOrganizer] Dispatcher dead");
                dispatcher.Enqueue(() => scenes[id].UseCard(message));
            });
        }

        void OnDestroy()
        {
            RPCClient_Plugin.StopServer();
        }

        public void OnLevelWasLoaded(int level)
        {
            switch(SceneManager.GetActiveScene().name)
            {
                case "Studio":
                {
                    RPCClient_Plugin.ChangeId("StudioNeo");
                    break;
                }

                case "HScene":
                {
                    RPCClient_Plugin.ChangeId("HScene");
                    break;
                }

                case "CustomScene":
                {
                    RPCClient_Plugin.ChangeId("Maker");
                    break;
                }
            }
        }
    }
}
