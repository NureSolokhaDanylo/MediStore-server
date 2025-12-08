using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using SharedConfiguration.Options;

namespace Application.Seeders
{
    public class IdentitySeeder : ISeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSeedOptions _options;

        public IdentitySeeder(
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            IOptions<AppSeedOptions> options)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _options = options.Value;
        }

        public async Task SeedAsync()
        {
            if (string.IsNullOrWhiteSpace(_options.AdminPassword) || string.IsNullOrWhiteSpace(_options.AdminLogin))
                throw new ArgumentNullException("Seed data has null references");

            string[] roles = { "Admin", "Sensor", "Operator", "Observer"};
            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            var owner = await _userManager.FindByNameAsync(_options.AdminLogin);
            if (owner == null)
            {
                var admin = new IdentityUser
                {
                    UserName = _options.AdminLogin,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(admin, _options.AdminPassword);
                if (result.Succeeded)
                    await _userManager.AddToRolesAsync(admin, roles);
            }
        }
    }
}
