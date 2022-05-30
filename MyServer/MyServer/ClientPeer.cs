using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    class ClientPeer
    {
        public Socket clientSocket { get; set; }

        //接收的异步套接字操作
        public SocketAsyncEventArgs ReceiveArgs { get; set;}

        public ClientPeer()
        {
            ReceiveArgs = new SocketAsyncEventArgs();
            ReceiveArgs.UserToken = this;

            //大小为2048,偏移量为0
            ReceiveArgs.SetBuffer(new byte[2048], 0, 2048);
        }

        /// <summary>
        /// 处理接收的数据
        /// </summary>
        /// <param name="packet"></param>
        public void ProcessReceive(byte[] packet)
        {
            
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        public void Disconnect()
        {
            
        }
    }
}
