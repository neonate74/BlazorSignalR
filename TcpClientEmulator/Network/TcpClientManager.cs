using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpClientEmulator.Network
{
    internal class TcpClientManager
    {
        private Socket clientSocket;
        private Socket cbSocket;

        private const int Maxsize = 40960;
        private byte[] recvBuffer;
        private string serverIP = string.Empty;
        private int serverPort;

        internal TcpClientManager(int port)
        {
            serverIP = ConfigurationManager.AppSettings.Get("ServerIP") ?? "";
            serverPort = port;

            recvBuffer = new byte[Maxsize];

            this.DoConnect();
        }

        private void DoConnect()
        {
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.BeginConnect();
        }

        public void BeginConnect()
        {
            Program.MainForm?.ShowMessage("Waiting to connect to server...");
            try
            {
                clientSocket.BeginConnect(serverIP, serverPort, new AsyncCallback(ConnectCallback), clientSocket);
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(SocketException))
                {
                    Program.MainForm?.ShowMessage($"Connecting to server is failed({((SocketException)ex).NativeErrorCode}):\r\n{ex.Message}");
                    Thread.Sleep(5000);
                    this.DoConnect();
                }
                else
                {
                    throw;
                }
            }
        }

        private void ConnectCallback(IAsyncResult IAR)
        {
            try
            {
                Socket tmpSock = (Socket)IAR.AsyncState;
                if (tmpSock != null)
                {
                    IPEndPoint svrEP = (IPEndPoint)tmpSock.RemoteEndPoint;
                    if (svrEP != null)
                    {
                        Program.MainForm?.ShowMessage($"Connecting to Server is success:{svrEP.Address}");
                        tmpSock.EndConnect(IAR);

                        this.cbSocket = tmpSock;
                        this.Receive();
                    }
                    else
                    {
                        throw new SocketException((int)SocketError.NotConnected);
                    }
                }
                else
                {
                    throw new SocketException((int)SocketError.NotConnected);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.NotConnected)
                {
                    Program.MainForm?.ShowMessage($"Retry connecting to Server:{ex.Message}");
                    Thread.Sleep(5000);
                    this.BeginConnect();
                }
            }
        }

        private void OnReceiveCallback(IAsyncResult IAR)
        {
            try
            {
                Socket tmpSock = (Socket)IAR.AsyncState;
                if (tmpSock != null )
                {
                    int nReadSize = tmpSock.EndReceive(IAR);
                    if (nReadSize > 0) {
                        string sData = Encoding.Unicode.GetString(recvBuffer, 0, nReadSize);
                        Program.MainForm?.ShowMessage(sData);
                    }

                    this.Receive();
                }
                else
                {
                    throw new SocketException((int)SocketError.ConnectionReset);
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.ConnectionReset)
                {
                    this.BeginConnect();
                }
            }
        }

        public void Receive()
        {
            cbSocket.BeginReceive(this.recvBuffer, 0, recvBuffer.Length, SocketFlags.None, new AsyncCallback(OnReceiveCallback), cbSocket);
        }
    }
}
