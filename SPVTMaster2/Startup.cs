﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SPVTMaster2.Data;
using SPVTMaster2.Models;
using SPVTMaster2.Services;
using SPVTMaster2.Models.CarsModels;
using Microsoft.AspNetCore.Http;


namespace SPVTMaster2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

           // var connection = @"Server=(localdb)\\mssqllocaldb;Database=Carsdb;Trusted_Connection=True;MultipleActiveResultSets=true";
            services.AddDbContext<AutomobileDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AutomobileDbContextConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMvc();

            // add security policies
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("IsAdmin", "PowerUser"));
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Cars}/{action=Index}/{id?}");
            });
            CreateRoles(serviceProvider).Wait();
        }

        private async Task CreateRoles(IServiceProvider serviceProvider)
        {
            //adding custom roles

            var RoleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var UserManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roleNames = { "Admin", "Manager", "Member" };

            IdentityResult roleResult;
            foreach (var roleName in roleNames)

            {

                //creating the roles and seeding them to the database

                var roleExist = await RoleManager.RoleExistsAsync(roleName);

                if (!roleExist)

                {

                    roleResult = await RoleManager.CreateAsync(new IdentityRole(roleName));

                }

            }

            //creating a super user who could maintain the web app

            var poweruser = new ApplicationUser

            {

                UserName = Configuration.GetSection("UserSettings")["UserEmail"],

                Email = Configuration.GetSection("UserSettings")["UserEmail"]

            };
            string UserPassword = Configuration.GetSection("UserSettings")["UserPassword"];

            var _user = await UserManager.FindByEmailAsync(Configuration.GetSection("UserSettings")["UserEmail"]);
            if (_user == null)

            {
                var createPowerUser = await UserManager.CreateAsync(poweruser, UserPassword);
                if (createPowerUser.Succeeded)
                {
                    await UserManager.AddToRoleAsync(poweruser, "Admin");
                }
            }

        }
    }
}
