using UnityEngine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

public class LanBroadcaster : MonoBehaviour {
    public delegate void delJoinServer(string strIP); // Definition of JoinServer Delegate, takes a string as argument that holds the ip of the server
    public delegate void delStartServer(); // Definition of StartServer Delegate
    private enum enuState { NotActive, Searching, Announcing }; // Definition of State Enumeration.
    private struct ReceivedMessage { public float fTime; public string strIP; public bool bIsReady; } // Definition of a Received Message struct. This is the form in which we will store messages

    private string strMessage = ""; // A simple message string, that can be read by other objects (eg. NetworkController), to show what this object is doing.
    private enuState currentState = enuState.NotActive;
    private UdpClient objUDPClient; // The UDPClient we will use to send and receive messages
    private List<ReceivedMessage> lstReceivedMessages; // The list we store all received messages in, when searching
    private delJoinServer delWhenServerFound; // Reference to the delegate that will be called when a server is found, set by StartSearchBroadcasting()
    private delStartServer delWhenServerMustStarted; // Reference to the delegate that will be called when a server must be created, set by StartSearchBroadcasting()
    private string strServerNotReady = "wanttobeaserver"; // The actual content of the 'i am willing to start a server' message
    private string strServerReady = "iamaserver"; // The actual content of the 'i have a server ready' message
    private float fTimeLastMessageSent;
    private float fIntervalMessageSending = 1f; // The interval in seconds between the sending of messages
    private float fTimeMessagesLive = 3; // The time a message 'lives' in our list, before it gets deleted
    private float fTimeToSearch = 5; // The time the script will search, before deciding what to do
    private float fTimeSearchStarted;

    public string Message { get { return strMessage; } } // Property to read the strMessage

    /// <summary>
    /// IP should be cached because Network.player.ipAddress cannot be accessed from any thread except the main one
    /// </summary>
    private string playerIpAddress;
    /// <summary>
    /// Same reason for caching as for playerIpAddress
    /// </summary>
    private float lastFrameTime;
    void Start() {
        lastFrameTime = Time.time;
        // Create our list
        lstReceivedMessages = new List<ReceivedMessage>();
        playerIpAddress = Network.player.ipAddress;
    }

    void Update() {
        lastFrameTime = Time.time;
        // Check if we need to send messages and the interval has espired
        if((currentState == enuState.Searching || currentState == enuState.Announcing)
            && Time.time > fTimeLastMessageSent + fIntervalMessageSending) {
            // Determine out of our current state what the content of the message will be
            byte[] objByteMessageToSend = System.Text.Encoding.ASCII.GetBytes(currentState == enuState.Announcing ? strServerReady : strServerNotReady);
            // Send out the message
            objUDPClient.Send(objByteMessageToSend, objByteMessageToSend.Length, new IPEndPoint(IPAddress.Broadcast, 22043));
            // Restart the timer
            fTimeLastMessageSent = Time.time;

            // Refresh the list of received messages (remove old messages)
            if(currentState == enuState.Searching) {
                // This rather complex piece of code is needed to be able to loop through a list while deleting members of that same list
                bool bLoopedAll = false;
                while(!bLoopedAll && lstReceivedMessages.Count > 0) {
                    foreach(ReceivedMessage objMessage in lstReceivedMessages) {
                        if(Time.time > objMessage.fTime + fTimeMessagesLive) {
                            // If this message is too old, delete it and restart the foreach loop
                            lstReceivedMessages.Remove(objMessage);
                            break;
                        }
                        // If this whas the last message, make sure we exit the while loop
                        if(lstReceivedMessages[lstReceivedMessages.Count - 1].Equals(objMessage))
                            bLoopedAll = true;
                    }
                }
            }
        }

        if(currentState == enuState.Searching) {
            // Check the list of messages to see if there is any 'i have a server ready' message present
            foreach(ReceivedMessage objMessage in lstReceivedMessages) {
                // If we have a server that is ready, call the right delegate and stop searching
                if(objMessage.bIsReady) {
                    StopSearching();
                    strMessage = "We will join";
                    delWhenServerFound(objMessage.strIP);
                    break;
                }
            }
            // Check if we're ready searching.
            if(currentState == enuState.Searching && Time.time > fTimeSearchStarted + fTimeToSearch) {
                // We are. Now determine who's gonna be the server.

                // This string holds the ip of the new server. We will start off pointing ourselves as the new server
                string strIPOfServer = playerIpAddress;
                // Next, we loop through the other messages, to see if there are other players that have more right to be the server (based on IP)
                foreach(ReceivedMessage objMessage in lstReceivedMessages) {
                    if(ScoreOfIP(objMessage.strIP) > ScoreOfIP(strIPOfServer)) {
                        // The score of this received message is higher, so this will be our new server
                        strIPOfServer = objMessage.strIP;
                    }
                }
                // If after the loop the highest IP is still our own, call delegate to start a server and stop searching
                if(strIPOfServer == playerIpAddress) {
                    StopSearching();
                    strMessage = "We will start server.";
                    delWhenServerMustStarted();
                }
                // If it's not, someone else must start the server. We will simply have to wait as the server is clearly not ready yet
                else {
                    strMessage = "Found server. Waiting for server to get ready...";
                    // Clear the list and do the search again.
                    lstReceivedMessages.Clear();
                    fTimeSearchStarted = Time.time;
                }
            }
        }
    }

    // Method to start an Asynchronous receive procedure. The UDPClient is told to start receiving.
    // When it received something, the UDPClient is told to call the EndAsyncReceive() method.
    private void BeginAsyncReceive() {
        objUDPClient.BeginReceive(new AsyncCallback(EndAsyncReceive), null);
    }
    // Callback method from the UDPClient.
    // This is called when the asynchronous receive procedure received a message
    private void EndAsyncReceive(IAsyncResult objResult) {
        // Create an empty EndPoint, that will be filled by the UDPClient, holding information about the sender
        IPEndPoint objSendersIPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        // Read the message
        byte[] objByteMessage = objUDPClient.EndReceive(objResult, ref objSendersIPEndPoint);
        // If the received message has content and it was not sent by ourselves...
        if(objByteMessage.Length > 0 &&
            !objSendersIPEndPoint.Address.ToString().Equals(playerIpAddress)) {
            // Translate message to string
            string strReceivedMessage = System.Text.Encoding.ASCII.GetString(objByteMessage);
            // Create a ReceivedMessage struct to store this message in the list
            ReceivedMessage objReceivedMessage = new ReceivedMessage();
            objReceivedMessage.fTime = lastFrameTime;
            objReceivedMessage.strIP = objSendersIPEndPoint.Address.ToString();
            objReceivedMessage.bIsReady = strReceivedMessage == strServerReady ? true : false;
            lstReceivedMessages.Add(objReceivedMessage);
        }
        // Check if we're still searching and if so, restart the receive procedure
        if(currentState == enuState.Searching)
            BeginAsyncReceive();
    }
    // Method to start this object announcing this is a server, used by the script itself
    private void StartAnnouncing() {
        currentState = enuState.Announcing;
        strMessage = "Announcing we are a server...";
    }
    // Method to stop this object announcing this is a server, used by the script itself
    private void StopAnnouncing() {
        currentState = enuState.NotActive;
        strMessage = "Announcements stopped.";
    }
    // Method to start this object searching for LAN Broadcast messages sent by players, used by the script itself
    private void StartSearching() {
        lstReceivedMessages.Clear();
        BeginAsyncReceive();
        fTimeSearchStarted = Time.time;
        currentState = enuState.Searching;
        strMessage = "Searching for other players...";
    }
    // Method to stop this object searching for LAN Broadcast messages sent by players, used by the script itself
    private void StopSearching() {
        currentState = enuState.NotActive;
        strMessage = "Search stopped.";
    }

    // Method to be called by some other object (eg. a NetworkController) to start a broadcast search
    // It takes two delegates; the first for when this object finds a server that can be connected to, 
    // the second for when this player is determined to start a server itself.
    public void StartSearchBroadCasting(delJoinServer connectToServer, delStartServer startServer) {
        // Set the delegate references, so other functions within this class can call it
        delWhenServerFound = connectToServer;
        delWhenServerMustStarted = startServer;
        // Start a broadcasting session (this basically prepares the UDPClient)
        StartBroadcastingSession();
        // Start a search
        StartSearching();
    }
    // Method to be called by some other object (eg. a NetworkController) to start a broadcast announcement. Announcement means; tell everyone you have a server.
    public void StartAnnounceBroadCasting() {
        // Start a broadcasting session (this basically prepares the UDPClient)
        StartBroadcastingSession();
        // Start an announcement
        StartAnnouncing();
    }
    // Method to start a general broadcast session. It prepares the object to do broadcasting work. Used by the script itself.
    private void StartBroadcastingSession() {
        // If the previous broadcast session was for some reason not closed, close it now
        if(currentState != enuState.NotActive)
            StopBroadCasting();
        // Create the client
        objUDPClient = new UdpClient(22043);
        objUDPClient.EnableBroadcast = true;
        // Reset sending timer
        fTimeLastMessageSent = Time.time;
    }
    // Method to be called by some other object (eg. a NetworkController) to stop this object doing any broadcast work and free resources.
    // Must be called before the game quits!
    public void StopBroadCasting() {
        if(currentState == enuState.Searching)
            StopSearching();
        else if(currentState == enuState.Announcing)
            StopAnnouncing();
        if(objUDPClient != null) {
            objUDPClient.Close();
            objUDPClient = null;
        }
    }
    // Method that calculates a 'score' out of an IP adress. This is used to determine which of multiple clients will be the server. Used by the script itself.
    private long ScoreOfIP(string strIP) {
        long lReturn = 0;
        string strCleanIP = strIP.Replace(".", "");
        lReturn = long.Parse(strCleanIP);
        return lReturn;
    }
}
