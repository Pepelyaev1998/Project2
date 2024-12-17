using System;

namespace Project2.Models
{
    public class Package : Entity
    {
        public int Id { get; set; }
        public string TrackNumber { get; set; }
        public string StartingPoint { get; set; }
        public string EndPoint { get; set; }
        public string LastLocation { get; set; }
        public string CurrentState { get; set; }
        public DateTime LastDateOfUpdate { get; set; }
    }
}
