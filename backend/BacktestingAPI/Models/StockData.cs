using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BacktestingAPI.Models
{

    /// Represents a single daily OHLCV data point for a stock symbol.

    public class StockData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(16)]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Open { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal High { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Low { get; set; }

        [Column(TypeName = "decimal(18,6)")]
        public decimal Close { get; set; }

        public long Volume { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}
