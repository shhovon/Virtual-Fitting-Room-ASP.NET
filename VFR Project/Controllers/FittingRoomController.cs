using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VFR_Project.Models;

namespace VFR_Project.Controllers
{
    public class FittingRoomController : Controller
    {
        private VFREntities db = new VFREntities();

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase photo)
        {
            if (photo != null && photo.ContentLength > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    photo.InputStream.CopyTo(memoryStream);
                    var userPhoto = new UserPhoto
                    {
                        FileName = photo.FileName,
                        ImageData = memoryStream.ToArray(),
                        UploadDate = DateTime.Now
                    };

                    db.UserPhotos.Add(userPhoto);
                    db.SaveChanges();

                    return RedirectToAction("TryOn", new { photoId = userPhoto.Id });
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult TryOn(int photoId)
        {
            var userPhoto = db.UserPhotos.Find(photoId);
            var garments = db.Garments.ToList(); 

            var viewModel = new TryOnViewModel
            {
                UserPhoto = userPhoto,
                Garments = garments
            };

            return View(viewModel);
        }


        [HttpPost]
        public ActionResult ApplyGarment(int photoId, int garmentId, string selectionPath)
        {
            try
            {
                var userPhoto = db.UserPhotos.Find(photoId);
                var garment = db.Garments.Find(garmentId);

                if (userPhoto != null && garment != null)
                {
                    var processedImage = ProcessImage(userPhoto.ImageData, garment.ImageData, selectionPath);

                    string base64Image = Convert.ToBase64String(processedImage);

                    return Json(new { success = true, image = base64Image });
                }
                else
                {
                    return Json(new { success = false, error = "UserPhoto or Garment not found" });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ApplyGarment: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }

        private byte[] ProcessImage(byte[] userPhotoData, byte[] garmentData, string selectionPath)
        {
            try
            {
                using (var userPhotoStream = new MemoryStream(userPhotoData))
                using (var garmentStream = new MemoryStream(garmentData))
                using (var userPhoto = Image.FromStream(userPhotoStream))
                using (var garment = Image.FromStream(garmentStream))
                using (var graphics = Graphics.FromImage(userPhoto))
                {
                    // Create a GraphicsPath from the selectionPath string
                    var path = new System.Drawing.Drawing2D.GraphicsPath();
                    var points = selectionPath.Split(',')
                        .Select((v, i) => new { Value = int.Parse(v), Index = i })
                        .GroupBy(x => x.Index / 2)
                        .Select(g => new Point(g.First().Value, g.Last().Value))
                        .ToArray();
                    path.AddPolygon(points);

                    // Create a region from the path
                    var region = new Region(path);

                    // Set the clipping region
                    graphics.SetClip(region, System.Drawing.Drawing2D.CombineMode.Replace);

                    // Draw the garment
                    graphics.DrawImage(garment, 0, 0, userPhoto.Width, userPhoto.Height);

                    using (var resultStream = new MemoryStream())
                    {
                        userPhoto.Save(resultStream, ImageFormat.Png);
                        return resultStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessImage: {ex.Message}");
                throw;
            }
        }


        public ActionResult GetPhoto(int id)
        {
            var photo = db.UserPhotos.Find(id);
            if (photo == null)
                return HttpNotFound();

            return File(photo.ImageData, "image/jpeg");
        }

        private byte[] ProcessImage(byte[] userPhotoData, byte[] garmentData, int x, int y, int width, int height)
        {
            try
            {
                using (var userPhotoStream = new MemoryStream(userPhotoData))
                using (var garmentStream = new MemoryStream(garmentData))
                using (var userPhoto = Image.FromStream(userPhotoStream))
                using (var garment = Image.FromStream(garmentStream))
                using (var graphics = Graphics.FromImage(userPhoto))
                {
                    var garmentResized = new Bitmap(garment, new Size(width, height));

                    graphics.DrawImage(garmentResized, x, y, width, height);
                    using (var resultStream = new MemoryStream())
                    {
                        userPhoto.Save(resultStream, System.Drawing.Imaging.ImageFormat.Png);
                        return resultStream.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in ProcessImage: {ex.Message}");
                throw;
            }
        }

        [HttpPost]
        public ActionResult SaveProcessedImage(string imageData)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(imageData.Split(',')[1]);
                var userPhoto = new UserPhoto
                {
                    FileName = "ProcessedImage.png",
                    ImageData = imageBytes,
                    UploadDate = DateTime.Now
                };

                db.UserPhotos.Add(userPhoto);
                db.SaveChanges();

                return Json(new { success = true, photoId = userPhoto.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in SaveProcessedImage: {ex.Message}");
                return Json(new { success = false, error = ex.Message });
            }
        }


        public ActionResult UploadGarment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult UploadGarment(HttpPostedFileBase garmentFile, string garmentName, string category, string size, string color)
        {
            if (garmentFile != null && garmentFile.ContentLength > 0)
            {
                using (var memoryStream = new MemoryStream())
                {
                    garmentFile.InputStream.CopyTo(memoryStream);
                    var garment = new Garment
                    {
                        Name = garmentName,
                        FileName = garmentFile.FileName,
                        ImageData = memoryStream.ToArray(),
                        Category = category,
                        Size = size,
                        Color = color
                    };

                    db.Garments.Add(garment);
                    db.SaveChanges();

                    return RedirectToAction("TryOn");
                }
            }

            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class TryOnViewModel
    {
        public UserPhoto UserPhoto { get; set; }
        public List<Garment> Garments { get; set; }
        public string ProcessedImage { get; set; }
    }
}