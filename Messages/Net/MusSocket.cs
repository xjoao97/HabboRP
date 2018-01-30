using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

using Plus.Messages.Net;

namespace Plus.Net
{
    public class MusSocket
    {
        private Socket _musSocket;
        private List<String> _allowedIPs;

        private String _musIP;
        private int _musPort;

        public MusSocket(String MusIP, int MusPort, String[] AllowdIPs, int backlog)
        {
            this._musIP = MusIP;
            this._musPort = MusPort;

            this._allowedIPs = new List<String>();
            foreach (String ip in AllowdIPs)
            {
                this._allowedIPs.Add(ip);
            }

            try
            {
                _musSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _musSocket.Bind(new IPEndPoint(IPAddress.Any, _musPort));
                _musSocket.Listen(backlog);
                _musSocket.BeginAccept(OnEvent_NewConnection, _musSocket);
            }

            catch (Exception e)
            {
                throw new ArgumentException("Could not set up MUS socket:\n" + e);
            }
        }

        private void OnEvent_NewConnection(IAsyncResult iAr)
        {
            try
            {
                Socket socket = ((Socket)iAr.AsyncState).EndAccept(iAr);
                String ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                if (_allowedIPs.Contains(ip) || ip == "127.0.0.1")
                {
                    var nC = new MusConnection(socket);
                }
                else
                {
                    socket.Close();
                }
            }
            catch (Exception)
            {
            }

            _musSocket.BeginAccept(OnEvent_NewConnection, _musSocket);
        }
    }
}