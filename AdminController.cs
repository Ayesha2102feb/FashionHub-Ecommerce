using EcomWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EcomWebsite.Controllers
{
    public class AdminController : Controller
    {
        private myContext _context;
        private IWebHostEnvironment _env;

        public AdminController(myContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("user_role");

            if (role == null || role != "admin")
            {
                return RedirectToAction("Login", "Admin");
            }

            var totalOrders = _context.Orders.Count();
            var totalUsers = _context.tbl_customer.Count();
            var totalProducts = _context.tbl_product.Count();

            var totalRevenue = _context.Orders
                .Where(o => o.Status != "Cancelled")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalProducts = totalProducts;
            ViewBag.TotalRevenue = totalRevenue;

            return View();
        }
        public IActionResult Login()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("user_role")))
            {
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }


        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.tbl_users
                .FirstOrDefault(x => x.email == email && x.password == password);

            if (user == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            // ✅ COMMON SESSION
            HttpContext.Session.SetString("user_email", user.email);
            HttpContext.Session.SetString("user_role", user.role.ToLower());

            // ✅ IMPORTANT: STORE ID BASED ON ROLE

            if (user.role.ToLower() == "admin")
            {
                // 🔥 get admin from tbl_admin
                var admin = _context.tbl_admin
                    .FirstOrDefault(a => a.admin_email == user.email);

                if (admin != null)
                {
                    HttpContext.Session.SetString("admin_session", admin.admin_id.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                // 🔥 get customer from tbl_customer
                var customer = _context.tbl_customer
                    .FirstOrDefault(c => c.customer_email == user.email);

                if (customer != null)
                {
                    HttpContext.Session.SetString("user_id", customer.customer_id.ToString());
                }

                return RedirectToAction("Index", "Home");
            }
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("CustomerLogin", "Customer");
        }
        public IActionResult Profile()
        {
            var adminId = HttpContext.Session.GetString("admin_session");

            // ✅ check session
            if (string.IsNullOrEmpty(adminId))
            {
                return RedirectToAction("Login", "Admin");
            }

            // ✅ safe parse
            if (!int.TryParse(adminId, out int id))
            {
                return RedirectToAction("Login", "Admin");
            }

            var row = _context.tbl_admin
                .Where(a => a.admin_id == id)
                .ToList();

            return View(row);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Profile(Admin admin)
        {
            if (ModelState.IsValid)
            {
                _context.tbl_admin.Update(admin);
                _context.SaveChanges();
            }

            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult ChangeProfileImage(IFormFile admin_image, Admin admin)
        {
            string Imagepath = Path.Combine(_env.WebRootPath, "admin_image",admin_image.FileName);
            FileStream fs = new FileStream(Imagepath, FileMode.Create);
            admin_image.CopyTo(fs);
            admin.admin_image = admin_image.FileName;
            _context.tbl_admin.Update(admin);
            _context.SaveChanges();
            return RedirectToAction("Profile");
        }
        public IActionResult fetchCustomer()
        {
            return View(_context.tbl_customer.ToList());
        }
        public IActionResult CustomerDetails(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }

        public IActionResult updateCustomer(int id)
        {
            return View(_context.tbl_customer.Find(id));
        }
        
        [HttpPost]
        public IActionResult updateCustomer(Customer customer, IFormFile? customer_image)
        {
            var existing = _context.tbl_customer.Find(customer.customer_id);

            if (existing == null)
                return NotFound();

            // ✅ Update only required fields
            existing.customer_name = customer.customer_name;
            existing.customer_email = customer.customer_email;
            existing.customer_phone = customer.customer_phone;
            existing.customer_gender = customer.customer_gender;
            existing.customer_country = customer.customer_country;
            existing.customer_city = customer.customer_city;

            // ❌ Do NOT update password here (optional feature)

            // ✅ Image optional
            if (customer_image != null)
            {
                string imagePath = Path.Combine(_env.WebRootPath, "customer_images", customer_image.FileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    customer_image.CopyTo(fs);
                }

                existing.customer_image = customer_image.FileName;
            }

            _context.SaveChanges();

            return RedirectToAction("fetchCustomer");
        }
        public IActionResult deletePermission(int id)
        {
            return View(_context.tbl_customer.FirstOrDefault(c => c.customer_id == id));
        }
        public IActionResult deleteCustomer(int id)
        {
            var customer = _context.tbl_customer.Find(id);
            _context.tbl_customer.Remove(customer);
            _context.SaveChanges();
            return RedirectToAction("fetchCustomer");
        }
        public IActionResult fetchCategory()
        {
            return View(_context.tbl_category.ToList());
        }
        public IActionResult addCategory()
        {
            return View();
        }
        [HttpPost]
        public IActionResult addCategory(Category cat)
        {
            _context.tbl_category.Add(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult updateCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            return View(category);
        }
        [HttpPost]
        public IActionResult updateCategory(Category cat)
        {
            _context.tbl_category.Update(cat);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult deletePermissionCategory(int id)
        {
           
            return View(_context.tbl_category.FirstOrDefault(c => c.category_id == id));
        }
        public IActionResult deleteCategory(int id)
        {
            var category = _context.tbl_category.Find(id);
            _context.tbl_category.Remove(category);
            _context.SaveChanges();
            return RedirectToAction("fetchCategory");
        }
        public IActionResult fetchProduct()
        {
            return View(_context.tbl_product.ToList());
        }
        public IActionResult addProduct()
        {
            List <Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            return View();
        }
        [HttpPost]
        public IActionResult addProduct(Product prod,IFormFile product_image)
        {
            string imageName = Path.GetFileName(product_image.FileName);
            string imagePath = Path.Combine(_env.WebRootPath,"product_images",imageName);
            FileStream fs = new FileStream(imagePath, FileMode.Create);
            product_image.CopyTo(fs);
            prod.product_image = imageName;
            _context.tbl_product.Add(prod);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }
        public IActionResult productDetails(int id)

        {
            return View(_context.tbl_product.Include(p => p.Category).FirstOrDefault(p=>p.product_id == id));
        }
        public IActionResult deletePermissionProduct(int id)
        {

            return View(_context.tbl_product.FirstOrDefault(p => p.product_id == id));
        }
        public IActionResult deleteProduct(int id)
        {
            var product = _context.tbl_product.Find(id);
            _context.tbl_product.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }

        public IActionResult updateProduct(int id)
        {
            List<Category> categories = _context.tbl_category.ToList();
            ViewData["category"] = categories;
            var product = _context.tbl_product.Find(id);
            ViewBag.selectedCategoryId = product.cat_id;
            return View(product);
        }
        [HttpPost]
        public IActionResult updateProduct(Product product)
        {
            _context.tbl_product.Update(product);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }
        [HttpPost]
        public IActionResult ChangeProductImage(IFormFile product_image, Product product)
        {
            string Imagepath = Path.Combine(_env.WebRootPath, "product_images", product_image.FileName);
            FileStream fs = new FileStream(Imagepath, FileMode.Create);
            product_image.CopyTo(fs);
            product.product_image = product_image.FileName;
            _context.tbl_product.Update(product);
            _context.SaveChanges();
            return RedirectToAction("fetchProduct");
        }
        public IActionResult fetchFeedback()
            {

            return View(_context.tbl_feedback.ToList());

            }
        public IActionResult deletePermissionFeedback(int id)
        {

            return View(_context.tbl_feedback.FirstOrDefault(f => f.feedback_id == id));
        }
        public IActionResult deleteFeedback(int id)
        {
            var feedback = _context.tbl_feedback.Find(id);
            _context.tbl_feedback.Remove(feedback);
            _context.SaveChanges();
            return RedirectToAction("fetchFeedback");
        }
        public IActionResult fetchCart()
        {

            
               var cart = _context.tbl_cart.Include(c=>c.products).Include(c => c.customers).ToList();
            return View(cart);
        }
        public IActionResult deletePermissionCart(int id)
        {

            return View(_context.tbl_cart.FirstOrDefault(c => c.cart_id == id));
        }
        public IActionResult deleteCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);
            _context.tbl_cart.Remove(cart);
            _context.SaveChanges();
            return RedirectToAction("fetchCart");
        }
        public IActionResult updateCart(int id)
        {
            var cart = _context.tbl_cart.Find(id);

            return View(cart);
        }
        [HttpPost]
        public IActionResult updateCart(int cart_status,Cart cart)
        {
            cart.cart_status = cart_status;
             _context.tbl_cart.Update(cart);
           
            _context.SaveChanges();
            return RedirectToAction("fetchCart");
           
        }
        public IActionResult Orders()
        {
            try
            {
                // ✅ 1. CHECK ADMIN SESSION
                var role = HttpContext.Session.GetString("user_role");

                if (string.IsNullOrEmpty(role) || role.ToLower() != "admin")
                {
                    return RedirectToAction("Login", "Admin");
                }

                // ✅ 2. FETCH ORDERS WITH RELATED DATA
                var orders = _context.Orders
    .Include(o => o.Customer)
    .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
    .AsEnumerable() // ✅ forces safe loading
    .ToList();

                // ✅ 3. RETURN TO VIEW
                return View(orders);
            }
            catch (Exception ex)
            {
                // ✅ DEBUG (OPTIONAL)
                return Content("Error: " + ex.Message);
            }
        }
        // GET: Load page
        public IActionResult UpdateStatus(int id)
        {
            var order = _context.Orders.Find(id);
            return View(order);
        }

        // POST: Save update
        [HttpPost]
        public IActionResult UpdateStatus(Order model)
        {
            var order = _context.Orders.Find(model.Id);

            if (order != null)
            {
                order.Status = model.Status;
                _context.SaveChanges();
            }

            return RedirectToAction("Orders");
        }

        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.Customer)
                .FirstOrDefault(o => o.Id == id);

            var items = _context.OrderItems
                .Where(o => o.OrderId == id)
                .Include(o => o.Product)
                .ToList();

            ViewBag.Order = order;

            return View(items);
        }
        
    }
}
