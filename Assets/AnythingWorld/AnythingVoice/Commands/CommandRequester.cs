using AnythingWorld.Networking;
using AnythingWorld.Utilities;
using System;
using System.Collections;

namespace AnythingWorld.Voice
{
    /// <summary>
    /// Represents a Command Requester that can request and handle plain text commands.
    /// </summary>
    public class CommandRequester
    {
        /// <summary>
        /// Requests a command from a plain text input string and handles resulting command through CommandHandler utility.
        /// </summary>
        /// <param name="input">String to extract command from.</param>
        /// <param name="ReturnedCommandAction">An optional action to be invoked with the executed command's output.</param>
        /// <remarks>
        /// This method starts a coroutine to execute the command asynchronously. Once the command has been executed, 
        /// the output is passed to the `ReturnedCommandAction` delegate (if it is not null).
        /// </remarks>
        public static void RequestCommand(string input, Action<string> ReturnedCommandAction = null)
        {
            CoroutineExtension.StartCoroutine(RequestAndHandleCommandCoroutine(input, ReturnedCommandAction));
        }
        /// <summary>
        /// Requests a plain text command from the user and returns it without any further parsing.
        /// </summary>
        /// <param name="input">The plain text input from the user, e.g. "Make 2 dogs".</param>
        /// <param name="ReturnedCommandAction">An action to be invoked with the returned command, represented as a string in JSON format that can be deserialized into the ParsedSpeechCommand data structure or used manually.</param>
        /// <remarks>
        public static void RequestAndReturnCommand(string input, Action<string> ReturnedCommandAction)
        {
            CoroutineExtension.StartCoroutine(RequestAndReturnCommandCoroutine(input, ReturnedCommandAction));
        }

        /// <summary>
        /// Requests a command from a string input and handles the result using a success delegate.
        /// </summary>
        /// <param name="input">The plain text input from the user, e.g. "Make 2 dogs".</param>
        /// <param name="OnSuccess">An action to be invoked with the successfully requested raw JSON-formatted command string.</param>
        /// <remarks>
        /// This method starts a coroutine to process the plain text input and handle the resulting command. The command is parsed using the AudioProcessor utility's `RequestCommandFromStringInput` method. If the command is successfully parsed, the `OnSuccess` delegate is invoked with the returned command represented as a string in JSON format. 
        /// </remarks>
        private static IEnumerator RequestAndHandleCommandCoroutine(string input, Action<string> OnSuccess)
        {
            yield return AudioProcessor.RequestCommandFromStringInput(input, null, null, ParseCommandOnSuccess, OnSuccess);
        }
        /// <summary>
        /// Requests a command from a plain text input string and returns the resulting command as a JSON-formatted string.
        /// </summary>
        /// <param name="input">A plain text input string representing a command.</param>
        /// <param name="OnSuccess">An action to be invoked with the successfully requested raw JSON-formatted command string.</param>
        /// <remarks>
        /// This method requests a command from a plain text input string, without any further parsing or processing of the command. 
        /// The resulting command is returned as a JSON-formatted string that can be deserialized into a ParsedSpeechCommand object using the VoiceJsonParser utility. 
        /// The `ReturnAction` parameter is an action that will be invoked with the resulting JSON-formatted command string as an argument. 
        /// </remarks>
        private static IEnumerator RequestAndReturnCommandCoroutine(string input, Action<string> OnSucces)
        {
            yield return AudioProcessor.RequestCommandFromStringInput(input, null, null, OnSucces);
        }
        /// <summary>
        /// Parses a JSON-formatted string representing a command and passes it to the CommandHandler for execution.
        /// </summary>
        /// <param name="rawCommandJson">A JSON-formatted string representing a command.</param>
        /// <remarks>
        /// This method is called when a command has been successfully returned from the speech recognition service. 
        /// The `rawCommandJson` parameter should be a JSON-formatted string that can be deserialized into a ParsedSpeechCommand object using the VoiceJsonParser utility. 
        /// The resulting ParsedSpeechCommand object is then passed to the CommandHandler for execution.
        /// </remarks>
        private static void ParseCommandOnSuccess(string rawCommandJson)
        {
            var command = VoiceJsonParser.ProcessReturnedCommand(rawCommandJson, CommandResult.Success);
            CommandHandler.ParseCommand(command);
        }
    }
}
