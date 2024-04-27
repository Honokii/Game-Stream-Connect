using System.Collections.Generic;
/// <summary>
/// Basic infos of channel. Also contains list of viewers used for:
/// - Watching for new viewers (trigger type)
/// - Checking if a user belongs to a specific category
/// </summary>
[System.Serializable]
public class Chatters
{
    public List<string> broadcaster = new List<string>();
    public List<string> vips = new List<string>();
    public List<string> moderators = new List<string>();
    public List<string> staff = new List<string>();
    public List<string> admins = new List<string>();
    public List<string> global_mods = new List<string>();
    public List<string> viewers = new List<string>();

    public HashSet<string> allViewers;

    public void Update()
    {
        if (allViewers == null)
            allViewers = new HashSet<string>();

        allViewers.UnionWith(AddToSet(broadcaster));
        allViewers.UnionWith(AddToSet(vips));
        allViewers.UnionWith(AddToSet(moderators));
        allViewers.UnionWith(AddToSet(staff));
        allViewers.UnionWith(AddToSet(admins));
        allViewers.UnionWith(AddToSet(global_mods));
        allViewers.UnionWith(AddToSet(viewers));
    }

    private HashSet<string> AddToSet(List<string> set)
    {
        HashSet<string> result = new HashSet<string>();

        foreach (string s in set)
        {
            result.Add(s); // if a viewer is added, add it to result
        }

        return result;
    }
}

[System.Serializable]
public class TwitchChannelInfos
{
    public int chatter_count
    {
        get
        {
            return (chatters == null || chatters.allViewers == null) ? 0 : chatters.allViewers.Count;
        }
    }
    public Chatters chatters;
}