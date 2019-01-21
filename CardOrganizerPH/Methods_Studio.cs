using System;
using System.IO;
using System.Linq;
using static BepInEx.Logger;
using BepInEx.Logging;
using PluginLibrary;

namespace CardOrganizerPH
{
    class Methods_Studio : CardHandler
    {
        public override void Scene_Save(MsgObject message)
        {
            string path = Path.Combine(message.path, GetTimeNow() + ".png");
            Log(LogLevel.Message, $"Save scene [{Path.GetFileName(path)}]");

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
            DelayAction(() => Studio.Studio.Instance.LoadScene(message.path));
        }

        public override void Scene_ImportAll(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene [{Path.GetFileName(message.path)}]");
            DelayAction(() => Studio.Studio.Instance.ImportScene(message.path));
        }
    }
}
