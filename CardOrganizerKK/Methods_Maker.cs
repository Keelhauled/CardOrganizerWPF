using System;
using System.IO;
using UnityEngine;
using MessagePack;
using ChaCustom;
using Illusion.Game;
using Manager;

namespace CardOrganizerKK
{
    class Methods_Maker : Methods_Common
    {
        // Copied from CustomControl.Start
        public override void Character_Save(MsgObject message)
        {
            var customCtrl = Singleton<CustomControl>.Instance;
            var customBase = CustomBase.Instance;

            //customCtrl.saveMode = true;
            Utils.Sound.Play(SystemSE.ok_s);

            byte[] facePngData = customCtrl.customCap.CapCharaFace(true);
            customBase.chaCtrl.chaFile.facePngData = facePngData;
            customCtrl.customCap.UpdateFaceImage(customBase.chaCtrl.chaFile.facePngData);

            byte[] pngData = customCtrl.customCap.CapCharaCard(true, customBase.saveFrameAssist);
            customBase.chaCtrl.chaFile.pngData = pngData;
            customCtrl.customCap.UpdateCardImage(customBase.chaCtrl.chaFile.pngData);

            var param = customBase.chaCtrl.fileParam;
            //string prefix = customBase.chaCtrl.sex == 0 ? "Koikatu_M" : "Koikatu_F";
            string filename = $"{param.lastname}_{param.firstname}_{GetTimeNow()}";
            string path = Path.Combine(message.path, filename + ".png");
            customBase.chaCtrl.chaFile.SaveCharaFile(filename, byte.MaxValue, false);

            TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));

            if(customCtrl.saveFileListCtrl)
            {
                string club = "";
                string personality = "";
                if(customBase.chaCtrl.sex != 0)
                {
                    club = Voice.Instance.voiceInfoDic.TryGetValue(customBase.chaCtrl.fileParam.personality, out VoiceInfo.Param param1) ? param1.Personality : "不明";
                    personality = Game.ClubInfos.TryGetValue(customBase.chaCtrl.fileParam.clubActivities, out ClubInfo.Param param2) ? param2.Name : "不明";
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
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            LoadCharacter(message.path);
        }

        public override void Character_LoadFemaleResolver(MsgObject message)
        {
            ResolverWrap(() => LoadCharacter(message.path));
        }

        public override void Character_LoadMale(MsgObject message)
        {
            LoadCharacter(message.path);
        }

        public override void Character_LoadMaleResolver(MsgObject message)
        {
            ResolverWrap(() => LoadCharacter(message.path));
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            LoadCharacter(message.path);
        }

        public override void Character_ReplaceAllResolver(MsgObject message)
        {
            ResolverWrap(() => LoadCharacter(message.path));
        }

        // Copied from CustomCharaFile.Start
        void LoadCharacter(string path)
        {
            ForceDisableOneFrame();
            Utils.Sound.Play(SystemSE.ok_s);

            bool loadFace = true;
            bool loadBody = true;
            bool loadHair = true;
            bool parameter = true;
            bool loadCoord = true;

            var chaCtrl = CustomBase.Instance.chaCtrl;
            chaCtrl.chaFile.LoadFileLimited(path, chaCtrl.sex, loadFace, loadBody, loadHair, parameter, loadCoord);
            chaCtrl.ChangeCoordinateType(true);
            chaCtrl.Reload(!loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
            CustomBase.Instance.updateCustomUI = true;
            CustomHistory.Instance.Add5(chaCtrl, new Func<bool, bool, bool, bool, bool>(chaCtrl.Reload), !loadCoord, !loadFace && !loadCoord, !loadHair, !loadBody);
        }

        public override void Outfit_Save(MsgObject message)
        {
            FindObjectOfType<CustomCoordinateFile>().CreateCoordinateFile("coordinateName");
        }

        public override void Outfit_Load(MsgObject message)
        {
            Utils.Sound.Play(SystemSE.ok_s);
            var chaCtrl = CustomBase.Instance.chaCtrl;

            bool loadClothes = true;
            bool loadAcs = true;

            byte[] bytes = MessagePackSerializer.Serialize(chaCtrl.nowCoordinate.clothes);
            byte[] bytes2 = MessagePackSerializer.Serialize(chaCtrl.nowCoordinate.accessory);
            chaCtrl.nowCoordinate.LoadFile(message.path);

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
