using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Pong.Network {
    public class UdpListener {
        protected Thread threadLoop = null;
        protected UdpClient client;
        protected int port;

        public int Port { get { return port; } }

        public UdpListener(int port) {
            this.port = port;
        }

        public void StartListening() {
            if(threadLoop != null && !threadLoop.IsAlive) {
                Debug.LogError("UPD listening thread is already started");
                return;
            }
            threadLoop = new Thread(new ThreadStart(Loop));
            threadLoop.IsBackground = true;
            client = new UdpClient(port);
            //client.Client.Blocking = false;
            threadLoop.Start();
        }

        public void StopListening() {
            if(threadLoop == null || !threadLoop.IsAlive) {
                Debug.LogWarning("UPD listening thread is already stoped or not started yet");
                return;
            }
            threadLoop.Abort();
            client.Close();
        }

        protected void Loop() {
            UdpClient client = new UdpClient(2222);

            client.ExclusiveAddressUse = false;
            IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 2222);

            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;

            client.Client.Bind(localEp);

            IPAddress multicastaddress = IPAddress.Parse("239.0.0.222");
            client.JoinMulticastGroup(multicastaddress);

            Debug.Log("Listening this will never quit so you will need to ctrl-c it");

            while(true) {
                Byte[] data = client.Receive(ref localEp);
                string strData = Encoding.Unicode.GetString(data);
                Debug.Log("Recv: "+ strData);
            }
            //var endPoint = new IPEndPoint(IPAddress.Any, 0);
            //while(true) {
            //    try {
            //        byte[] data = client.Receive(ref endPoint);
            //        // encode UTF8-coded bytes to text format
            //        string text = Encoding.UTF8.GetString(data);
            //        // show received message
            //        Debug.Log(">> " + text);
            //    } catch(Exception err) {
            //        Debug.LogError("UDP listening exception: " + err.ToString());
            //    } finally {
            //        StopListening();
            //    }
            //}
        }
    }
}