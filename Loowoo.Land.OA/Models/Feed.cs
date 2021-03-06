﻿using Loowoo.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.Models
{
    [Table("feed")]
    public class Feed
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public int FromUserId { get; set; }

        public virtual User FromUser { get; set; }

        public int ToUserId { get; set; }

        public virtual User ToUser { get; set; }

        public UserAction Action { get; set; }

        public int InfoId { get; set; }

        public virtual FormInfo Info { get; set; }

        public FeedType Type { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public DateTime? UpdateTime { get; set; }

        public bool Deleted { get; set; }
    }

    public enum UserAction
    {
        [Description("创建")]
        Create,
        [Description("查阅")]
        Read,
        [Description("更新")]
        Update,
        [Description("删除")]
        Delete,
        [Description("申请")]
        Apply,
        [Description("提交")]
        Submit,
        [Description("退回")]
        Back,
        [Description("结束")]
        Complete
    }

    public enum FeedType
    {
        [Description("信息")]
        Info,
        [Description("流程")]
        Flow,
        [Description("转发")]
        FreeFlow,
        [Description("评论")]
        Comment,
        [Description("任务")]
        Task
    }

}
