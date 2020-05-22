using Newtonsoft.Json;
using RabbitMQ.Client;

using System;
using System.Text;

namespace SuperRabbitMQ
{
    /// <summary>
    /// 用于RabbitMQ发送者
    /// </summary>
    public class Sender : IDisposable
    {
        /// <summary>
        /// 构造方法
        /// </summary>
        protected Sender()
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
        /// 连接状态
        /// </summary>
        public bool IsOpen { get { return conn != null && conn.IsOpen; } }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            conn?.Close();
            conn?.Dispose();
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <param name="bytes">发送的数据</param>
        /// <param name="exchange">交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <returns></returns>
        protected bool TrySend(byte[] bytes, string exchange, string routingKey)
        {
            try
            {
                if (conn == null)
                {
                    conn = factory.CreateConnection();
                }
                if (!conn.IsOpen)
                {
                    throw new Exception($"连接未打开【{HostName}】【{Port}】【{UserName}】【{Password}】");
                }
                using (IModel channel = conn.CreateModel())
                {
                    channel.BasicPublish(exchange, routingKey, null, bytes);
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorEvent?.Invoke(ex);
                return false;
            }
        }

        /// <summary>
        /// 发送
        /// </summary>
        /// <typeparam name="T">发送的数据的类型</typeparam>
        /// <param name="data">发送的数据</param>
        /// <param name="exchange">交换机</param>
        /// <param name="routingKey">路由键</param>
        /// <param name="encoding">编码方式</param>
        /// <returns></returns>
        protected bool TrySend<T>(T data, string exchange, string routingKey, Encoding encoding = default)
        {
            if (encoding == default)
            {
                encoding = Encoding.UTF8;
            }
            return TrySend(encoding.GetBytes(JsonConvert.SerializeObject(data)), exchange, routingKey);
        }
    }
}
