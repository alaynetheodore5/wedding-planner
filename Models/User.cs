using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class User
    {
        [Key]

            public int UserId { get; set; }

            [Required]
            [MinLength(2, ErrorMessage="First Name must be 2 characters or longer!")]
            public string FirstName {get;set;}

            [Required]
            [MinLength(2, ErrorMessage="Last Name must be 2 characters or longer!")]
            public string LastName {get;set;}

            [EmailAddress]
            [Required]
            public string Email {get;set;}

            [DataType(DataType.Password)]
            [Required]
            [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
            public string Password {get;set;}

            public DateTime CreatedAt {get;set;} = DateTime.Now;
            public DateTime UpdatedAt {get;set;} = DateTime.Now;

            // We use the NotMapped Annotation so that this variable doesn't end up in our database.
            [NotMapped]
            [Compare("Password")]
            [DataType(DataType.Password)]
            public string Confirm {get;set;}

            public List<Association> MyRSVPs { get; set; }

            public List<Wedding> PlannedWeddings { get; set; }
    }
}