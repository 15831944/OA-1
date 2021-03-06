﻿using Loowoo.Common;
using Loowoo.Land.OA.API.Models;
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
                Name = user.RealName
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
                Page = new PageParameter(page, rows)
            };
            var list = Core.UserManager.GetList(parameter).OrderByDescending(x => x.Sort);
            return new PagingResult
            {
                List = list.Select(e => new UserViewModel(e)),
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
        public IHttpActionResult Save([FromBody] User user)
        {
            if (user == null
                || string.IsNullOrEmpty(user.Username)
                || string.IsNullOrEmpty(user.RealName))
            {
                return BadRequest("用户名或姓名没有填写");
            }
            if (user.ID == 0)
            {
                user.Password = "123456";
            }

            Core.UserManager.Save(user);
            if (user.DepartmentIds != null)
            {
                Core.DepartmentManager.UpdateUserDepartments(user.ID, user.DepartmentIds);
            }

            if (user.GroupIds != null)
            {
                Core.GroupManager.UpdateUserGroups(user.ID, user.GroupIds);
            }
            return Ok();
        }

        [HttpDelete]
        public void Delete(int id)
        {
            Core.UserManager.Delete(id);
        }

        [HttpGet]
        public IHttpActionResult UpdatePassword(string oldPassword, string newPassword, string rePassword)
        {
            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(rePassword))
            {
                return BadRequest("填写不完整");
            }
            if (newPassword != rePassword)
            {
                return BadRequest("两次输入密码不相同，请重新输入");
            }
            var user = Core.UserManager.GetModel(Identity.ID);
            if (user.Password != oldPassword.MD5())
            {
                return BadRequest("旧密码填写不正确");
            }
            user.Password = newPassword;
            Core.UserManager.Save(user);
            return Ok();
        }

        [HttpGet]
        public IEnumerable<UserViewModel> RecentList(int formId = 0)
        {
            var userIds = Core.FeedManager.GetList(new FeedParameter
            {
                FormId = formId,
                BeginTime = DateTime.Today.AddDays(-30),
                FromUserId = Identity.ID,
                Page = new PageParameter(1, 10)
            }).Where(e => e.ToUserId > 0).GroupBy(e => e.ToUserId).Select(g => g.Key).ToArray();
            if (userIds.Length > 0)
            {
                return Core.UserManager.GetList(new UserParameter
                {
                    UserIds = userIds
                }).Select(e => new UserViewModel(e));
            }
            return null;

        }

        [HttpGet]
        public IEnumerable<UserViewModel> FlowContactList()
        {
            return Core.UserManager.GetFlowContacts(Identity.ID).Select(e => new UserViewModel(e.Contact));
        }
        [HttpGet]
        public void SaveFlowContact(int userId)
        {
            Core.UserManager.SaveFlowContact(new UserFlowContact { UserId = Identity.ID, ContactId = userId });
        }

        [HttpDelete]
        public void DeleteFlowContacts(string userIds)
        {
            Core.UserManager.DeleteFlowContact(userIds.ToIntArray(), Identity.ID);
        }

        /// <summary>
        /// 获取上级审核人
        /// </summary>
        [HttpGet]
        public IEnumerable<UserViewModel> ParentTitleUserList(int userId = 0)
        {
            var user = Core.UserManager.GetModel(userId > 0 ? userId : Identity.ID);
            return Core.UserManager.GetParentTitleUsers(user).Select(e => new UserViewModel(e));
        }

        [HttpGet]
        public void ResetPassword(int userId)
        {
            if (CurrentUser.Role != UserRole.Administrator)
            {
                throw new Exception("权限不足");
            }
            var user = Core.UserManager.GetModel(userId);
            user.Password = !string.IsNullOrWhiteSpace(user.Mobile) ? user.Mobile : "123abc";
            Core.UserManager.Save(user);
        }
    }
}
