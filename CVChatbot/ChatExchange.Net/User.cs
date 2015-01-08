using Newtonsoft.Json.Linq;



namespace ChatExchangeDotNet
{
    public class User
    {
        public string Name { get; private set; }
        public int ID { get; private set; }
        public bool IsMod { get; private set; }
        public bool IsRoomOwner { get; private set; }
        public int Reputation { get; private set; }
        public int RoomID { get; private set; }
        public string Host { get; private set; }



        public User(string host, int roomID, int id)
        {
            ID = id;
            RoomID = roomID;
            Host = host;

            var res = RequestManager.SendPOSTRequest("http://chat." + host + "/user/info", "ids=" + id + "&roomid=" + roomID);

            if (res == null)
            {
                Reputation = -1;
            }
            else
            {
                var resContent = RequestManager.GetResponseContent(res);

                var json = JObject.Parse(resContent);

                var name = json["users"][0]["name"];
                var isMod = json["users"][0]["is_moderator"];
                var isOwner = json["users"][0]["is_owner"];
                var rep = json["users"][0]["reputation"];

                Name = name != null && name.Type == JTokenType.String ? (string)name : "";
                IsMod = isMod != null && isMod.Type == JTokenType.Boolean && (bool)isMod;
                IsRoomOwner = isOwner != null && isOwner.Type == JTokenType.Boolean && (bool)isOwner;
                Reputation = rep == null || rep.Type != JTokenType.Integer ? 1 : (int)rep;
            }
        }
    }
}
