namespace TwitchConnect
{
    /// <summary>
    /// Interface for viewers history.
    /// </summary>
    public interface IViewersHistory
    {
        void ClearHistory();
        void UpdateHistory();
        bool CanUseCommand(int _id, bool AddToHistory = true);
        void InitializeHistory();
    }
}
