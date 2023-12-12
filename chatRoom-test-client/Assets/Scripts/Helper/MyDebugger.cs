using TMPro;
using UnityEngine;

namespace Helper
{
    public class MyDebugger : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI displayTmp = null;
        
        private void UpdateDisplayText(string message)
        {
            displayTmp.text += $"\n {message}";
        }
        
        public void SimpleDebug(string message)
        {
            Debug.Log(message);
            UpdateDisplayText(message);
        }
    
        public void ErrorDebug(string message)
        {
            Debug.LogError(message);
            UpdateDisplayText(message);
        }

        public void ClearMessages()
        {
            displayTmp.text = "";
        }
        
        
    }
}