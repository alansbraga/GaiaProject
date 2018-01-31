using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GaiaDbContext.Models.SystemModels;
using GaiaProject.Data;
using Microsoft.AspNetCore.Mvc;

namespace GaiaProject.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext dbContext;

        public AdminController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        #region ����

        /// <summary>
        /// �����б�
        /// </summary>
        /// <returns></returns>

        public IActionResult NewsIndex()
        {
            List<NewsInfoModel> newsInfoModels = this.dbContext.NewsInfoModel.ToList();
            return View(newsInfoModels);
        }
        /// <summary>
        /// ��������
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public IActionResult NewsUpdate(int? id)
        {
            NewsInfoModel newModel = null;
            if (id > 0)
            {
                newModel = this.dbContext.NewsInfoModel.SingleOrDefault(item => item.Id == id);
            }
            return View(newModel);
        }
        [HttpPost]
        public IActionResult NewsUpdate(NewsInfoModel model)
        {
            NewsInfoModel newModel;
            //�༭
            if (model.Id > 0)
            {
                newModel = this.dbContext.NewsInfoModel.SingleOrDefault(item => item.Id == model.Id);
            }
            else//���
            {
                newModel = new NewsInfoModel();
                newModel.AddTime = DateTime.Now;
            }
            //��ֵ
            newModel.name = model.name;
            newModel.contents = model.contents;
            newModel.type = model.type;
            newModel.state = model.state;
            //newModel.name = model.name;


            //����
            if (model.Id > 0)
            {
                this.dbContext.NewsInfoModel.Update(newModel);
            }
            else
            {
                this.dbContext.NewsInfoModel.Add(newModel);
            }
            this.dbContext.SaveChanges();
            return Redirect("/Admin/NewsIndex");
            return View(newModel);
        }

        #endregion


    }
}