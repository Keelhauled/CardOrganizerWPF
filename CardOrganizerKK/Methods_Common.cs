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
        UnityEngine.Object kiyaseObject;
        Traverse ForceDisableOneFrame;

        public void KKKiyase_ForceDisableOneFrame()
        {
            try
            {
                if(ForceDisableOneFrame == null)
                {
                    var kiyaseType = PluginUtils.FindType("KK_Kiyase.KK_Kiyase");
                    kiyaseObject = FindObjectOfType(kiyaseType);
                    ForceDisableOneFrame = Traverse.Create(kiyaseObject).Field("someScenesBtnExpantion").Method("ForceDisableOneFrame");
                }

                ForceDisableOneFrame.GetValue();
            }
            catch(Exception)
            {
                Log(LogLevel.Debug, "KK_Kiyase not found");
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
