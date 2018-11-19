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
using MessagePack;
using System.Reflection;
using System.Reflection.Emit;

namespace CardOrganizerKK
{
    class Methods_CharaStudio : Methods_Common
    {
        static class LoadFilePatch
        {
            static HarmonyInstance harmony;
            static MethodInfo transpiler;
            static MethodInfo original;

            public static bool doPatch = false;
            static bool loadFace = true;
            static bool loadBody = true;
            static bool loadHair = true;
            static bool parameter = true;
            static bool loadCoord = true;

            public static void SetParam(bool dopatch, bool loadface, bool loadbody, bool loadhair, bool param, bool loadcoord)
            {
                doPatch = dopatch;
                loadFace = loadface;
                loadBody = loadbody;
                loadHair = loadhair;
                parameter = param;
                loadCoord = loadcoord;
            }

            public static void Patch()
            {
                harmony = HarmonyInstance.Create("keelhauled.cardorganizerkk.loadfilepatch.harmony");
                original = AccessTools.Method(typeof(OCIChar), nameof(OCIChar.ChangeChara));
                transpiler = AccessTools.Method(typeof(LoadFilePatch), nameof(PatchTranspiler));
                harmony.Patch(original, null, null, new HarmonyMethod(transpiler));
            }

            public static void RemovePatch()
            {
                harmony.RemovePatch(original, transpiler);
            }

            public static IEnumerable<CodeInstruction> PatchTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                var codes = new List<CodeInstruction>(instructions);

                for(int i = 0; i < codes.Count; i++)
                {
                    if(codes[i].ToString() == "callvirt Boolean LoadCharaFile(System.String, Byte, Boolean, Boolean)")
                    {
                        codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LoadFilePatch), nameof(CheckPatch)));
                        codes[i+1] = new CodeInstruction(OpCodes.Nop);

                        break;
                    }
                }

                return codes;
            }

            public static void CheckPatch(ChaFileControl chara, string path, byte sex, bool noLoadPng, bool noLoadStatus)
            {
                if(doPatch)
                {
                    chara.LoadFileLimited(path, sex, loadFace, loadBody, loadHair, parameter, loadCoord);
                }
                else
                {
                    chara.LoadCharaFile(path, sex, noLoadPng, noLoadStatus);
                }
            }
        }

        void Start()
        {
            LoadFilePatch.Patch();
        }

        void OnDestroy()
        {
            LoadFilePatch.RemovePatch();
        }

        // Copied from Studio.SaveScene
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
            DelayAction(() => StartCoroutine(Studio.Studio.Instance.LoadSceneCoroutine(message.path)));
        }

        public override void Scene_ImportAll(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => Studio.Studio.Instance.ImportScene(message.path));
        }

        public override void Scene_ImportChara(MsgObject message)
        {
            Log(LogLevel.Message, $"Import scene characters [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => ImportSceneChara(message.path));
        }

        // Edited version of SceneInfo.Import
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
                //KKKiyase.ForceDisableOneFrame();
                PlaySaveSound();

                foreach(var item in characters)
                {
                    var param = item.charInfo.fileParam;
                    var charFile = item.oiCharInfo.charFile;
                    var path = Path.Combine(message.path, $"{param.lastname}_{param.firstname}_{date}.png");
                    Log(LogLevel.Message, $"Save character [{Path.GetFileName(path)}]");

                    DelayAction(() =>
                    {
                        Traverse.Create(charFile).Property("charaFileName").SetValue(Path.GetFileNameWithoutExtension(path));
                        CustomCapture.CreatePng(ref charFile.pngData, 252, 352, null, null, Camera.main, null);
                        CustomCapture.CreatePng(ref charFile.facePngData, 240, 320, null, null, Camera.main, null);

                        using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                        {
                            charFile.SaveCharaFile(fileStream, true);
                        }
                    });
                }
            }
            else
            {
                Log(LogLevel.Message, "Select characters to save");
                PlayFailSound();
            }
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
            ReplaceChara(message.path, false, true, true, true, true, true);
        }

        public override void Character_ReplaceFace(MsgObject message)
        {
            ReplaceChara(message.path, true, true, false, false, false, false);
        }

        public override void Character_ReplaceBody(MsgObject message)
        {
            ReplaceChara(message.path, true, false, true, false, false, false);
        }

        public override void Character_ReplaceHair(MsgObject message)
        {
            ReplaceChara(message.path, true, false, false, true, false, false);
        }

        public override void Character_ReplaceOutfit(MsgObject message)
        {
            ReplaceChara(message.path, true, false, false, false, false, true);
        }

        void ReplaceChara(string path, bool doPatch, bool loadFace, bool loadBody, bool loadHair, bool parameter, bool loadCoord)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, $"Replace character{(characters.Count == 1 ? "" : "s")} [{Path.GetFileName(path)}]");
                PlayLoadSound();

                DelayAction(() =>
                {
                    LoadFilePatch.SetParam(doPatch, loadFace, loadBody, loadHair, parameter, loadCoord);
                    foreach(var x in characters) x.ChangeChara(path);
                    LoadFilePatch.doPatch = false;
                    UpdateStateInfo();
                });
            }
            else
            {
                Log(LogLevel.Message, "Select characters to replace");
                PlayFailSound();
            }
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
                    PlaySaveSound();

                    DelayAction(() =>
                    {
                        var chaFile = chara.charInfo.chaFile;
                        var outfit = chaFile.coordinate[chaFile.status.coordinateType];
                        CustomCapture.CreatePng(ref outfit.pngData, 252, 352, null, null, Camera.main, null);
                        outfit.coordinateName = "coordinateName";
                        outfit.SaveFile(path);
                    });
                }
            }
            else
            {
                Log(LogLevel.Message, "Select character before saving outfit");
                PlayFailSound();
            }
        }

        public override void Outfit_Load(MsgObject message)
        {
            LoadOutfit(message.path, true, true, $"Load outfit [{Path.GetFileName(message.path)}]");
        }

        public override void Outfit_LoadAccOnly(MsgObject message)
        {
            LoadOutfit(message.path, false, true, $"Load outfit accessories [{Path.GetFileName(message.path)}]");
        }

        public override void Outfit_LoadClothOnly(MsgObject message)
        {
            LoadOutfit(message.path, true, false, $"Load outfit clothing [{Path.GetFileName(message.path)}]");
        }

        // Copied from CustomCoordinateFile.Start
        void LoadOutfit(string path, bool loadClothes, bool loadAcs, string logMsg)
        {
            var characters = GetSelectedCharacters();
            if(characters.Count > 0)
            {
                Log(LogLevel.Message, logMsg);
                PlayLoadSound();

                DelayAction(() =>
                {
                    foreach(var chara in characters)
                    {
                        byte[] bytes = MessagePackSerializer.Serialize(chara.charInfo.nowCoordinate.clothes);
                        byte[] bytes2 = MessagePackSerializer.Serialize(chara.charInfo.nowCoordinate.accessory);
                        chara.charInfo.nowCoordinate.LoadFile(path);

                        if(!loadClothes)
                            chara.charInfo.nowCoordinate.clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(bytes);

                        if(!loadAcs)
                            chara.charInfo.nowCoordinate.accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(bytes2);

                        chara.charInfo.Reload(false, true, true, true);
                        chara.charInfo.AssignCoordinate((ChaFileDefine.CoordinateType)chara.charInfo.chaFile.status.coordinateType);
                    }

                    UpdateStateInfo();
                });
            }
            else
            {
                Log(LogLevel.Message, "Select character before loading outfit");
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
