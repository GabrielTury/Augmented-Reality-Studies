using AnythingWorld.Utilities.Networking;
using UnityEngine;
using UnityEditor;

namespace AnythingWorld.Editor
{
    public class EditorNetworkErrorHandler
    {
        public static void HandleError(NetworkErrorMessage errorMessage)
        {
            switch (errorMessage.code)
            {
                case "Unrepeatable action":
                    break;
                case "Too many requests error": // API key quota exceeded
                    AnythingEditor.DisplayAWDialog("API Key Quote Exceeded", errorMessage.message, "Go to Profile", "Close", () => Application.OpenURL("https://get.anything.world/profile"));
                    PrintNetworkLogWarning(errorMessage);
                    break;
                default:
                    PrintNetworkLogWarning(errorMessage);
                    break;
            }
        
        }
        private static void PrintNetworkLogWarning(NetworkErrorMessage errorMessage)
        {
            Debug.LogWarning($"Network Error: {errorMessage.code}({errorMessage.errorCode}): {errorMessage.message}");
        }
    }
}
