using System.IO;
using System.Linq;
using static BepInEx.Logger;
using BepInEx.Logging;
using MessagePack;

namespace CardOrganizerKK
{
    class Methods_HScene : Methods_Common
    {
        public override void Outfit_Load(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, true, true));
        }

        public override void Outfit_LoadAccOnly(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit accessories [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, false, true));
        }

        public override void Outfit_LoadClothOnly(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit clothing [{Path.GetFileName(message.path)}]");
            PlayLoadSound();
            DelayAction(() => LoadOutfit(message.path, true, false));
        }

        void LoadOutfit(string path, bool loadClothes, bool loadAcs)
        {
            var chara = FindObjectsOfType<ChaControl>().Where((x) => x.sex == 1).First();
            if(chara)
            {
                byte[] bytes = MessagePackSerializer.Serialize(chara.nowCoordinate.clothes);
                byte[] bytes2 = MessagePackSerializer.Serialize(chara.nowCoordinate.accessory);
                chara.nowCoordinate.LoadFile(path);

                if(!loadClothes)
                    chara.nowCoordinate.clothes = MessagePackSerializer.Deserialize<ChaFileClothes>(bytes);

                if(!loadAcs)
                    chara.nowCoordinate.accessory = MessagePackSerializer.Deserialize<ChaFileAccessory>(bytes2);

                chara.Reload(false, true, true, true);
                chara.AssignCoordinate((ChaFileDefine.CoordinateType)chara.chaFile.status.coordinateType);
            }
        }
    }
}
