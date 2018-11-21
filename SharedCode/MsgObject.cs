using System;
using MessagePack;

[MessagePackObject(true)]
public class MsgObject
{
    public Action action;
    public string path;
    public string process;

    public static MsgObject Create(Action action, string process, string path)
    {
        return new MsgObject{ action = action, process = process, path = path };
    }

    public void Print()
    {
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"Action: {action}");
        Console.WriteLine($"Process: {process}");
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
