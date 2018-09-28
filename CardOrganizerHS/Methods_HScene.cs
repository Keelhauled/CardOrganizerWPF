using System;

namespace CardOrganizerHS
{
    class Methods_HScene : Methods_Common
    {
        public override void Character_LoadFemale(MsgObject message)
        {
            var chara = Manager.Character.Instance.dictFemale[0];
            chara.chaFile.Load(message.path);
            chara.Reload(false, false, false);
            chara.UpdateBustSoftnessAndGravity();
        }

        public override void Character_LoadMale(MsgObject message)
        {
            var chara = Manager.Character.Instance.dictMale[0];
            chara.chaFile.Load(message.path);
            chara.Reload(false, false, false);
            chara.maleStatusInfo.visibleSon = false;
        }

        public override void Outfit_Load(MsgObject message)
        {
            var chara = Manager.Character.Instance.dictFemale[0];
            chara.clothesInfo.Load(message.path);
            chara.chaFile.SetCoordinateInfo(chara.statusInfo.coordinateType);
            chara.Reload(false, false, false);

            //if(chara.Sex == 0)
            //    (chara as CharMale).maleStatusInfo.visibleSon = false;
            //else
                (chara as CharFemale).UpdateBustSoftnessAndGravity();
        }
    }
}
