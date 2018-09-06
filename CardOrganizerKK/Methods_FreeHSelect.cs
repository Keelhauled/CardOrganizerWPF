using System;
using PluginLibrary;
using Harmony;

namespace CardOrganizerKK
{
    class Methods_FreeHSelect : CardHandler
    {
        public override void Character_LoadFemale(MsgObject message)
        {
            SetupCharacter(message.path, ResultType.Heroine);
        }

        public override void Character_LoadMale(MsgObject message)
        {
            SetupCharacter(message.path, ResultType.Player);
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
