using System;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Web.Mvc;
using Warehouse_25dh230057.Models;

namespace Warehouse_25dh230057.Controllers
{
    public class ProductsController : Controller
    {
        private readonly Warehouse_25dh230057Entities1 db =
            new Warehouse_25dh230057Entities1();

        // Câu 2a: Danh sách sản phẩm
        public ActionResult Index()
        {
            return View(db.Products.OrderBy(p => p.ProductID).ToList());
        }

        // Câu 2b: Mở trang sửa
        public ActionResult Edit(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(product);
        }

        // Câu 2b: Chỉ cập nhật ProductName, Category, Price.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, string ProductName, string Category, decimal? Price)
        {
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            ProductName = (ProductName ?? "").Trim();
            Category = (Category ?? "").Trim();

            if (ProductName.Length == 0)
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");
            if (Category.Length == 0)
                ModelState.AddModelError("Category", "Danh mục không được để trống.");
            if (!Price.HasValue || Price.Value <= 0)
                ModelState.AddModelError("Price", "Đơn giá phải lớn hơn 0.");

            if (!ModelState.IsValid)
            {
                product.ProductName = ProductName;
                product.Category = Category;
                if (Price.HasValue) product.Price = Price.Value;
                return View(product);
            }

            product.ProductName = ProductName;
            product.Category = Category;
            product.Price = Price.Value;
            // Không gán ProductID hoặc Quantity.
            db.SaveChanges();

            TempData["Message"] = "Cập nhật sản phẩm thành công.";
            return RedirectToAction("Index");
        }

        // Câu 6: Form thêm sản phẩm
        public ActionResult Create()
        {
            return View();
        }

        // Câu 6: Kiểm tra dữ liệu và tên trùng không phân biệt hoa/thường.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string ProductName, string Category, decimal? Price, int? Quantity)
        {
            ProductName = (ProductName ?? "").Trim();
            Category = (Category ?? "").Trim();

            if (ProductName.Length == 0)
                ModelState.AddModelError("ProductName", "Tên sản phẩm không được để trống.");
            if (Category.Length == 0)
                ModelState.AddModelError("Category", "Danh mục không được để trống.");
            if (!Price.HasValue || Price.Value <= 0)
                ModelState.AddModelError("Price", "Giá phải lớn hơn 0.");
            if (!Quantity.HasValue || Quantity.Value < 0)
                ModelState.AddModelError("Quantity", "Số lượng phải là số nguyên không âm.");

            if (ProductName.Length > 0)
            {
                string normalizedName = ProductName.ToLower();
                bool duplicate = db.Products.Any(p => p.ProductName.ToLower() == normalizedName);
                if (duplicate)
                    ModelState.AddModelError("ProductName", "Tên sản phẩm đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View();

            Product product = new Product
            {
                ProductName = ProductName,
                Category = Category,
                Price = Price.Value,
                Quantity = Quantity.Value
            };

            db.Products.Add(product);
            db.SaveChanges();

            TempData["Message"] = "Thêm sản phẩm thành công.";
            return RedirectToAction("Index");
        }

        public ActionResult Details(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        public ActionResult Delete(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
