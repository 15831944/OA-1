﻿using Loowoo.Common;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;

namespace Loowoo.Land.OA.Managers
{
    public class SealManager : ManagerBase
    {
        public IEnumerable<Seal> GetList()
        {
            return DB.Seals.Where(e => !e.Deleted);
        }

        public void Save(Seal model)
        {
            DB.Seals.AddOrUpdate(model);
            DB.SaveChanges();
        }

        public Seal Get(int id)
        {
            return DB.Seals.FirstOrDefault(e => e.ID == id);
        }

        public void Delete(int id)
        {
            if (DB.FormInfoExtend1s.Any(e => e.ExtendInfoId == id))
            {
                throw new Exception("会议室已被使用，无法删除");
            }
            var entity = Get(id);
            entity.Deleted = true;
            DB.SaveChanges();
        }

        public FormInfo Apply(FormInfoExtend1 data)
        {
            var model = Get(data.ExtendInfoId);
            var info = new FormInfo
            {
                Title = "申请公章：" + model.Name,
                FormId = (int)FormType.Seal,
                PostUserId = data.UserId,
            };
            info.Form = Core.FormManager.GetModel(FormType.Seal);

            Core.FormInfoManager.Save(info);
            Core.FormInfoExtend1Manager.Apply(info, data);
            return info;
        }

        public void UpdateStatus(int roomId, SealStatus status)
        {
            var car = Get(roomId);
            car.Status = status;
            DB.SaveChanges();
        }
    }
}