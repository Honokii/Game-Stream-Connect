using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

#if UNITY_EDITOR
namespace TwitchConnect
{
    public class FeatureSerializer
    {
        public static void CreateTriggersInProject(List<Trigger> source, List<Trigger> destination)
        {
            foreach (Trigger t in source)
            {
                if (AssetDatabase.Contains(t))
                    destination.Add(t);
                else
                {
                    string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Twitch Connect 2/Scriptable Objects/Internals/Triggers/Trigger.asset");

                    if (t is Trigger_BitsDonation)
                    {
                        Trigger_BitsDonation _t = ScriptableObject.CreateInstance<Trigger_BitsDonation>();
                        _t.enabled = (t as Trigger_BitsDonation).enabled;
                        _t.inspectorFoldout = (t as Trigger_BitsDonation).inspectorFoldout;
                        _t.minimumBitsAmount = (t as Trigger_BitsDonation).minimumBitsAmount;
                        _t.maximumBitsAmount = (t as Trigger_BitsDonation).maximumBitsAmount;
                        AssetDatabase.CreateAsset(_t, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(_t);
                    }
                    else if (t is Trigger_ChatCommand)
                    {
                        Trigger_ChatCommand _t = ScriptableObject.CreateInstance<Trigger_ChatCommand>();
                        _t.enabled = (t as Trigger_ChatCommand).enabled;
                        _t.inspectorFoldout = (t as Trigger_ChatCommand).inspectorFoldout;
                        _t.command = (t as Trigger_ChatCommand).command;
                        AssetDatabase.CreateAsset(_t, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(_t);
                    }
                    else if (t is Trigger_ChatMessage)
                    {
                        Trigger_ChatMessage _t = ScriptableObject.CreateInstance<Trigger_ChatMessage>();
                        _t.enabled = (t as Trigger_ChatMessage).enabled;
                        _t.inspectorFoldout = (t as Trigger_ChatMessage).inspectorFoldout;
                        _t.chatText = (t as Trigger_ChatMessage).chatText;
                        AssetDatabase.CreateAsset(_t, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(_t);
                    }
                    else if (t is Trigger_NewSubscriber)
                    {
                        Trigger_NewSubscriber _t = ScriptableObject.CreateInstance<Trigger_NewSubscriber>();
                        _t.enabled = (t as Trigger_NewSubscriber).enabled;
                        _t.inspectorFoldout = (t as Trigger_NewSubscriber).inspectorFoldout;
                        AssetDatabase.CreateAsset(_t, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(_t);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }

        public static void CreateActionsInProject(List<Action> source, List<Action> destination)
        {
            foreach (Action a in source)
            {
                if (AssetDatabase.Contains(a))
                    destination.Add(a);
                else
                {
                    string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Twitch Connect 2/Scriptable Objects/Internals/Actions/Action.asset");

                    if (a is Action_BroadcastMessage)
                    {
                        Action_BroadcastMessage _a = ScriptableObject.CreateInstance<Action_BroadcastMessage>();
                        _a.cooldown = (a as Action_BroadcastMessage).cooldown;
                        _a.openOnStart = (a as Action_BroadcastMessage).openOnStart;
                        _a.opened = (a as Action_BroadcastMessage).opened;
                        _a.duration = (a as Action_BroadcastMessage).duration;
                        _a.remainingTime = (a as Action_BroadcastMessage).remainingTime;
                        _a.inspectorFoldout = (a as Action_BroadcastMessage).inspectorFoldout;
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else if (a is Action_Method)
                    {
                        Action_Method _a = ScriptableObject.CreateInstance<Action_Method>();
                        _a.cooldown = (a as Action_Method).cooldown;
                        _a.openOnStart = (a as Action_Method).openOnStart;
                        _a.opened = (a as Action_Method).opened;
                        _a.duration = (a as Action_Method).duration;
                        _a.remainingTime = (a as Action_Method).remainingTime;
                        _a.inspectorFoldout = (a as Action_Method).inspectorFoldout;
                        _a.methods = new Action_Method.OnMethod();
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else if(a is Action_ReplyInChat)
                    {
                        Action_ReplyInChat _a = ScriptableObject.CreateInstance<Action_ReplyInChat>();
                        _a.cooldown = (a as Action_ReplyInChat).cooldown;
                        _a.openOnStart = (a as Action_ReplyInChat).openOnStart;
                        _a.opened = (a as Action_ReplyInChat).opened;
                        _a.duration = (a as Action_ReplyInChat).duration;
                        _a.remainingTime = (a as Action_ReplyInChat).remainingTime;
                        _a.inspectorFoldout = (a as Action_ReplyInChat).inspectorFoldout;
                        _a.messageTemplate = (a as Action_ReplyInChat).messageTemplate;
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else if (a is Action_ViewersPool)
                    {
                        Action_ViewersPool _a = ScriptableObject.CreateInstance<Action_ViewersPool>();
                        _a.cooldown = (a as Action_ViewersPool).cooldown;
                        _a.openOnStart = (a as Action_ViewersPool).openOnStart;
                        _a.opened = (a as Action_ViewersPool).opened;
                        _a.duration = (a as Action_ViewersPool).duration;
                        _a.remainingTime = (a as Action_ViewersPool).remainingTime;
                        _a.inspectorFoldout = (a as Action_ViewersPool).inspectorFoldout;
                        _a.maxPoolSize = (a as Action_ViewersPool).maxPoolSize;
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else if (a is Session_Vote)
                    {
                        Session_Vote _a = ScriptableObject.CreateInstance<Session_Vote>();
                        _a.cooldown = (a as Session_Vote).cooldown;
                        _a.openOnStart = (a as Session_Vote).openOnStart;
                        _a.opened = (a as Session_Vote).opened;
                        _a.duration = (a as Session_Vote).duration;
                        _a.remainingTime = (a as Session_Vote).remainingTime;
                        _a.inspectorFoldout = (a as Session_Vote).inspectorFoldout;
                        _a.allowViewersToCreateNewOptions = (a as Session_Vote).allowViewersToCreateNewOptions;
                        _a.options = (a as Session_Vote).options;
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else if (a is Session_Bet)
                    {
                        Session_Bet _a = ScriptableObject.CreateInstance<Session_Bet>();
                        _a.cooldown = (a as Session_Bet).cooldown;
                        _a.openOnStart = (a as Session_Bet).openOnStart;
                        _a.opened = (a as Session_Bet).opened;
                        _a.duration = (a as Session_Bet).duration;
                        _a.remainingTime = (a as Session_Bet).remainingTime;
                        _a.inspectorFoldout = (a as Session_Bet).inspectorFoldout;
                        _a.allowViewersToCreateNewOptions = (a as Session_Bet).allowViewersToCreateNewOptions;
                        _a.options = (a as Session_Bet).options;
                        _a.winningCondition = (a as Session_Bet).winningCondition;
                        AssetDatabase.CreateAsset(a, path);
                        AssetDatabase.SaveAssets();
                        destination.Add(a);
                    }
                    else
                    {
                        return;
                    }
                }
            }
        }
    }
}
#endif