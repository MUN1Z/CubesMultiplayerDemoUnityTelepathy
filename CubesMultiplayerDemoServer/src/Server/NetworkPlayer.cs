namespace Server
{
    public class NetworkPlayer
    {
        public int ConnectionID { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        
        public bool Moved { get; set; }
        
        public NetworkPlayer(int connectionID)
        {
            ConnectionID = connectionID;

            X = 0.0f;
            Y = 0.0f;
            Z = 0.0f;

            Moved = false;
        }
    }
}
