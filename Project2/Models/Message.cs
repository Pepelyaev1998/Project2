﻿namespace Project2.Models
{
    public class Message : Entity
    {
        public int Id { get; set; }
        public int FromUserId { get; set; }
        public int ToUserId { get; set; }
        public string SentMessage { get; set; }
    }
}
