using System.Collections.Generic;

namespace TwitchConnect
{
    /// <summary>
    /// Contains all viewers that are prohibited from triggering an action. Needed for cooldown system.
    /// </summary>
    public class ViewersHistory
    {
        /// <summary>
        /// Queue of viewers (First-in - First-out) that are not allowed to trigger the action associated with this history.
        /// </summary>
        public Queue<TwitchViewer> queue;

        public ViewersHistory()
        {
            queue = new Queue<TwitchViewer>();
        }

        public bool Contains(int _id)
        {
            foreach (TwitchViewer tv in queue)
            {
                if (_id == tv.id) // not a new cheerer, exit now and do nothing
                {
                    return true;
                }
            }
            return false;
        }
        
        public void Add(int _id)
        {
            queue.Enqueue(new TwitchViewer("", _id));
        }
        
        public TwitchViewer Peek()
        {
            return queue.Peek();
        }
        
        public TwitchViewer Remove()
        {
            return queue.Dequeue();
        }

        public void Update(float deleteEntryCreatedAfter)
        {
            if (queue == null)
                queue = new Queue<TwitchViewer>();

            while (queue.Count > 0) // exit if queue becomes empty
            {
                if (queue.Peek().timestamp < deleteEntryCreatedAfter) // If last item's creation time is PRIOR to the limit, remove it from queue
                    queue.Dequeue();
                else
                    return; // return safely because it's a queue --> Viewers in history are sorted chronologically.
            }
        }
    }
}


