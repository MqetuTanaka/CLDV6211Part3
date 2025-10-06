using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.Data.Tables;

namespace cloud1.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; } = "Customer";

        // Use CustomerID as RowKey for consistency
        public string RowKey
        {
            get => CustomerID;
            set => CustomerID = value;
        }

        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        [Display(Name = "Customer ID")]
        public string CustomerID { get; set; } = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Shipping Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        // Remove these duplicate properties as they're not needed
        // public object CustomerName { get; internal set; }
        // public object CustomerEmail { get; internal set; }
        // public object CustomerPhone { get; internal set; }
    }
}