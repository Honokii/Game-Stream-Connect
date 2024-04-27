namespace TwitchConnect
{
    /// <summary>
    /// Interface for all triggers.
    /// </summary>
    public interface ITriggerable
    {
        // Object management
        void Initialize(); // register on bot event
        void Update(); // eventually do stuff each update
        void Deinitialize(); // deregister on bot event

        // Trigger management
        void Enable();
        void Disable();

        // Pulling the trigger
        event OnTriggerSuccesful OnTriggerSuccesful;
    }
}