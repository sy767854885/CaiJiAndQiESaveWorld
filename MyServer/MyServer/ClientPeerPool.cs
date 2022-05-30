using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyServer
{
    //客户端对象连接池
    class ClientPeerPool
    {
        //创建一个队列用于存储客户端对象
        private Queue<ClientPeer> clientPeerQueue;

        public ClientPeerPool(int maxCount)
        {
            clientPeerQueue = new Queue<ClientPeer>(maxCount);
        }

        //方法用于将对象添加到Queue的末尾
        public void Enqueue(ClientPeer client)
        {
            clientPeerQueue.Enqueue(client);
        }
        //取出对象
        public ClientPeer Dequeue()
        {
            return clientPeerQueue.Dequeue();
        }


    }
}
