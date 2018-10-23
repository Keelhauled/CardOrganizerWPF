﻿using System;
using System.ComponentModel;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardOrganizerKK
{
    [BepInPlugin("keelhauled.cardorganizerkk", "CardOrganizerKK", "1.0.0")]
    class CardOrganizerKK : BaseUnityPlugin
    {
        [Advanced(true)]
        [DisplayName("Disable ingame card lists")]
        [Description("These lists drain a lot of performance and are useless with this plugin so they should be disabled.\nChanges take effect after game restart.")]
        ConfigWrapper<bool> DisableLists { get; }

        CardOrganizerKK()
        {
            DisableLists = new ConfigWrapper<bool>("DisableLists", this, true);
        }

        void Awake()
        {
            if(DisableLists.Value) DisableCharaList.Patch();

            var gameobject = new GameObject("CardOrganizerKK");
            gameobject.AddComponent<UnityMainThreadDispatcher>();
            var studio = gameobject.AddComponent<Methods_CharaStudio>();
            var freeh = gameobject.AddComponent<Methods_FreeHSelect>();
            var maker = gameobject.AddComponent<Methods_Maker>();
            var hscene = gameobject.AddComponent<Methods_HScene>();

            gameobject.AddComponent<TCPServerManager>().MessageAction = (message) =>
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
            };
        }
    }
}
