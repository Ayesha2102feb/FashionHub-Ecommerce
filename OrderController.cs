using System.Security.Cryptography;
using System.Text;
using EcomWebsite.Models;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Razorpay.Api;
using OrderModel = EcomWebsite.Models.Order;


namespace EcomWebsite.Controllers
{
    public class OrderController : Controller
    {
        private readonly myContext _context;

        public OrderController(myContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // =========================
        // CHECKOUT PAGE
        // =========================

        [HttpPost]
        public IActionResult Checkout(List<int> selectedItems)
        {
            // ✅ FIXED SESSION
            var custIdString = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(custIdString))
                return RedirectToAction("CustomerLogin", "Customer");

            if (!int.TryParse(custIdString, out int customerId))
                return RedirectToAction("CustomerLogin", "Customer");

            if (selectedItems == null || !selectedItems.Any())
                return Content("Please select items");

            var cartItems = _context.tbl_cart
                .Where(c => selectedItems.Contains(c.cart_id))
                .Include(c => c.products)
                .ToList();

            if (!cartItems.Any())
                return Content("Cart is empty");

            decimal total = cartItems.Sum(x =>
                (x.products?.product_price ?? 0) * x.product_quantity);

            // ✅ SAVE SELECTED ITEMS IN SESSION
            HttpContext.Session.SetString("selected_cart_items",
                string.Join(",", selectedItems));

            ViewBag.TotalAmount = total;

            return View();
        }
        // =========================
        // SUCCESS PAGE
        // =========================
        public IActionResult Success()
        {
            return View();
        }


        public IActionResult MyOrders()
        {
            var custIdString = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(custIdString))
            {
                return RedirectToAction("CustomerLogin", "Customer");
            }

            if (!int.TryParse(custIdString, out int customerId))
            {
                return RedirectToAction("CustomerLogin", "Customer");
            }

            
            var orders = _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .ToList();

            return View(orders);
        }
        public IActionResult CancelOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);

            if (order != null)
            {
                order.Status = "Cancelled";
                _context.SaveChanges();
            }

            return RedirectToAction("MyOrders");
        }
        // =========================
        // ORDER DETAILS
        // =========================
        public IActionResult OrderDetails(int id)
        {
            var orderItems = _context.OrderItems
                .Where(o => o.OrderId == id)
                .Include(o => o.Product)
                .Include(o => o.Order)   // 🔥 ADD THIS LINE
                .ToList();

            return View(orderItems);
        }




        public IActionResult PaymentSuccess(string paymentId, string address)
        {
            // ✅ USE CORRECT SESSION
            var customerIdStr = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(customerIdStr))
                return Content("Session expired");

            // ✅ SAFE PARSE
            if (!int.TryParse(customerIdStr, out int customerId))
                return Content("Invalid session");

            // ✅ CHECK CUSTOMER EXISTS (VERY IMPORTANT)
            var customerExists = _context.tbl_customer
                .Any(c => c.customer_id == customerId);

            if (!customerExists)
                return Content("Customer not found");

            // ✅ GET SELECTED CART ITEMS
            var selectedItemsStr = HttpContext.Session.GetString("selected_cart_items");

            if (string.IsNullOrEmpty(selectedItemsStr))
                return Content("No selected items");

            var selectedIds = selectedItemsStr.Split(',')
                .Select(int.Parse)
                .ToList();

            var cartItems = _context.tbl_cart
                .Where(c => selectedIds.Contains(c.cart_id))
                .Include(c => c.products)
                .ToList();

            if (!cartItems.Any())
                return Content("Cart empty");

            // ✅ CALCULATE TOTAL
            decimal total = cartItems.Sum(x =>
                (x.products?.product_price ?? 0) * x.product_quantity);

            // ✅ CREATE ORDER
            var order = new OrderModel
            {
                CustomerId = customerId,
                OrderDate = DateTime.Now,
                TotalAmount = total,
                Status = "Paid",
                PaymentId = paymentId,
                PaymentStatus = "Success",
                ShippingAddress = address
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // ✅ ADD ORDER ITEMS
            foreach (var item in cartItems)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = item.prod_id,
                    Quantity = item.product_quantity,
                    Price = item.products.product_price
                });

                item.cart_status = 1;
            }

            _context.SaveChanges();

            return View("Success");
        }

        // =========================
        // PAYMENT FAILED
        // =========================
        public IActionResult PaymentFailed()
        {
            return View();
        }
        public IActionResult Invoice(int id)
        {
            var orderItems = _context.OrderItems
                .Where(o => o.OrderId == id)
                .Include(o => o.Product)
                .Include(o => o.Order)
                .ToList();

            if (!orderItems.Any())
            {
                return Content("No invoice data found");
            }

            return View(orderItems);
        }
        public IActionResult DownloadInvoice(int id)
        {
            var orderItems = _context.OrderItems
                .Where(o => o.OrderId == id)
                .Include(o => o.Product)
                .Include(o => o.Order)
                .ToList();

            if (!orderItems.Any())
                return Content("No data found");

            var order = orderItems.First().Order;

            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdf = new PdfDocument(writer);
                Document doc = new Document(pdf);

                // Fonts
                PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

                // Colors
                var headerBg = new iText.Kernel.Colors.DeviceRgb(63, 81, 181); // Blue
                var whiteColor = iText.Kernel.Colors.ColorConstants.WHITE;
                var borderColor = new iText.Kernel.Colors.DeviceRgb(200, 200, 200);

                // ================= HEADER =================
                doc.Add(new Paragraph("EShopping")
                    .SetFont(boldFont)
                    .SetFontSize(16)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Add(new Paragraph("INVOICE")
                    .SetFont(boldFont)
                    .SetFontSize(20)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetMarginBottom(10));

                // ================= ORDER DETAILS =================
                doc.Add(new Paragraph($"Order ID: {order?.Id}").SetFont(normalFont));
                doc.Add(new Paragraph($"Date: {order?.OrderDate:dd-MM-yyyy}").SetFont(normalFont));
                doc.Add(new Paragraph($"Payment ID: {order?.PaymentId ?? "N/A"}").SetFont(normalFont));
                doc.Add(new Paragraph($"Address: {order?.ShippingAddress}").SetFont(normalFont));

                doc.Add(new Paragraph("\n"));

                // ================= TABLE =================
                Table table = new Table(new float[] { 3, 2, 1, 2 });
                table.SetWidth(iText.Layout.Properties.UnitValue.CreatePercentValue(100));

                // Header Cells (Colored)
                table.AddHeaderCell(CreateHeaderCell("Product", boldFont, headerBg, whiteColor));
                table.AddHeaderCell(CreateHeaderCell("Price", boldFont, headerBg, whiteColor));
                table.AddHeaderCell(CreateHeaderCell("Qty", boldFont, headerBg, whiteColor));
                table.AddHeaderCell(CreateHeaderCell("Total", boldFont, headerBg, whiteColor));

                decimal grandTotal = 0;

                foreach (var item in orderItems)
                {
                    decimal total = item.Price * item.Quantity;
                    grandTotal += total;

                    table.AddCell(CreateBodyCell(item.Product?.product_name ?? "N/A", normalFont, borderColor));
                    table.AddCell(CreateBodyCell($"₹{item.Price:0.00}", normalFont, borderColor));
                    table.AddCell(CreateBodyCell(item.Quantity.ToString(), normalFont, borderColor));
                    table.AddCell(CreateBodyCell($"₹{total:0.00}", normalFont, borderColor));
                }

                doc.Add(table);

                // ================= GRAND TOTAL =================
                doc.Add(new Paragraph("\n"));

                doc.Add(new Paragraph($"Grand Total: ₹{grandTotal:0.00}")
                    .SetFont(boldFont)
                    .SetFontSize(14)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.RIGHT));

                // ================= FOOTER =================
                doc.Add(new Paragraph("\nThank you for your purchase!")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER));

                doc.Close();

                return File(ms.ToArray(), "application/pdf", $"Invoice_{order?.Id}.pdf");
            }
        }

        private Cell CreateHeaderCell(string text, PdfFont font, iText.Kernel.Colors.Color bgColor, iText.Kernel.Colors.Color fontColor)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font).SetFontColor(fontColor))
                .SetBackgroundColor(bgColor)
                .SetPadding(5)
                .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER);
        }

        private Cell CreateBodyCell(string text, PdfFont font, iText.Kernel.Colors.Color borderColor)
        {
            return new Cell()
                .Add(new Paragraph(text).SetFont(font))
                .SetBorder(new iText.Layout.Borders.SolidBorder(borderColor, 1))
                .SetPadding(5);
        }

    }
}
