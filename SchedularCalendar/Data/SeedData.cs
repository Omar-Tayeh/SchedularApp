using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchedularCalendar.Authorisation;

namespace SchedularCalendar.Data
{
    public class SeedData
    {
        private readonly ApplicationDbContext _context;
        public SeedData(ApplicationDbContext context)
        {

            _context = context;
        }

        //initilize database content to have some data in the system when setup.
        public static async Task Initialize(
            IServiceProvider serviceProvider,
            string password = "Test@1234")
        {
            using (var _context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                //Staff Role and User
                var staffUid = await EnsureUser(serviceProvider, "teststaff@fab.com", password);
                await EnsureRole(serviceProvider, staffUid, Constants.StaffRole);
                //Manager Role and User
                var managerUid = await EnsureUser(serviceProvider, "manager@fab.com", password);
                await EnsureRole(serviceProvider, managerUid, Constants.ManagerRole);

                //Admin Role and User
                var adminUid = await EnsureUser(serviceProvider, "admin@fab.com", password);
                await EnsureRole(serviceProvider, adminUid, Constants.AdminRole);
            }
        }
        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                            string userName, string initPw)
        {
            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

            var user = await userManager.FindByNameAsync(userName);
            if (user == null)
            {
                user = new IdentityUser
                {
                    UserName = userName,
                    Email = userName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, initPw);
            }
            if (user == null)
                throw new Exception("User did not get created. password Error?");
            return user.Id;
        }
        //ensure that a role does not exisit to avoid creating it, and assign the user to a role.
        public static async Task<IdentityResult> EnsureRole(
            IServiceProvider serviceProvider,
            string uid, string role)
        {
            var roleManager = serviceProvider.GetService<RoleManager<IdentityRole>>();
            IdentityResult ir;

            if (await roleManager.RoleExistsAsync(role) == false)
            {
                ir = await roleManager.CreateAsync(new IdentityRole(role));
            }

            var userManager = serviceProvider.GetService<UserManager<IdentityUser>>();
            var user = await userManager.FindByIdAsync(uid);

            if (user == null)
                throw new Exception("USer does not exist");

            ir = await userManager.AddToRoleAsync(user, role);

            return ir;
        }
    }
}
