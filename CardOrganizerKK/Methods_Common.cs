using System;
using UnityEngine;
using Harmony;
using PluginLibrary;
using static BepInEx.Logger;
using BepInEx.Logging;

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

        public void ResolverWrap(Action action)
        {
            bool save = Event.current.alt;
            Event.current.alt = true;
            action();
            Event.current.alt = save;
        }

        public void ResolverDelay(Action action, int wait = 1)
        {
            DelayAction(() => ResolverWrap(action), wait);
        }
    }
}
