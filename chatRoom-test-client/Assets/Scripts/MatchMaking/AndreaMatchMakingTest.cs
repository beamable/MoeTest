using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Content;
using Beamable.Server.Clients;
using Custom_Content;
using Helper;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
        
        #endregion

        #region PRIVATE_VARIABLES
        
        private MyDebugger _myDebugger = null;
        private BeamContext _beamContext;
        private GameServerClient _gameServerClient = null;
        private MyMatchMaking _myMatchmaking = null;
        private SimGameType _simGameType = null;
        private MatchmakingServiceExampleData _data = new MatchmakingServiceExampleData();

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS
        
        private void Start()
        {
            _myDebugger = FindObjectOfType<MyDebugger>();
            SetupBeamable();
        }

        #endregion

        #region PRIVATE_METHODS

        private async void SetupBeamable()
        {
            await Task.Delay(2000);
            _myDebugger.SimpleDebug("Beamable Setup Start...");
            _beamContext = await BeamContext.Default.Instance;
            _simGameType = await gameTypeRef.Resolve();
            _data.SessionState = SessionState.Disconnected;
            _myDebugger.SimpleDebug($"Beamable Setup Complete!");
            _myDebugger.SimpleDebug($"Player Id = {_beamContext.PlayerId}");
            
            _myMatchmaking = new MyMatchMaking(_beamContext.Api.Experimental.MatchmakingService, 
                _simGameType,
                _beamContext.PlayerId,
                _myDebugger);
            
            _myMatchmaking.OnProgress.AddListener(MyMatchMaking_OnProgress);
            _myMatchmaking.OnComplete.AddListener(MyMatchMaking_OnComplete);
            _myMatchmaking.OnError.AddListener(MyMatchMaking_OnError);
            
        }
        
        private void MyMatchMaking_OnProgress(MyMatchMakingResult myMatchMakingResult)
        {
            _myDebugger.SimpleDebug($"MyMatchMaking_OnProgress()...{myMatchMakingResult.ErrorMessage}");
            _data.SessionState = SessionState.Connecting;
        }
        
        private void MyMatchMaking_OnComplete(MyMatchMakingResult myMatchMakingResult)
        {
            _myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...{myMatchMakingResult.MyMatchmakingHandle.Match.matchId}");
            _myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Teams = {myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].name}");
            _myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Team player count {myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].players.Count}");
            foreach (var player in myMatchMakingResult.MyMatchmakingHandle.Match.teams[0].players)
            {
                _myDebugger.SimpleDebug($"MyMatchMaking_OnComplete()...Team player {player}");
            }
            
            _data.SessionState = SessionState.Connected;
        }
        
        private void MyMatchMaking_OnError(MyMatchMakingResult myMatchMakingResult)
        {
            _myDebugger.SimpleDebug($"MyMatchMaking_OnError()...{myMatchMakingResult.ErrorMessage}");
            _data.SessionState = SessionState.Disconnected;
        }

        #endregion

        #region PUBLIC_METHODS

        public async void SetPowerStat()
        {
            int.TryParse(powerStatValueInputField.text, out var powerStatValue);
            var service = _beamContext.Microservices().GameServer();
            service.SetPowerStat(powerStatString, powerStatValue.ToString());
            _myDebugger.SimpleDebug($"{powerStatString} = {powerStatValue} for {_beamContext.PlayerId}");
        }
        

        public async void StartMatchMaking()
        {
            _myDebugger.SimpleDebug("Connecting to match...");
            _data.SessionState = SessionState.Connecting;
            await _myMatchmaking.StartMatchMaking();
        }
        
        public async void CancelMatchMaking()
        {
            _myDebugger.SimpleDebug("Disconnecting from match...");
            _data.SessionState = SessionState.Disconnecting;
            await _myMatchmaking.CancelMatchMaking();
            
            _myDebugger.SimpleDebug("Match Disconnected!");
            _data.SessionState = SessionState.Disconnected;
        }

        #endregion

        
    }
}