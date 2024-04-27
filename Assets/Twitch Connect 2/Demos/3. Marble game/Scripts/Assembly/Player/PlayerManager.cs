using System.Collections.Generic;
using UnityEngine;
using TwitchConnect;

namespace TwitchConnect.Demo.MarbleGame
{
    /// <summary>
    /// Offers a way for the game manager to interact with the players
    /// </summary>
    [RequireComponent(typeof(GameManager))]
    public class PlayerManager : MonoBehaviour
    {
        public static PlayerManager instance;

        [SerializeField] private GameObject viewerPrefab;
        public Transform startZoneTransform;
        [SerializeField] private float playerStartingClearZoneRadius;
        public int currentStartZoneBounds;

        public List<Player> players = new List<Player>();

        //[SerializeField]
        private string[] botNames = {"accurateanywhere","remembergeorgian","sootheoffice","buildingafter","shriekconviction","stoicresemble","ourselvesgenerally","swanorderly","jellyfaraway","yieldingdismiss","termsoffer","greekafrican","exemplarybombing","reevebone","skillfulpatroon","klutzyuseless","pokeboink","stairsfemur","mournadvocate","draughtexcitable","catholicassured","subtletyaustralian","lapoddball","cluestripped","beaconavocados","reductiondangerous","glandenormous","ramenherald","anarchistflag","mutualspar","salsabooby","ugliestcontest","wallowterrible","bobsleighadidas","developerirritated","musselyoung","samosamanagement","jealouscourier","clewnaturally","emperortension","instanttyke","sportbreath","gruntherself","jumpycathead","pushproficient","buryripen","easeknowledge","feelingharness","balaclavayak","underclothesubiquity","regardlessrifle","hystericalsaveloy","jumpingboorish","squeamishhorned","onionyslacks","sweatyunseasoned","infamousresearcher","stripmight","obviouslangmaid","ceilingavocet","setaceousrid","nyesaid","meetequivalent","anusfive","eyeballsjellied","curfrost","dangeroussensual","recallhedgehog","mischiefkaaba","saffrondutiful","sourtasty","accountantdescent","hardlysweet","eachlever","basslackey","tillerindeed","drownexcellent","innerjunior","incidentbrassiere","oinkthese","waxroad","witherbelch","chimpanzeeterrify","postulatenumber","riptideyeoman","excuseclove","definitivematernal","hurdletrampoline","museproducer","cousinincredible","championreproduce","pepsiapprove","flagmourner","assignmentheel","giggleskiver","girlzebra","groundedrecent","defensiveamazon","concernedbicycling","wallyskateboard","duringastern","rundowntechnical","unaffectedarchery","mousechervil","suggestgaiters","touchsilurian","untiefielding","stareinterview","hilariousinfinity","glovebiological","wellmuddled","meagerconsumer","cobblerream","discountiris","fuelpumpbrave","solarhour","roachagressive","decantertoucan","produceblazerod","engineergymnasium","literallyforesail","aldermanopossum","geocachingexcitement","clownsignpost","vanishcauliflower","hissencounter","turtlewalker","pillockengage","featherguarantee","duallimbic","confessvisa","foxtenuous","lipdisclose","howeverloss","reflecttiara","pensivepouch","elsewherecycling","bossyfasten","harmoniousessential","yemeninovember","marchatmosphere","gratismust","woefulrub","chewbrag","ninnyfamily","haresoda","miletripwire","althoughyours","frockregard","emphasizekeyboard","onlinegeocaching","bushplucky","commitdidactic","whirrmediocre","hattaper","betrayedmelons","emailbuckwheat","undresspolecats","stillparamedic","digenjoin","breamliberian","puddlingexplain","anticipateaching","truthdecor","juicecelery","homelymph","cringlesuspect","blousepercentage","shotinto","cashewssturdy","hoggreens","transhipcloset","elsehandmade","boingbucket","spanpet","hinderedition","titteringmaniacal","keysquat","couldsunglasses","bizarrefunding","dunbirdpigface","neighboremployer","locationzipper","testiclesembarrass","pakistaniactually","myceliumsquish","purseturban","portrayimply","crocodilecolombian","favorcompete","channelingfolk","reactionfanny","middleenchanting","selectiontime","disgustedintestine","meatheadsubstance","melodyphantom","seafowlromaine","essexminor","smithingpersuade" };
        

        #region MB callbacks
        private void Awake()
        {
            if (instance != null && instance != this)
                Destroy(gameObject);    // Delete other next instance(s)

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (GameManager.instance == null)
            {
                Debug.LogWarning("No game manager found, destroying player manager.");
                Destroy(this);
            }
                
            if (players == null)
                players = new List<Player>();
            else
            {
                // do some clean-up on list (must be done after because we can't remove objects from a list being iterated on in a foreach loop)
                foreach (Player p in players)
                {
                    if (p == null)
                        players.Remove(p);
                }
            }
        }
        #endregion

        #region public methods

        public void StartRace()
        {
            if (players == null)
            {
                players = new List<Player>();
                return;
            }

            foreach (Player p in players)
            {
                p.BeginRacing(Time.timeSinceLevelLoad);
            }
        }

        public void EndRace()
        {
            if (players == null)
            {
                players = new List<Player>();
                return;
            }

            foreach (Player p in players)
            {
                if (p.state == Player.EPlayerState.Racing || p.state == Player.EPlayerState.Ready)
                    p.EndRace(Time.timeSinceLevelLoad, Player.EPlayerState.FinishedRaceTimeOut);
            }
        }

        public void DestroyPlayersGameObjects()
        {
            if (players == null)
            {
                players = new List<Player>();
                return;
            }

            foreach (Player p in players)
            {
                p.DestroyPlayersGameObject();
            }
        }

        public void ClearPlayers()
        {
            if (players == null)
            {
                players = new List<Player>();
                return;
            }

            foreach (Player p in players)
            {
                p.DestroyPlayersGameObject();
            }

            players.Clear();
        }

        public void AddPlayer(int userID, string userName)
        {
            InstantiateViewer(userID, userName, false);
        }

        public void AddBot()
        {
            // add this to userdictionnary to not break things
            int botID = Random.Range(0, 99999999);

            bool uniqueIDFound = false;

            while (!uniqueIDFound)
            {
                uniqueIDFound = true; // assume its unique
                foreach (Player p in players)
                {
                    if (botID == p.userId)
                    {
                        uniqueIDFound = false; // it was not unique..!
                        botID = Random.Range(0, 99999999); // retry
                    }   
                }
            }

            string botName = botNames[Random.Range(0, botNames.Length)];

            InstantiateViewer(botID,botName,true);
        }
        #endregion

        #region private methods
        private Vector3 NewPosition()
        {
            return startZoneTransform.position +
                new Vector3(Random.Range(-currentStartZoneBounds, currentStartZoneBounds) * playerStartingClearZoneRadius, 0f, Random.Range(-currentStartZoneBounds, currentStartZoneBounds) * playerStartingClearZoneRadius);
        }

        private void InstantiateViewer(int userID = -1, string userName = "unknown", bool bot = false)
        {
            // make sure player is not already in
            foreach (Player p in players)
            {
                if (p.userId == userID)
                {
                    Debug.LogWarning("Player #" + userID + " named " + (userID == -1 ? userName : UserDictionnary.GetUserName(userID)) + " was already in the game. Did not instantiate player.");
                    return;
                }
            }

            // instantiate
            PlayerMonobehaviour pmb = Instantiate(viewerPrefab, NewPosition(), Quaternion.identity).GetComponent<PlayerMonobehaviour>();
            pmb.gameObject.name = "P: " + userName;

            // check if it overlaps with another player
            int counter = 0;
            while (counter < GameManager.instance.maxPlayers*2 && Physics.CheckSphere(pmb.transform.position, Mathf.Max(pmb.col.bounds.extents.x, pmb.col.bounds.extents.z)))
            {
                pmb.transform.position = NewPosition(); // if so, try another position
                // anti-infinite loop counter (trying approx 2 times the maxplayers because RNG here does not account for previous tries.
                counter++;
                if (counter == GameManager.instance.maxPlayers *2)
                {
                    Debug.LogWarning("Can't find a proper position to spawn Player #" + userID + " named " + (userID == -1 ? userName : UserDictionnary.GetUserName(userID)) + ". Adjust spawn boundaries check method.");
                    Destroy(pmb.gameObject);
                    return;
                }   
            }

            // now create player context that goes with this player's monobehaviour :
            Player context = new Player(pmb, userID, userName, bot ? Player.EPlayerType.Bot : Player.EPlayerType.TwitchViewer);

            // initialize context
            context.InitializePlayer();

            // Finally add it to player's list
            if (players == null)
                players = new List<Player>();

            players.Add(context);
        }
        #endregion
    }
}
