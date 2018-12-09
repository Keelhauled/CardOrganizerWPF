using System;
using MessagePack;

[MessagePackObject(true)]
public class MsgObject
{
    public Action action;
    public string path;
    public string id;

    public static MsgObject Create(Action action, string id, string path)
    {
        return new MsgObject{ action = action, id = id, path = path };
    }

    public void Print()
    {
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"Action: {action}");
        Console.WriteLine($"ID: {id}");
        Console.WriteLine($"Path: {path}");
        Console.WriteLine(new string('=', 40));
    }

    public enum Action
    {
        SceneSave,
        SceneLoad,
        SceneImportAll,
        SceneImportChara,

        CharaSave,
        CharaLoadFemale,
        CharaLoadMale,
        CharaLoadSpecial,
        CharaReplaceAll,
        CharaReplaceFace,
        CharaReplaceBody,
        CharaReplaceHair,
        CharaReplaceOutfit,

        OutfitSave,
        OutfitLoad,
        OutfitLoadAccOnly,
        OutfitLoadClothOnly,
        OutfitReplace,

        PoseSave,
        PoseLoad,
    }
}
