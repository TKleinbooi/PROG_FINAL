
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models;

public class EnrolmentViewModel
{
    [Required]
    public int EventID { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Enter a valid category ID.")]
    public int CategoryID { get; set; }
}

