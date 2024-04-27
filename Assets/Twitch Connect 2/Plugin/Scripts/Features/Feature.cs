using System.Collections.Generic;
using TwitchLib.Client.Events;
using TwitchLib.PubSub.Events;
using UnityEngine;

namespace TwitchConnect
{
    /// <summary>
    /// Base class for features. Associate triggers to actions.
    /// </summary>
    [System.Serializable]
    public class Feature
    {
        /// <summary>
        /// The string by which your feature can be accessed.
        /// <para>TIP : it is advised to cache the reference to a feature as the search method is not optimized.</para>
        /// </summary>
        public string name;

        /// <summary>
        /// Is this feature enabled ? If not, triggers won't call actions.
        /// </summary>
        public bool enabled;

        /// <summary>
        /// List of Triggers listed in this feature. Each trigger will trigger ALL actions if raised.
        /// </summary>
        public List<Trigger> triggers;
        /// <summary>
        /// List of Actions listed in this feature. ALL actions will be executed if any of the trigger is raised.
        /// </summary>
        public List<Action> actions;

        [HideInInspector]
        public bool inspectorFoldout;
        [HideInInspector]
        public int selectedTrigger;
        [HideInInspector]
        public int selectedAction;

        public Feature(string _name, Trigger trigger, Action action)
        {
            name = _name;
            enabled = true;

            triggers = new List<Trigger>();
            actions = new List<Action>();

            triggers.Add(trigger);
            actions.Add(action);
        }

        public Feature()
        {
            name = "New Feature";
            enabled = true;
            triggers = new List<Trigger>();
            actions = new List<Action>();
        }

        public Feature(Feature source)
        {
            name = source.name;
            enabled = source.enabled;

            // ===== get all actions and triggers, and create instances of them if they are not in AssetDB
            triggers = new List<Trigger>();
            actions = new List<Action>();
#if UNITY_EDITOR
            FeatureSerializer.CreateTriggersInProject(source.triggers, triggers);
            FeatureSerializer.CreateActionsInProject(source.actions, actions);
#endif

            inspectorFoldout = source.inspectorFoldout;
            selectedTrigger = source.selectedTrigger;
            selectedAction = source.selectedAction;
        }

        public void Initialize()
        {
            foreach (Trigger trigger in triggers)
            {
                // init the triggers
                trigger.Initialize();

                // Add feature listener to trigger
                trigger.OnTriggerSuccesful += OnTriggerSuccesful;
            }

            foreach (Action action in actions)
            {
                action.Initialize();
            }
        }

        public void Deinitialize()
        {
            foreach (Trigger trigger in triggers)
            {
                trigger.Deinitialize();
                trigger.OnTriggerSuccesful -= OnTriggerSuccesful;
            }

            foreach (Action action in actions)
            {
                action.Deinitialize();
            }
        }

        public void ReInitialize()
        {
            foreach (Trigger trigger in triggers)
            {
                trigger.ReInitialize();
            }
        }

        public void Update()
        {
            foreach (Trigger trigger in triggers)
            {
                trigger.Update();
            }

            if (!enabled)
                return;

            // don't decrease actions timers if feature is disabled.
            foreach (Action action in actions)
            {
                action.Update();
            }
        }

        /// <summary>
        /// Enable this feature and all triggers of this feature. Actions should be opened / started manually depending on what is wanted.
        /// </summary>
        public void Enable()
        {
            enabled = true;

            foreach (Trigger t in triggers)
            {
                t.enabled = true;
            }
        }

        /// <summary>
        /// Disable this feature and all triggers of this feature. Suspends all action timers when the feature is disabled.
        /// </summary>
        public void Disable()
        {
            enabled = false;

            foreach (Trigger t in triggers)
            {
                t.enabled = false;
            }
        }

        /// <summary>
        /// Enable or Diasble this feature and all triggers of this feature. All actions timers are suspended when the feature is disabled.
        /// </summary>
        /// <param name="_enabled">Enable ?</param>
        public void SetActive(bool _enabled)
        {
            enabled = _enabled;

            foreach (Trigger t in triggers)
            {
                t.enabled = enabled;
            }
        }

        internal string GetCommandList() // comma-separated command list
        {
            string commandList = "";

            foreach (Trigger t in triggers)
            {
                if (t is Trigger_ChatCommand && (t as Trigger_ChatCommand).GetCommandName().Length > 0)
                {
                    if (commandList.Length > 0) // not the first one
                        commandList += ", ";

                    commandList += (t as Trigger_ChatCommand).GetCommandName();
                }
            }

            return commandList;
        }

        private void OnTriggerSuccesful(object sender, object payload, int userID, string userMessage)
        {
            if (!enabled)
                return;

            foreach (Action action in actions)
            {
                action.OnAction(sender, payload, userID, userMessage);
            }
        }
    }
}

