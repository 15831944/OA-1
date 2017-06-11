﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loowoo.Land.OA.Models
{
    /// <summary>
    /// 车辆申请记录
    /// </summary>
    [Table("car_apply")]
    public class CarApply
    {
        /// <summary>
        /// 申请ID，和FormInfoID 一一对应
        /// </summary>
        [Key,DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ID { get; set; }

        public int CarId { get; set; }

        public virtual Car Car { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.Now;

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public int ApprovalUserId { get; set; }

        public DateTime ScheduleBeginTime { get; set; }

        public DateTime? ScheduleEndTime { get; set; }

        public DateTime? RealEndTime { get; set; }

        public string Reason { get; set; }

        public bool? Result { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}