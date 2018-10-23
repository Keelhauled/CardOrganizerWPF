using System.IO;
using System.Linq;
using static BepInEx.Logger;
using BepInEx.Logging;
using Illusion.Game;

namespace CardOrganizerKK
{
    class Methods_HScene : Methods_Common
    {
        public override void Outfit_Load(MsgObject message)
        {
            Log(LogLevel.Message, $"Load outfit [{Path.GetFileName(message.path)}]");
            Utils.Sound.Play(SystemSE.sel);

            DelayAction(() =>
            {
                var chara = FindObjectsOfType<ChaControl>().Where((x) => x.sex == 1).ToList();
                if(chara.Count > 0 && chara[0])
                {
                    chara[0].nowCoordinate.LoadFile(message.path);
                    chara[0].Reload(false, true, true, true);
                }
            });
        }
    }
}
