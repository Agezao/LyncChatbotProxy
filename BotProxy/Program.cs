using Microsoft.Lync.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotProxy
{
    class Program
    {
        private static Microsoft.Lync.Model.LyncClient client;

        private static LyncConversationManager conversation;

        static void Main(string[] args)
        {
            client = Microsoft.Lync.Model.LyncClient.GetClient();
            conversation = new LyncConversationManager(client);
            conversation.StartListening();
            //conversation.SendLyncMessage("some.ad.mail@company.com", "Test");
            Console.ReadKey();
        }
    }
}
