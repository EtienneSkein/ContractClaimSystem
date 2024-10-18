
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ContractClaimSystem.Models
{
    public class ClaimSubmission
    {
        public int Id { get; set; }

        [Required]
        public string LecturerName { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Hours Worked must be a positive value.")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Hourly Rate must be a positive value.")]
        public decimal HourlyRate { get; set; }

        public string? AdditionalNotes { get; set; }

        public DateTime ClaimDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        public string? SupportingDocument { get; set; }

        // New property to hold multiple file paths for additional uploads
        public List<string> AdditionalUploads { get; set; } = new List<string>();
    }
}
