﻿namespace Domain.Entities.Responses
{
    public class UserProfileResponse : BaseEntity
    {
        public string Name { get; set; }
        public string Entity { get; set; }
        public string Email { get; set; }
        public string Photo { get; set; }
        public string Roles { get; set; }
    }
}
