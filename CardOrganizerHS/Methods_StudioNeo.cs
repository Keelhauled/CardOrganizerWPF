using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Studio;
using PluginLibrary;
using TraverseStandalone;

namespace CardOrganizerHS
{
    public class Methods_StudioNeo : Methods_Common
    {
        public override void Scene_Save(MsgObject message)
        {
            Studio.Studio.Instance.dicObjectCtrl.Values.ToList().ForEach(x => x.OnSavePreprocessing());
            Studio.Studio.Instance.sceneInfo.cameraSaveData = Studio.Studio.Instance.cameraCtrl.Export();
            string path = Path.Combine(message.path, GetTimeNow() + ".png");
            Studio.Studio.Instance.sceneInfo.Save(path);
            PluginUtils.InvokePluginMethod("HSStudioNEOExtSave.StudioNEOExtendSaveMgr", "SaveExtData", path);
        }

        public override void Scene_Load(MsgObject message)
        {
            PluginUtils.InvokePluginMethod("LockOnPlugin.LockOnBase", "ResetModState");
            Studio.Studio.Instance.LoadScene(message.path);
            StartCoroutine(StudioNEOExtendSaveMgrLoad());

            IEnumerator StudioNEOExtendSaveMgrLoad()
            {
                for(int i = 0; i < 3; i++) yield return null;
                PluginUtils.InvokePluginMethod("HSStudioNEOExtSave.StudioNEOExtendSaveMgr", "LoadExtData", message.path);
                PluginUtils.InvokePluginMethod("HSStudioNEOExtSave.StudioNEOExtendSaveMgr", "LoadExtDataRaw", message.path);
            }
        }

        public override void Scene_ImportAll(MsgObject message)
        {
            Studio.Studio.Instance.ImportScene(message.path);
        }

        public override void Scene_ImportChara(MsgObject message)
        {
            using(var fileStream = new FileStream(message.path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using(var binaryReader = new BinaryReader(fileStream))
                {
                    long size = 0L;
                    PngAssist.CheckPngData(binaryReader, ref size, true);
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
                        else
                        {
                            ObjectInfo objectInfo = null;
                            switch(type)
                            {
                                case 1:
                                    objectInfo = new OIItemInfo(-1, Studio.Studio.GetNewIndex());
                                    break;
                                case 2:
                                    objectInfo = new OILightInfo(-1, Studio.Studio.GetNewIndex());
                                    break;
                                case 3:
                                    objectInfo = new OIFolderInfo(Studio.Studio.GetNewIndex());
                                    break;
                                default:
                                    Console.WriteLine($"対象外 : {type}");
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

        public override void Character_Save(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                string date = GetTimeNow();

                foreach(var chara in characters)
                {
                    string path = Path.Combine(message.path, $"{chara.charInfo.customInfo.name}_{date}.png");
                    SaveChara(chara.charInfo, path);
                }
            }
            else
            {
                Console.WriteLine("Select characters to save");
            }
        }

        public override void Character_LoadFemale(MsgObject message)
        {
            Studio.Studio.Instance.AddFemale(message.path);
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Studio.Studio.Instance.AddMale(message.path);
        }

        public override void Character_ReplaceAll(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                foreach(var chara in characters)
                {
                    chara.ChangeChara(message.path);
                    if(chara.charInfo.Sex == 1)
                    {
                        var female = chara as OCICharFemale;
                        female.SetTuyaRate(female.oiCharInfo.skinRate);
                    }

                    UpdateStateInfo();
                }
            }
            else
            {
                Console.WriteLine("Select characters to replace");
            }
        }

        public override void Character_ReplaceBody(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                foreach(var chara in characters)
                {
                    if(chara.charInfo.Sex == 0)
                    {

                    }
                    else
                    {
                        using(var memoryStream = new MemoryStream())
                        {
                            using(var binaryWriter = new BinaryWriter(memoryStream))
                            {
                                chara.charInfo.chaFile.clothesInfo.Save(binaryWriter);
                                var coordinateType = chara.charInfo.statusInfo.coordinateType;

                                chara.ChangeChara(message.path);
                                chara.charInfo.chaFile.clothesInfo.Load(new MemoryStream(memoryStream.ToArray()), true);
                                chara.charInfo.chaFile.SetCoordinateInfo(coordinateType);
                                chara.charInfo.Reload(false, true, true);

                                var female = chara as OCICharFemale;
                                female.female.UpdateBustSoftnessAndGravity();
                                female.SetTuyaRate(female.oiCharInfo.skinRate);
                            }
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Select characters to replace");
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
                    string prefix = chara.oiCharInfo.sex == 0 ? "coordM" : "coordF";
                    string path = Path.Combine(message.path, $"{prefix}_{date}.png");
                    SaveOutfit(chara.charInfo, path);
                }
            }
            else
            {
                Console.WriteLine("Select character to save outfit");
            }
        }

        public override void Outfit_Load(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                foreach(var chara in characters)
                {
                    chara.LoadClothesFile(message.path);
                    if(chara.charInfo.Sex == 1)
                    {
                        var female = chara as OCICharFemale;
                        female.SetTuyaRate(female.oiCharInfo.skinRate);
                    }
                }

                UpdateStateInfo();
            }
            else
            {
                Console.WriteLine("Select character before loading outfit");
            }
        }

        public override void Pose_Save(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                foreach(var chara in characters)
                    PauseCtrl.Save(chara, message.path);
            }
            else
            {
                Console.WriteLine("Select character to save pose");
            }
        }

        public override void Pose_Load(MsgObject message)
        {
            var characters = GetSelectedCharacters();

            if(characters.Count > 0)
            {
                foreach(var chara in characters)
                    PauseCtrl.Load(chara, message.path);
            }
            else
            {
                Console.WriteLine("Select character to pose");
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
