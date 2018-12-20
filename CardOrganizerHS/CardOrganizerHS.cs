using System;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using PluginLibrary;
using UnityEngine.SceneManagement;

namespace CardOrganizerHS
{
    class CardOrganizerHS : IEnhancedPlugin
    {
        public string Name { get; } = "CardOrganizerHS";
        public string Version { get; } = "1.0.0";

        public string[] Filter { get; } = new string[]
        {
            "StudioNEO_32",
            "StudioNEO_64",
            "HoneySelect_32",
            "HoneySelect_64",
        };

        public void OnApplicationStart()
        {
            var gameobject = new GameObject(Name);
            var dispatcher = gameobject.AddComponent<UnityMainThreadDispatcher>();

            var scenes = new Dictionary<string, CardHandler>
            {
                { "StudioNeo", gameobject.AddComponent<Methods_StudioNeo>() },
                { "Maker", gameobject.AddComponent<Methods_Maker>() },
                { "HScene", gameobject.AddComponent<Methods_HScene>() },
                //{ "MapSelect", gameobject.AddComponent<Methods_MapSelect>() },
            };

            RPCClient_Plugin.Init("CardOrganizerServer", 9125, "HS", (message, id) => {
                dispatcher.Enqueue(() => scenes[id].UseCard(message));
            });
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

        public void OnApplicationQuit(){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
