﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GaiaDbContext.Models.AccountViewModels;
using GaiaDbContext.Models.HomeViewModels;
using GaiaDbContext.Models;

namespace GaiaProject.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// 是否保存结果
        /// </summary>
        public const bool isSaveResult = false;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }

        //public static ApplicationDbContext Create()
        //{
        //    return new ApplicationDbContext();
        //}

        public DbSet<UserFriend> UserFriend { get; set; }

        /// <summary>
        /// 游戏信息
        /// </summary>
        public DbSet<GameInfoModel> GameInfoModel { get; set; }

        /// <summary>
        /// 游戏玩家种族信息
        /// </summary>
        public DbSet<GameFactionModel> GameFactionModel { get; set; }

    }
}
