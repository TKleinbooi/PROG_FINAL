
using System.ComponentModel.DataAnnotations;

namespace RaceDayMVC.Models;

public class EventViewModel
{
    public int EventID { get; set; }

    [Required(ErrorMessage = "Event name is required.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Event date is required.")]
    [DataType(DataType.Date)]
    public DateTime EventDate { get; set; } = DateTime.Today.AddDays(7);

    [Required(ErrorMessage = "Location is required.")]
    public string Location { get; set; } = "";

    [Required(ErrorMessage = "Distance is required.")]
    [Range(0.01, 1000, ErrorMessage = "Distance must be between 0.01 and 1000 km.")]
    public decimal Distance { get; set; }

    [Required(ErrorMessage = "Event type is required.")]
    public string EventType { get; set; } = "Run";
}
