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
        public const string PLUGIN_NAME = "CardOrganizerHS";
        public const string PLUGIN_VERSION = "1.0.0";
        public string Name => PLUGIN_NAME;
        public string Version => PLUGIN_VERSION;

        public string[] Filter => new string[]
        {
            "StudioNEO_32",
            "StudioNEO_64",
            "HoneySelect_32",
            "HoneySelect_64",
        };

        public void OnApplicationStart()
        {
            var gameobject = new GameObject(PLUGIN_NAME);
            gameobject.AddComponent<UnityMainThreadDispatcher>();

            var methods = new Dictionary<string, CardHandler>
            {
                { "Studio", gameobject.AddComponent<Methods_StudioNeo>() },
                { "CustomScene", gameobject.AddComponent<Methods_Maker>() },
                { "HScene", gameobject.AddComponent<Methods_HScene>() },
                { "MapSelect", gameobject.AddComponent<Methods_MapSelect>() },
            };

            gameobject.AddComponent<TCPServerManager>().MessageAction = (x) =>
            {
                if(methods.TryGetValue(SceneManager.GetActiveScene().name, out CardHandler manager))
                    manager.UseCard(x);
            };
        }

        public void OnApplicationQuit(){}
        public void OnLevelWasLoaded(int level){}
        public void OnUpdate(){}
        public void OnLateUpdate(){}
        public void OnLevelWasInitialized(int level){}
        public void OnFixedUpdate(){}
    }
}
