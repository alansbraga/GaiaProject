using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaiaCore.Gaia;
using GaiaCore.Util;
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
        /// �������
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
                //����
                int distanceNeed = gaiaGame.Map.CalShipDistanceNeed(row, col, faction.FactionName);
                distanceNeed = distanceNeed - tempship;
                //��Ҫ��Q
                int QSHIP = Math.Max((distanceNeed - faction.GetShipDistance + 1) / 2, 0);

                //
                string message=null;
                //��Ҫ����
                int Ore = Faction.m_MineOreCost;
                int Credit= Faction.m_MineCreditCost;
                jsonData.info.state = 200;

                if (gaiaGame.Map.HexArray[row, col] == null)
                {
                    message = "�������ֵ�";
                    jsonData.info.state = 0;

                }
                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Purple)
                {
                    Ore = 0;
                    Credit = 0;
                    //message = "��������ɫ�����Ͻ���";
                }
//                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Empty)
//                {
//                    message = "������������Ͻ��н���";
//                }
                //����Ǹ������򲻼���ȼ�
                else if (gaiaGame.Map.HexArray[row, col].TFTerrain == Terrain.Green)
                {

                }
                else
                {
                    //����ȼ�
                    int transNumNeed = Math.Min(7 - Math.Abs(gaiaGame.Map.HexArray[row, col].OGTerrain - faction.OGTerrain), Math.Abs(gaiaGame.Map.HexArray[row, col].OGTerrain - faction.OGTerrain));
                    //��Ҫ���� faction.TerraFormNumber����ʱ����
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
    }
}