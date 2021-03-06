﻿using Loowoo.Common;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Loowoo.Land.OA.Managers
{
    public class MissiveManager:ManagerBase
    {
        /// <summary>
        /// 作用：保存公文
        /// 作者：汪建龙
        /// 编写时间：2017年2月21日11:58:26
        /// </summary>
        /// <param name="missive"></param>
        /// <returns></returns>
        public int Save(Missive missive)
        {
            using (var db = GetDbContext())
            {
                db.Missives.Add(missive);
                db.SaveChanges();
                return missive.ID;
            }
        }

        /// <summary>
        /// 作用：编辑公文信息
        /// 作者：汪建龙
        /// 编写时间：2017年2月21日12:09:12
        /// </summary>
        /// <param name="missive"></param>
        /// <returns></returns>
        public bool Edit(Missive missive)
        {
            using (var db = GetDbContext())
            {
                var entry = db.Missives.Find(missive.ID);
                if (entry == null)
                {
                    return false;
                }
                db.Entry(entry).CurrentValues.SetValues(missive);
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// 作用：通过ID获取公文
        /// 作者：汪建龙
        /// 编写时间：2017年2月21日12:13:10
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Missive Get(int id)
        {
            using (var db = GetDbContext())
            {
                var model= db.Missives.Find(id);
                return model;
            }
        }

        /// <summary>
        /// 作用：删除公文
        /// 作者：汪建龙
        /// 编写时间：2017年2月21日13:08:24
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool Delete(int id)
        {
            using (var db = GetDbContext())
            {
                var entry = db.Missives.Find(id);
                if (entry == null)
                {
                    return false;
                }
                entry.Deleted = true;
                db.SaveChanges();
                return true;
            }
        }

        /// <summary>
        /// 作用：查询公文拟稿
        /// 作者：汪建龙
        /// 编写时间：2017年2月25日12:35:08
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public List<Missive> Search(MissiveParameter parameter)
        {
            using (var db = GetDbContext())
            {
                var query = db.Missives.Where(e => e.Deleted == false).AsQueryable();
                if (parameter.UserID.HasValue)
                {
                    query = query.Where(e => e.UserID == parameter.UserID.Value);
                }
                //query = query.OrderByDescending(e => e.ID).SetPage(parameter.Page);
                return query.ToList();
            }
        }
        /// <summary>
        /// 作用：获取ID数组的公文列表
        /// 作者：汪建龙
        /// 编写时间：2017年2月27日11:20:55
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public List<Missive> GetList(int[] ids)
        {
            var list = new List<Missive>();
            foreach(var id in ids)
            {
                var model = Get(id);
                if (model != null)
                {
                    list.Add(model);
                }
            }
            return list;
        }

        //public List<Missive> ReBody(List<Missive> list)
        //{
        //    foreach(var item in list)
        //    {
        //        item.UnderTaker = Core.UserManager.Get(item.UserID);
        //        item.Category = Core.CategoryManager.Get(item.CategoryID);
        //        item.Born = Core.DepartmentManager.Get(item.BornOrganID);
        //        item.To = Core.DepartmentManager.Get(item.ToOrganID);
        //        item.FlowNode = Core.FlowNodeManager.Get(item.NodeID);
        //    }
        //    return list;
        //}
    }
}