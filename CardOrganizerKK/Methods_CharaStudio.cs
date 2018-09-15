using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Studio;
using ChaCustom;
using Harmony;
using static BepInEx.Logger;
using BepInEx.Logging;

namespace CardOrganizerKK
{
    class Methods_CharaStudio : Methods_Common
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
                TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));
            });
        }

        public override void Scene_Load(MsgObject message)
        {
            Log(LogLevel.Message, $"Load scene [{Path.GetFileName(message.path)}]");
            StartCoroutine(Studio.Studio.Instance.LoadSceneCoroutine(message.path));
        }

        public override void Scene_LoadResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Load scene (resolver) [{Path.GetFileName(message.path)}]");
            // this is very bad, map doesn't load sometimes
            ResolverDelay(() => Studio.Studio.Instance.LoadScene(message.path));
        }

        public override void Scene_ImportAll(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene [{Path.GetFileName(message.path)}]");
            DelayAction(() => Studio.Studio.Instance.ImportScene(message.path));
        }

        public override void Scene_ImportAllResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => Studio.Studio.Instance.ImportScene(message.path));
        }

        public override void Scene_ImportChara(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene characters [{Path.GetFileName(message.path)}]");
            DelayAction(() => ImportSceneChara(message.path));
        }

        public override void Scene_ImportCharaResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene characters (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => ImportSceneChara(message.path));
        }

        void ImportSceneChara(string path)
        {
            using(var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using(var binaryReader = new BinaryReader(fileStream))
                {
                    PngFile.SkipPng(binaryReader);
                    var version = new Version(binaryReader.ReadString());
                    var sceneInfo = Studio.Studio.Instance.sceneInfo;
                    var traverse = Traverse.Create(sceneInfo);
                    traverse.Property("dicImport").SetValue(new Dictionary<int, ObjectInfo>());
                    traverse.Property("dicChangeKey").SetValue(new Dictionary<int, int>());

                    int num = binaryReader.ReadInt32();
                    for(int i = 0; i < num; i++)
                    {
                        int value = binaryReader.ReadInt32();
                        int type = binaryReader.ReadInt32();

                        if(type == 0)
                        {
                            var objectInfo = new OICharInfo(null, Studio.Studio.GetNewIndex());
                            objectInfo.Load(binaryReader, version, true, true);

                            sceneInfo.dicObject.Add(objectInfo.dicKey, objectInfo);
                            sceneInfo.dicImport.Add(objectInfo.dicKey, objectInfo);
                            sceneInfo.dicChangeKey.Add(objectInfo.dicKey, value);
                        }
                        else if(type == 1)
                        {
                            var objectInfo = new OIItemInfo(-1, -1, -1, Studio.Studio.GetNewIndex());
                            objectInfo.Load(binaryReader, version, true, true);

                            foreach(var item in FindCharacters(objectInfo.child))
                            {
                                sceneInfo.dicObject.Add(item.dicKey, item);
                                sceneInfo.dicImport.Add(item.dicKey, item);
                                sceneInfo.dicChangeKey.Add(item.dicKey, value);
                            }
                        }
                        else if(type == 3)
                        {
                            var objectInfo = new OIFolderInfo(Studio.Studio.GetNewIndex());
                            objectInfo.Load(binaryReader, version, true, true);

                            foreach(var item in FindCharacters(objectInfo.child))
                            {
                                sceneInfo.dicObject.Add(item.dicKey, item);
                                sceneInfo.dicImport.Add(item.dicKey, item);
                                sceneInfo.dicChangeKey.Add(item.dicKey, value);
                            }
                        }
                        else
                        {
                            ObjectInfo objectInfo = null;
                            switch(type)
                            {
                                case 2:
                                    objectInfo = new OILightInfo(-1, Studio.Studio.GetNewIndex());
                                    break;
                                case 4:
                                    objectInfo = new OIRouteInfo(Studio.Studio.GetNewIndex());
                                    break;
                                case 5:
                                    objectInfo = new OICameraInfo(Studio.Studio.GetNewIndex());
                                    break;
                            }

                            objectInfo.Load(binaryReader, version, true, true);
                        }
                    }
                }
            }

            AddObjectAssist.LoadChild(Studio.Studio.Instance.sceneInfo.dicImport, null, null);
            Studio.Studio.Instance.treeNodeCtrl.RefreshHierachy();
        }

        IEnumerable<ObjectInfo> FindCharacters(List<ObjectInfo> list)
        {
            foreach(var item in list)
            {
                if(item is OICharInfo)
                {
                    yield return item;
                }
                else if(item is OIItemInfo)
                {
                    foreach(var item1 in FindCharacters((item as OIItemInfo).child))
                        yield return item1;
                }
                else if(item is OIFolderInfo)
                {
                    foreach(var item1 in FindCharacters((item as OIFolderInfo).child))
                        yield return item1;
                }
            }
        }

        // Copied from CustomControl.Start
        public override void Character_Save(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                string date = GetTimeNow();

                foreach(var item in characters)
                {
                    var param = item.charInfo.fileParam;
                    var charFile = item.oiCharInfo.charFile;
                    var path = Path.Combine(message.path, $"{param.lastname}_{param.firstname}_{date}.png");
                    Log(LogLevel.Message, $"Save character [{Path.GetFileName(path)}]");

                    Traverse.Create(charFile).Property("charaFileName").SetValue(Path.GetFileNameWithoutExtension(path));
                    CustomCapture.CreatePng(ref charFile.pngData, 252, 352, null, null, Camera.main, null);
                    CustomCapture.CreatePng(ref charFile.facePngData, 240, 320, null, null, Camera.main, null);

                    DelayAction(() =>
                    {
                        using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            charFile.SaveCharaFile(fileStream, true);
                            TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));
                        }
                    });
                }
            }
            else
            {
                Log(LogLevel.Message, "Select characters to save");
            }
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female [{Path.GetFileName(message.path)}]");
            DelayAction(() => Studio.Studio.Instance.AddFemale(message.path));
        }

        public override void Character_LoadFemaleResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => Studio.Studio.Instance.AddFemale(message.path));
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male [{Path.GetFileName(message.path)}]");
            DelayAction(() => Studio.Studio.Instance.AddMale(message.path));
        }

        public override void Character_LoadMaleResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => Studio.Studio.Instance.AddMale(message.path));
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Replace character{(characters.Count == 1 ? "" : "s")} [{Path.GetFileName(message.path)}]");
                DelayAction(() => { foreach(var x in characters) x.ChangeChara(message.path); });
            }
            else
            {
                Log(LogLevel.Message, "Select characters to replace");
            }
        }

        public override void Character_ReplaceAllResolver(MsgObject message)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Replace character{(characters.Count == 1 ? "" : "s")} (resolver) [{Path.GetFileName(message.path)}]");
                ResolverDelay(() => { foreach(var x in characters) x.ChangeChara(message.path); });
            }
            else
            {
                Log(LogLevel.Message, "Select characters to replace");
            }
        }

        public override void Character_ReplaceBody(MsgObject message)
        {
            //var characters = GetSelectedCharacters();

            //if(characters.Count > 0)
            //{
            //    foreach(var chara in characters)
            //    {
            //        byte[] clothes = MessagePackSerializer.Serialize(chara.charInfo.nowCoordinate.clothes);
            //        byte[] accessory = MessagePackSerializer.Serialize(chara.charInfo.nowCoordinate.accessory);
            //        var coordinateType = chara.charFileStatus.coordinateType;

            //        chara.ChangeChara(message.path);
            //        chara.charInfo.nowCoordinate.clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(clothes);
            //        chara.charInfo.nowCoordinate.accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(accessory);
            //        chara.SetCoordinateInfo((ChaFileDefine.CoordinateType)coordinateType);
            //        chara.charInfo.Reload(false, true, true, true);
            //        if(chara.sex == 1) chara.charInfo.UpdateBustSoftnessAndGravity();
            //    }
            //}
            //else
            //{
            //    Log(LogLevel.Message, "Select characters to replace");
            //}
        }

        public override void Outfit_Save(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                string date = GetTimeNow();

                foreach(var chara in characters)
                {
                    string prefix = chara.sex == 0 ? "KKCoordeM" : "KKCoordeF";
                    string path = Path.Combine(message.path, $"{prefix}_{date}.png");
                    Log(LogLevel.Message, $"Save outfit [{Path.GetFileName(path)}]");

                    DelayAction(() =>
                    {
                        var chaFile = chara.charInfo.chaFile;
                        var outfit = chaFile.coordinate[chaFile.status.coordinateType];
                        CustomCapture.CreatePng(ref outfit.pngData, 252, 352, null, null, Camera.main, null);
                        outfit.coordinateName = "coordinateName";
                        outfit.SaveFile(path);
                        TCPServerManager.Instance.SendMessage(MsgObject.AddMsg(path));
                    });
                }
            }
            else
            {
                Log(LogLevel.Message, "Select character before saving outfit");
            }
        }

        public override void Outfit_Load(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Load outfit [{Path.GetFileName(message.path)}]");
                DelayAction(() =>
                {
                    foreach(var chara in characters) chara.LoadClothesFile(message.path);

                    var mpCharCtrl = FindObjectOfType<MPCharCtrl>();
                    if(mpCharCtrl)
                    {
                        int select = Traverse.Create(mpCharCtrl).Field("select").GetValue<int>();
                        if(select == 0) mpCharCtrl.OnClickRoot(0);
                    }
                });
            }
            else
            {
                Log(LogLevel.Message, "Select character before loading outfit");
            }
        }

        List<OCIChar> GetSelectedCharacters()
        {
            return GuideObjectManager.Instance.selectObjectKey.Select(x => Studio.Studio.GetCtrlInfo(x) as OCIChar).Where(x => x != null).ToList();
        }
    }
}
