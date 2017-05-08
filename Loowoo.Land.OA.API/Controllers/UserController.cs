﻿using Loowoo.Land.OA.API.Models;
using Loowoo.Land.OA.API.Security;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Security;

namespace Loowoo.Land.OA.API.Controllers
{
    public class UserController : ControllerBase
    {
        [HttpGet]
        [AllowAnonymous]
        public object Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return BadRequest("登录名以及密码不能为空");
            }
            var user = Core.UserManager.Login(username, password);
            if (user == null)
            {
                return BadRequest("请核对用户名以及密码");
            }

            user.Token = AuthorizeHelper.GetToken(new UserIdentity
            {
                ID = user.ID,
                Username = user.Username,
                Role = user.Role,
                RealName = user.RealName
            });

            return user;
        }

        [HttpGet]
        public object List(int departmentId = 0, int groupId = 0, string searchKey = null, int page = 1, int rows = 20)
        {
            var parameter = new UserParameter
            {
                DepartmentId = departmentId,
                GroupId = groupId,
                SearchKey = searchKey,
                Page = new Loowoo.Common.PageParameter(page, rows)
            };
            var list = Core.UserManager.GetList(parameter);
            return new PagingResult
            {
                List = list.Select(e => new UserViewModel
                {
                    ID = e.ID,
                    Username = e.Username,
                    RealName = e.RealName,
                    Role = e.Role,
                    JobTitle = e.JobTitle == null ? null : e.JobTitle.Name,
                    Departments = e.UserDepartments.Select(d => new
                    {
                        d.Department.Name,
                        ID = d.DepartmentId
                    }),
                    JobTitleId = e.JobTitleId,
                    Groups = e.UserGroups.Select(g => new
                    {
                        g.Group.Name,
                        g.Group.ID,
                    })
                }),
                Page = parameter.Page
            };
        }

        [HttpGet]
        public IHttpActionResult GetModel(int id)
        {
            if (id <= 0)
            {
                return BadRequest("用户ID参数不正确");
            }
            var user = Core.UserManager.GetModel(id);
            return Ok(user);
        }

        [HttpPost]
        public IHttpActionResult Save([FromBody] User user, [FromUri]string departmentIds, [FromUri] string groupIds)
        {
            if (user == null
                || string.IsNullOrEmpty(user.Username)
                || string.IsNullOrEmpty(user.RealName))
            {
                return BadRequest("编辑用户参数错误");
            }
            if (user.ID == 0)
            {
                user.Password = "123456";
            }

            Core.UserManager.Save(user);
            if (!string.IsNullOrEmpty(departmentIds))
            {
                var ids = groupIds.Split(',').Select(str => int.Parse(str)).ToArray();
                Core.DepartmentManager.UpdateUserDepartments(user.ID, ids);
            }

            if (!string.IsNullOrEmpty(groupIds))
            {
                var ids = groupIds.Split(',').Select(str => int.Parse(str)).ToArray();
                Core.GroupManager.UpdateUserGroups(user.ID, ids);
            }
            return Ok();
        }
        [HttpDelete]
        public void Delete(int id)
        {
            Core.UserManager.Delete(id);
        }

    }
}
