using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Dependencies;
using Beamable.Experimental.Api.Sim;
using Helper;
using TMPro;
using UnityEngine;

namespace Multiplayer
{
    public enum SessionState
    {
        None,
        Initializing,
        Initialized,
        Connected,
        Disconnected
    }
    
    [System.Serializable]
    public class MultiplayerExampleData
    {
        public string MatchId = null;
        public string SessionSeed;
        public long CurrentFrame;
        public long LocalPlayerDbid;
        public bool IsSessionConnected { get { return SessionState == SessionState.Connected; }}
        public SessionState SessionState = SessionState.None;
        public List<string> PlayerMoveLogs = new List<string>();
        public List<string> PlayerDbids = new List<string>();
    }
    
    public class MultiplayerTest : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private PlayerEventsTest playerEventsTest = null;
        [SerializeField] private MyDebugger debugger = null;
        [SerializeField] private TMP_InputField matchIdInputField = null;

        #endregion

        #region PRIVATE_VARIABLES
        
        private const long FramesPerSecond = 20;
        private const long TargetNetworkLead = 4;
        private bool _startedMultiplayer = true;
        private MultiplayerExampleData _multiplayerExampleData = new MultiplayerExampleData();
        private SimClient _simClient = null;
        private BeamContext _beamContext = null;
        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            SetupBeamable();
        }

        private void Update()
        {
            _simClient?.Update();
        }

        private void OnDisable()
        {
            if(_simClient == null) return;
            StopMultiplayer();
        }

        #endregion

        #region PRIVATE_METHODS
        
        private async void SetupBeamable()
        {
            await Task.Delay(2500);
            _beamContext = await BeamContext.Default.Instance;
            debugger.SimpleDebug($"Beamable Setup Complete for player: {_beamContext.PlayerId}");
            _multiplayerExampleData.LocalPlayerDbid = _beamContext.PlayerId;
            await Task.Delay(2000);
            playerEventsTest.UpdatePlayerId(_beamContext.PlayerId);
        }
        
        private void StopMultiplayer(bool startedMulti = false)
        {
            if(_simClient == null)return;
            _multiplayerExampleData.SessionState = SessionState.Disconnected;
            if(_multiplayerExampleData.PlayerDbids.Contains(_beamContext.PlayerId.ToString()))
                _multiplayerExampleData.PlayerDbids.Remove(_beamContext.PlayerId.ToString());
            
            if(!startedMulti)
                debugger.SimpleDebug($"<color=red>Stopping Multiplayer for {_multiplayerExampleData.MatchId} & player {_multiplayerExampleData.LocalPlayerDbid}</color>");
        }
        
        private void SimClient_OnInit(string sessionSeed)
        {
            _multiplayerExampleData.SessionState = SessionState.Initialized;
            _multiplayerExampleData.SessionSeed = sessionSeed;
            debugger.SimpleDebug($"SimClient_OnInit");
        }
        
        private void SimClient_OnConnect(string dbid)
        {
            _multiplayerExampleData.SessionState = SessionState.Connected;
            _multiplayerExampleData.PlayerDbids.Add(dbid);
            var playerEventData = playerEventsTest.GetPlayerEvent();
            _simClient.On<PlayerEventsData>(playerEventData.name, dbid, SimClient_OnPlayerEvent);
            
            debugger.SimpleDebug($"SimClient_OnConnect for {dbid} with {_multiplayerExampleData.PlayerDbids.Count} players");

        }
        
        private void SimClient_OnDisconnect(string dbid)
        {
            if(long.Parse(dbid) == _multiplayerExampleData.LocalPlayerDbid)
                StopMultiplayer();
            
            _multiplayerExampleData.PlayerDbids.Remove(dbid);
            debugger.SimpleDebug($"SimClient_OnDisconnect for {dbid}");
        }
        
        private void SimClient_Tick(long frame)
        {
            _multiplayerExampleData.CurrentFrame = frame;
            Debug.Log($"SimClient_Tick: {frame}");
            if(_startedMultiplayer) return;
            debugger.SimpleDebug($"SimClient_Tick: {_multiplayerExampleData.CurrentFrame}");
            _startedMultiplayer = true;

        }
        
        private void SimClient_OnPlayerEvent(PlayerEventsData playerEventsData)
        {
            playerEventsTest.UpdateEvent(playerEventsData);
            debugger.SimpleDebug($"SimClient_OnPlayerEvent: {playerEventsData.name} for {playerEventsData.position}");
        }
        
        private string GetMatchId()
        {
            var prefix = "Match_";
            var random = UnityEngine.Random.Range(0, 99999);
            var number = matchIdInputField.text == "" ? random : int.Parse(matchIdInputField.text);
            return prefix + number;
        }


        #endregion

        #region PUBLIC_METHODS

        public void StartMultiplayer()
        {
            if(_simClient != null) return;
            StopMultiplayer(true);
            _multiplayerExampleData.SessionState = SessionState.Initializing;
            _multiplayerExampleData.MatchId = GetMatchId();
            debugger.SimpleDebug($"Starting Multiplayer for {_multiplayerExampleData.MatchId}");

            var simNetworkEventStream = new SimNetworkEventStream(_multiplayerExampleData.MatchId, 
                _beamContext.ServiceProvider);
            _simClient = new SimClient(simNetworkEventStream, FramesPerSecond, TargetNetworkLead);

            _simClient.OnInit(SimClient_OnInit);
            _simClient.OnConnect(SimClient_OnConnect);
            _simClient.OnDisconnect(SimClient_OnDisconnect);
            _simClient.OnTick(SimClient_Tick);
            
            _simClient.OnErrorStarted += (e) =>
            {
                // TODO: show "connecting to server error message"
                debugger.ErrorDebug($"Sim Client Errors Starting... {e.ErrorMessage}");
            };
            _simClient.OnErrorRecovered += (r) =>
            {
                // TODO: remove any error UI and resume the game
                debugger.SimpleDebug($"<color=green>Sim Client disaster averted!</color>");
            };
            _simClient.OnErrorFailed += r =>
            {
                // TODO: force exit the match, because the network is too unstable to continue or recover.
                debugger.SimpleDebug($"<color=red>Connection failed, quiting match due to: {r.ErrorMessage} </color>");
            };
        }

        public void SendPlayerEvent()
        {
            if (_simClient == null)
            {
                debugger.ErrorDebug($"SimClient is null!");
                return;
            }
            var playerEvent = playerEventsTest.GetPlayerEvent();

            _simClient.SendEvent(playerEvent.name, playerEvent);
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        #endregion

        
    }
}