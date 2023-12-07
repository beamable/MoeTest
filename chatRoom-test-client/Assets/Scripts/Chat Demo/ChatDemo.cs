using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable;
using UnityEngine;
using Beamable.Experimental.Api.Chat;
using TMPro;
using UnityEngine.UI;

namespace Chat_Demo
{
    [System.Serializable]
    public class ChatServiceExampleData
    {
        public List<string> RoomNames = new List<string>();
        public List<string> RoomPlayers = new List<string>();
        public List<string> RoomMessages = new List<string>();
        public string RoomToCreateName = "";
        public string RoomToLeaveName = "";
        public bool IsInRoom = false;
        public string MessageToSend = "";
        public string RoomId;
    }
   
    public class ChatDemo : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        [Header("Headers")]
        [SerializeField] private TextMeshProUGUI roomIDTmp = null;
        [SerializeField] private TextMeshProUGUI playerIdTmp = null;
        [SerializeField] private TextMeshProUGUI logsDisplay = null;
        
        [Header("Create Room")]
        [SerializeField] private TMP_InputField roomNameInputField = null;
        [SerializeField] private GameObject createRoomContainer = null;
        [SerializeField] private Button createRoomBackButton = null;

        [Header("Chat Room")]
        [SerializeField] private TextMeshProUGUI messageDisplayText = null;
        [SerializeField] private TMP_InputField messageInputField = null;
        [SerializeField] private Button sendMessageButton = null;
        [SerializeField] private GameObject roomContainer = null;
        [SerializeField] private ScrollRect chatScrollRect = null;

        [Header("Players Select")] 
        [SerializeField] private GameObject playerSelectContainer = null;
        [SerializeField] private Button playerOneButton = null;
        [SerializeField] private Button playerTwoButton = null;
        [SerializeField] private Button playerSelectBackButton = null;

        
        [Header("Room Prefab")]
        [SerializeField] private Button roomNameButton = null;
        [SerializeField] private Transform roomNamesParentTransform = null;

        #endregion

        #region PRIVATE_VARIABLES

        private long _playerId = 0;
        private IBeamableAPI _beamContext = null;
        private BeamContext _beamContextActive = null;
        private BeamContext _beamContextP1 = null;
        private BeamContext _beamContextP2 = null;
        private ChatView _chatView = null;
        private ChatServiceExampleData _data = new ChatServiceExampleData();
        private List<Button> _instantiatedRoomsButtons = new List<Button>();
        
        private const string _playerOneCode = "Player1";
        private const string _playerTwoCode = "Player2";

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = false;
            roomContainer.SetActive(false);
            createRoomContainer.SetActive(false);
            playerSelectContainer.SetActive(false);
            SetupBeamable();
        }

        #endregion

        #region PRIVATE_METHODS

        #region Setup Beamable

        private async void SetupBeamable()
        {
            logsDisplay.text = $"Beamable Setup started...";
            _beamContextP1 =  await BeamContext.ForPlayer(_playerOneCode).Instance;
            _beamContextP2 =  await BeamContext.ForPlayer(_playerTwoCode).Instance;
            playerSelectContainer.SetActive(true);
            logsDisplay.text += $"\n Beamable Setup done";
        }
        
        //Set active Beamable Context based on selected local player
        private void SetActiveBeamContext(BeamContext ctx, string playerCode)
        {
            _beamContextActive = ctx;
            
            _playerId = _beamContextActive.Api.User.id;
            playerIdTmp.text = _playerId.ToString();
            _beamContextActive.Api.Experimental.ChatService.Subscribe(chatView =>
            {
                _chatView = chatView;
                foreach (var room in chatView.roomHandles)
                {
                    var roomName = $"{room.Name}";
                    InstantiateRoomButton(roomName);

                    room.Subscribe();
                    room.OnMessageReceived += Room_OnMessageReceived;
                    room.OnRemoved += Room_OnRemoved;
                }
            });

            logsDisplay.text += $"\n Active Player Set To {playerCode}" +
                                $"\n ID =  {_playerId}";
        }

        #endregion
        

        #region Roon Events
        
        private void Room_OnRemoved()
        {
            Debug.Log($"Room_OnRemoved");
            DebugLogs();
        }
        
        private void Room_OnMessageReceived(Message message)
        {
            var ownMessage = message.gamerTag == _playerId;
            var roomMessage = $"{message.gamerTag}: {message.content}";
            if (roomMessage == _data.MessageToSend) return;
            Debug.Log($"Room_OnMessageReceived() roomMessage = {roomMessage}");
            AddMessagesText(roomMessage, message.gamerTag);
            DebugLogs();
        }
        
        #endregion
        
        private void DebugLogs()
        {
            _data.IsInRoom = _data.RoomPlayers.Count > 0;
            
            logsDisplay.text = "";
            
            string refreshLog = $"\n RoomNames.Count = {_data.RoomNames.Count}" +
                                $"\n RoomPlayers.Count = {_data.RoomPlayers.Count}" +
                                $"\n RoomMessages.Count = {_data.RoomMessages.Count}" +
                                $"\n RoomMessages.Count = {_data.RoomMessages.Count}" +
                                $"\n IsInRoom = {_data.IsInRoom}\n\n";
            
            logsDisplay.text += refreshLog;
        }
        
        //Update the chat message display with received messages
        private void AddMessagesText(string message, long gamerTag)
        {
            var messageColor = gamerTag == _playerId ? "green" : "white";
            _data.RoomMessages.Add(message);
            _data.MessageToSend = message;
            messageDisplayText.text += $"\n " + $"<color={messageColor}>{message}</color>";
            chatScrollRect.verticalNormalizedPosition = 0f;
        }
        
        //Check if the message contains profanity
        private async Task<bool> IsProfanity(string text)
        {
            bool isProfanityText = true;
            try
            {
                var result = await _beamContextActive.Api.Experimental.ChatService.ProfanityAssert(text);
                isProfanityText = false;
            }
            catch
            {
                Debug.LogWarning($"Profanity detected: {text}");
            }

            return isProfanityText;
        }

        //Instantiate MY ROOMS buttons
        private void InstantiateRoomButton(string roomName)
        {
            if (_instantiatedRoomsButtons.Any(room => room.name == roomName) && _instantiatedRoomsButtons.Count > 0)
            {
                return;
            }
            
            var roomButton = Instantiate(roomNameButton, roomNamesParentTransform);
            roomButton.GetComponentInChildren<TextMeshProUGUI>().text = roomName;
            roomButton.name = roomName;
            roomButton.onClick.AddListener(() =>
            {
                roomNameInputField.text = roomName;
            });
            _instantiatedRoomsButtons.Add(roomButton);
        }
        
        //Remove a room button after leaving a room
        private void RemoveRoomButton(string roomName)
        {
            var roomButton = _instantiatedRoomsButtons.FirstOrDefault(room => room.name == roomName);
            if (roomButton == null) return;
            
            _instantiatedRoomsButtons.Remove(roomButton);
            Destroy(roomButton.gameObject);
        }

        //Destroy all instantiated room buttons 
        private void DestroyRoomButtons()
        {
            foreach (var roomButton in _instantiatedRoomsButtons)
            {
                Destroy(roomButton.gameObject);
            }
            _instantiatedRoomsButtons.Clear();
        }

        #endregion

        #region PUBLIC_METHODS
        
        //OnClick Button Event for Send Message
        public async void SendRoomMessage()
        {
            Debug.Log("SendRoomMessage()");
            if (messageInputField.text == string.Empty) return;
            
            var messageToSend  = messageInputField.text;
            _data.MessageToSend = messageToSend;
            var isProfanity = await IsProfanity(messageToSend);
            Debug.Log($"IsProfanity = {isProfanity}");
            
            if(isProfanity)
            {
                messageToSend = "****";
                sendMessageButton.enabled = false;
            }

            foreach (var room in _chatView.roomHandles)
            {
                if(room.Players.Count > 0 && room.Id == _data.RoomId)
                {
                    await room.SendMessage(messageToSend);
                }
            }
            
            DebugLogs();
            sendMessageButton.enabled = true;
            messageInputField.text = "";
        }
        
        //OnClick Button Event to Create OR Join a Room
        public async void CreateRoom()
        {
            var roomName = roomNameInputField.text;
            _data.RoomNames.Clear();
            _data.RoomPlayers.Clear();
            _data.RoomMessages.Clear();
            _data.RoomNames.Add(roomName);
            _data.RoomToLeaveName = roomName;
            _data.RoomToCreateName = roomName;
            
            var keepSubscribed = true;
            var players = new List<long>{_beamContextActive.Api.User.id};
            
            var result = await _beamContextActive.Api.Experimental.ChatService.CreateRoom(
                roomName, keepSubscribed, players);
            foreach (var room in _chatView.roomHandles.Where(room => room.Id == result.id))
            {
                foreach (var message in room.Messages)
                {
                    var roomMessage = $"{message.gamerTag}: {message.content}";
                    AddMessagesText(roomMessage, message.gamerTag);
                }

                foreach (var playerName in room.Players.Select(player => $"{player}"))
                {
                    _data.RoomPlayers.Add(playerName);
                    Debug.Log($"Player = {playerName}");
                }
            }
            
            roomIDTmp.text = roomName;
            _data.RoomId = result.id;
            
            DebugLogs();
            createRoomBackButton.enabled = true;
            playerSelectBackButton.enabled = true;
            roomContainer.SetActive(true);
            createRoomContainer.SetActive(false);
        }
        
        //Leave all rooms test method
        [ContextMenu("LeaveAllMyRooms")]
        public async void LeaveAllMyRooms()
        {
            var roomInfos = await _beamContextActive.Api.Experimental.ChatService.GetMyRooms();
            
            foreach(var roomInfo in roomInfos)
            {
                await _beamContextActive.Api.Experimental.ChatService.LeaveRoom(roomInfo.id);
                RemoveRoomButton(roomInfo.name);
            }
    
            DebugLogs();
            
            createRoomContainer.SetActive(true);
            roomContainer.SetActive(false);
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = true;
        }

        //OnClick Button Event to Leave a Room
        public async void LeaveRoom()
        {
            await _beamContextActive.Api.Experimental.ChatService.LeaveRoom(_data.RoomId);
            RemoveRoomButton(_data.RoomToLeaveName);
            DebugLogs();
            
            createRoomContainer.SetActive(true);
            roomContainer.SetActive(false);
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = true;
        }

        #region OnClick Events for Player Select

        public void SetPlayerOne()
        {
            SetActiveBeamContext(_beamContextP1, _playerOneCode);
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = true;
            roomContainer.SetActive(false);
            createRoomContainer.SetActive(true);
            playerSelectContainer.SetActive(false);
        }
        
        public void SetPlayerTwo()
        {
            SetActiveBeamContext(_beamContextP2, _playerTwoCode);
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = true;
            roomContainer.SetActive(false);
            createRoomContainer.SetActive(true);
            playerSelectContainer.SetActive(false);
        }

        #endregion

        public void GoBackToPlayerSelect()
        {
            roomIDTmp.text = "";
            messageDisplayText.text = "";
            playerIdTmp.text = "";
            _data.MessageToSend = "";
            createRoomBackButton.enabled = false;
            playerSelectBackButton.enabled = false;
            _beamContextActive = null;
            roomContainer.SetActive(false);
            createRoomContainer.SetActive(false);
            playerSelectContainer.SetActive(true);
            DestroyRoomButtons();
        }
        
        public void GoBackToCreate()
        {
            roomIDTmp.text = "";
            messageDisplayText.text = "";
            _data.MessageToSend = "";
            createRoomContainer.SetActive(true);
            roomContainer.SetActive(false);
        }

        #endregion
    }
}