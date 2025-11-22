using System;

namespace Application.DTOs
{
    public class CreateClientRequest
    {
        public string TaxId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class ClientDetailsResponse
    {
        public Guid ClientId { get; set; }
        public string TaxId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
