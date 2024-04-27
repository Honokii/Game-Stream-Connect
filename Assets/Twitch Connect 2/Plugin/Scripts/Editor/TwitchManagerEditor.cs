using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace TwitchConnect
{
    [CustomEditor(typeof(TwitchManager))]
    public class TwitchManagerEditor : Editor
    {
        private TwitchManager twitchManager;

        // ==== tabs ==========

        int currentTab;
        string[] tabNames = { "Set-up", "Tester", "Settings"};

        // ===== Features =====
        Vector2 scrollPos;

        static List<string> triggerList = new List<string>
    {
        "Add",
        "Chat Command",
        "Bits donation",
        "New subscriber",
        "New follower",
        "Chat message",
        "New viewer",
        "Viewer left",
        "Custom reward",
        "Any custom reward",
        "Channel raid"
    };

        static List<string> actionList = new List<string>
    {
        "Add",
        "Invoke methods",
        "Reply in chat",
        "Broadcast message",
        "Pool",
        "Vote",
        "Bet",
        "Log"
    };

        // ==== Inspector =====
        private static GUILayoutOption miniButtonWidth = GUILayout.Width(20f);
        private float deltaTime;
        private Stopwatch stopwatch;
        // ===== Styles ========
        Color twitchColor = new Color(145, 70, 255);
        Color twitchColorLight = new Color(177, 157, 216);

        private void OnEnable()
        {
            twitchManager = target as TwitchManager;

            deltaTime = 0f;
            if (Application.isPlaying)
                stopwatch = new Stopwatch();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (twitchManager.settings.editorConstantRepaint && Application.isPlaying)
                stopwatch.Start();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("settings.autoConnect"));
            //twitchManager.settings.autoConnect = EditorGUILayout.ToggleLeft("Auto-connect", twitchManager.settings.autoConnect, GUILayout.Width(145));
            serializedObject.ApplyModifiedProperties();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(4f);
            if (twitchManager.settings.editorConstantRepaint)
            {
                if (Application.isPlaying)
                    EditorGUILayout.LabelField("(" + deltaTime.ToString() + "ms)", EditorStyles.label, GUILayout.Width(40f));
                else
                {
                    if (twitchManager.settings.editorConstantRepaint)
                        EditorGUILayout.LabelField("Force refresh ON", EditorStyles.label, GUILayout.Width(100f));
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            currentTab = GUILayout.Toolbar(currentTab, tabNames);

            switch (currentTab)
            {
                case 0:
                    DrawSetup();
                    break;

                case 1:
                    DrawTester();
                    break;

                case 2:
                    DrawSettings();
                    break;

                default:
                    currentTab = 0;
                    break;
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();

            if (twitchManager.settings.editorConstantRepaint && Application.isPlaying)
            {
                deltaTime = stopwatch.ElapsedMilliseconds;
                stopwatch.Reset();
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return twitchManager.settings.editorConstantRepaint;
        }

        #region Default inspector

        private void DrawRawView()
        {
            EditorGUILayout.LabelField("Monitor live changes in data during playmode. Use Set-up tab to create features or add actions or triggers", EditorStyles.wordWrappedLabel);
        }

        #endregion

        #region Setup
        private void DrawSetup()
        {
            //EditorGUILayout.Space();
            //DrawImportExport();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);
            GuiLine();

            int featureSize = twitchManager.features.Count;

            foreach (Feature f in twitchManager.features)
            {
                DrawFeature(f);
                if (featureSize != twitchManager.features.Count) // abort if feature list is modified
                    break;
            }

            if (GUILayout.Button("Add new", GUILayout.Width(100f)))
            {
                twitchManager.features.Add(new Feature());
            }
        }

        private void DrawFeature(Feature f)
        {
            EditorGUILayout.BeginVertical("GroupBox");
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginHorizontal();
            f.inspectorFoldout = EditorGUILayout.Foldout(f.inspectorFoldout, f.name, true, EditorStyles.foldout) ;
            if (GUILayout.Button("x", EditorStyles.miniButton, miniButtonWidth)) twitchManager.features.Remove(f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4);

            if (f.inspectorFoldout)
            {
                f.name = EditorGUILayout.TextField("Feature name", f.name);
                f.enabled = EditorGUILayout.Toggle("Enabled", f.enabled);

                EditorGUILayout.Space(4);

                // ===== Triggers =======
                DrawTriggers(f);

                EditorGUILayout.Space(8);
                // ======== ACTIONS ============
                DrawActions(f);
            }

            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }

        private void DrawTriggers(Feature f)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);

            f.selectedTrigger = 0;
            f.selectedTrigger = EditorGUILayout.Popup(f.selectedTrigger, triggerList.ToArray());
            Trigger newTrigger = null;
            switch (f.selectedTrigger)
            {
                case 1:
                    newTrigger = CreateInstance<Trigger_ChatCommand>();
                    break;

                case 2:
                    newTrigger = CreateInstance<Trigger_BitsDonation>();
                    break;

                case 3:
                    newTrigger = CreateInstance<Trigger_NewSubscriber>();
                    break;

                case 4:
                    newTrigger = CreateInstance<Trigger_NewFollower>();
                    break;

                case 5:
                    newTrigger = CreateInstance<Trigger_ChatMessage>();
                    break;

                case 6:
                    newTrigger = CreateInstance<Trigger_NewViewer>();
                    break;

                case 7:
                    newTrigger = CreateInstance<Trigger_ViewerLeft>();
                    break;

                case 8:
                    newTrigger = CreateInstance<Trigger_NewCustomReward>();
                    break;

                case 9:
                    newTrigger = CreateInstance<Trigger_NewCustomReward_Any>();
                    break;

                case 10:
                    newTrigger = CreateInstance<Trigger_ChannelRaid>();
                    break;

                default:
                    break;
            }
            if (newTrigger != null)
            {
                newTrigger.Initialize();
                f.triggers.Add(newTrigger);
            }

            EditorGUILayout.EndHorizontal();

            int size = f.triggers.Count;

            foreach (Trigger t in f.triggers)
            {
                if (t == null)
                {
                    f.triggers.Remove(t);
                    break;
                } 
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                DrawTrigger(f, t);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                if (f.triggers.Count != size) // abort if list has been modified
                    break;
            }


        }

        private void DrawTrigger(Feature f, Trigger t)
        {
            int horizontalSize = (int) (Screen.width * 0.77f);
            horizontalSize = Mathf.Max(horizontalSize, 150);

            EditorGUILayout.BeginVertical("HelpBox", GUILayout.Width(horizontalSize));
            EditorGUILayout.BeginHorizontal();
            t.inspectorFoldout = EditorGUILayout.Foldout(t.inspectorFoldout, t.InspectorName(), true, EditorStyles.foldout);

            if (GUILayout.Button("x", EditorStyles.miniButton, miniButtonWidth))
            {
                t.Deinitialize();
                f.triggers.Remove(t);
            }
            EditorGUILayout.EndHorizontal();

            DrawTriggerDetails(t);
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawTriggerDetails(Trigger t)
        {
            t.DrawInspector();
        }

        private void DrawActions(Feature f)
        {


            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            f.selectedAction = 0;
            f.selectedAction = EditorGUILayout.Popup(f.selectedAction, actionList.ToArray());
            Action newAction = null;
            switch (f.selectedAction)
            {
                case 1:
                    newAction = CreateInstance<Action_Method>();
                    break;

                case 2:
                    newAction = CreateInstance < Action_ReplyInChat>();
                    break;

                case 3:
                    newAction = CreateInstance <Action_BroadcastMessage>();
                    break;

                case 4:
                    newAction = CreateInstance<Action_ViewersPool>();
                    break;

                case 5:
                    newAction = CreateInstance<Session_Vote>();
                    break;

                case 6:
                    newAction = CreateInstance<Session_Bet>();
                    break;

                case 7:
                    newAction = CreateInstance<Action_Log>();
                    break;

                default:
                    break;
            }

            if (newAction != null)
            {
                newAction.Initialize();
                f.actions.Add(newAction);
            }

            EditorGUILayout.EndHorizontal();

            int size = f.actions.Count;

            foreach (Action a in f.actions)
            {
                if (a == null)
                {
                    f.actions.Remove(a);
                    break;
                }
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.Space();
                DrawAction(f, a);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);

                if (f.actions.Count != size) // abort if list has been modified
                    break;
            }
        }

        private void DrawAction(Feature f, Action a)
        {
            int horizontalSize = (int)(Screen.width * 0.77f);
            horizontalSize = Mathf.Max(horizontalSize, 150);

            EditorGUILayout.BeginVertical("HelpBox", GUILayout.Width(horizontalSize));
            EditorGUILayout.BeginHorizontal();
            a.inspectorFoldout = EditorGUILayout.Foldout(a.inspectorFoldout, a.InspectorName(), true, EditorStyles.foldout);

            if (GUILayout.Button("x", EditorStyles.miniButton, miniButtonWidth))
            {
                a.Deinitialize();
                f.actions.Remove(a);
            }
            EditorGUILayout.EndHorizontal();

            DrawActionDetails(a);
            EditorGUILayout.Space(3);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        private void DrawActionDetails(Action a)
        {
            a.DrawInspector();
        }

        private void DrawImportExport()
        {
            EditorGUILayout.LabelField("Save / Load", EditorStyles.boldLabel);
            GuiLine();
            EditorGUILayout.Space();

            twitchManager.featureSet = EditorGUILayout.ObjectField("Current file", twitchManager.featureSet, typeof(FeatureSet), false) as FeatureSet;


            if (twitchManager.featureSet == null)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save as new", EditorStyles.miniButton, GUILayout.Width(110f)))
                {
                    twitchManager.ExportFeatureSetAsNew();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Save current", EditorStyles.miniButton, GUILayout.Width(110f)))
                {
                    twitchManager.ExportFeatureSet();
                }
                EditorGUILayout.Space();
                if (GUILayout.Button("Load from asset", EditorStyles.miniButton, GUILayout.Width(110f)))
                {
                    twitchManager.ImportFeatureSet(twitchManager.featureSet);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        #endregion

        #region Settings
        private void DrawSettings()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Manage settings.", EditorStyles.label);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);

            ShowProperty("settings.singleton");
            ShowProperty("settings.verbose");
            ShowProperty("settings.commandPrefix");

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Bot", EditorStyles.boldLabel);

            ShowProperty("settings.useBot");
            if (twitchManager.settings.useBot)
            {
                ShowProperty("botUsername", "Bot username");
                ShowProperty("botOAuthToken", "Bot OAuth token");
                Secrets.IRC_bot_oauth = twitchManager.botUsername;
                Secrets.IRC_bot_oauth = twitchManager.botOAuthToken;
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Auto messages", EditorStyles.boldLabel);
            ShowProperty("settings.listCommand");
            ShowProperty("settings.helpCommand");
            ShowProperty("settings.helpMessage");

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GUI", EditorStyles.boldLabel);

            ShowProperty("settings.editorConstantRepaint", "Force Inspector refresh");
            if (twitchManager.settings.editorConstantRepaint) EditorGUILayout.LabelField("Warning: substantial performance drawback during playmode", EditorStyles.label);
            ShowProperty("settings.builtinGui", "Built-in GUI in game");
            ShowProperty("settings.uIKeyCode");
        }
        #endregion

        #region Tester
        private void DrawTester()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Use test messages for your own test scenario.", EditorStyles.wordWrappedLabel);
            EditorGUILayout.Space();

            twitchManager.testCase = EditorGUILayout.ObjectField("Current test case", twitchManager.testCase, typeof(TestCase), false) as TestCase;

            if (twitchManager.testCase == null)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Please add or create a test case to trigger test messages.", EditorStyles.wordWrappedLabel);
                if (GUILayout.Button("Create new"))
                {
                    TestCase asset = CreateInstance<TestCase>();

                    string name = AssetDatabase.GenerateUniqueAssetPath("Assets/Twitch Connect 2/Objects/Test cases/New Test Case.asset");
                    AssetDatabase.CreateAsset(asset, name);
                    AssetDatabase.SaveAssets();

                    //EditorUtility.FocusProjectWindow();
                    //Selection.activeObject = asset;

                    // keep the ref to the newly created object
                    twitchManager.testCase = asset;
                }
                return;
            }

            DrawUnitTest("Chat message", twitchManager.testCase.chatMessages, TestChatMessage);
            DrawUnitTest("Chat mess. with args", twitchManager.testCase.chatCommandsWithArgument, TestChatCommandWithArguments);
            DrawUnitTest("Chat command", twitchManager.testCase.chatCommands, TestChatCommand);
            DrawUnitTest("Bits donation", twitchManager.testCase.bitsDonations, TestBitsDonation);
            DrawUnitTest("New suscriber", twitchManager.testCase.newSuscribers, TestNewSuscriber);
            DrawUnitTest("Other message", twitchManager.testCase.otherMessage, TestOtherMessages);
            DrawUnitTest("Channel reward", twitchManager.testCase.channelRewards, TestChannelRewards);

            EditorGUILayout.Space();



            GuiLine();
            EditorGUILayout.LabelField("Message Sniffer", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Copy and paste messages into a Test Case for further use.", EditorStyles.label);

            EditorGUILayout.Space();

            if (twitchManager.testCase != null)
            {
                twitchManager.testCase.inspectorFoldout = EditorGUILayout.Foldout(twitchManager.testCase.inspectorFoldout, "Last messages received");

                if (twitchManager.testCase.inspectorFoldout)
                {
                    foreach (string message in twitchManager.testCase.lastMessages)
                    {
                        EditorGUILayout.TextArea(message);
                    }
                }
            }
        }

        private void DrawUnitTest(string label, string[] messages, System.Func<int, bool> method)
        {
            int size = Mathf.Min(messages.Length, 8);

            if (size == 0)
                return;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(label);

            if (size == 1)
            {
                if (messages[0].Length > 0 && GUILayout.Button(new GUIContent("1", messages[0]), EditorStyles.miniButtonLeft, miniButtonWidth))
                    method(0);
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (messages[0].Length > 0 && GUILayout.Button(new GUIContent("1", messages[0]), EditorStyles.miniButtonLeft, miniButtonWidth))
                                method(0);
                            break;

                        case 7:
                            if (messages[7].Length > 0 && GUILayout.Button(new GUIContent("8", messages[7]), EditorStyles.miniButtonRight, miniButtonWidth))
                                method(7);
                            break;

                        default:
                            if (messages[i].Length > 0 && GUILayout.Button(new GUIContent((i + 1).ToString(), messages[i]), EditorStyles.miniButtonMid, miniButtonWidth))
                                method(i);
                            break;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

        }

        private bool TestOtherMessages(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.otherMessage, index);
            return false;
        }

        private bool TestChatMessage(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.chatMessages, index);
            return false;
        }

        private bool TestChatCommand(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.chatCommands, index);
            return false;
        }

        private bool TestChatCommandWithArguments(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.chatCommandsWithArgument, index);
            return false;
        }

        private bool TestBitsDonation(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.bitsDonations, index);
            return false;
        }

        private bool TestChannelRewards(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestPubSub(ref twitchManager, twitchManager.testCase.channelRewards, index);
            return false;
        }

        private bool TestNewSuscriber(int index)
        {
            if (twitchManager.testCase != null)
                return twitchManager.testCase.SendTestMessage(ref twitchManager, twitchManager.testCase.newSuscribers, index);
            return false;
        }
        #endregion

        public void ShowProperty(string name)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name));
            serializedObject.ApplyModifiedProperties();
        }

        public void ShowProperty(string name, string label)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(name), new GUIContent(label));
            serializedObject.ApplyModifiedProperties();
        }

        void GuiLine(int i_height = 1)

        {
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);

            rect.height = i_height;

            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));

        }
    }
}
