using RabbitMQ.Client;
using RabbitMQ.Client.Events;

using System;

namespace SuperRabbitMQ
{
    /// <summary>
    /// 队列参数
    /// </summary>
    internal class QueueParameter
    {
        /// <summary>
        /// 队列名
        /// </summary>
        internal string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal IModel Channel { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal EventingBasicConsumer Consumer { get; set; }

        /// <summary>
        /// 
        /// </summary>
        internal Action<byte[]> OnReceived { get; set; }
    }
}