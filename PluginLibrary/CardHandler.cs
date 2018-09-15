using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PluginLibrary
{
    public abstract class CardHandler : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(this);
        }

        public string GetTimeNow()
        {
            return DateTime.Now.ToString("yyyy_MMdd_HHmm_ss_fff");
        }

        public void DelayAction(UnityAction action, int wait = 1)
        {
            if(wait < 0) wait = 0;
            StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                for(int i = 0; i < wait; i++) yield return null;
                action();
            }
        }

        public void UseCard(MsgObject message)
        {
            switch(message.action)
            {
                case MsgObject.Action.SceneSave:
                    Scene_Save(message);
                    break;

                case MsgObject.Action.SceneLoad:
                    Scene_Load(message);
                    break;

                case MsgObject.Action.SceneLoadResolver:
                    Scene_LoadResolver(message);
                    break;

                case MsgObject.Action.SceneImportAll:
                    Scene_ImportAll(message);
                    break;

                case MsgObject.Action.SceneImportAllResolver:
                    Scene_ImportAllResolver(message);
                    break;

                case MsgObject.Action.SceneImportChara:
                    Scene_ImportChara(message);
                    break;

                case MsgObject.Action.SceneImportCharaResolver:
                    Scene_ImportCharaResolver(message);
                    break;


                case MsgObject.Action.CharaSave:
                    Character_Save(message);
                    break;

                case MsgObject.Action.CharaLoadFemale:
                    Character_LoadFemale(message);
                    break;

                case MsgObject.Action.CharaLoadFemaleResolver:
                    Character_LoadFemaleResolver(message);
                    break;

                case MsgObject.Action.CharaLoadMale:
                    Character_LoadMale(message);
                    break;

                case MsgObject.Action.CharaLoadMaleResolver:
                    Character_LoadMaleResolver(message);
                    break;

                case MsgObject.Action.CharaReplaceAll:
                    Character_ReplaceAll(message);
                    break;

                case MsgObject.Action.CharaReplaceAllResolver:
                    Character_ReplaceAllResolver(message);
                    break;

                case MsgObject.Action.CharaReplaceBody:
                    Character_ReplaceBody(message);
                    break;

                case MsgObject.Action.CharaReplaceBodyResolver:
                    Character_ReplaceBodyResolver(message);
                    break;


                case MsgObject.Action.OutfitSave:
                    Outfit_Save(message);
                    break;

                case MsgObject.Action.OutfitLoad:
                    Outfit_Load(message);
                    break;

                case MsgObject.Action.OutfitReplace:
                    Outfit_Replace(message);
                    break;

                    
                case MsgObject.Action.PoseSave:
                    Pose_Save(message);
                    break;

                case MsgObject.Action.PoseLoad:
                    Pose_Load(message);
                    break;
            }
        }
        
        public virtual void Scene_Save(MsgObject message){}
        public virtual void Scene_Load(MsgObject message){}
        public virtual void Scene_LoadResolver(MsgObject message){}
        public virtual void Scene_ImportAll(MsgObject message){}
        public virtual void Scene_ImportAllResolver(MsgObject message){}
        public virtual void Scene_ImportChara(MsgObject message){}
        public virtual void Scene_ImportCharaResolver(MsgObject message){}

        public virtual void Character_Save(MsgObject message){}
        public virtual void Character_LoadFemale(MsgObject message){}
        public virtual void Character_LoadFemaleResolver(MsgObject message){}
        public virtual void Character_LoadMale(MsgObject message){}
        public virtual void Character_LoadMaleResolver(MsgObject message){}
        public virtual void Character_ReplaceAll(MsgObject message){}
        public virtual void Character_ReplaceAllResolver(MsgObject message){}
        public virtual void Character_ReplaceBody(MsgObject message){}
        public virtual void Character_ReplaceBodyResolver(MsgObject message){}

        public virtual void Outfit_Save(MsgObject message){}
        public virtual void Outfit_Load(MsgObject message){}
        public virtual void Outfit_Replace(MsgObject message){}

        public virtual void Pose_Save(MsgObject message){}
        public virtual void Pose_Load(MsgObject message){}
    }
}
