using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication2.Models
{
    public class Ticket
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        // ✅ DB’de TEK kolon: "Şehir | Müze"
        [Required]
        public string Museum { get; set; }

        public DateTime CreatedDate { get; set; }

        [Required]
        public string QrCode { get; set; }

        // ✅ Senin DB’de var ve NOT NULL olduğu için mutlaka modelde olmalı
        public bool IsCanceled { get; set; } = false;

        // ✅ DB’de var ve NOT NULL olduğu için mutlaka modelde olmalı
        public bool IsActive { get; set; } = true;


        // ✅ DB’de kolon yok → NotMapped (Museum’dan türetiliyor)
        [NotMapped]
        public string City
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Museum)) return "";
                var parts = Museum.Split('|');
                return parts.Length > 1 ? parts[0].Trim() : "";
            }
        }

        // ✅ DB’de kolon yok → NotMapped (Museum’dan türetiliyor)
        [NotMapped]
        public string Place
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Museum)) return "";
                var parts = Museum.Split('|');
                return parts.Length > 1 ? parts[1].Trim() : Museum.Trim();
            }
        }
    }
}
