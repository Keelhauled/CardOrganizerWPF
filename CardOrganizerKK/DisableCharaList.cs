using Harmony;
using ChaCustom;
using Studio;
using System.Reflection;
using System.Collections.Generic;

namespace CardOrganizerKK
{
    public static class DisableCharaList
    {
        static bool patched = false;
        static HarmonyInstance harmony;
        static List<MethodBase> targets = new List<MethodBase>();
        static MethodInfo patchMethod;

        public static void Patch()
        {
            patched = true;
            harmony = HarmonyInstance.Create("keelhauled.cardorganizerkk.disablecharalist.harmony");
            patchMethod = AccessTools.Method(typeof(DisableCharaList), nameof(ReturnFalse));

            DisableMethod(AccessTools.Method(typeof(CustomFileListCtrl), "AddList")); // maker chara list
            DisableMethod(AccessTools.Method(typeof(CustomCharaFile), "Initialize"));
            DisableMethod(AccessTools.Method(typeof(CustomCoordinateFile), "Initialize")); // maker outfit list
            DisableMethod(AccessTools.Method(typeof(CharaList), "InitFemaleList")); // studio female list
            DisableMethod(AccessTools.Method(typeof(CharaList), "InitMaleList")); // studio male list
            var costumeInfoType = Assembly.GetAssembly(typeof(MPCharCtrl)).GetType("Studio.MPCharCtrl+CostumeInfo");
            DisableMethod(AccessTools.Method(costumeInfoType, "InitFileList")); // studio outfit list
            DisableMethod(AccessTools.Method(typeof(clothesFileControl), "Initialize")); // hscene outfit list
        }

        public static void RemovePatches()
        {
            if(patched)
            {
                foreach(var item in targets)
                {
                    harmony.RemovePatch(item, patchMethod);
                } 
            }
        }

        static void DisableMethod(MethodBase original)
        {
            harmony.Patch(original, new HarmonyMethod(patchMethod), null);
            targets.Add(original);
        }

        public static bool ReturnFalse()
        {
            return false;
        }
    }
}
