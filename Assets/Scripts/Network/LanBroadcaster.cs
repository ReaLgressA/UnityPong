namespace Pong.Network {
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using UnityEngine;

    public class LanBroadcaster : MonoBehaviour {
        public enum BroadcasterState {
            NotActive,
            Searching,
            Announcing
        };
        protected struct ReceivedMessage {
            public float fTime;
            public string strIP;
            public bool bIsReady;
        }

        public delegate void ServerNotFound();
        public delegate void ServerFound(string ip);

        private float msgLifeTime = 3f;
        private float lastMsgSentTime;
        private float msgSentInterval = 1f;
        private float searchTime = 2f;
        private float searchStartedTime;
        /// <summary>
        /// IP should be cached because Network.player.ipAddress cannot be accessed from any thread except the main one
        /// </summary>
        private string playerIpAddress;
        /// <summary>
        /// Same reason for caching as for playerIpAddress
        /// </summary>
        private float lastFrameTime;
        protected int port = 22043;
        protected UdpClient udp;
        protected ServerFound cbServerFound;
        protected ServerNotFound cbServerNotFound; 
        protected BroadcasterState state = BroadcasterState.NotActive;
        protected List<ReceivedMessage> receivedMessages;
        
        public string strMessage = ""; // A simple message string, that can be read by other objects (eg. NetworkController), to show what this object is doing.
        

        protected string CmdSearchingServer { get { return BroadcasterState.Searching.ToString(); } }
        protected string CmdAnnouncingServer { get { return BroadcasterState.Announcing.ToString(); } }
        

        void Start() {
            lastFrameTime = Time.time;
            receivedMessages = new List<ReceivedMessage>();
            playerIpAddress = Network.player.ipAddress;
        }

        private void Search() {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(CmdSearchingServer);
            udp.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));
            lastMsgSentTime = Time.time;
            if(state == BroadcasterState.Searching) {
                bool bLoopedAll = false;
                while(!bLoopedAll && receivedMessages.Count > 0) {
                    foreach(ReceivedMessage objMessage in receivedMessages) {
                        if(Time.time > objMessage.fTime + msgLifeTime) {
                            receivedMessages.Remove(objMessage);
                            break;
                        }
                        if(receivedMessages[receivedMessages.Count - 1].Equals(objMessage))
                            bLoopedAll = true;
                    }
                }
            }
        }

        private void Announce() {
            byte[] bytes = System.Text.Encoding.ASCII.GetBytes(CmdAnnouncingServer );
            udp.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, port));
            lastMsgSentTime = Time.time;

        }

        private long IpToInt(string strIP) {
            return long.Parse(strIP.Replace(".", ""));
        }

        void Update() {
            lastFrameTime = Time.time;
            if(Time.time > lastMsgSentTime + msgSentInterval) {
                if(state == BroadcasterState.Searching) {
                    Search();
                } else if(state == BroadcasterState.Announcing) { 
                    Announce();
                }
            }

            if(state == BroadcasterState.Searching) {
                foreach(var msg in receivedMessages) {
                    if(msg.bIsReady) {
                        StopSearching();
                        cbServerFound(msg.strIP);
                        break;
                    }
                }
                if(state == BroadcasterState.Searching && Time.time > searchStartedTime + searchTime) {
                    string serverIp = playerIpAddress;
                    //Compare ip addresses to decide who should start the server
                    foreach(var msg in receivedMessages) {
                        if(IpToInt(msg.strIP) > IpToInt(serverIp)) {
                            serverIp = msg.strIP;
                        }
                    }
                    if(serverIp == playerIpAddress) {
                        StopSearching();
                        cbServerNotFound();
                    } else {
                        receivedMessages.Clear();
                        searchStartedTime = Time.time;
                    }
                }
            }
        }
        
        private void BeginAsyncReceive() {
            udp.BeginReceive(new AsyncCallback(EndAsyncReceive), null);
        }

        private void EndAsyncReceive(IAsyncResult res) {
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
            byte[] bytes = udp.EndReceive(res, ref sender);
            if(bytes.Length > 0 && !sender.Address.ToString().Equals(playerIpAddress)) {
                string recvMsg = System.Text.Encoding.ASCII.GetString(bytes);
                ReceivedMessage msg = new ReceivedMessage();
                msg.fTime = lastFrameTime;
                msg.strIP = sender.Address.ToString();
                msg.bIsReady = recvMsg == CmdAnnouncingServer ? true : false;
                receivedMessages.Add(msg);
            }
            if(state == BroadcasterState.Searching) {
                BeginAsyncReceive();
            }
        }

        private void StartAnnouncing() {
            state = BroadcasterState.Announcing;
            strMessage = "Announcing we are a server...";
        }

        private void StopAnnouncing() {
            state = BroadcasterState.NotActive;
            strMessage = "Announcements stopped.";
        }

        private void StartSearching() {
            receivedMessages.Clear();
            BeginAsyncReceive();
            searchStartedTime = Time.time;
            state = BroadcasterState.Searching;
            strMessage = "Searching for other players...";
        }

        private void StopSearching() {
            state = BroadcasterState.NotActive;
            strMessage = "Search stopped.";
        }

        private void StartBroadcasting() {
            if(state != BroadcasterState.NotActive)
                StopBroadcasting();
            udp = new UdpClient(port);
            udp.EnableBroadcast = true;
            lastMsgSentTime = Time.time;
        }

        public void StopBroadcasting() {
            if(state == BroadcasterState.Searching)
                StopSearching();
            else if(state == BroadcasterState.Announcing)
                StopAnnouncing();
            if(udp != null) {
                udp.Close();
                udp = null;
            }
        }
        public void StartSearchBroadCasting(ServerFound cbServerFound, ServerNotFound cbServerNotFound, int port) {
            this.cbServerFound = cbServerFound;
            this.cbServerNotFound = cbServerNotFound;
            StartBroadcasting();
            StartSearching();
        }
        public void StartAnnounceBroadcasting() {
            StartBroadcasting();
            StartAnnouncing();
        }
        
    }
}