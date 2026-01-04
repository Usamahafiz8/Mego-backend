// using MeGo.Api.Models;
// using Microsoft.EntityFrameworkCore;

// namespace MeGo.Api.Data
// {
//     public static class DbSeeder
//     {
//         public static void Seed(AppDbContext context)
//         {
//             // ✅ Apply pending migrations automatically
//             context.Database.Migrate();

//             // ===========================
//             // Seed Users
//             // ===========================
//             if (!context.Users.Any())
//             {
//                 context.Users.AddRange(
//                     new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
//                     new User { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@example.com" }
//                 );
//                 context.SaveChanges();
//             }

//             var alice = context.Users.First(u => u.Name == "Alice");
//             var bob = context.Users.First(u => u.Name == "Bob");

//             // ===========================
//             // Seed Categories
//             // ===========================
//             if (!context.Categories.Any())
//             {
//                 context.Categories.AddRange(
//                     new Category { Name = "Cars", Slug = "cars" },
//                     new Category { Name = "Electronics", Slug = "electronics" },
//                     new Category { Name = "Furniture", Slug = "furniture" },
//                     new Category { Name = "Mobile Phones", Slug = "mobile-phones" },
//                     new Category { Name = "Real Estate", Slug = "real-estate" }
//                 );
//                 context.SaveChanges();
//             }

//             var carsCategory = context.Categories.First(c => c.Slug == "cars");
//             var electronicsCategory = context.Categories.First(c => c.Slug == "electronics");

//             // ===========================
//             // Seed Ads
//             // ===========================
//             if (!context.Ads.Any())
//             {
//                 context.Ads.AddRange(
//                     new Ad
//                     {
//                         Id = Guid.NewGuid(),
//                         Title = "Honda Civic 2020",
//                         Description = "Well maintained car, single owner.",
//                         Price = 2500000,
//                         City = "Karachi",
//                         CategoryId = carsCategory.Id,
//                         UserId = alice.Id,
//                         CreatedAt = DateTime.UtcNow,
//                         ExtraData = "{}"
//                     },
//                     new Ad
//                     {
//                         Id = Guid.NewGuid(),
//                         Title = "iPhone 14 Pro",
//                         Description = "Brand new iPhone 14 Pro, 256GB",
//                         Price = 280000,
//                         City = "Lahore",
//                         CategoryId = electronicsCategory.Id,
//                         UserId = bob.Id,
//                         CreatedAt = DateTime.UtcNow,
//                         ExtraData = "{}"
//                     }
//                 );
//                 context.SaveChanges();
//             }

//             // ===========================
//             // Seed AdMedia
//             // ===========================
//             if (!context.AdMedia.Any())
//             {
//                 var hondaAd = context.Ads.First(a => a.Title.Contains("Honda Civic"));
//                 var iphoneAd = context.Ads.First(a => a.Title.Contains("iPhone 14 Pro"));

//                 context.AdMedia.AddRange(
//                     new AdMedia { Id = Guid.NewGuid(), AdId = hondaAd.Id, MediaUrl = "honda1.jpg", MediaType = "image" },
//                     new AdMedia { Id = Guid.NewGuid(), AdId = hondaAd.Id, MediaUrl = "honda2.jpg", MediaType = "image" },
//                     new AdMedia { Id = Guid.NewGuid(), AdId = iphoneAd.Id, MediaUrl = "iphone1.jpg", MediaType = "image" }
//                 );
//                 context.SaveChanges();
//             }

//             // ===========================
//             // Seed Favorites
//             // ===========================
//             if (!context.Favorites.Any())
//             {
//                 var hondaAd = context.Ads.First(a => a.Title.Contains("Honda Civic"));
//                 var iphoneAd = context.Ads.First(a => a.Title.Contains("iPhone 14 Pro"));

//                 context.Favorites.AddRange(
//                     new Favorite { UserId = alice.Id, AdId = iphoneAd.Id, Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow },
//                     new Favorite { UserId = bob.Id, AdId = hondaAd.Id, Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow }
//                 );
//                 context.SaveChanges();
//             }
//         }
//     }
// }
