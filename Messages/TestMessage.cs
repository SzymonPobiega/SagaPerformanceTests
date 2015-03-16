using System;
using NServiceBus;

namespace Messages
{
    public class TestMessage : IMessage
    {
        public Guid TaskId { get; set; }
        public string Data { get; set; }
    }
}
