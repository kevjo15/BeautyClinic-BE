using Domain_Layer.Models;
using Infrastructure_Layer.Database;
using Infrastructure_Layer.Identity;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure_Layer.DataSeeder
{
    public class DataSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ElsaBeautyDbContext _context;

        public DataSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ElsaBeautyDbContext context)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _context = context;
        }

        // ImageUrl lagras som "<container>/<blobNamn>" — aldrig som färdig URL.
        // SAS-läslänkar genereras vid varje läsning (se ServiceImageUrlResolver).
        private static readonly Dictionary<Guid, string> ServiceImageBlobPaths = new()
        {
            [Guid.Parse("78BD2011-7143-4344-A909-03D533B1E99E")] = "images/services/c51afadc-460c-4901-8292-1f98c58cb355.png",
            [Guid.Parse("9C3A162A-C74C-42E1-A8F7-42E2A7379720")] = "images/services/91581ea5-8ce9-4f00-8dc1-20f88eac78ae.png",
            [Guid.Parse("8FFA1DB5-0965-4EC2-8C89-4459C9ACFDB1")] = "images/services/5565f6e4-51d3-4a78-9954-88d60bfaa585.png",
            [Guid.Parse("067FD02F-7BCE-4ECD-8C18-C0E8045FDD42")] = "images/services/2524f7e4-cceb-41cd-88bc-0a3d2c222d29.png",
            [Guid.Parse("BE9909FB-D2BF-4A90-B5A8-D8259489ED5F")] = "images/services/e6e0f306-3093-477b-9e0b-21be10b6c5b8.png",
            [Guid.Parse("E606735E-4248-4898-9FF5-DE847C9FA8CB")] = "images/services/c39355a4-2709-4484-b2fe-1857cd6543ab.png",
            [Guid.Parse("D08C2D95-9199-4C97-94DF-F38B144241EB")] = "images/services/4d405cc9-319a-43a4-a4c4-d4f52ff6a8f3.png",
        };

        public async Task SeedAsync()
        {
            var roles = new[] { "Admin", "Customer", "Employee" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(adminEmail) == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Elsa",
                    LastName = "Admin",
                    PhoneNumber = "0701234567",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(adminUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var employeeEmail = "employee@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(employeeEmail) == null)
            {
                var employeeUser = new ApplicationUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    FirstName = "Emma",
                    LastName = "Andersson",
                    PhoneNumber = "0709876543",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(employeeUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }

            var customerEmail = "customer@elsabeauty.se";
            if (await _userManager.FindByEmailAsync(customerEmail) == null)
            {
                var customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FirstName = "Karin",
                    LastName = "Karlsson",
                    PhoneNumber = "0705555555",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(customerUser, "Password123!");

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            if (!_context.EmployeeSchedules.Any())
            {
                var employee = await _userManager.FindByEmailAsync(employeeEmail);
                if (employee != null)
                {
                    var weekdays = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
                    var schedules = weekdays.Select(day => new EmployeeScheduleModel
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = employee.Id,
                        DayOfWeek = day,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(17, 0, 0)
                    }).ToList();

                    _context.EmployeeSchedules.AddRange(schedules);
                    await _context.SaveChangesAsync();
                }
            }

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
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("9C3A162A-C74C-42E1-A8F7-42E2A7379720"),
                        Name = "Läppfillers - 0.5 ml",
                        Description = "En lätt volymökning med 0.5 ml fillers för en naturlig look.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 1500.00m,
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("8FFA1DB5-0965-4EC2-8C89-4459C9ACFDB1"),
                        Name = "Botox Panna",
                        Description = "En behandling för att reducera linjer och rynkor i pannan.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 2000.00m,
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("067FD02F-7BCE-4ECD-8C18-C0E8045FDD42"),
                        Name = "Hyalase (Borttagning av Fillers)",
                        Description = "En behandling för att lösa upp oönskade fillers.",
                        Duration = TimeSpan.FromMinutes(20),
                        Price = 2000.00m,
                        CategoryId = categories.First(c => c.Name == "Fillers").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("BE9909FB-D2BF-4A90-B5A8-D8259489ED5F"),
                        Name = "Botox Käklinje",
                        Description = "Botox i käkmuskulaturen för att lindra tandgnissling.",
                        Duration = TimeSpan.FromMinutes(30),
                        Price = 2500.00m,
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("E606735E-4248-4898-9FF5-DE847C9FA8CB"),
                        Name = "Microneedling Ansikte",
                        Description = "Behandling som förbättrar hudens struktur genom små nålstick.",
                        Duration = TimeSpan.FromMinutes(60),
                        Price = 1800.00m,
                        CategoryId = categories.First(c => c.Name == "Microneedling").Id
                    },
                    new ServiceModel
                    {
                        Id = Guid.Parse("D08C2D95-9199-4C97-94DF-F38B144241EB"),
                        Name = "Botox Kråksparkar",
                        Description = "Behandling för att minska rynkor runt ögonen med botox.",
                        Duration = TimeSpan.FromMinutes(15),
                        Price = 1800.00m,
                        CategoryId = categories.First(c => c.Name == "Botox").Id
                    }
                };

                foreach (var service in services)
                {
                    if (ServiceImageBlobPaths.TryGetValue(service.Id, out var blobPath))
                    {
                        service.ImageUrl = blobPath;
                    }
                }

                _context.Services.AddRange(services);
                await _context.SaveChangesAsync();
            }

            await NormalizeLegacyImageUrlsAsync();
        }

        /// <summary>
        /// Äldre databaser kan ha fulla URL:er lagrade i ImageUrl (t.ex. Azurite-adresser
        /// från en tidigare seeder). Skriv om kända rader till blob-paths så att
        /// SAS-generering vid läsning fungerar. Idempotent — no-op när inget matchar.
        /// </summary>
        private async Task NormalizeLegacyImageUrlsAsync()
        {
            var legacyServices = _context.Services
                .Where(s => s.ImageUrl.StartsWith("http"))
                .ToList();

            var changed = false;
            foreach (var service in legacyServices)
            {
                if (ServiceImageBlobPaths.TryGetValue(service.Id, out var blobPath))
                {
                    service.ImageUrl = blobPath;
                    changed = true;
                }
            }

            if (changed)
            {
                await _context.SaveChangesAsync();
            }
        }
    }
}
