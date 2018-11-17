using System;
using Harmony;
using PluginLibrary;
using static BepInEx.Logger;
using BepInEx.Logging;
using Illusion.Game;

namespace CardOrganizerKK
{
    class Methods_Common : CardHandler
    {
        public static class KKKiyase
        {
            static bool tryKiyase = true;
            static UnityEngine.Object kiyaseObject;
            static Traverse ForceDisableOneFrameTraverse;

            public static void ForceDisableOneFrame()
            {
                if(!tryKiyase) return;

                try
                {
                    if(ForceDisableOneFrameTraverse == null)
                    {
                        var kiyaseType = PluginUtils.FindType("KK_Kiyase.KK_Kiyase");
                        kiyaseObject = FindObjectOfType(kiyaseType);
                        ForceDisableOneFrameTraverse = Traverse.Create(kiyaseObject).Field("someScenesBtnExpantion").Method("ForceDisableOneFrame");
                    }

                    ForceDisableOneFrameTraverse.GetValue();
                }
                catch(Exception)
                {
                    Log(LogLevel.Debug, "KK_Kiyase not found");
                    tryKiyase = false;
                }
            }
        }

        public static void PlaySaveSound()
        {
            if(CardOrganizerKK.PlaySounds.Value) Utils.Sound.Play(SystemSE.photo);
        }

        public static void PlayLoadSound()
        {
            if(CardOrganizerKK.PlaySounds.Value) Utils.Sound.Play(SystemSE.result_single);
        }

        public static void PlayFailSound()
        {
            if(CardOrganizerKK.PlaySounds.Value) Utils.Sound.Play(SystemSE.cancel);
        }
    }
}
