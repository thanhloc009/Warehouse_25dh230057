using System;
using System.Linq;
using System.Data.Entity;
using System.Net;
using System.Web.Mvc;
using Warehouse_25dh230057.Models;

namespace Warehouse_25dh230057.Controllers
{
    public class ProductController : Controller
    {
        private readonly Warehouse_25dh230057Entities1 db =
            new Warehouse_25dh230057Entities1();

        // Câu 3: Tìm kiếm, lọc trạng thái và sắp xếp bằng LINQ/EF.
        public ActionResult Search(string q, string status, string sort)
        {
            var query = db.Products.AsQueryable();

            if (!String.IsNullOrWhiteSpace(q))
            {
                string keyword = q.Trim();
                // SQL Server mặc định không phân biệt hoa thường nên không cần ToLower().
                query = query.Where(p => p.ProductName.Contains(keyword));
            }

            if (status == "out")
                query = query.Where(p => p.Quantity == 0);
            else if (status == "low")
                query = query.Where(p => p.Quantity >= 1 && p.Quantity <= 4);
            else if (status == "enough")
                query = query.Where(p => p.Quantity >= 5);

            if (sort == "desc")
                query = query.OrderByDescending(p => p.Price);
            else
                query = query.OrderBy(p => p.Price);

            ProductSearchViewModel model = new ProductSearchViewModel
            {
                Products = query.ToList(),
                Keyword = q,
                StockStatus = status,
                SortOrder = String.IsNullOrEmpty(sort) ? "asc" : sort
            };

            return View(model);
        }

        // Câu 4: Mở trang kiểm kê một sản phẩm.
        public ActionResult UpdateQuantity(int? id)
        {
            if (id == null)
            {
                // Chưa chọn sản phẩm: hiện trang với danh sách để chọn.
                ViewBag.Products = new SelectList(
                    db.Products.OrderBy(p => p.ProductName).ToList(),
                    "ProductID", "ProductName");
                return View(new UpdateQuantityViewModel());
            }
            Product product = db.Products.Find(id);
            if (product == null) return HttpNotFound();

            return View(new UpdateQuantityViewModel
            {
                ProductID = product.ProductID,
                ProductName = product.ProductName,
                OldQuantity = product.Quantity
            });
        }

        // Câu 4: Chỉ lưu khi số lượng hợp lệ.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(UpdateQuantityViewModel model)
        {
            Product product = db.Products.Find(model.ProductID);
            if (product == null) return HttpNotFound();

            model.ProductName = product.ProductName;
            model.OldQuantity = product.Quantity;

            // FIX: báo lỗi rõ ràng thay vì chỉ load lại form.
            if (!model.ActualQuantity.HasValue)
                ModelState.AddModelError("ActualQuantity", "Vui lòng nhập số lượng thực tế.");
            else if (model.ActualQuantity.Value < 0)
                ModelState.AddModelError("ActualQuantity", "Số lượng thực tế phải là số nguyên không âm.");

            if (!ModelState.IsValid)
                return View(model);

            int difference = model.ActualQuantity.Value - product.Quantity;
            if (difference > 0)
                model.DifferenceMessage = "Chênh lệch tăng";
            else if (difference < 0)
                model.DifferenceMessage = "Chênh lệch giảm";
            else
                model.DifferenceMessage = "Không chênh lệch";

            model.Difference = Math.Abs(difference);
            product.Quantity = model.ActualQuantity.Value;
            db.SaveChanges();

            TempData["Message"] = model.DifferenceMessage +
                " (" + model.Difference + "). Số lượng cũ: " + model.OldQuantity +
                ", số lượng mới: " + product.Quantity + ".";
            return RedirectToAction("Index", "Products");
        }

        // Câu 5: Form kiểm kê đồng thời hai sản phẩm.
        public ActionResult UpdateTwoQuantities()
        {
            return View(new UpdateTwoQuantitiesViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateTwoQuantities(UpdateTwoQuantitiesViewModel model)
        {
            // FIX: báo lỗi theo từng trường thay vì một thông báo chung.
            if (!model.ProductID1.HasValue)
                ModelState.AddModelError("ProductID1", "Vui lòng nhập ProductID thứ nhất.");
            if (!model.ProductID2.HasValue)
                ModelState.AddModelError("ProductID2", "Vui lòng nhập ProductID thứ hai.");

            if (!model.ActualQuantity1.HasValue)
                ModelState.AddModelError("ActualQuantity1", "Vui lòng nhập số lượng thực tế thứ nhất.");
            else if (model.ActualQuantity1.Value < 0)
                ModelState.AddModelError("ActualQuantity1", "Số lượng phải là số nguyên không âm.");

            if (!model.ActualQuantity2.HasValue)
                ModelState.AddModelError("ActualQuantity2", "Vui lòng nhập số lượng thực tế thứ hai.");
            else if (model.ActualQuantity2.Value < 0)
                ModelState.AddModelError("ActualQuantity2", "Số lượng phải là số nguyên không âm.");

            if (model.ProductID1.HasValue && model.ProductID2.HasValue &&
                model.ProductID1.Value == model.ProductID2.Value)
                ModelState.AddModelError("", "Phải chọn hai ProductID khác nhau.");

            if (!ModelState.IsValid)
                return View(model);

            Product product1 = db.Products.Find(model.ProductID1.Value);
            Product product2 = db.Products.Find(model.ProductID2.Value);

            if (product1 == null)
                ModelState.AddModelError("ProductID1", "Không tìm thấy sản phẩm có ID này.");
            if (product2 == null)
                ModelState.AddModelError("ProductID2", "Không tìm thấy sản phẩm có ID này.");
            if (!ModelState.IsValid)
                return View(model);

            // Chỉ bắt đầu transaction sau khi toàn bộ dữ liệu đã hợp lệ.
            using (DbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    product1.Quantity = model.ActualQuantity1.Value;
                    product2.Quantity = model.ActualQuantity2.Value;
                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Cập nhật thất bại. Cả hai sản phẩm đã được rollback.");
                    return View(model);
                }
            }

            TempData["Message"] = "Đã cập nhật đồng thời hai sản phẩm thành công.";
            return RedirectToAction("Index", "Products");
        }

        // Câu 7: Tính trực tiếp từ database bằng LINQ/EF.
        public ActionResult Statistics()
        {
            InventoryStatisticsViewModel model = new InventoryStatisticsViewModel
            {
                ProductCount = db.Products.Count(),
                OutOfStockCount = db.Products.Count(p => p.Quantity == 0),
                TotalInventoryValue = db.Products
                    .Select(p => (decimal?)(p.Price * p.Quantity))
                    .Sum() ?? 0m
            };

            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}