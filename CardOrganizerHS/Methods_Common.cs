using System;
using System.IO;
using UnityEngine;
using PluginLibrary;

namespace CardOrganizerHS
{
    public class Methods_Common : CardHandler
    {
        public void SaveChara(CharInfo chara, string path)
        {
            chara.chaFile.charaFileName = Path.GetFileNameWithoutExtension(path);
            CreatePng(ref chara.chaFile.charaFilePNG);

            if(chara.Sex == 0)
            {
                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    chara.chaFile.Save(fileStream);
                }
            }
            else
            {
                int length = Enum.GetValues(typeof(CharDefine.CoordinateType)).Length;
                int[] array = new int[length];
                int[] array2 = new int[length];
                int[] array3 = new int[length];
                var charFileInfoCoordinateFemale = chara.chaFile.coordinateInfo as CharFileInfoCoordinateFemale;

                for(int i = 0; i < length; i++)
                {
                    var charFileInfoClothesFemale = charFileInfoCoordinateFemale.GetInfo((CharDefine.CoordinateType)i) as CharFileInfoClothesFemale;
                    int key = charFileInfoClothesFemale.clothesId[0];
                    array[i] = charFileInfoClothesFemale.clothesId[1];
                    array2[i] = charFileInfoClothesFemale.clothesId[2];
                    array3[i] = charFileInfoClothesFemale.clothesId[3];

                    var femaleFbxList = chara.ListInfo.GetFemaleFbxList(CharaListInfo.TypeFemaleFbx.cf_f_top, true);
                    if(femaleFbxList.TryGetValue(key, out ListTypeFbx listTypeFbx))
                    {
                        if(byte.Parse(listTypeFbx.Etc[1]) == 2)
                            charFileInfoClothesFemale.clothesId[1] = 0;

                        if(byte.Parse(listTypeFbx.Etc[2]) != 0)
                            charFileInfoClothesFemale.clothesId[2] = 0;
                    }

                    femaleFbxList = chara.ListInfo.GetFemaleFbxList(CharaListInfo.TypeFemaleFbx.cf_f_bot, true);
                    listTypeFbx = null;

                    if(femaleFbxList.TryGetValue(array[i], out listTypeFbx))
                    {
                        if(byte.Parse(listTypeFbx.Etc[3]) != 0)
                            charFileInfoClothesFemale.clothesId[3] = 0;
                    }
                }

                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    chara.chaFile.Save(fileStream);
                }

                for(int j = 0; j < length; j++)
                {
                    var charFileInfoClothesFemale2 = charFileInfoCoordinateFemale.GetInfo((CharDefine.CoordinateType)j) as CharFileInfoClothesFemale;
                    charFileInfoClothesFemale2.clothesId[2] = array2[j];
                    charFileInfoClothesFemale2.clothesId[3] = array3[j];
                    charFileInfoClothesFemale2.clothesId[1] = array[j];
                }
            }
        }

        public void SaveOutfit(CharInfo chara, string path)
        {
            CreatePng(ref chara.clothesInfo.clothesPNG);

            if(chara.Sex == 0)
            {
                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using(var binaryWriter = new BinaryWriter(fileStream))
                        chara.clothesInfo.Save(binaryWriter);
                }
            }
            else
            {
                int num = chara.clothesInfo.clothesId[1];
                int num2 = chara.clothesInfo.clothesId[2];
                int num3 = chara.clothesInfo.clothesId[3];

                var charFemaleClothes = chara.chaClothes as CharFemaleClothes;
                if(charFemaleClothes.NotBot) chara.clothesInfo.clothesId[1] = 0;
                if(charFemaleClothes.NotBra) chara.clothesInfo.clothesId[2] = 0;
                if(charFemaleClothes.NotShorts) chara.clothesInfo.clothesId[3] = 0;

                using(var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using(var binaryWriter = new BinaryWriter(fileStream))
                        chara.clothesInfo.Save(binaryWriter);
                }

                chara.clothesInfo.clothesId[2] = num2;
                chara.clothesInfo.clothesId[3] = num3;
                chara.clothesInfo.clothesId[1] = num;
            }
        }

        public void CreatePng(ref byte[] pngData, int createW = 252, int createH = 352, float renderRate = 1f)
        {
            if(createW == 0 || createH == 0)
            {
                createW = 252;
                createH = 352;
            }

            Vector2 screenSize = ScreenInfo.GetScreenSize();
            float screenRate = ScreenInfo.GetScreenRate();
            float screenCorrectY = ScreenInfo.GetScreenCorrectY();
            float num = 720f * screenRate / screenSize.y;
            int num4 = (int)(504f * renderRate);
            int num5 = (int)(704f * renderRate);

            RenderTexture temporary;
            if(QualitySettings.antiAliasing == 0)
                temporary = RenderTexture.GetTemporary((int)(1280f * renderRate / num), (int)(720f * renderRate / num), 24);
            else
                temporary = RenderTexture.GetTemporary((int)(1280f * renderRate / num), (int)(720f * renderRate / num), 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, QualitySettings.antiAliasing);

            if(Camera.main)
            {
                Camera main = Camera.main;
                RenderTexture targetTexture = main.targetTexture;
                Rect rect = main.rect;
                main.targetTexture = temporary;
                main.Render();
                main.targetTexture = targetTexture;
                main.rect = rect;
            }

            Texture2D texture2D = new Texture2D(num4, num5, TextureFormat.RGB24, false, true);
            RenderTexture.active = temporary;
            float x = 388f * renderRate + (1280f / num - 1280f) * 0.5f * renderRate;
            float y = 8f * renderRate + screenCorrectY / screenRate * renderRate;
            texture2D.ReadPixels(new Rect(x, y, num4, num5), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
            RenderTexture.ReleaseTemporary(temporary);
            if(num4 != createW || num5 != createH)
            {
                TextureScale.Bilinear(texture2D, createW, createH);
            }
            pngData = texture2D.EncodeToPNG();
            Destroy(texture2D);
        }
    }
}
