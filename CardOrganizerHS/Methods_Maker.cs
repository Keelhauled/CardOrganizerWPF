using System;
using System.IO;

namespace CardOrganizerHS
{
    class Methods_Maker : Methods_Common
    {
        public override void Character_Save(MsgObject message)
        {
            var customCtrl = Singleton<CustomControl>.Instance;
            string path = Path.Combine(message.path, $"{customCtrl.chainfo.customInfo.name}_{GetTimeNow()}.png");
            customCtrl.chainfo.chaFile.charaFileName = Path.GetFileNameWithoutExtension(path);
            CreatePng(ref customCtrl.chainfo.chaFile.charaFilePNG);
            customCtrl.CustomSaveCharaAssist(path);
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
            var customCtrl = Singleton<CustomControl>.Instance;
            var chara = customCtrl.chainfo;

            chara.chaFile.Load(message.path);
            chara.Reload(false, false, false);

            if(chara.Sex == 0)
                (chara as CharMale).maleStatusInfo.visibleSon = false;
            else
                (chara as CharFemale).UpdateBustSoftnessAndGravity();

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
            var customCtrl = Singleton<CustomControl>.Instance;
            string prefix = customCtrl.chainfo.Sex == 0 ? "coordM" : "coordF";
            string path = Path.Combine(message.path, $"{prefix}_{GetTimeNow()}.png");
            CreatePng(ref customCtrl.chainfo.clothesInfo.clothesPNG);
            customCtrl.CustomSaveClothesAssist(path);
        }

        public override void Outfit_Load(MsgObject message)
        {
            var customCtrl = Singleton<CustomControl>.Instance;
            var chara = customCtrl.chainfo;

            chara.clothesInfo.Load(message.path);
            chara.chaFile.SetCoordinateInfo(chara.statusInfo.coordinateType);
            chara.Reload(false, false, false);

            if(chara.Sex == 0)
                (chara as CharMale).maleStatusInfo.visibleSon = false;
            else
                (chara as CharFemale).UpdateBustSoftnessAndGravity();

            customCtrl.subMenuCtrl.UpdateLimitMainMenu();
            customCtrl.ChangeSwimTypeFromLoad();
            customCtrl.UpdateAcsName();
        }
    }
}
