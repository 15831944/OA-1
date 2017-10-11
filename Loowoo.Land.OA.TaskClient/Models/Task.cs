﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.TaskClient.Models
{
    public class Task
    {
        public int ID { get; set; }

        /// <summary>
        /// 任务编号
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 任务来源
        /// </summary>
        public TaskFromType FromType { get; set; }
        /// <summary>
        /// 来源
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// 工作目标
        /// </summary>
        public string Goal { get; set; }
        /// <summary>
        /// 计划完成时间
        /// </summary>
        public DateTime? ScheduleDate { get; set; }
    }

    public enum TaskFromType
    {
        [Description("省")]
        Province = 1,
        [Description("市")]
        City,
        [Description("区")]
        Area
    }
}
