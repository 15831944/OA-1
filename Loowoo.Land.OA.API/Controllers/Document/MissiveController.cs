﻿using Loowoo.Land.OA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Loowoo.Land.OA.API.Controllers
{
    public class MissiveController : LoginControllerBase
    {
        [HttpPost]
        public IHttpActionResult Save([FromBody] Missive missive)
        {
            TaskName = "保存公文";
            if (missive == null
                || string.IsNullOrEmpty(missive.Number)
                || string.IsNullOrEmpty(missive.Title))
            {
                return BadRequest($"{TaskName}:未获取公文相关信息、公文字号、公文标题不能为空");
            }
            var user = Core.UserManager.Get(missive.UserID);
            if (user == null)
            {
                return BadRequest($"{TaskName}:未找到承办人相关信息");
            }
            var born = Core.DepartmentManager.Get(missive.BornOrganID);
            if (born == null)
            {
                return BadRequest($"{TaskName}:未找到公文机关部门信息");
            }
            var to = Core.DepartmentManager.Get(missive.ToOrganID);
            if (to == null)
            {
                return BadRequest($"{TaskName}:未找到发往部门信息");
            }
            if (missive.ID > 0)
            {
                if (!Core.MissiveManager.Edit(missive))
                {
                    return BadRequest($"{TaskName}:更新公文拟稿失败，可能未找到系统拟稿");
                }
            }
            else
            {
                var id = Core.MissiveManager.Save(missive);
                if (id <= 0)
                {
                    return BadRequest($"{TaskName}:新建公文失败");
                }
            }
            return Ok(missive);
        }

        /// <summary>
        /// 作用：删除公文
        /// 作者：汪建龙
        /// 编写时间：2017年2月24日16:35:14
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete]
        public IHttpActionResult Delete(int id)
        {
            if (Core.MissiveManager.Delete(id))
            {
                return Ok();
            }
            return BadRequest("删除公文：删除失败，未找到需要删除的公文信息");
        }
        /// <summary>
        /// 作用：获取一个公文实体
        /// 作者：汪建龙
        /// 编写时间：2017年2月24日16:46:52
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult Model(int id)
        {
            var model = Core.MissiveManager.Get(id);
            if (model == null)
            {
                return NotFound();
            }
            model.Level = Core.ConfidentialLevelManager.Get(model.ConfidentialLevel);
            model.UnderTaker = Core.UserManager.Get(model.UserID);
            model.Category = Core.CategoryManager.Get(model.CategoryID);
            model.Emergency = Core.EmergencyManager.Get(model.EmergencyID);
            return Ok(model);
        }
    }
}
