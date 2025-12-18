using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Linq;

namespace WebApplication2.Controllers
{
    public class TicketController : Controller
    {
        private readonly AppDbContext _context;

        public TicketController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: /Ticket/Buy?city=İstanbul
        [HttpGet]
        public IActionResult Buy(string? city)
        {
            ViewBag.SelectedCity = city ?? "";
            ViewBag.CityMuseums = GetCityMuseums();
            return View();
        }

        // ✅ POST: /Ticket/Buy
        [HttpPost]
        public IActionResult Buy(string fullName, string city, string museum)
        {
            ViewBag.CityMuseums = GetCityMuseums();
            ViewBag.SelectedCity = city ?? "";

            if (string.IsNullOrWhiteSpace(fullName) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(museum))
            {
                ViewBag.Error = "Ad Soyad, Şehir ve Müze seçimi zorunludur.";
                return View();
            }

            // QR id
            var qrCodeId = Guid.NewGuid().ToString();

            // ✅ DB’ye TEK kolon yazıyoruz: Museum = "Şehir | Müze"
            var museumDb = $"{city.Trim()} | {museum.Trim()}";

            var ticket = new Ticket
            {
                FullName = fullName.Trim(),
                Museum = museumDb,
                CreatedDate = DateTime.Now,
                QrCode = qrCodeId,
                IsCanceled = false,
                IsActive = true
                // ✅ NOT NULL hatası yememek için şart
            };

            _context.Tickets.Add(ticket);
            _context.SaveChanges();

            // ✅ Telefonda okutunca açılacak link (QR içine bu yazılır)
            var qrLink = Url.Action("Show", "Qr", new { id = ticket.QrCode }, Request.Scheme);

            // ✅ QR görseli (okunur olsun)
            var qrBytes = GenerateQrPngBytes(qrLink!, pixelsPerModule: 14);

            ViewBag.QrCodeImage = "data:image/png;base64," + Convert.ToBase64String(qrBytes);
            ViewBag.QrLink = qrLink;

            ViewBag.Success = "Bilet başarıyla oluşturuldu.";
            ViewBag.TicketId = ticket.Id;
            ViewBag.FullName = ticket.FullName;
            ViewBag.City = ticket.City;      // NotMapped (Museum’dan türetiliyor)
            ViewBag.Museum = ticket.Place;   // NotMapped (Museum’dan türetiliyor)
            ViewBag.CreatedDate = ticket.CreatedDate.ToString("dd.MM.yyyy HH:mm");

            return View();
        }

        private static byte[] GenerateQrPngBytes(string data, int pixelsPerModule)
        {
            var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrData);
            return qrCode.GetGraphic(pixelsPerModule);
        }

        // ✅ Şehirler + her şehirde en az 3 yer (Mardin eklendi)
        private static Dictionary<string, List<string>> GetCityMuseums()
        {
            return new Dictionary<string, List<string>>
            {
                ["İstanbul"] = new List<string> { "Topkapı Sarayı", "Ayasofya", "Galata Kulesi", "Dolmabahçe Sarayı" },
                ["Ankara"] = new List<string> { "Anadolu Medeniyetler Müzesi", "Anıtkabir", "Etnografya Müzesi" },
                ["İzmir"] = new List<string> { "Efes Antik Kenti", "Bergama", "Agora Ören Yeri" },
                ["Antalya"] = new List<string> { "Aspendos", "Perge Antik Kenti", "Side Antik Kenti" },
                ["Çanakkale"] = new List<string> { "Troya Ören Yeri", "Assos Antik Kenti", "Çimenlik Kalesi" },
                ["Nevşehir"] = new List<string> { "Göreme Açık Hava Müzesi", "Kaymaklı Yeraltı Şehri", "Derinkuyu Yeraltı Şehri" },
                ["Şanlıurfa"] = new List<string> { "Göbeklitepe", "Harran Ören Yeri", "Şanlıurfa Müzesi" },
                ["Bursa"] = new List<string> { "Bursa Kent Müzesi", "Cumalıkızık", "Muradiye Külliyesi" },
                ["Trabzon"] = new List<string> { "Sümela Manastırı", "Trabzon Müzesi", "Ayasofya (Trabzon)" },
                ["Gaziantep"] = new List<string> { "Zeugma Mozaik Müzesi", "Gaziantep Kalesi", "Rumkale" },

                // ✅ Mardin eklendi
                ["Mardin"] = new List<string> { "Dara Antik Kenti", "Mardin Müzesi", "Zinciriye Medresesi" }
            };
        }
    }
}

