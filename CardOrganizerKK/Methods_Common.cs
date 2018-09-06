using System;
using UnityEngine;
using Harmony;
using PluginLibrary;

namespace CardOrganizerKK
{
    class Methods_Common : CardHandler
    {
        public void ForceDisableOneFrame()
        {
            try
            {
                var kiyaseType = PluginUtils.FindType("KK_Kiyase.KK_Kiyase");
                var kiyaseObject = FindObjectOfType(kiyaseType);
                Traverse.Create(kiyaseObject).Field("someScenesBtnExpantion").Method("ForceDisableOneFrame").GetValue();
            }
            catch(Exception)
            {
                Console.WriteLine("KK_Kiyase not found");
            }
        }

        public void ResolverWrap(Action action)
        {
            bool save = Event.current.alt;
            Event.current.alt = true;
            action();
            Event.current.alt = save;
        }
    }
}
