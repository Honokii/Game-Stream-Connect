using UnityEngine;
using System.Collections.Generic;
using TwitchLib.Client.Events;
using UnityEditor;

namespace TwitchConnect
{
    /// <summary>
    /// Create a voting session.
    /// </summary>
    [System.Serializable]
    public class Session_Vote : Session
    {
        #region Public variables

        /// <summary>
        /// List of options for this vote session.
        /// </summary>
        public List<Option_Vote> options;

        public Session_Vote()
        {
            cooldown = -1; // by default, a viewer can vote only once
            allowViewersToCreateNewOptions = true; // by default, votes options are created by the game, not the viewers
        }

        internal Session_Vote(Session_Vote source) : base(source)
        {
            allowViewersToCreateNewOptions = source.allowViewersToCreateNewOptions;

            Option_Vote[] vo = new Option_Vote[source.options.Count];
            source.options.CopyTo(vo);
            options = new List<Option_Vote>(vo);
        }

        #endregion

        #region Action callbacks

        public override void Initialize()
        {
            base.Initialize();

            if (options == null)
                options = new List<Option_Vote>();

            foreach (Option_Vote vo in options)
            {
                vo.opters = new HashSet<int>();
            }
        }

        public override void Deinitialize()
        {
            if (options != null)
                options.Clear();

            base.Deinitialize();
        }

        public override void OnAction(object sender, object payload, int userID, string userMessage)
        {
            // Can receive:
            // COMMANDS with args
            // Bits with first message after whitespace (will ignore nexts words in bits messages)

            if (!opened)
            {
                return;
            }

            if ((payload is OnChatCommandReceivedArgs) && CanUseCommand(userID) && restriction.HasPrivilege(userID))
            {
                if ((payload as OnChatCommandReceivedArgs).Command.ArgumentsAsList.Count > 0)
                    RegisterVote(userID, (payload as OnChatCommandReceivedArgs).Command.ArgumentsAsList[0]);
            }
            else if (payload is OnMessageReceivedArgs && (payload as OnMessageReceivedArgs).ChatMessage.Bits > 0 && restriction.HasPrivilege(userID)) // special bits usecase
            {
                if (userMessage.Contains(" ")) // there is indeed something after the cheerX message
                {
                    // register vote by removing the first substring before the whitespace
                    RegisterVote(userID, userMessage.Split(' ')[1]);
                }
            }
        }
        #endregion

        #region Vote system

        /// <summary>
        /// Register a new vote (argument) for a User (userID).
        /// <para>If the vote argument exists, the user is added under its corresponding vote options.</para>
        /// <para>If the vote argument does not exist, optionnally create a new vote option depending on flag 'allowViewersToCreateNewOptions'.</para>
        /// </summary>
        /// <param name="_userID">The userID that the vote should be registered for.</param>
        /// <param name="_argument">The vote option (argument as string) the user voted for.</param>
        protected virtual void RegisterVote(int _userID, string _argument)
        {
            if (options == null)
                options = new List<Option_Vote>();

            // find vote option
            foreach (Option_Vote vo in options)
            {
                if (vo.argument == _argument)
                {
                    // found it!
                    vo.Register(_userID);
                    // compare count to other vote options to refresh each voteoption status
                    RefreshOptionsStatus();
                    return;
                }
            }

            // vote option was not found
            if (allowViewersToCreateNewOptions) // if the session is opened
            {
                Option_Vote vo = new Option_Vote(_argument, _argument);
                vo.Register(_userID);
                options.Add(vo);
                RefreshOptionsStatus();
            }

        }

        /// <summary>
        /// Refresh vote option statuses, assuming that votes that have the most voters are winning.
        /// </summary>
        public override void RefreshOptionsStatus()
        {
            if (options.Count == 0)
                return;

            int max = 0;

            if (options.Count == 1)
                options[0].status = Option.EOptionStatus.Winning;

            // find max
            for (int i = 0; i < options.Count; i++)
            {
                max = Mathf.Max(options[i].count, max);
            }

            // check if max appears twice
            int numberOfMax = 0;

            for (int i = 0; i < options.Count; i++)
            {
                if (options[i].count == numberOfMax)
                    numberOfMax++;
            }

            foreach (Option_Vote vo in options)
            {
                if (vo.count == max)
                {
                    vo.status = numberOfMax > 1 ? Option.EOptionStatus.Tie : Option.EOptionStatus.Winning;
                }
                else
                {
                    vo.status = Option.EOptionStatus.Loosing;
                }
            }
        }

        #endregion

#if UNITY_EDITOR

        public override void DrawInspector()
        {
            if (inspectorFoldout)
            {
                base.DrawInspector();
                EditorGUI.indentLevel++;
                SerializedObject o = new SerializedObject(this);
                ShowProperty(o, "allowViewersToCreateNewOptions");
                ShowProperty(o, "options");
                EditorGUI.indentLevel--;

                if (options == null)
                    return;

                foreach (Option_Vote vo in options)
                {
                    if (vo.opters != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(vo.name, GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / 4));
                        EditorGUILayout.LabelField(vo.count.ToString(), GUILayout.MaxWidth(30));
                        EditorGUILayout.LabelField(vo.status.ToString(), GUILayout.MaxWidth(80));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        public override string InspectorName()
        {
            return "Vote session";
        }
#endif
    }
}