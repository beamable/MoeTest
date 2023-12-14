using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Content;
using Beamable.Server.Clients;
using Custom_Content;
using Helper;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace MatchMaking
{
    public enum SessionState
    {
        None,
        Connecting,
        Connected,
        Disconnecting,
        Disconnected

    }
    
    [System.Serializable]
    public class MatchmakingServiceExampleData
    {
        public SessionState SessionState = SessionState.None;
        public string MatchId = "";
        public bool CanStart { get { return SessionState == SessionState.Disconnected;}} 
        public bool CanCancel { get { return SessionState == SessionState.Connected;}} 
    }
    
    [System.Serializable]
    public class RefreshedUnityEvent : UnityEvent<MatchmakingServiceExampleData> { }
    
    public class AndreaMatchMakingTest : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private SimGameTypeRef gameTypeRef = null;
        [SerializeField] private string powerStatString = "power";
        [SerializeField] private TMP_InputField powerStatValueInputField = null;
        [SerializeField] private MyDebugger myDebugger = null;
        
        [Header("Random Match Making")]
        [SerializeField] private Toggle useRandomMatchToggle = null;
        [SerializeField] private SimGameTypeRef[] randomMatchRefs = null;
        [SerializeField] private TMP_InputField fieldSizeInputField = null;
        [SerializeField] private TextMeshProUGUI randomMatchNameText = null;
        
        #endregion

        #region PRIVATE_VARIABLES
        
        private BeamContext _beamContext;
        private GameServerClient _gameServerClient = null;
        private MyMatchMaking _myMatchmaking = null;
        private SimGameType _simGameType = null;
        private MatchmakingServiceExampleData _data = new MatchmakingServiceExampleData();

        #endregion

        #region PUBLIC_VARIABLES
        
        private bool _useRandomMatch = false;

        #endregion

        #region UNITY_CALLS
        
        private void Start()
        {
            myDebugger.SimpleDebug($"Check the Random Match Toggle then SetupBeamable to start...");
            // SetupBeamable();
        }

        #endregion

        #region PRIVATE_METHODS

        public async void SetupBeamable()
        {
            _useRandomMatch = useRandomMatchToggle.isOn;
            myDebugger.SimpleDebug("Beamable Setup Start...");
            _beamContext = await BeamContext.Default.Instance;
            myDebugger.SimpleDebug($"Beamable Setup Complete!");
            myDebugger.SimpleDebug($"Player Id = {_beamContext.PlayerId}");
            if(_useRandomMatch) return;
            SimGameTypeRef gameTypeRefToUse = null;
            gameTypeRefToUse = _useRandomMatch ? GetRandomMatchRef() : gameTypeRef;
            _simGameType = await gameTypeRefToUse.Resolve();
            
            _data.SessionState = SessionState.Disconnected;
            myDebugger.SimpleDebug($"Game Type = {_simGameType.Id}");
            _myMatchmaking = new MyMatchMaking(_beamContext.Api.Experimental.MatchmakingService, 
                _simGameType,
                _beamContext.PlayerId,
                myDebugger);
            
            _myMatchmaking.OnProgress.AddListener(MyMatchMaking_OnProgress);
            _myMatchmaking.OnComplete.AddListener(MyMatchMaking_OnComplete);
            _myMatchmaking.OnError.AddListener(MyMatchMaking_OnError);
            
        }
        
        private SimGameTypeRef GetRandomMatchRef()
        {
            if(!int.TryParse(fieldSizeInputField.text, out var currentFieldSize))
            {
                currentFieldSize = 4;
            }
            
            var generateId = $"game_types.random_match_{currentFieldSize}x{currentFieldSize}";
            myDebugger.SimpleDebug($"getrandommatchref()...{generateId}");
            randomMatchNameText.text = generateId;
            return randomMatchRefs.FirstOrDefault(match => match.Id == generateId);
        }

        public async void OnSubscribeToRandomMatch()
        {
            SimGameTypeRef gameTypeRefToUse = null;
            gameTypeRefToUse = GetRandomMatchRef();
            _simGameType = await gameTypeRefToUse.Resolve();
            
            _data.SessionState = SessionState.Disconnected;
            myDebugger.SimpleDebug($"Game Type = {_simGameType.Id}");
            _myMatchmaking = new MyMatchMaking(_beamContext.Api.Experimental.MatchmakingService, 
                _simGameType,
                _beamContext.PlayerId,
                myDebugger);
            
            _myMatchmaking.OnProgress.AddListener(MyMatchMaking_OnProgress);
            _myMatchmaking.OnComplete.AddListener(MyMatchMaking_OnComplete);
            _myMatchmaking.OnError.AddListener(MyMatchMaking_OnError);
        }
        
        private void MyMatchMaking_OnProgress(MyMatchMakingResult myMatchMakingResult)
        {
            myDebugger.SimpleDebug($"MyMatchMaking_OnProgress()...{myMatchMakingResult.ErrorMessage}");
            _data.SessionState = SessionState.Connecting;
        }
        
        private void MyMatchMaking_OnComplete(MyMatchMakingResult myMatchMakingResult)
        {
            myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...{myMatchMakingResult.MyMatchmakingHandle.Match.matchId}");
            myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Teams = {myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].name}");
            myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Team player count {myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].players.Count}");
            foreach (var player in myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].players)
            {
                myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Team player {player}");
            }
            
            _data.SessionState = SessionState.Connected;
        }
        
        private void MyMatchMaking_OnError(MyMatchMakingResult myMatchMakingResult)
        {
            myDebugger.SimpleDebug($"MyMatchMaking_OnError()...{myMatchMakingResult.ErrorMessage}");
            _data.SessionState = SessionState.Disconnected;
        }

        #endregion

        #region PUBLIC_METHODS

        public async void SetPowerStat()
        {
            int.TryParse(powerStatValueInputField.text, out var powerStatValue);
            var service = _beamContext.Microservices().GameServer();
            service.SetPowerStat(powerStatString, powerStatValue.ToString());
            myDebugger.SimpleDebug($"{powerStatString} = {powerStatValue} for {_beamContext.PlayerId}");
        }
        

        public async void StartMatchMaking()
        {
            myDebugger.SimpleDebug("Connecting to match...");
            _data.SessionState = SessionState.Connecting;
            await _myMatchmaking.StartMatchMaking();
        }
        
        public async void CancelMatchMaking()
        {
            myDebugger.SimpleDebug("Disconnecting from match...");
            _data.SessionState = SessionState.Disconnecting;
            await _myMatchmaking.CancelMatchMaking();
            
            myDebugger.SimpleDebug("Match Disconnected!");
            _data.SessionState = SessionState.Disconnected;
        }

        #endregion

        
    }
}