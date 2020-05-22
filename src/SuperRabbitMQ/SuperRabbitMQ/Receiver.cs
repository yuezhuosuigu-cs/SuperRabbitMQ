using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;

namespace SuperRabbitMQ
{
    /// <summary>
    /// 用于RabbitMQ接收者
    /// </summary>
    public class Receiver : IDisposable
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        protected Receiver()
        {
            HostName = "127.0.0.1";
            Port = 5672;
            UserName = "guest";
            Password = "guest";
            VirtualHost = "/";
        }

        /// <summary>
        /// 连接工厂
        /// </summary>
        private ConnectionFactory factory = new ConnectionFactory();

        /// <summary>
        /// 连接
        /// </summary>
        private IConnection conn = null;

        /// <summary>
        /// 异常处理事件
        /// </summary>
        protected event Action<Exception> ErrorEvent;

        /// <summary>
        /// 主机名/IP
        /// </summary>
        protected string HostName { get { return factory.HostName; } set { factory.HostName = value; } }

        /// <summary>
        /// 端口
        /// </summary>
        protected int Port { get { return factory.Port; } set { factory.Port = value; } }

        /// <summary>
        /// 用户名
        /// </summary>
        protected string UserName { get { return factory.UserName; } set { factory.UserName = value; } }

        /// <summary>
        /// 密码
        /// </summary>
        protected string Password { get { return factory.Password; } set { factory.Password = value; } }

        /// <summary>
        /// 虚拟主机
        /// </summary>
        protected string VirtualHost { get { return factory.VirtualHost; } set { factory.VirtualHost = value; } }

        /// <summary>
        /// 队列缓存字典
        /// </summary>
        private Dictionary<string, QueueParameter> Queues = new Dictionary<string, QueueParameter>();

        /// <summary>
        /// 连接状态
        /// </summary>
        public bool IsOpen { get { return conn != null && conn.IsOpen; } }

        /// <summary>
        /// 添加队列
        /// </summary>
        /// <param name="queueName">队列名称</param>
        /// <param name="action">接收到数据的处理</param>
        /// <returns></returns>
        public bool TryAddQueue(string queueName, Action<byte[]> action)
        {
            try
            {
                if (Queues.ContainsKey(queueName))
                {
                    throw new Exception($"已存在队列【{queueName}】");
                }
                if (conn == null)
                {
                    conn = factory.CreateConnection();
                }
                if (!conn.IsOpen)
                {
                    throw new Exception($"连接未打开【{HostName}】【{Port}】【{UserName}】【{Password}】");
                }
                var queue = new QueueParameter();
                queue.Name = queueName;
                queue.OnReceived = action;
                queue.Channel = conn.CreateModel();
                queue.Consumer = new EventingBasicConsumer(queue.Channel);
                queue.Channel.BasicQos(0, 1, false);//告诉broker，同一时间只处理一个消息
                queue.Consumer = new EventingBasicConsumer(queue.Channel);
                queue.Consumer.Received += (obj, arg) =>
                {
                    queue.OnReceived?.Invoke(arg.Body.ToArray());
                    queue.Channel.BasicAck(arg.DeliveryTag, false);
                };
                queue.Channel.BasicConsume(queue.Name, false, queue.Consumer);//告诉broker，发送消息之后，消息暂时不要删除，等消费者处理完成再说
                Queues.Add(queue.Name, queue);
                return Queues.ContainsKey(queueName) && conn.IsOpen;
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Queues.Clear();
            conn?.Close();
            conn?.Dispose();
        }
    }
}
