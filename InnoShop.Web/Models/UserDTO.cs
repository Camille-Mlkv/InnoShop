﻿namespace InnoShop.Web.Models
{
    public class UserDTO
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Name { get; set; }
        public bool IsEmailConfirmed { get; set; } = false;
    }
}
