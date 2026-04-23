using EcomWebsite.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcomWebsite.Controllers
{
    public class CustomerController : Controller
    {
        private myContext _context;
        private IWebHostEnvironment _env;
        public CustomerController(myContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        
        public IActionResult Index()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            List<Product> products = _context.tbl_product.Take(4).ToList();
            ViewData["product"] = products;
            ViewBag.CartCount = GetCartCount();
            ViewBag.checkSession = HttpContext.Session.GetString("customer_session");

            return View();
        }
        public IActionResult CustomerLogin()
        {
            return View();
        }


       
        
       
        
       
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult CustomerLogin(string customer_email, string customer_password)
        {
            if (string.IsNullOrEmpty(customer_email) || string.IsNullOrEmpty(customer_password))
            {
                ViewBag.message = "Enter email & password";
                return View();
            }

            // ✅ LOGIN FROM USERS TABLE
            var user = _context.tbl_users
                .FirstOrDefault(x => x.email == customer_email);

            if (user == null || user.password != customer_password)
            {
                ViewBag.message = "Invalid login";
                return View();
            }

            // ✅ SAFE ROLE
            string role = user.role?.ToLower() ?? "";

            HttpContext.Session.SetString("user_email", user.email ?? "");
            HttpContext.Session.SetString("user_role", role);

            // =========================
            // 🔥 ADMIN LOGIN
            // =========================
            if (role == "admin")
            {
                HttpContext.Session.SetString("admin_id", user.id.ToString());
                return RedirectToAction("Index", "Admin");
            }

            // =========================
            // 🔥 CUSTOMER LOGIN (IMPORTANT)
            // =========================
            if (role == "customer")
            {
                var customer = _context.tbl_customer
                    .FirstOrDefault(c => c.customer_email == user.email);

                if (customer == null)
                {
                    ViewBag.message = "Customer data not found. Please register again.";
                    return View();
                }

                // ✅ ONLY THIS SESSION IS USED EVERYWHERE
                HttpContext.Session.SetString("customer_id", customer.customer_id.ToString());

                return RedirectToAction("Index", "Customer");
            }

            return View();
        }
        
        public IActionResult CustomerRegister()
        {
            return View();
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public IActionResult CustomerRegister(Customer customer)
        {
            if (ModelState.IsValid)
            {
                // Save in customer table
                customer.role = "Customer";
                _context.tbl_customer.Add(customer);

                // Save in users table
                var uuser = new Users
                {
                    email = customer.customer_email,
                    phone = customer.customer_phone,
                    password = customer.customer_password,
                    role = "customer"
                };

                _context.tbl_users.Add(uuser);

                _context.SaveChanges();

                // 🔥 ADD THIS (AUTO LOGIN)
                HttpContext.Session.SetString("customer_id", customer.customer_id.ToString());
                HttpContext.Session.SetString("user_role", "customer");
                HttpContext.Session.SetString("user_email", customer.customer_email ?? "");
                
                // ✅ REDIRECT TO HOME (NOT LOGIN)
                return RedirectToAction("CustomerLogin", "Customer");
            }

            return View(customer);
        }
        public IActionResult CustomerLogout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("CustomerLogin");
        }
        public IActionResult CustomerProfile()
        {
            var userId = HttpContext.Session.GetString("customer_id");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("CustomerLogin");
            }

            int id = int.Parse(userId);

            var customer = _context.tbl_customer
                .FirstOrDefault(x => x.customer_id == id);

            return View(customer);
        }
        [HttpPost]
        public IActionResult updateCustomerProfile(Customer customer)
        {
            var email = HttpContext.Session.GetString("user_email");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("CustomerLogin");
            }

            var existingCustomer = _context.tbl_customer
                .FirstOrDefault(c => c.customer_email == email);

            if (existingCustomer != null)
            {
                existingCustomer.customer_name = customer.customer_name;
                existingCustomer.customer_email = customer.customer_email;
                existingCustomer.customer_phone = customer.customer_phone;
                existingCustomer.customer_password = customer.customer_password;
                existingCustomer.customer_address = customer.customer_address;

                _context.SaveChanges();

                // ✅ SUCCESS MESSAGE
                TempData["Success"] = "Profile updated successfully!";
            }

            return RedirectToAction("CustomerProfile");
        }
        [HttpPost]
        public IActionResult changeProfileImage(IFormFile customer_image)
        {
            var email = HttpContext.Session.GetString("user_email");

            if (string.IsNullOrEmpty(email))
            {
                return RedirectToAction("CustomerLogin");
            }

            var customer = _context.tbl_customer
                .FirstOrDefault(c => c.customer_email == email);

            if (customer != null && customer_image != null)
            {
                // ✅ Unique file name
                string fileName = Guid.NewGuid().ToString() + "_" + customer_image.FileName;

                string imagePath = Path.Combine(_env.WebRootPath, "customer_images", fileName);

                using (FileStream fs = new FileStream(imagePath, FileMode.Create))
                {
                    customer_image.CopyTo(fs);
                }

                customer.customer_image = fileName;

                _context.SaveChanges();
            }

            return RedirectToAction("CustomerProfile");
        }
        public IActionResult feedback()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            return View();
        }
        [HttpPost]
        public IActionResult feedback(Feedback feedback)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("customer_session")))
            {
                return RedirectToAction("CustomerLogin");
            }
            else
            {
                TempData["message"] = "Thank You For Your feedback";
                _context.tbl_feedback.Add(feedback);
                _context.SaveChanges();
                return RedirectToAction("feedback");
            }
        }

        public IActionResult fetchAllProducts()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;
            List<Product> products = _context.tbl_product.ToList();
            ViewData["product"] = products;

            return View();
        }
        public IActionResult ProductDetails(int id)
        {
            var product = _context.tbl_product
                .FirstOrDefault(p => p.product_id == id);

            return View(product); // ✅ single object
        }
        //public IActionResult ProductDetails(int id)
        //{
        //    var product = _context.tbl_product
        //        .FirstOrDefault(p => p.product_id == id);

        //    var sizes = _context.tbl_product_size
        //        .Where(s => s.product_id == id)
        //        .ToList();

        //    ViewBag.Sizes = sizes;

        //    return View(product);
        //}


        [HttpPost]
        public IActionResult AddToCart(int prod_id)
        {
            var customerIdStr = HttpContext.Session.GetString("customer_id");

            // ✅ SESSION CHECK
            if (string.IsNullOrEmpty(customerIdStr))
            {
                return RedirectToAction("CustomerLogin", "Customer");
            }

            // ✅ SAFE PARSE (avoid crash)
            if (!int.TryParse(customerIdStr, out int custId))
            {
                return RedirectToAction("CustomerLogin", "Customer");
            }

            // ✅ CHECK CUSTOMER EXISTS (VERY IMPORTANT for FK)
            var customerExists = _context.tbl_customer
                .Any(c => c.customer_id == custId);

            if (!customerExists)
            {
                return Content("Invalid customer session. Please login again.");
            }

            // ✅ CHECK PRODUCT EXISTS (good practice)
            var productExists = _context.tbl_product
                .Any(p => p.product_id == prod_id);

            if (!productExists)
            {
                return Content("Product not found");
            }

            // ✅ CHECK EXISTING CART ITEM
            var existing = _context.tbl_cart
                .FirstOrDefault(c => c.cust_id == custId
                                  && c.prod_id == prod_id
                                  && c.cart_status == 0);

            if (existing != null)
            {
                existing.product_quantity += 1;
            }
            else
            {
                _context.tbl_cart.Add(new Cart
                {
                    cust_id = custId,   // ✅ correct FK
                    prod_id = prod_id,
                    product_quantity = 1,
                    cart_status = 0
                });
            }

            _context.SaveChanges();

            return RedirectToAction("fetchCart");
        }
        public IActionResult IncreaseQty(int id)
        {
            var item = _context.tbl_cart.FirstOrDefault(c => c.cart_id == id);

            if (item != null)
            {
                item.product_quantity += 1;
                _context.tbl_cart.Update(item);
                _context.SaveChanges();
            }

            return RedirectToAction("fetchCart");
        }
        public IActionResult DecreaseQty(int id)
        {
            var item = _context.tbl_cart.FirstOrDefault(c => c.cart_id == id);

            if (item != null)
            {
                if (item.product_quantity > 1)
                {
                    item.product_quantity -= 1;
                    _context.tbl_cart.Update(item);
                }
                else
                {
                    // If quantity = 1 → remove item
                    _context.tbl_cart.Remove(item);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("fetchCart");
        }


        public IActionResult fetchCart()
        {
            List<Category> category = _context.tbl_category.ToList();
            ViewData["category"] = category;

            var customerIdStr = HttpContext.Session.GetString("customer_id");

            if (!string.IsNullOrEmpty(customerIdStr) && int.TryParse(customerIdStr, out int custId))
            {
                var cart = _context.tbl_cart
                    .Where(c => c.cust_id == custId && c.cart_status == 0)
                    .Include(c => c.products)
                    .ToList();

                return View(cart);
            }

            return RedirectToAction("CustomerLogin");
        }
        //public IActionResult removeProduct(int id)
        //{
        //    var product = _context.tbl_cart.Find(id);
        //    _context.tbl_cart.Remove(product);
        //     _context.SaveChanges();

        //    return RedirectToAction("fetchCart");
        //}
        public IActionResult removeProduct(int id)
        {
            var product = _context.tbl_cart.Find(id);

            if (product != null)
            {
                _context.tbl_cart.Remove(product);
                _context.SaveChanges();
            }

            return RedirectToAction("fetchCart");
        }
        public IActionResult ProductByCategory(int id)
        {
            var products = _context.tbl_product
                .Where(p => p.cat_id == id)
                .ToList();

            // Optional: send category name
            var category = _context.tbl_category.Find(id);
            ViewBag.CategoryName = category?.category_name;

            return View(products);
        }
        public int GetCartCount()
        {
            var customerIdStr = HttpContext.Session.GetString("customer_id");

            if (!string.IsNullOrEmpty(customerIdStr) && int.TryParse(customerIdStr, out int custId))
            {
                return _context.tbl_cart
                    .Where(c => c.cust_id == custId && c.cart_status == 0)
                    .Count();
            }

            return 0;
        }
        public IActionResult Search(string query)
        {
            ViewData["category"] = _context.tbl_category.ToList();

            if (string.IsNullOrEmpty(query))
            {
                ViewData["product"] = new List<Product>();
                ViewBag.Message = "Enter something to search";
                return View("SearchResults");
            }

            var products = _context.tbl_product
                .Where(p => p.product_name.Contains(query))
                .ToList();

            ViewData["product"] = products;
            ViewBag.Query = query;
            ViewBag.CartCount = GetCartCount();

            return View("SearchResults");
        }

    }
}
