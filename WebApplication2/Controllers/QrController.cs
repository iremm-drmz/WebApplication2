using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using WebApplication2.Models;
using System.Linq;

namespace WebApplication2.Controllers
{
    public class QrController : Controller
    {
        private readonly AppDbContext _context;

        public QrController(AppDbContext context)
        {
            _context = context;
        }

        // 🔹 SAYFA
        [HttpGet]
        public IActionResult Check()
        {
            return View();
        }

        // 🔹 QR / LINK KONTROL
        [HttpPost]
        public IActionResult Check(string qrCode)
        {
            if (string.IsNullOrWhiteSpace(qrCode))
            {
                ViewBag.IsValid = false;
                ViewBag.Message = "QR kod boş olamaz.";
                return View();
            }

            // ✅ ID'yi URL'den veya direkt değerden çıkar
            var id = ExtractId(qrCode.Trim());

            var ticket = _context.Tickets.FirstOrDefault(t => t.QrCode == id);

            if (ticket == null)
            {
                ViewBag.IsValid = false;
                ViewBag.Message = "Geçersiz bilet.";
                return View();
            }

            if (!ticket.IsActive)
            {
                ViewBag.IsValid = false;
                ViewBag.Message = "Bu bilet pasif (iptal edilmiş).";
                ViewBag.Ticket = ticket;
                return View();
            }

            ViewBag.IsValid = true;
            ViewBag.Message = "Geçerli bilet.";
            ViewBag.Ticket = ticket;
            return View();
        }

        // 🔹 TELEFON QR OKUTUNCA AÇILAN SAYFA
        [HttpGet("Qr/Show/{id}")]
        public IActionResult Show(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return NotFound();

            var ticket = _context.Tickets.FirstOrDefault(t => t.QrCode == id);
            if (ticket == null)
                return NotFound();

            return View(ticket);
        }

        // ✅ HEM query (?id=) HEM path (/Show/{id}) DESTEKLER
        private static string ExtractId(string input)
        {
            // URL ise
            if (input.StartsWith("http"))
            {
                try
                {
                    var uri = new System.Uri(input);

                    // 1️⃣ ?id= varsa
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    if (query.TryGetValue("id", out var idValue))
                        return idValue.ToString();

                    // 2️⃣ /Show/{id} varsa
                    var segments = uri.AbsolutePath.Split('/');
                    return segments.Last(); // GUID
                }
                catch
                {
                    return input;
                }
            }

            // Direkt GUID girildiyse
            return input;
        }
    }
}



