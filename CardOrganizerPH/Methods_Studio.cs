using System.IO;
using System.Collections.Generic;
using System.Linq;
using static BepInEx.Logger;
using BepInEx.Logging;
using Studio;
using Harmony;

namespace CardOrganizerPH
{
    class Methods_Studio : Methods_Common
    {
        public override void Scene_Save(MsgObject message)
        {
            string path = Path.Combine(message.path, GetTimeNow() + ".png");
            Log(LogLevel.Message, $"Save scene [{Path.GetFileName(path)}]");
            PlaySaveSound();

            DelayAction(() =>
            {
                Studio.Studio.Instance.dicObjectCtrl.Values.ToList().ForEach(x => x.OnSavePreprocessing());
                Studio.Studio.Instance.sceneInfo.cameraSaveData = Studio.Studio.Instance.cameraCtrl.Export();
                Studio.Studio.Instance.sceneInfo.Save(path);
            });
        }

        public override void Scene_Load(MsgObject message)
        {
            Log(LogLevel.Message, $"Load scene [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => Studio.Studio.Instance.LoadScene(message.path));
        }

        public override void Scene_ImportAll(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => Studio.Studio.Instance.ImportScene(message.path));
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => Studio.Studio.Instance.AddFemale(message.path));
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => Studio.Studio.Instance.AddMale(message.path));
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Replace character{(characters.Count == 1 ? "" : "s")} [{Path.GetFileName(message.path)}]");
                PlayLoadSound();

                DelayAction(() =>
                {
                    foreach(var chara in characters) chara.ChangeChara(message.path);
                    UpdateStateInfo();
                });
            }
            else
            {
                Log(LogLevel.Message, "Select characters to replace");
                PlayFailSound();
            }
        }

        public override void Outfit_Load(MsgObject message)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Load outfit [{Path.GetFileName(message.path)}]");
                PlayLoadSound();

                DelayAction(() =>
                {
                    foreach(var chara in characters) chara.LoadClothesFile(message.path);
                    UpdateStateInfo();
                });
            }
            else
            {
                Log(LogLevel.Message, "Select at least one character before loading an outfit");
                PlayFailSound();
            }
        }

        List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }

        void UpdateStateInfo()
        {
            var mpCharCtrl = FindObjectOfType<MPCharCtrl>();
            if(mpCharCtrl)
            {
                int select = Traverse.Create(mpCharCtrl).Field("select").GetValue<int>();
                if(select == 0) mpCharCtrl.OnClickRoot(0);
            }
        }
    }
}
