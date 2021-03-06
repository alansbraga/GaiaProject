using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaiaCore.Gaia;
using GaiaCore.Util;
using GaiaDbContext.Models.AccountViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GaiaProject.Controllers
{
    public class AjaxController : Controller
    {
        //        public IActionResult Index()
        //        {
        //            return View();
        //        }

        /// <summary>
        /// 计算距离
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> CalShipDistanceNeed(string id,string pos, string factionName,int tempship=0,int TerraFormNumber=0)
        {
            GaiaGame gaiaGame = GameMgr.GetGameByName(id);
            Faction faction = gaiaGame.FactionList.Find(x => x.FactionName.ToString() == factionName);

            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();

            //Faction faction=null;
            if (!string.IsNullOrEmpty(pos))
            {

                pos = pos.ToLower();

                ConvertPosToRowCol(pos, out int row, out int col);
                //距离
                int distanceNeed = gaiaGame.Map.CalShipDistanceNeed(row, col, faction.FactionName);
                distanceNeed = distanceNeed - tempship;
                //需要的Q
                int QSHIP = Math.Max((distanceNeed - faction.GetShipDistance + 1) / 2, 0);

                //
                string message=null;
                //需要工人
                int Ore = Faction.m_MineOreCost;
                int Credit= Faction.m_MineCreditCost;
                jsonData.info.state = 200;

                if (gaiaGame.Map.HexArray[row, col] == null)
                {
                    message = "出界了兄弟";
                    jsonData.info.state = 0;

                }
                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Purple)
                {
                    Ore = 0;
                    Credit = 0;
                    //message = "不能在紫色星球上建造";
                }
//                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Empty)
//                {
//                    message = "你必须在星球上进行建造";
//                }
                //如果是盖亚星球不计算等级
                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Green)
                {

                }
                else
                {
                    //改造等级
                    int transNumNeed = Math.Min(7 - Math.Abs(gaiaGame.Map.HexArray[row, col].OGTerrain - faction.OGTerrain), Math.Abs(gaiaGame.Map.HexArray[row, col].OGTerrain - faction.OGTerrain));
                    //需要工人 faction.TerraFormNumber：临时铲子
                    Ore = Faction.m_MineOreCost + Math.Max((transNumNeed - TerraFormNumber), 0) * faction.GetTransformCost;
                    //int Credit = Faction.m_MineCreditCost;
                }

                jsonData.info.message = message;
                jsonData.data = new
                {
                    QSHIP= QSHIP,
                    Ore= Ore,
                    Credit= Credit,
                };
            }
            return new JsonResult(jsonData);
        }

        private void ConvertPosToRowCol(string position, out int row, out int col)
        {
            row = position.Substring(0, 1).ToCharArray().First() - 'a';
            col = position.Substring(1).ParseToInt(0);
        }

        /// <summary>
        /// 保存备忘信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> SaveRemark(string id,string remark,bool isTishi)
        {
            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();

            GaiaGame gaiaGame = GameMgr.GetGameByName(id);
            if (gaiaGame != null)
            {
                UserGameModel userGameModel = gaiaGame.UserGameModels.Find(x => x.username.ToString() == HttpContext.User.Identity.Name);
                //备忘
                userGameModel.remark = remark;
                userGameModel.isTishi = isTishi;
                jsonData.info.state = 200;

            }
            else
            {
                jsonData.info.state = 0;
                jsonData.info.message = "保存失败";

            }

            return new JsonResult(jsonData);
        }
        /// <summary>
        /// 提示信息设置
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> SetTishi(string type)
        {
            var list = GameMgr.GetAllGame(User.Identity.Name);
            foreach (KeyValuePair<string, GaiaGame> keyValuePair in list)
            {
                UserGameModel singleOrDefault = keyValuePair.Value.UserGameModels.Find(item => item.username == User.Identity.Name);
                if (singleOrDefault != null)
                {
                    if (type == "open")
                    {
                        singleOrDefault.isTishi = true;
                    }
                    else
                    {
                        singleOrDefault.isTishi = false;
                    }
                }

            }

            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();
            jsonData.info.state = 200;
            return new JsonResult(jsonData);
        }
    }
}