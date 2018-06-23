namespace Pedidos.Migrations.ApplicationDbContext
{
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Pedidos.DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Pedidos.DAL.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            MigrationsDirectory = @"Migrations\ApplicationDbContext";
        }

        protected override async void Seed(Pedidos.DAL.ApplicationDbContext context)
        {
            var UserStore = new UserStore<ApplicationUser>(context);
            var RoleStore = new RoleStore<IdentityRole>(context);
            var UserManager = new UserManager<ApplicationUser>(UserStore);
            var RoleManager = new RoleManager<IdentityRole>(RoleStore);
            var PasswordHash = new PasswordHasher();

            // Create Roles
            var ListOfRoles = new List<string>();
            ListOfRoles.Add("Webmaster");
            ListOfRoles.Add("Gerencia");
            ListOfRoles.Add("Jefe");
            ListOfRoles.Add("Cliente");

            IdentityResult roleResult;

            foreach (string Role in ListOfRoles)
            {
                // Check to see if Role Exists, if not create it
                if (!RoleManager.RoleExists(Role)) roleResult = RoleManager.Create(new IdentityRole(Role));
            }

            // Create Users
            ApplicationUser tempUser;
            var UsersToAdd = new Dictionary<ApplicationUser, string>();

            tempUser = new ApplicationUser { UserName = "javier.mesaslt@gmail.com", PasswordHash = PasswordHash.HashPassword("MAteMAte1981") };
            UsersToAdd.Add(tempUser, "Webmaster");
            tempUser = new ApplicationUser { UserName = "agustin.mesaslt@gmail.com", PasswordHash = PasswordHash.HashPassword("mesaslT135") };
            UsersToAdd.Add(tempUser, "Gerencia");
            tempUser = new ApplicationUser { UserName = "carmen.mesaslt@gmail.com", PasswordHash = PasswordHash.HashPassword("mesaslT135") };
            UsersToAdd.Add(tempUser, "Gerencia");
            tempUser = new ApplicationUser { UserName = "luis.mesaslt@gmail.com", PasswordHash = PasswordHash.HashPassword("mesaslT135") };
            UsersToAdd.Add(tempUser, "Gerencia");

            foreach (KeyValuePair<ApplicationUser, string> user in UsersToAdd)
            {
                if (!(context.Users.Any(u => u.UserName == user.Key.UserName))) UserManager.Create(user.Key);
            }

            await context.SaveChangesAsync();

            foreach (KeyValuePair<ApplicationUser, string> user in UsersToAdd)
            {
                try
                {
                    tempUser = UserManager.FindByName(user.Key.UserName);
                    UserManager.AddToRole(tempUser.Id, user.Value);
                }
                catch
                {
                    throw;
                }
            }

            base.Seed(context);
        }
    }
}
