using System;
using System.Collections.Generic;
using UnityEngine;
using PluginLibrary;
using UnityEngine.SceneManagement;
using BepInEx;

namespace CardOrganizerPH
{
    [BepInPlugin("keelhauled.cardorganizerph", "CardOrganizerPH", "1.0.0")]
    class CardOrganizerPH : BaseUnityPlugin
    {
        public void Awake()
        {
            var gameobject = new GameObject(GetType().Name);
            gameobject.transform.SetParent(gameObject.transform);
            var dispatcher = gameobject.AddComponent<UnityMainThreadDispatcher>();

            var scenes = new Dictionary<string, CardHandler>
            {
                { "H", gameobject.AddComponent<Methods_HScene>() }
            };

            RPCClient_Plugin.Init("CardOrganizerServer", 9125, "PH", (message, id) => {
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
                case "H":
                {
                    RPCClient_Plugin.ChangeId("HScene");
                    break;
                }
            }
        }
    }
}
