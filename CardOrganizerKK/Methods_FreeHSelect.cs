using System;
using System.IO;
using PluginLibrary;
using Harmony;
using static BepInEx.Logger;
using BepInEx.Logging;

namespace CardOrganizerKK
{
    class Methods_FreeHSelect : Methods_Common
    {
        public override void Character_LoadFemale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female [{Path.GetFileName(message.path)}]");
            DelayAction(() => SetupCharacter(message.path, ResultType.Heroine));
        }

        public override void Character_LoadFemaleResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Load female (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => SetupCharacter(message.path, ResultType.Heroine));
        }

        public override void Character_LoadMale(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male [{Path.GetFileName(message.path)}]");
            DelayAction(() => SetupCharacter(message.path, ResultType.Player));
        }

        public override void Character_LoadMaleResolver(MsgObject message)
        {
            Log(LogLevel.Message, $"Load male (resolver) [{Path.GetFileName(message.path)}]");
            ResolverDelay(() => SetupCharacter(message.path, ResultType.Player));
        }

        void SetupCharacter(string path, ResultType type)
        {
            var chaFileControl = new ChaFileControl();
            if(chaFileControl.LoadCharaFile(path, 255, false, true))
            {
                var hscene = FindObjectOfType<FreeHScene>();
                var member = Traverse.Create(hscene).Field("member");

                switch(type)
                {
                    case ResultType.Heroine:
                        member.Field("resultHeroine").Property("Value").SetValue(new SaveData.Heroine(chaFileControl, false));
                        break;

                    case ResultType.Player:
                        member.Field("resultPlayer").Property("Value").SetValue(new SaveData.Player(chaFileControl, false));
                        break;

                    case ResultType.Partner:
                        member.Field("resultPartner").Property("Value").SetValue(new SaveData.Heroine(chaFileControl, false));
                        break;
                }
            }
        }

        enum ResultType
        {
            Heroine,
            Player,
            Partner,
        }
    }
}
