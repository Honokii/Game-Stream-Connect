namespace TwitchConnect
{
    /// <summary>
    /// Interface for actions.
    /// </summary>
    public interface IActionable
    {
        // Object management
        void Initialize(); // register on trigger event
        void Update(); // eventually do stuff each update
        void Deinitialize(); // deregister from trigger event

        // Invoking the action
        void OnAction(object sender, object payload, int userID, string userMessage); // subscribed to a trigger's event
    }
}

