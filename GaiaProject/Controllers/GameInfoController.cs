﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaiaCore.Gaia;
using GaiaDbContext.Models;
using GaiaDbContext.Models.HomeViewModels;
using GaiaProject.Data;
using GaiaProject.Models.HomeViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GaiaProject.Controllers
{
    public class GameInfoController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> _userManager;


        public GameInfoController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            this.dbContext = dbContext;
            this._userManager = userManager;

        }
        // GET: /<controller>/
        /// <summary>
        /// 进行的游戏
        /// </summary>
        /// <returns></returns>
        public IActionResult Index(string username)
        {
            if (username == null)
            {
                username = HttpContext.User.Identity.Name;
            }
            //this.dbContext.GameInfoModel.AsEnumerable()
            //var myfaction = from score in this.dbContext.GameFactionModel.AsEnumerable() where score.username == HttpContext.User.Identity.Name select score.gameinfo_id;
            var list = from game in this.dbContext.GameInfoModel
                from score in this.dbContext.GameFactionModel
                where game.GameStatus==8 && score.username == username && game.Id == score.gameinfo_id
                select game;
            var result = list.ToList();
            return View(result);
        }
        /// <summary>
        /// 显示详细
        /// </summary>
        /// <returns></returns>
        public IActionResult Show()
        {
            return View();
        }
        /// <summary>
        /// 使用的种族信息
        /// </summary>
        /// <returns></returns>
        public IActionResult FactionList(string username)
        {
            if (username == null)
            {
                username = HttpContext.User.Identity.Name;
            }
            var gameFactionModels = this.dbContext.GameFactionModel.Where(item => item.username == username).ToList();
            return View(gameFactionModels);
        }
        /// <summary>
        /// 种族统计
        /// </summary>
        /// <returns></returns>

        public IActionResult FactionStatistics()
        {
            var list = this.dbContext.GameFactionModel.GroupBy(item => item.FactionChineseName).Select(
                g=>new StatisticsFaction ()
                {
                    ChineseName = g.Key,
                    count = g.Count(),
                    numberwin = g.Count(faction => faction.rank == 1),
                    winprobability = g.Count(faction => faction.rank == 1)* 100 / (g.Count()),
                    scoremin = g.Min(faction=>faction.scoreTotal),
                    scoremax = g.Max(faction => faction.scoreTotal),
                    scoremaxuser = g.OrderBy(faction => faction.scoreTotal).ToList()[0].username,
                    scoreavg = g.Sum(faction => faction.scoreTotal)/g.Count(),

                }).ToList();
            return View(list);
        }

        public class StatisticsFaction
        {
            /// <summary>
            /// 名称
            /// </summary>
            public string ChineseName { get; set; }
            /// <summary>
            /// 局数
            /// </summary>
            public int count { get; set; }
            /// <summary>
            /// 最低分
            /// </summary>
            public int scoremin { get; set; }

            /// <summary>
            /// 最高分
            /// </summary>
            public int scoremax { get; set; }

            public string scoremaxuser { get; set; }
            /// <summary>
            /// 平均分
            /// </summary>
            public int scoreavg { get; set; }
            /// <summary>
            /// 胜率
            /// </summary>
            public int winprobability { get; set; }
            /// <summary>
            /// 胜利场次
            /// </summary>
            public int numberwin { get; set; }

        }

    }
}