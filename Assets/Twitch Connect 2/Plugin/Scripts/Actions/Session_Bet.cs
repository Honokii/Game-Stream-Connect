using System.Collections.Generic;
using TwitchLib.Client.Events;
using UnityEditor;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Create a betting session.
    /// <para>When viewers bet, this searches for an existing bet options with the same numerical (exact) value.</para>
    /// <para>If the option is found, the viewer is added to the list under this option (added using userID).</para>
    /// <para>If the option is not found, you can allow viewers to create a new option using flag 'allowViewersToCreateNewOptions'.</para>
    /// </summary>
    public class Session_Bet : Session
    {
        public enum EWinningCondition
        {
            Highest,
            Lowest,
            Closest,
            ClosestAbove,
            ClosestBelow,
            Furthest
        }

        /// <summary>
        /// What is the winning condition for this bet ? Note: Bets are based on numerical (float) values.
        /// </summary>
        public EWinningCondition winningCondition;

        /// <summary>
        /// List of options for this bet session.
        /// </summary>
        public List<Option_Bet> options;

        public Session_Bet()
        {
            cooldown = -1; // by default, a viewer can bet only once
            allowViewersToCreateNewOptions = true; // by default, bet options are created by the game, not the viewers. Override if needed.
        }

        public Session_Bet(Session_Bet source) : base(source)
        {
            allowViewersToCreateNewOptions = source.allowViewersToCreateNewOptions;

            Option_Bet[] vo = new Option_Bet[source.options.Count];
            source.options.CopyTo(vo);
            options = new List<Option_Bet>(vo);
        }

        #region Action callbacks

        public override void Initialize()
        {
            base.Initialize();

            if (options == null)
                options = new List<Option_Bet>();

            foreach (Option_Bet vo in options)
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
                    RegisterBet(userID, float.Parse((payload as OnChatCommandReceivedArgs).Command.ArgumentsAsList[0], System.Globalization.NumberStyles.AllowDecimalPoint));
            }
            else if (payload is OnMessageReceivedArgs && (payload as OnMessageReceivedArgs).ChatMessage.Bits > 0 && restriction.HasPrivilege(userID)) // special bits usecase
            {
                if (userMessage.Contains(" ")) // there is indeed something after the cheerX message
                {
                    // register vote by removing the first substring before the whitespace
                    RegisterBet(userID, float.Parse(userMessage.Split(' ')[1])); // could be better
                }
            }
        }
        #endregion

        #region Bet system

        /// <summary>
        /// Register a new bet (value) for a User (userID).
        /// <para>If the bet already exists, the user is added under this bet.</para>
        /// <para>If the bet does not exists, optionnally create a new bet depending on flag 'allowViewersToCreateNewOptions'.</para>
        /// </summary>
        /// <param name="_userID">The userID that the bet should be registered for.</param>
        /// <param name="value">The numerical value of the bet that this user just placed.</param>
        protected virtual void RegisterBet(int _userID, float value)
        {
            if (options == null)
                options = new List<Option_Bet>();

            foreach (Option_Bet vo in options)
            {
                if (vo.value == value)
                {
                    // found it!
                    vo.Register(_userID);
                    // compare count to other vote options to refresh each voteoption status
                    RefreshOptionsStatus();
                    return;
                }
            }

            if (allowViewersToCreateNewOptions)
            {
                Option_Bet vo = new Option_Bet(value);
                vo.Register(_userID);
                options.Add(vo);
                RefreshOptionsStatus();
            }

        }

        public override void RefreshOptionsStatus()
        {
            
        }

        /// <summary>
        /// End the bet session by passing the end result.
        /// <para>After ending the bet session, all bet option statuses are updated (winning, loosing, or tie depending on winning condition).</para>
        /// </summary>
        /// <param name="targetResult">The numerical outcome on which all existing bet options will be ranked against depending on winning condition.</param>
        public void EndBetSession(float targetResult = 0f)
        {
            CloseSession();

            if (options == null || options.Count == 0)
                return;

            switch (winningCondition)
            {
                case EWinningCondition.Highest: // not a very interesting winning condition...

                    float highestValue = float.MinValue;

                    foreach (Option_Bet bo in options)
                    {
                        highestValue = Mathf.Max(highestValue, bo.value);
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (bo.value == highestValue)
                        {
                            bo.status = Option.EOptionStatus.Winning;
                        }
                        else
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }

                    break;

                case EWinningCondition.Lowest: // not a very interesting winning condition either...

                    float lowestValue = float.MaxValue;

                    foreach (Option_Bet bo in options)
                    {
                        lowestValue = Mathf.Min(lowestValue, bo.value);
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (bo.value == lowestValue)
                        {
                            bo.status = Option.EOptionStatus.Winning;
                        }
                        else
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }

                    break;

                case EWinningCondition.Closest: // this is interesting now... A tie can happen
                    float minClosestInterval = float.MaxValue;
                    bool multipleClosestOptions = false;

                    foreach (Option_Bet bo in options)
                    {
                        float interval = Mathf.Abs(bo.value - targetResult);

                        if (interval < minClosestInterval)
                        {
                            minClosestInterval = interval;
                        }
                        else if (interval == minClosestInterval)
                        {
                            multipleClosestOptions = true;
                        }
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (Mathf.Abs(bo.value - targetResult) == minClosestInterval)
                        {
                            bo.status = multipleClosestOptions ? Option.EOptionStatus.Tie : Option.EOptionStatus.Winning;
                        }
                        else 
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }
                    break;

                case EWinningCondition.ClosestAbove: // no tie can happen

                    float minClosestAboveInterval = float.MaxValue;

                    foreach (Option_Bet bo in options)
                    {
                        float interval = bo.value - targetResult;

                        if (interval >= 0)
                            minClosestAboveInterval = Mathf.Min(interval, minClosestAboveInterval);
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (bo.value >= targetResult)
                        {
                            bo.status = (bo.value - targetResult == minClosestAboveInterval) ? Option.EOptionStatus.Winning : Option.EOptionStatus.Loosing;
                        }
                        else
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }
                    break;

                case EWinningCondition.ClosestBelow:
                    float minClosestBelowInterval = float.MaxValue;

                    foreach (Option_Bet bo in options)
                    {
                        float interval = targetResult - bo.value;

                        if (interval >= 0)
                            minClosestAboveInterval = Mathf.Min(interval, minClosestBelowInterval);
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (bo.value <= targetResult)
                        {
                            bo.status = (targetResult - bo.value == minClosestBelowInterval) ? Option.EOptionStatus.Winning : Option.EOptionStatus.Loosing;
                        }
                        else
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }
                    break;

                case EWinningCondition.Furthest:
                    float maxFurthestInterval = 0f;
                    bool multipleFurthestOptions = false;

                    foreach (Option_Bet bo in options)
                    {
                        float interval = Mathf.Abs(bo.value - targetResult);

                        if (interval > maxFurthestInterval)
                        {
                            maxFurthestInterval = interval;
                        }
                        else if (interval == maxFurthestInterval)
                        {
                            multipleFurthestOptions = true;
                        }
                    }

                    foreach (Option_Bet bo in options)
                    {
                        if (Mathf.Abs(bo.value - targetResult) == maxFurthestInterval)
                        {
                            bo.status = multipleFurthestOptions ? Option.EOptionStatus.Tie : Option.EOptionStatus.Winning;
                        }
                        else
                        {
                            bo.status = Option.EOptionStatus.Loosing;
                        }
                    }
                    break;

                default:
                    break;
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
                ShowProperty(o, "winningCondition");
                ShowProperty(o, "options");
                EditorGUI.indentLevel--;

                if (options == null)
                    return;

                foreach (Option_Bet vo in options)
                {
                    if (vo.opters != null)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(vo.value.ToString(), GUILayout.MaxWidth(EditorGUIUtility.currentViewWidth / 6));
                        EditorGUILayout.LabelField(vo.count.ToString(), GUILayout.MaxWidth(30));
                        EditorGUILayout.LabelField(vo.status.ToString(), GUILayout.MaxWidth(80));
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        public override string InspectorName()
        {
            return "Bet Session";
        }
#endif
    }
}

