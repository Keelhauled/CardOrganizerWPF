using System;
using System.Collections.Generic;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using PluginLibrary;

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

            var methods = new Dictionary<string, CardHandler>
            {
                { "Studio", gameobject.AddComponent<Methods_StudioNeo>() },
                { "CustomScene", gameobject.AddComponent<Methods_Maker>() },
                { "HScene", gameobject.AddComponent<Methods_HScene>() },
                { "MapSelect", gameobject.AddComponent<Methods_MapSelect>() },
            };

            Action<MsgObject> action = (message) =>
            {
                dispatcher.Enqueue(() =>
                {
                    if(methods.TryGetValue(SceneManager.GetActiveScene().name, out CardHandler manager))
                        manager.UseCard(message);
                });
            };

            RPCClient_Plugin.Init("CardOrganizerServer.HS", 9125, action);
        }

        public void OnApplicationQuit(){}
        public void OnLevelWasLoaded(int level){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
