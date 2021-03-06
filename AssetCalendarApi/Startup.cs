﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using AssetCalendarApi.Repository;
using AssetCalendarApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json.Serialization;
using AssetCalendarApi.Validators;
using AssetCalendarApi.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using AssetCalendarApi.ViewModels;
using AssetCalendarApi.Hubs;
using AssetCalendarApi.Tools;

namespace AssetCalendarApi
{
    public class Startup
    {
        #region Data Members

        private readonly IConfiguration _configuration;

        private readonly IHostingEnvironment _hostingEnvironment;

        #endregion

        #region Properties


        #endregion

        #region Constructor

        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _hostingEnvironment = hostingEnvironment;
        }

        #endregion

        #region Public Methods

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            services.AddIdentity<AceUser, IdentityRole>()
                .AddEntityFrameworkStores<AssetCalendarDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

                })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = _configuration["JwtIssuer"],
                        ValidAudience = _configuration["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtKey"])),
                        ClockSkew = TimeSpan.Zero // remove delay of token when expire
                    };
                });

            services.AddSignalR();

            services.AddMvc(options =>
            {
                if (_hostingEnvironment.IsProduction())
                    options.Filters.Add(new RequireHttpsAttribute());
            }).AddJsonOptions(jsonOptions =>
            {
                jsonOptions.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            });

            if (_hostingEnvironment.IsProduction())
            {
                services.AddDbContext<AssetCalendarDbContext>(options =>
                    options.UseSqlServer(_configuration.GetConnectionString("AceCalendarDb_Prod")));
            }
            else
            {
                services.AddDbContext<AssetCalendarDbContext>(options =>
                   options.UseSqlServer(_configuration.GetConnectionString("AceCalendarDb_Stg")));
            }


            services.AddScoped<WorkerRepository>();
            services.AddScoped<JobRepository>();
            services.AddScoped<OrganizationRepository>();
            services.AddScoped<TagRepository>();
            services.AddScoped<CalendarRepository>();
            services.AddScoped<SignalRService>();
            services.AddScoped<StripeRepository>();
            services.AddScoped<JwtTokenService>();
            services.AddScoped<SendGridService>();
            services.AddScoped<RazorService>();

            services.AddTransient<AssetCalendarSeeder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var origins = _configuration["CORS_Origin"].Split(',');
            
            app.UseCors(
                    options => options.WithOrigins(origins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials());

            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            

            app.UseAuthentication();
            app.UseSignalR(config =>
            {
               config.MapHub<CalendarHub>("/ws");
            });

            app.UseMvc();

            Mapper.Initialize(config =>
            {
                config.CreateMap<JobsByDate, Job>();
                config.CreateMap<JobsByDateWorker, JobsByDate>();
                config.CreateMap<JobsByDateWorker, Job>();
                config.CreateMap<AvailableWorkers, Worker>();
                config.CreateMap<TimeOffWorkers, Worker>();
                config.CreateMap<WorkersByJob, Worker>();
                config.CreateMap<WorkersByJobDate, Worker>();
                config.CreateMap<Tag, TagViewModel>();
                config.CreateMap<TagsByJob, TagViewModel>();
                config.CreateMap<TagsByJobDate, TagViewModel>();
                config.CreateMap<WorkerTags, TagViewModel>();
                config.CreateMap<AceUser, UserViewModel>().ReverseMap();
                config.CreateMap<Organization, OrganizationViewModel>().ReverseMap();
                config.CreateMap<Calendar, CalendarViewModel>();
            });
        }

        #endregion
    }
}
