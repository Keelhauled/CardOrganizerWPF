using System;
using System.IO;
using MessagePack;
using ChaCustom;
using Manager;
using static BepInEx.Logger;
using BepInEx.Logging;

namespace CardOrganizerKK
{
    class Methods_Maker : Methods_Common
    {
        // Copied from CustomControl.Start
        public override void Character_Save(MsgObject message)
        {
            var customCtrl = Singleton<CustomControl>.Instance;
            var customBase = CustomBase.Instance;

            var param = customBase.chaCtrl.fileParam;
            string filename = $"{param.lastname}_{param.firstname}_{GetTimeNow()}";
            string filenameext = filename + ".png";
            string path = Path.Combine(message.path, filenameext);

            Log(LogLevel.Message, $"Save character [{filenameext}]");
            PlaySaveSound();

            DelayAction(() =>
            {
                //customCtrl.saveMode = true;
                //KKKiyase.ForceDisableOneFrame();

                byte[] facePngData = customCtrl.customCap.CapCharaFace(true);
                customBase.chaCtrl.chaFile.facePngData = facePngData;
                customCtrl.customCap.UpdateFaceImage(customBase.chaCtrl.chaFile.facePngData);

                byte[] pngData = customCtrl.customCap.CapCharaCard(true, customBase.saveFrameAssist);
                customBase.chaCtrl.chaFile.pngData = pngData;
                customCtrl.customCap.UpdateCardImage(customBase.chaCtrl.chaFile.pngData);

                customBase.chaCtrl.chaFile.SaveCharaFile(filename, byte.MaxValue, false);

                if(customCtrl.saveFileListCtrl)
                {
                    string club = "";
                    string personality = "";
                    if(customBase.chaCtrl.sex != 0)
                    {
                        club = Voice.Instance.voiceInfoDic.TryGetValue(param.personality, out VoiceInfo.Param param1) ? param1.Personality : "不明";
                        personality = Game.ClubInfos.TryGetValue(param.clubActivities, out ClubInfo.Param param2) ? param2.Name : "不明";
                    }
                    else
                    {
                        customCtrl.saveFileListCtrl.DisableAddInfo();
                    }

                    int noUseIndex = customCtrl.saveFileListCtrl.GetNoUseIndex();
                    customCtrl.saveFileListCtrl.AddList(noUseIndex, customBase.chaCtrl.chaFile.parameter.fullname, club, personality, path, filename, DateTime.Now, false);
                    customCtrl.saveFileListCtrl.ReCreate();
                }

                customCtrl.saveMode = false;
            });
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, true));
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, true));
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            Log(LogLevel.Message, $"Replace character [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, true));
        }

        public override void Character_ReplaceFace(MsgObject message)
        {
            Log(LogLevel.Message, $"Replace character face [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, true, false, false, false, false));
        }

        public override void Character_ReplaceBody(MsgObject message)
        {
            Log(LogLevel.Message, $"Replace character body [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, false, true, false, false, false));
        }

        public override void Character_ReplaceHair(MsgObject message)
        {
            Log(LogLevel.Message, $"Replace character hair [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, false, false, true, false, false));
        }

        public override void Character_ReplaceOutfit(MsgObject message)
        {
            Log(LogLevel.Message, $"Replace character outfits [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadCharacter(message.path, false, false, false, false, true));
        }

        void LoadCharacter(string path, bool all)
        {
            LoadCharacter(path, true, true, true, true, true);
        }

        // Copied from CustomCharaFile.Start
        void LoadCharacter(string path, bool loadFace, bool loadBody, bool loadHair, bool parameter, bool loadCoord)
        {
            //KKKiyase.ForceDisableOneFrame();

            var chaCtrl = CustomBase.Instance.chaCtrl;
            chaCtrl.chaFile.LoadFileLimited(path, chaCtrl.sex, loadFace, loadBody, loadHair, parameter, loadCoord);
            chaCtrl.ChangeCoordinateType(true);
            chaCtrl.Reload(!loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
            CustomBase.Instance.updateCustomUI = true;
            CustomHistory.Instance.Add5(chaCtrl, chaCtrl.Reload, !loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
        }

        // Copied from CustomCoordinateFile.CreateCoordinateFileCoroutine
        public override void Outfit_Save(MsgObject message)
        {
            string coordName = "coordinateName";
            var customBase = CustomBase.Instance;
            var chaCtrl = customBase.chaCtrl;
            var customCtrl = Singleton<CustomControl>.Instance;

            string date = GetTimeNow();
            string prefix = chaCtrl.sex == 0 ? "KKCoordeM" : "KKCoordeF";
            string filename = $"{prefix}_{date}";
            string filenameext = filename + ".png";
            string path = Path.Combine(message.path, filenameext);

            Log(LogLevel.Message, $"Save outfit [{Path.GetFileName(path)}]");
            PlaySaveSound();

            DelayAction(() =>
            {
                var outfit = chaCtrl.chaFile.coordinate[chaCtrl.chaFile.status.coordinateType];
                outfit.pngData = customCtrl.customCap.CapCharaCard(true, customBase.saveFrameAssist);
                outfit.coordinateName = coordName;
                outfit.SaveFile(path);
            });
        }

        public override void Outfit_Load(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, true, true));
        }

        public override void Outfit_LoadAccOnly(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit accessories [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, false, true));
        }

        public override void Outfit_LoadClothOnly(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit clothing [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, true, false));
        }

        // Copied from CustomCoordinateFile.Start
        void LoadOutfit(string path, bool loadClothes, bool loadAcs)
        {
            var chaCtrl = CustomBase.Instance.chaCtrl;

            byte[] bytes = MessagePackSerializer.Serialize(chaCtrl.nowCoordinate.clothes);
            byte[] bytes2 = MessagePackSerializer.Serialize(chaCtrl.nowCoordinate.accessory);
            chaCtrl.nowCoordinate.LoadFile(path);

            if(!loadClothes)
                chaCtrl.nowCoordinate.clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(bytes);

            if(!loadAcs)
                chaCtrl.nowCoordinate.accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(bytes2);

            chaCtrl.Reload(false, true, true, true);
            chaCtrl.AssignCoordinate((ChaFileDefine.CoordinateType)chaCtrl.chaFile.status.coordinateType);
            CustomBase.Instance.updateCustomUI = true;
            CustomHistory.Instance.Add5(chaCtrl, new Func<bool, bool, bool, bool, bool>(chaCtrl.Reload), false, true, true, true);
        }
    }
}
