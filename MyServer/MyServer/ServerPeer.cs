using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyServer
{
    class ServerPeer
    {
        /// <summary>
        /// 服务器
        /// </summary>
        private Socket serverSocket;
        /// <summary>
        /// 计量器
        /// </summary>
        private Semaphore semaphore;

        /// <summary>
        /// 客户端对象连接池
        /// </summary>
        private ClientPeerPool clientPeerPool;
        /// <summary>
        /// 开起服务器
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="maxClient"></param>
        public void StartServer(string ip, int port, int maxClient)
        {
            //声明对象池和大小
            clientPeerPool = new ClientPeerPool(maxClient);
            semaphore = new Semaphore(maxClient, maxClient);
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IP地址与端口号  绑定到进程
            serverSocket.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
            for (int i = 0; i < maxClient; i++)
            {
                ClientPeer temp = new ClientPeer();
                clientPeerPool.Enqueue(temp);
            }
            //最大监听数量
            serverSocket.Listen(maxClient);
            Console.WriteLine("服务器启动成功");
            //服务器启动成功后开始接受客户端的链接

            StartAccept(null);
        }
        #region 处理客户端链接请求
        /// <summary>
        /// 接收客户端的链接
        /// </summary>
        /// <param name="e"></param>
        private void StartAccept(SocketAsyncEventArgs e)
        {
            if (e == null)
            {
                e = new SocketAsyncEventArgs();

                e.Completed += E_Completed;
            }

            //如果Result为true，代表正在接收链接，连接成功后会触发Completed事件
            //如果Result为false，代表接收成功

            bool result = serverSocket.AcceptAsync(e);

            if (result == false)
            {
                ProcessAccept(e);
            }
        }


        private void ProcessAccept(SocketAsyncEventArgs e)
        {
            //等待线程释放
            semaphore.WaitOne();
            ClientPeer client = clientPeerPool.Dequeue();
            client.clientSocket = e.AcceptSocket;

            Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端链接成功");
            //接收消息 继续开启等待下一个用户接入
            e.AcceptSocket = null;
            StartAccept(e);
        }

        private void E_Completed(Object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        #endregion
        #region 接收数据
        /// <summary>
        /// 开始接收数据
        /// </summary>
        /// <param name="client"></param>
        private void StartReceive(ClientPeer client)
        {
            try
            {
                //判断这个Client的Socket是否有数据进来
                bool result = client.clientSocket.ReceiveAsync(client.ReceiveArgs);
                //有数据进来
                if (result == false)
                {
                    ProcessReceive(client.ReceiveArgs);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        /// <summary>
        /// 异步接收数据完成后调用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReceiveArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            //为了获取clientPeer
            ClientPeer client = e.UserToken as ClientPeer;
            //判断数据是否接收成功，后面字节大于0说明有数据接收
            if (client.ReceiveArgs.SocketError == SocketError.Success && client.ReceiveArgs.BytesTransferred > 0)
            {
                //当前接收到的长度Client.ReceiveArgs.BytesTransferred
                byte[] packet = new byte[client.ReceiveArgs.BytesTransferred];

                Buffer.BlockCopy(client.ReceiveArgs.Buffer, 0, packet, 0, client.ReceiveArgs.BytesTransferred);

                //上面拿到了数据

                //让clientPeer自身处理接收到的数据
                client.ProcessReceive(packet);
                //继续开启线程等待接收数据
                StartReceive(client);
            }
            else
            {
                //没有传输的字节数，就代表断开链接了
                if (client.ReceiveArgs.BytesTransferred == 0)
                {
                    //客户端主动来连接
                    if (client.ReceiveArgs.SocketError == SocketError.Success)
                    {
                        Disconnect(client, "客户端主动断开链接");
                    }
                    //因为网络异常被动断开链接
                    else
                    {
                        Disconnect(client, client.ReceiveArgs.SocketError.ToString());
                    }
                }
            }
        }
        #endregion
        #region 断开链接
        private void Disconnect(ClientPeer client, string reason)
        {
            try
            {
                if (client == null)
                {
                    throw new Exception("客户端为空，无法断开链接");
                }
                Console.WriteLine(client.clientSocket.RemoteEndPoint + "客户端断开链接,原因" + reason);
                //让客户端处理断开链接
                client.Disconnect();
                clientPeerPool.Enqueue(client);
                semaphore.Release();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

        }
        #endregion

    }
}
