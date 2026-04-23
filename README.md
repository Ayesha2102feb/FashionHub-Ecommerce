# 🛍️ FashionHub - E-Commerce Web Application

## 📌 Overview

FashionHub is a basic e-commerce web application built using ASP.NET Core.
It allows users to browse products, add items to cart, and place orders.
This project demonstrates CRUD operations, session management, and database integration using Entity Framework Core.

---

## 🚀 Features

* 🛒 Product listing and details
* 🔍 Search functionality
* 🧺 Add to cart
* 📦 Order placement
* 👤 Customer login/logout (Session-based)
* 🧾 Order history
* 🛠️ Admin product management

---

## 🧑‍💻 Tech Stack

* ASP.NET Core MVC
* Entity Framework Core
* SQL Server
* Bootstrap
* C#

---

## ⚙️ Setup Instructions

### 1️⃣ Clone the repository

```bash id="c5j3lt"
git clone https://github.com/Ayesha2102feb/FashionHub-Ecommerce.git
cd FashionHub-Ecommerce
```

---

### 2️⃣ Open in Visual Studio

* Open `EcomWebsite.sln`

---

### 3️⃣ Configure Database

Open `appsettings.json` and update:

```json id="8np1px"
"ConnectionStrings": {
  "myconnection": "Server=.;Database=EcomWebsite;Trusted_Connection=True;"
}
```

---

### 4️⃣ Run Migrations

Open Package Manager Console and run:

```bash id="vqun6r"
Update-Database
```

---

### 5️⃣ Run the Project

* Press **F5** or click **Run**
* Application will open in browser

---

## 📂 Project Structure

Controllers/
Models/
Views/
Services/
Repositories/
wwwroot/

---

## 💡 Future Improvements

* JWT Authentication
* API architecture enhancement
* Product size & inventory management

---

## 🙋‍♀️ Author

Developed by **Isha Siddiqa**

---

## ⭐ Note

This project is created for learning and demonstration purposes.
