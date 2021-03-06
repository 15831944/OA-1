﻿using Loowoo.Common;
using Loowoo.Land.OA.API.Models;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Loowoo.Land.OA.API.Controllers
{
    public class MeetingRoomController : ControllerBase
    {
        [HttpGet]
        public object List()
        {
            var list = Core.MeetingRoomManager.GetList();
            return list.ToList().Select(e => new
            {
                e.Name,
                e.Number,
                e.PhotoId,
                e.Status,
                e.ID,
            });
        }

        [HttpPost]
        public void Save([FromBody]MeetingRoom data)
        {
            var model = Core.MeetingRoomManager.Get(data.ID) ?? data;
            model.Name = data.Name;
            model.Number = data.Number;
            model.Type = data.Type;

            Core.MeetingRoomManager.Save(model);
        }

        [HttpPost]
        public void Apply([FromBody]FormInfoExtend1 data)
        {
            var car = Core.MeetingRoomManager.Get(data.ExtendInfoId);
            if (car == null)
            {
                throw new ArgumentException("参数不正确，没有找该会议室");
            }
            if (car.Status != MeetingRoomStatus.Unused)
            {
                throw new Exception("当前会议室在使用中，无法申请");
            }
            if (data.ApprovalUserId == 0)
            {
                throw new Exception("没有选择审核人");
            }
            data.UserId = Identity.ID;
            if (Core.FormInfoExtend1Manager.HasApply(data))
            {
                throw new Exception("你已经申请过该会议室，还未通过审核");
            }

            var info = Core.MeetingRoomManager.Apply(data);
            var feed = new Feed
            {
                Action = UserAction.Apply,
                Title = info.Title,
                InfoId = data.ID,
                Type = FeedType.Flow,
                ToUserId = data.ApprovalUserId,
                FromUserId = Identity.ID,
            };
            Core.FeedManager.Save(feed);
            Core.MessageManager.Add(feed);
        }

        [HttpDelete]
        public void Delete(int id)
        {
            Core.MeetingRoomManager.Delete(id);
        }
    }
}