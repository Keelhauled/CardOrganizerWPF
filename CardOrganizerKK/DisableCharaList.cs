using Harmony;
using ChaCustom;
using Studio;
using static BepInEx.Logger;
using BepInEx.Logging;

namespace CardOrganizerKK
{
    public static class DisableCharaList
    {
        public static void Patch()
        {
            var harmony = HarmonyInstance.Create("keelhauled.cardorganizerkk.disablecharalist.harmony");
            harmony.PatchAll(typeof(DisableCharaList));
        }

        // Prevent populating the maker character list
        [HarmonyPrefix, HarmonyPatch(typeof(CustomFileListCtrl), "AddList")]
        public static bool HarmonyPatch_CustomFileListCtrl_AddList()
        {
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CharaList), "InitFemaleList")]
        public static bool HarmonyPatch_CharaList_InitFemaleList()
        {
            ShowWarning();
            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(CharaList), "InitMaleList")]
        public static bool HarmonyPatch_CharaList_InitMaleList()
        {
            ShowWarning();
            return false;
        }

        public static void ShowWarning()
        {
            Log(LogLevel.Message, "[CardOrganizer] List disabled.");
        }
    }
}
