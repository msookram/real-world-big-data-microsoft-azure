using System;
using System.ComponentModel.DataAnnotations;

namespace Telemetry.Entities.Model
{
    public partial class EventMetric
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Period { get; set; }

        [Required]
        [StringLength(40)]
        public string EventName { get; set; }

        [Required]
        [StringLength(5)]
        public string PartitionId { get; set; }

        public long? Count { get; set; }

        public DateTime ProcessedAt { get; set; }
    }
}
