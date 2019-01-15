using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gaia.Model.Final;
using GaiaDbContext.Models.SystemModels;
using GaiaProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace GaiaProject.Controllers
{
    public class NewsController   : BaseControlNews
    {
        private readonly ApplicationDbContext dbContext;

        public NewsController(ApplicationDbContext dbContext):base(dbContext)
        {
            this.dbContext = dbContext;
        }
        /// <summary>
        /// ������ϸ
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult ShowInfo(int id)
        {
            //������Ϣ
            NewsInfoModel singleOrDefault = this.dbContext.NewsInfoModel.SingleOrDefault(item => item.Id == id);
            return View(singleOrDefault);
            
        }
        /// <summary>
        /// �����б�
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult Index()
        {
            IQueryable<NewsInfoModel> newsInfoModels = this.dbContext.NewsInfoModel.Where(item => item.type == NewsConfig.TYPE_GL).OrderBy(item=>item.Rank);
            return View(newsInfoModels);
        }

        [HttpGet]
        public IActionResult Modify(int? id)
        {
            NewsInfoModel newModel = base.News_Update(id);
            return View(newModel);
        }
        /// <summary>
        /// �ύ����
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Modify(NewsInfoModel model)
        {
            //����
            model.type = NewsConfig.TYPE_GL;
            //״̬
            model.state = 1;
            //�û�
            model.username = this.User.Identity.Name;

            NewsInfoModel newModel = base.News_Update(model);
            return Redirect("/News/Index");
            return View(newModel);
        }


    }
}