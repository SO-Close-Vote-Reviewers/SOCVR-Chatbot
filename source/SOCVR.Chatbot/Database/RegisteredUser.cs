namespace SOCVR.Chatbot.Bot.Database
{
    class RegisteredUser
    {
        public int Id { get; set; }
        public int ChatProfileId { get; set; }
        public bool IsOwner { get; set; }
    }
}
