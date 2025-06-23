using System.ComponentModel.DataAnnotations;
using Ensyu_E_PAN.Models.Attendance;

namespace Ensyu_E_PAN.Models.Masters
{
    public class WorkRoll
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        public ICollection<DateSchedule> DateSchedules { get; set; }
    }
}
