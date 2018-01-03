using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaiaDbContext.Models;
using GaiaDbContext.Models.HomeViewModels;
using GaiaProject.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GaiaProject.Controllers
{
    public class MatchInfoController : Controller
    {
        private readonly ApplicationDbContext dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public MatchInfoController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            this.dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var list = this.dbContext.MatchInfoModel.ToList();
            return View(list);
        }
        /// <summary>
        /// ��ӱ���
        /// </summary>
        /// <returns></returns>
        [HttpGet]

        public IActionResult AddMatch()
        {
            return View();
        }

        [HttpPost]

        public IActionResult AddMatch(GaiaDbContext.Models.HomeViewModels.MatchInfoModel model)
        {
            if (ModelState.IsValid)
            {
                this.dbContext.MatchInfoModel.Add(model);
                this.dbContext.SaveChanges();
                return Redirect("Index");
            }
            return View(model);
        }
        /// <summary>
        /// ɾ������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> DelMatch(int id)
        {
            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();

            MatchInfoModel matchInfoModel = this.dbContext.MatchInfoModel.SingleOrDefault(item => item.Id == id);
            if (matchInfoModel != null)
            {
                this.dbContext.MatchInfoModel.Remove(matchInfoModel);
                this.dbContext.SaveChanges();
                jsonData.info.state = 200;
            }
            return new JsonResult(jsonData);

        }


        /// <summary>
        /// ��ϸ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ShowInfo(int id)
        {
            //��Ҫ��Ϣ
            var matchInfoModel = this.dbContext.MatchInfoModel.SingleOrDefault(item => item.Id == id);
            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();
            //jsonData.data = matchInfoModel;
            //��ѯ��ǰ������
            IQueryable<MatchJoinModel> matchJoinModels = this.dbContext.MatchJoinModel.Where(item => item.matchInfo_id == matchInfoModel.Id);

            jsonData.data = new
            {
                matchInfoModel = matchInfoModel,
                matchJoinModels = matchJoinModels,
            };
            jsonData.info.state = 200;
            return new JsonResult(jsonData);
        }
        /// <summary>
        /// �������
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> JoinMatch(int id)
        {
            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();
            var user = await _userManager.GetUserAsync(HttpContext.User);

            var matchInfoModel = this.dbContext.MatchInfoModel.SingleOrDefault(item => item.Id == id);
            if (matchInfoModel != null)
            {
                if (this.dbContext.MatchJoinModel.Any(
                    item => item.matchInfo_id == matchInfoModel.Id && item.username == user.UserName))
                {
                    jsonData.info.state = 0;
                    jsonData.info.message = "�Ѿ�����";

                }
                else
                {
                    //�Ƿ��Ѿ����㱨��
                    if (matchInfoModel.NumberMax !=0 &&matchInfoModel.NumberNow == matchInfoModel.NumberMax)
                    {
                        jsonData.info.state = 0;
                        jsonData.info.message = "������Ա����";
                    }
                    else if (matchInfoModel.RegistrationEndTime<DateTime.Now)
                    {
                        jsonData.info.state = 0;
                        jsonData.info.message = "����ʱ���ֹ";
                    }
                    else
                    {
                        //��������+1
                        matchInfoModel.NumberNow++;
                        this.dbContext.MatchInfoModel.Update(matchInfoModel);
                        //������Ϣ
                        MatchJoinModel matchJoinModel = new MatchJoinModel();
                        matchJoinModel.matchInfo_id = id;//id
                        matchJoinModel.Name = matchInfoModel.Name;//name

                        matchJoinModel.AddTime = DateTime.Now;//ʱ��
                        matchJoinModel.username = user.UserName;
                        matchJoinModel.userid = user.Id;

                        matchJoinModel.Rank = 0;
                        matchJoinModel.Score = 0;

                        this.dbContext.MatchJoinModel.Add(matchJoinModel);
                        this.dbContext.SaveChanges();

                        jsonData.info.state = 200;
                    }
                }
            }
            return new JsonResult(jsonData);
        }

        /// <summary>
        /// �˳�����
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<JsonResult> ExitMatch(int id)
        {
            Models.Data.UserFriendController.JsonData jsonData = new Models.Data.UserFriendController.JsonData();

            //������Ϣ
            MatchInfoModel matchInfoModel = this.dbContext.MatchInfoModel.SingleOrDefault(item => item.Id == id);
            if (matchInfoModel != null)
            {
                MatchJoinModel matchJoinModel =
                    this.dbContext.MatchJoinModel.SingleOrDefault(item => item.matchInfo_id == matchInfoModel.Id &&
                                                                          item.username == HttpContext.User.Identity
                                                                              .Name);
                if (matchJoinModel != null)
                {
                    //ɾ��
                    this.dbContext.MatchJoinModel.Remove(matchJoinModel);
                    //��������-1
                    matchInfoModel.NumberNow--;
                    this.dbContext.MatchInfoModel.Update(matchInfoModel);

                    this.dbContext.SaveChanges();

                    jsonData.info.state = 200;
                }
                else
                {
                    jsonData.info.state = 0;
                    jsonData.info.message = "û�б���";

                }
            }

            return new JsonResult(jsonData);
        }
    }
}