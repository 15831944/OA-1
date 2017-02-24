﻿using Loowoo.Land.OA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Loowoo.Land.OA.API.Controllers
{
    /// <summary>
    /// 流程模板
    /// </summary>
    public class FlowController : LoginControllerBase
    {
        /// <summary>
        /// 作用：获取表单流程模板
        /// 作者：汪建龙
        /// 编写时间：2017年2月22日16:14:47
        /// </summary>
        /// <param name="formId">表单ID</param>
        /// <returns></returns>
        [HttpGet]
        public IHttpActionResult GetFlow(int formId)
        {
            var model= Core.FlowManager.GetByFormId(formId);
            if (model == null)
            {
                return NotFound();
            }
            return Ok(model);
        }
    }
}
