﻿using Loowoo.Land.OA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.Managers
{
    public class FreeFlowNodeDataManager : ManagerBase
    {
        public bool Add(FreeFlowNodeData model)
        {
            var user = Core.UserManager.GetModel(model.UserId);
            model.Signature = user.RealName;
            var entity = DB.FreeFlowNodeDatas.FirstOrDefault(e => e.FreeFlowDataId == model.FreeFlowDataId && e.UserId == model.UserId && e.ParentId == model.ParentId);
            if (entity == null)
            {
                DB.FreeFlowNodeDatas.Add(model);
                DB.SaveChanges();
                return true;
            }
            return false;
        }

        public FreeFlowNodeData Update(FreeFlowNodeData model)
        {
            var entity = DB.FreeFlowNodeDatas.FirstOrDefault(e => e.ID == model.ID);
            entity.Content = model.Content;
            entity.UpdateTime = DateTime.Now;
            DB.SaveChanges();
            return entity;
        }

        public FreeFlowNodeData GetModel(int parentId)
        {
            return DB.FreeFlowNodeDatas.FirstOrDefault(e => e.ID == parentId);
        }

        public void Save(FreeFlowNodeData model)
        {
            if (model.ID == 0)
            {
                Add(model);
            }
            else
            {
                Update(model);
            }
        }
    }
}
