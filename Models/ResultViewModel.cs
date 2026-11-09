
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models;

public class ResultViewModel
{
    [Required]
    [Range(1, int.MaxValue)]
    public int EnrolmentID { get; set; }

    [Required]
    public TimeSpan FinishTime { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Position { get; set; }
}
