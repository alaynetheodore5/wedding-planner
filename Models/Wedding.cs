using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]

            public int WeddingId { get; set; }

            [Required]
            [MinLength(2, ErrorMessage="First Wedder is required")]
            public string WedderOne {get;set;}

            [Required]
            [MinLength(2, ErrorMessage="Second Wedder is required")]
            public string WedderTwo {get;set;}

            [Required(ErrorMessage="Start Date is required")]
            [DataType(DataType.Date)]
            [FutureDate]
            public DateTime WeddingDate { get; set; }

            [Required]
            [MinLength(8, ErrorMessage="Address must be 8 characters or longer!")]
            public string Address {get;set;}

            public DateTime CreatedAt {get;set;} = DateTime.Now;
            public DateTime UpdatedAt {get;set;} = DateTime.Now;

            // This is the foreign key
            public int UserId { get; set; }

            // A wedding can have only one User that plans it.
            // Not stored in database.
            public User Planner { get; set; }

            public List<Association> WeddingAttendees {get; set;}
    }
}