using System;
using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CardOrganizerKK
{
    [BepInPlugin("com.keelhauled.cardorganizerkk", "CardOrganizerKK", "1.0.0")]
    class CardOrganizerKK : BaseUnityPlugin
    {
        void Awake()
        {
            var gameobject = new GameObject("CardOrganizerKK");
            gameobject.AddComponent<UnityMainThreadDispatcher>();
            var studio = gameobject.AddComponent<Methods_CharaStudio>();
            var freeh = gameobject.AddComponent<Methods_FreeHSelect>();
            var maker = gameobject.AddComponent<Methods_Maker>();

            gameobject.AddComponent<TCPServerManager>().MessageAction = (x) =>
            {
                if(FindObjectOfType<StudioScene>())
                {
                    studio.UseCard(x);
                }
                else if(FindObjectOfType<FreeHScene>() && !FindObjectOfType<FreeHCharaSelect>())
                {
                    freeh.UseCard(x);
                }
                else if(SceneManager.GetActiveScene().name == "CustomScene")
                {
                    maker.UseCard(x);
                }
            };
        }
    }
}
