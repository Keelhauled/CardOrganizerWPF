using System;
using MessagePack;

[MessagePackObject(true)]
public class MsgObject
{
    public Type type;
    public Action action;
    public string path;
    public int sex;

    public static MsgObject QuitMsg()
    {
        return new MsgObject{ type = Type.Quit };
    }

    public static MsgObject AddMsg(string path)
    {
        return new MsgObject{ type = Type.Add, path = path };
    }

    public static MsgObject UseMsg(Action action, string path)
    {
        return new MsgObject{ type = Type.Use, action = action, path = path};
    }

    public void Print(int length)
    {
        Console.WriteLine(new string('=', 40));
        Console.WriteLine($"Message length: {length}");
        Console.WriteLine($"Type: {type}");
        Console.WriteLine($"Action: {action}");
        Console.WriteLine($"Path: {path}");
        Console.WriteLine(new string('=', 40));
    }

    public enum Type
    {
        Quit,
        Use,
        Add,
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
        CharaReplaceBody,

        OutfitSave,
        OutfitLoad,
        OutfitLoadAccOnly,
        OutfitLoadClothOnly,
        OutfitReplace,

        PoseSave,
        PoseLoad,
    }
}
