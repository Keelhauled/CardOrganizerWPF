using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Studio;
using PluginLibrary;
using TraverseStandalone;
using Manager;

namespace CardOrganizerHS
{
    class Methods_MapSelect : Methods_Common
    {
        // MapSelectScene.OnEnter()
        public override void Character_LoadFemale(MsgObject message)
        {
            var mapSelect = FindObjectOfType<MapSelectScene>();
            var traverse = Traverse.Create(mapSelect);
            traverse.Field("charFile").SetValue(message.path);

            var keys = Map.Instance.MapInfoDic.Keys.ToList();
            traverse.Field("mapKeys").SetValue(keys);
            //mapSelect.SetMapListNormal();
            mapSelect.SetMapList();

            //int select = UnityEngine.Random.Range(0, keys.Count);
            int select = 1;
            if(keys.Count != 1)
            {
                traverse.Field("mapIdx").SetValue(UnityEngine.Random.Range(0, keys.Count));
            }

            mapSelect.SetMapImage(keys[select]);
            mapSelect.mapSelectInfo.buttons[2].interactable = true;
            mapSelect.mapSelectInfo.buttons[3].interactable = true;
            mapSelect.mapSelectInfo.objRoot.SetActive(true);
            mapSelect.charaSelectInfo.objRoot.SetActive(false);
        }
    }
}
