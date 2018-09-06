using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using PluginLibrary;

namespace CardOrganizerHS
{
    class Methods_Maker : Methods_Common
    {
        public override void Character_Save(MsgObject message)
        {
            var chara = Singleton<CustomControl>.Instance.chainfo;
            string path = Path.Combine(message.path, $"{chara.customInfo.name}_{GetTimeNow()}.png");
            SaveChara(chara, path);
            TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            Character_ReplaceAll(message);
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Character_ReplaceAll(message);
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            var chara = Singleton<CustomControl>.Instance.chainfo;
            chara.chaFile.Load(message.path);
            chara.Reload(false, false, false);

            if(chara.Sex == 0)
                (chara as CharMale).maleStatusInfo.visibleSon = false;
            else
                (chara as CharFemale).UpdateBustSoftnessAndGravity();

            var customCtrl = Singleton<CustomControl>.Instance;
            customCtrl.subMenuCtrl.UpdateLimitMainMenu();
            customCtrl.SetSameSetting();
            customCtrl.noChangeSubMenu = true;
            customCtrl.ChangeSwimTypeFromLoad();
            customCtrl.noChangeSubMenu = false;
            customCtrl.UpdateCharaName();
            customCtrl.UpdateAcsName();
        }

        public override void Outfit_Save(MsgObject message)
        {
            var chara = Singleton<CustomControl>.Instance.chainfo;
            string prefix = chara.Sex == 0 ? "coordM" : "coordF";
            string path = Path.Combine(message.path, $"{prefix}_{GetTimeNow()}.png");
            SaveOutfit(chara, path);
            TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));
        }

        public override void Outfit_Load(MsgObject message)
        {
            var chara = Singleton<CustomControl>.Instance.chainfo;
            chara.clothesInfo.Load(message.path);
            chara.chaFile.SetCoordinateInfo(chara.statusInfo.coordinateType);
            chara.Reload(false, false, false);

            if(chara.Sex == 0)
                (chara as CharMale).maleStatusInfo.visibleSon = false;
            else
                (chara as CharFemale).UpdateBustSoftnessAndGravity();

            var customCtrl = Singleton<CustomControl>.Instance;
            customCtrl.subMenuCtrl.UpdateLimitMainMenu();
            customCtrl.ChangeSwimTypeFromLoad();
            customCtrl.UpdateAcsName();
        }
    }
}
