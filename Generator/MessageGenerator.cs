using System;
using System.Linq;
using Messages;

namespace Generator
{
    public class MessageGenerator
    {
        private readonly Guid[] taskIds;
        private readonly Random random = new Random();
        private const string Characters = "ABCDEFGHIJKLMNOPQRSTUVXYZ";

        public MessageGenerator(int numberOfSagas)
        {
            taskIds = Enumerable.Range(0, numberOfSagas).Select(_ => Guid.NewGuid()).ToArray();
        }

        public TestMessage Generate()
        {
            var taskId = taskIds[random.Next(taskIds.Length)];
            var data = new string(Enumerable.Range(0, 5).Select(_ => Characters[random.Next(Characters.Length)]).ToArray());
            return new TestMessage()
            {
                TaskId = taskId,
                Data = data
            };
        }
    }
}