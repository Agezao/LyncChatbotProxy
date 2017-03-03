using Microsoft.Bot.Connector.DirectLine;
using Microsoft.Bot.Connector.DirectLine.Models;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Conversation;
using System.Linq;

namespace BotProxy
{
    public class LyncConversationManager
    {
        private readonly DirectLineClient directLineCLient;
        private readonly LyncClient lyncClient;
        public LyncConversationManager(LyncClient _lyncClient)
        {
            lyncClient = _lyncClient;
            directLineCLient = new DirectLineClient("<-- YOUR DIRECT LINE CODE HERE -->");
        }

        public void StartListening()
        {
            lyncClient.ConversationManager.ConversationAdded += ConversationManager_ConversationAdded;
        }

        private void ConversationManager_ConversationAdded(object sender, ConversationManagerEventArgs e)
        {
            e.Conversation.ParticipantAdded += Conversation_ParticipantAdded;

            foreach (var key in e.Conversation.Modalities.Keys)
            {
                Modality val = e.Conversation.Modalities[key];
                if (val != null)
                {
                    val.Accept();
                }
            }
        }

        private void Conversation_ParticipantAdded(object sender, ParticipantCollectionChangedEventArgs e)
        {
            if (e.Participant.IsSelf == false)
            {
                var imModality = (InstantMessageModality)e.Participant.Modalities[ModalityTypes.InstantMessage];
                imModality.InstantMessageReceived += ImModality_InstantMessageReceived;
            }
        }

        private void ImModality_InstantMessageReceived(object sender, MessageSentEventArgs e)
        {
            InstantMessageModality im = (InstantMessageModality)sender;
            if (!im.Participant.IsSelf)
            {
                var botConversation = directLineCLient.Conversations.NewConversationWithHttpMessagesAsync().Result.Body;

                var message = new Message();
                message.ConversationId = botConversation.ConversationId;
                message.Text = e.Text;
                var result = directLineCLient.Conversations.PostMessageWithHttpMessagesAsync(botConversation.ConversationId, message).Result;

                var response = directLineCLient.Conversations.GetMessagesWithHttpMessagesAsync(botConversation.ConversationId).Result;

                im.BeginSendMessage(response.Body.Messages.LastOrDefault().Text, null, response.Body.Messages.LastOrDefault());
            }
        }

        public void SendLyncMessage(string contactMail, string message)
        {
            var contact = lyncClient.ContactManager.GetContactByUri(contactMail);
            var conv = lyncClient.ConversationManager.AddConversation();

            conv.AddParticipant(lyncClient.ContactManager.GetContactByUri(contact.Uri));

            InstantMessageModality m = conv.Modalities[ModalityTypes.InstantMessage] as InstantMessageModality;
            m.BeginSendMessage(message, null, null);
        }
    }
}
