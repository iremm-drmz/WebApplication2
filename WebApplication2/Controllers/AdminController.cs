using Microsoft.AspNetCore.Mvc;
using WebApplication2.Data;
using System;
using System.Linq;

namespace WebApplication2.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin
        public IActionResult Index(string name, string city, string range)
        {
            ViewBag.Name = name ?? "";
            ViewBag.City = city ?? "";
            ViewBag.Range = range ?? "";

            ViewBag.Cities = new System.Collections.Generic.List<string>
            {
                "İstanbul","Ankara","İzmir","Antalya","Çanakkale","Nevşehir","Şanlıurfa",
                "Bursa","Trabzon","Gaziantep","Mardin"
            };

            var q = _context.Tickets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(name))
                q = q.Where(t => t.FullName.Contains(name));

            // City DB’de yok → NotMapped. Filtreyi Museum içinden yapıyoruz: "Şehir | ..."
            if (!string.IsNullOrWhiteSpace(city))
                q = q.Where(t => t.Museum.StartsWith(city + " |"));

            // ✅ Tek inputtan tarih aralığı parse: "YYYY-MM-DD to YYYY-MM-DD"
            if (!string.IsNullOrWhiteSpace(range))
            {
                // flatpickr "2025-12-01 to 2025-12-18" şeklinde gönderir
                var parts = range.Split(new string[] { " to " }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 1 && DateTime.TryParse(parts[0], out var fromDate))
                    q = q.Where(t => t.CreatedDate >= fromDate);

                if (parts.Length >= 2 && DateTime.TryParse(parts[1], out var toDate))
                    q = q.Where(t => t.CreatedDate < toDate.AddDays(1));
            }

            var list = q.OrderByDescending(t => t.CreatedDate).ToList();
            return View(list);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var t = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (t != null)
            {
                _context.Tickets.Remove(t);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ToggleStatus(int id)
        {
            var t = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (t != null)
            {
                t.IsActive = !t.IsActive;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var t = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (t == null) return RedirectToAction("Index");
            return View(t);
        }

        [HttpPost]
        public IActionResult Edit(int id, string fullName, string city, string museum, DateTime createdDate, bool isActive)
        {
            var t = _context.Tickets.FirstOrDefault(x => x.Id == id);
            if (t == null) return RedirectToAction("Index");

            t.FullName = fullName?.Trim();
            t.Museum = $"{(city ?? "").Trim()} | {(museum ?? "").Trim()}";
            t.CreatedDate = createdDate;
            t.IsActive = isActive;

            _context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
