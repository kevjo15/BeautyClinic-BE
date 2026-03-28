using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.DataSeeder
{
    public class DataSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserModel> _userManager;
        private readonly ElsaBeautyDbContext _context;

        public DataSeeder(RoleManager<IdentityRole> roleManager, UserManager<UserModel> userManager, ElsaBeautyDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Seed roles
            var roles = new[] { "Admin", "Customer", "Employee" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed admin user
            var adminEmail = "admin@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new UserModel
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Elsa",
                    LastName = "Admin",
                    PhoneNumber = "0701234567"
                };
                var result = await _userManager.CreateAsync(adminUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed employee user
            var employeeEmail = "employee@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(employeeEmail) == null)
            {
                var employeeUser = new UserModel
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FirstName = "Emma",
                    LastName = "Andersson",
                    PhoneNumber = "0709876543"
                };
                var result = await _userManager.CreateAsync(employeeUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }

            // Seed customer user
            var customerEmail = "customer@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(customerEmail) == null)
            {
                var customerUser = new UserModel
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Karin",
                    LastName = "Karlsson",
                    PhoneNumber = "0705555555"
                };
                var result = await _userManager.CreateAsync(customerUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            // Seed categories and services
            if (!_context.Categories.Any())
            {
                var categories = new List<CategoryModel>
                {
                    new CategoryModel { Id = Guid.NewGuid(), Name = "Fillers" },
                    new CategoryModel { Id = Guid.NewGuid(), Name = "Botox" },
                    new CategoryModel { Id = Guid.NewGuid(), Name = "Microneedling" },
                    new CategoryModel { Id = Guid.NewGuid(), Name = "Laser Treatments" },
                };

                _context.Categories.AddRange(categories);
                await _context.SaveChangesAsync();

                var services = new List<ServiceModel>
                {
                    new ServiceModel
                    {
                        Id = Guid.Parse("78BD2011-7143-4344-A909-03D533B1E99E"),
                        Name = "Läppfillers 1 ml",
                        Description = "En behandling för att ge volym och form till läpparna med 1 ml fillers.",
                        Duration = TimeSpan.FromMinutes(30),
                        Price = 2500.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/services/c51afadc-460c-4901-8292-1f98c58cb355.png",
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("9C3A162A-C74C-42E1-A8F7-42E2A7379720"),
                        Name = "Läppfillers - 0.5 ml",
                        Description = "En lätt volymökning med 0.5 ml fillers för en naturlig look.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 1500.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/services/91581ea5-8ce9-4f00-8dc1-20f88eac78ae.png",
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("8FFA1DB5-0965-4EC2-8C89-4459C9ACFDB1"),
                        Name = "Botox Panna",
                        Description = "En behandling för att reducera linjer och rynkor i pannan.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 2000.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/services/5565f6e4-51d3-4a78-9954-88d60bfaa585.png",
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("067FD02F-7BCE-4ECD-8C18-C0E8045FDD42"),
                        Name = "Hyalase (Borttagning av Fillers)",
                        Description = "En behandling för att lösa upp oönskade fillers.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 2000.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/service-images/hyalase.jpg",
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("BE9909FB-D2BF-4A90-B5A8-D8259489ED5F"),
                        Name = "Botox Käklinje",
                        Description = "Botox i käkmuskulaturen för att lindra tandgnissling.",
                        Duration = TimeSpan.FromMinutes(30),
                        Price = 2500.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/service-images/botox-kaklinje.jpg",
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("E606735E-4248-4898-9FF5-DE847C9FA8CB"),
                        Name = "Microneedling Ansikte",
                        Description = "Behandling som förbättrar hudens struktur genom små nålstick.",
                        Duration = TimeSpan.FromMinutes(60),
                        Price = 1800.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/service-images/microneedling.jpg",
                        CategoryId = categories.First(c => c.Name == "Microneedling").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("D08C2D95-9199-4C97-94DF-F38B144241EB"),
                        Name = "Botox Kråksparkar",
                        Description = "Behandling för att minska rynkor runt ögonen med botox.",
                        Duration = TimeSpan.FromMinutes(15),
                        Price = 1800.00m,
                        ImageUrl = "http://127.0.0.1:10000/devstoreaccount1/service-images/botox-kraksparkar.jpg",
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    }
                };

                _context.Services.AddRange(services);
                await _context.SaveChangesAsync();
            }
        }
    }
}
