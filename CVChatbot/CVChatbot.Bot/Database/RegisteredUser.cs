namespace SOCVR.Chatbot.Core.Database
{
    class RegisteredUser
    {
        public int Id { get; set; }
        public int ChatProfileId { get; set; }
        public bool IsOwner { get; set; }
    }
}
