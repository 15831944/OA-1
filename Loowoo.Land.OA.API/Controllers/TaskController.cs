﻿using Loowoo.Common;
using Loowoo.Land.OA.API.Models;
using Loowoo.Land.OA.Models;
using Loowoo.Land.OA.Parameters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Loowoo.Land.OA.API.Controllers
{
    public class TaskController : ControllerBase
    {
        [HttpGet]
        public object Model(int id)
        {
            return Core.TaskManager.GetModel(id);
        }

        [HttpGet]
        public object List(string searchKey = null, FlowStatus? status = null, int page = 1, int rows = 20)
        {
            var form = Core.FormManager.GetModel(FormType.Task);
            var parameter = new FormInfoParameter
            {
                FormId = form.ID,
                FlowStatus = status.HasValue ? new[] { status.Value } : null,
                UserId = status == null && CurrentUser.HasRight(FormType.Task, UserRightType.View) ? 0 : Identity.ID,
                SearchKey = searchKey,
                Page = new PageParameter(page, rows)
            };
            if (parameter.UserId == 0)
            {
                var datas = Core.TaskManager.GetTasks(parameter);
                return new PagingResult
                {
                    List = datas.Select(e => new TaskViewModel(e)),
                    Page = parameter.Page,
                };
            }
            else
            {
                var datas = Core.TaskManager.GetUserTasks(parameter);
                return new PagingResult
                {
                    List = datas.Select(e => new TaskViewModel(e)),
                    Page = parameter.Page
                };
            }
        }

        [AllowAnonymous]
        [HttpGet]
        public object TasksForLED()
        {
            var form = Core.FormManager.GetModel(FormType.Task);
            var parameter = new FormInfoParameter
            {
                FormId = form.ID,
                FlowStatus = new[] { FlowStatus.Doing, FlowStatus.Back, FlowStatus.Done },
            };

            var datas = Core.TaskManager.GetUserTasks(parameter).GroupBy(e => e.InfoId).Select(g => g.FirstOrDefault());
            return datas.Select(e => new TaskViewModel(e));
        }

        [AllowAnonymous]
        [HttpGet]
        public object SubTasksForLED(int taskId)
        {
            return Core.TaskManager.GetSubTaskList(taskId).Select(e => new SubTaskViewModel(e));
        }

        [HttpPost]
        public void Save([FromBody]Task model)
        {
            var form = Core.FormManager.GetModel(FormType.Task);
            var isAdd = model.ID == 0;
            //判断id，如果不存在则创建forminfo
            if (model.ID == 0)
            {
                model.Info = new FormInfo
                {
                    FormId = form.ID,
                    PostUserId = Identity.ID,
                    Title = model.Name
                };
                Core.FormInfoManager.Save(model.Info);
            }
            else
            {
                model.Info = Core.FormInfoManager.GetModel(model.ID);
                model.Info.Title = model.Name;
            }
            if (model.Info.FlowDataId == 0)
            {
                model.Info.Form = form;
                Core.FlowDataManager.CreateFlowData(model.Info);
            }
            Core.TaskManager.Save(model);

            Core.FeedManager.Save(new Feed
            {
                InfoId = model.ID,
                Title = model.Name,
                Description = model.Goal,
                FromUserId = Identity.ID,
                Action = isAdd ? UserAction.Create : UserAction.Update,
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public object SubTaskList(int taskId)
        {
            return Core.TaskManager.GetSubTaskList(taskId).Select(e => new SubTaskViewModel(e));
        }

        [HttpPost]
        public void AddSubTasks(JToken data)
        {
            var model = data["data"].ToObject<SubTask>();
            var mainUserId = data["mainUserId"] == null ? 0 : data["mainUserId"].Value<int>();
            var subUserIds = data["subUserIds"].ToObject<int[]>();

            if (model.ParentId == 0)
            {
                if (mainUserId == 0)
                {
                    throw new Exception("没有选择主办科室负责人");
                }
                var mainUser = Core.UserManager.GetModel(mainUserId);
                model.ToUserId = mainUserId;
                model.ToDepartmentId = mainUser.GetDepartmentId();

                var parent = SaveSubTask(model);

                if (subUserIds != null)
                {
                    foreach (var userId in subUserIds)
                    {
                        var subUser = Core.UserManager.GetModel(userId);

                        var child = data["data"].ToObject<SubTask>();
                        child.ParentId = parent.ID;
                        child.ToDepartmentId = subUser.GetDepartmentId();
                        child.ToUserId = userId;

                        SaveSubTask(child);
                    }
                }
            }
            else
            {
                foreach (var userId in subUserIds)
                {
                    var user = Core.UserManager.GetModel(userId);

                    var child = data["data"].ToObject<SubTask>();
                    child.ToDepartmentId = user.GetDepartmentId();
                    child.ToUserId = user.ID;
                    SaveSubTask(child);
                }
            }
        }

        [HttpPost]
        public SubTask SaveSubTask(SubTask data)
        {
            var model = data.ID == 0 ? data : Core.TaskManager.GetSubTask(data.ID);
            if (model.ToDepartmentId == 0)
            {
                throw new Exception("没有指定科室");
            }
            if (model.ToUserId == 0)
            {
                throw new Exception("没有指定责任人");
            }
            if (model.IsMaster && model.LeaderId == 0)
            {
                throw new Exception("没有指定分管领导");
            }

            var isAdd = model.ID == 0;
            if (isAdd)
            {
                var department = Core.DepartmentManager.Get(model.ToDepartmentId);
                if (department == null)
                {
                    throw new Exception("部门id不正确");
                }
                model.ToDepartmentName = department.Name;
                model.CreatorId = Identity.ID;
            }
            else
            {
                model.Content = data.Content;
                model.UpdateTime = data.UpdateTime;
                model.ScheduleDate = data.ScheduleDate;
            }
            Core.TaskManager.SaveSubTask(model);

            if (isAdd)
            {
                var info = Core.FormInfoManager.GetModel(model.TaskId);
                var flowData = info.FlowData;
                var flowNodeData = flowData.GetFirstNodeData();
                var toUserNodeData = Core.FlowNodeDataManager.GetModelByExtendId(model.ID, model.ToUserId);
                if (toUserNodeData == null)
                {
                    toUserNodeData = Core.FlowNodeDataManager.CreateChildNodeData(flowNodeData, model.ToUserId, model.ID);
                }
                //通知相关人员

                Core.FeedManager.Save(new Feed
                {
                    Action = UserAction.Create,
                    FromUserId = Identity.ID,
                    ToUserId = model.ToUserId,
                    Title = "[创建任务]" + info.Title,
                    Description = model.Content,
                    Type = FeedType.Task,
                    InfoId = model.TaskId,
                });

                Core.UserFormInfoManager.Save(new UserFormInfo
                {
                    InfoId = model.TaskId,
                    UserId = model.ToUserId,
                    FlowStatus = FlowStatus.Doing,
                });
                //通知分管领导
                Core.UserFormInfoManager.Save(new UserFormInfo
                {
                    InfoId = model.TaskId,
                    UserId = model.LeaderId,
                    FlowStatus = FlowStatus.Doing,
                });

                Core.FeedManager.Save(new Feed
                {
                    Action = UserAction.Create,
                    FromUserId = Identity.ID,
                    ToUserId = model.LeaderId,
                    Title = "[创建任务]" + info.Title,
                    Description = model.Content,
                    Type = FeedType.Task,
                    InfoId = model.TaskId,
                });

                Core.MessageManager.Add(new Message
                {
                    InfoId = info.ID,
                    Content = info.Title,
                    CreatorId = Identity.ID
                }, model.LeaderId, model.ToUserId);
            }

            return model;
        }

        [HttpDelete]
        public void DeleteSubTask(int id)
        {
            var model = Core.TaskManager.GetSubTask(id);
            Core.TaskManager.DeleteSubTask(model);

            var flowNodeData = Core.FlowNodeDataManager.GetModelByExtendId(model.ID, model.ToUserId);
            Core.FlowNodeDataManager.Remove(flowNodeData);
            Core.UserFormInfoManager.Remove(model.TaskId, model.ToUserId);
            //Core.FeedManager.Delete(new Feed
            //{
            //    ToUserId = model.ToUserId,
            //    InfoId = model.TaskId,
            //    Type = FeedType.Task,
            //    Action = UserAction.Delete,
            //    FromUserId = CurrentUser.ID,
            //    Title = model.Content,
            //});
        }

        /// <summary>
        /// 提交子任务，创建子任务相关流程
        /// </summary>
        [HttpPost]
        public void SubmitSubTask(int id, JToken data)
        {
            var content = data["content"].Value<string>();
            var result = true;
            var model = Core.TaskManager.GetSubTask(id);
            if (model.IsMaster)
            {
                var children = Core.TaskManager.GetSubTaskList(model.TaskId, model.ID);
                if (!children.All(e => e.Status == SubTaskStatus.Complete))
                {
                    throw new Exception("子任务还没完成，无法提交");
                }
            }
            if (model.Todos.Any(e => !e.Completed))
            {
                throw new Exception("子任务还没完成，无法提交");
            }
            model.Status = SubTaskStatus.Checking;
            model.UpdateTime = DateTime.Now;

            var flowNodeData = Core.FlowNodeDataManager.GetModelByExtendId(model.ID, model.ToUserId);
            if (flowNodeData.Submited)
            {
                return;
            }
            flowNodeData.Content = content;
            flowNodeData.Result = result;
            Core.FlowNodeDataManager.Submit(flowNodeData);
            //如果当前用户还有其他的任务或todo没有完成，则不能标记为done
            if (!Core.TaskManager.HasDoingTask(model.TaskId, Identity.ID))
            {
                Core.UserFormInfoManager.Save(new UserFormInfo
                {
                    InfoId = model.TaskId,
                    UserId = model.ToUserId,
                    FlowStatus = FlowStatus.Done
                });
            }

            int toUserId = 0;
            //如果是协办科室，则直接提交结束
            if (!model.IsMaster)
            {
                var parentSubTask = Core.TaskManager.GetSubTask(model.ParentId);
                toUserId = parentSubTask.ToUserId;
            }
            else
            {
                //主办科室提交，则需要创建分管领导主流程
                toUserId = model.LeaderId;
            }
            Core.FlowNodeDataManager.CreateChildNodeData(flowNodeData, toUserId, model.ID);
            Core.UserFormInfoManager.Save(new UserFormInfo
            {
                InfoId = model.TaskId,
                UserId = toUserId,
                FlowStatus = FlowStatus.Doing,
            });

            var feed = new Feed
            {
                FromUserId = model.ToUserId,
                ToUserId = toUserId,
                Action = UserAction.Submit,
                Type = FeedType.Flow,
                InfoId = model.TaskId,
                Title = "[提交任务]" + model.Content,
            };
            Core.FeedManager.Save(feed);
            Core.MessageManager.Add(feed);
        }

        [HttpGet]
        public IEnumerable<FlowNodeData> CheckList(int taskId, int userId = 0)
        {
            return Core.FlowNodeDataManager.GetList(taskId, userId);
        }

        /// <summary>
        /// 分管领导审核
        /// </summary>
        [HttpPost]
        public void CheckSubTask(int id, JToken data, bool result = true)
        {
            var content = data["content"].Value<string>();
            var model = Core.FlowNodeDataManager.GetModel(id);
            if (model == null || model.Submited)
            {
                throw new Exception("没有需要审核的流程");
            }
            model.Content = content;
            model.Result = result;
            var subTask = Core.TaskManager.GetSubTask(model.ExtendId);
            //更新自己当前的流程状态
            Core.FlowNodeDataManager.Submit(model);
            Core.UserFormInfoManager.Save(new UserFormInfo
            {
                InfoId = subTask.TaskId,
                FlowStatus = FlowStatus.Done,
                UserId = model.UserId
            });

            int toUserId = 0;
            if (result)
            {
                subTask.Status = SubTaskStatus.Complete;
                //如果是是主办科室，则需要发给局领导，如果是协办科室，则结束
                if (subTask.IsMaster)
                {
                    //判断该Task的其他的主办是否全部完成，如果是，则发给局领导
                    var list = Core.TaskManager.GetSubTaskList(subTask.TaskId, 0).ToList();
                    var allCompleted = list.Where(e => e.ID != subTask.ID).All(e => e.Status == SubTaskStatus.Complete);
                    if (allCompleted)
                    {
                        var info = Core.FormInfoManager.GetModel(subTask.TaskId);
                        var flowData = info.FlowData;
                        //第一步流程标记为完成
                        var firstNodeData = flowData.GetFirstNodeData();
                        firstNodeData.Result = true;
                        firstNodeData.UpdateTime = DateTime.Now;
                        Core.UserFormInfoManager.Save(new UserFormInfo
                        {
                            InfoId = info.ID,
                            UserId = firstNodeData.UserId,
                            FlowStatus = FlowStatus.Done
                        });
                        //局领导的ID在主流程的最后一步
                        var flowNode = flowData.Flow.GetLastNode();
                        var user = Core.FlowNodeManager.GetUserList(flowNode).FirstOrDefault();
                        if (user == null)
                        {
                            throw new Exception("流程未配置局领导ID");
                        }

                        toUserId = user.ID;
                        Core.FlowNodeDataManager.CreateNodeData(flowData.ID, flowNode, toUserId);

                        var feed = new Feed
                        {
                            FromUserId = Identity.ID,
                            ToUserId = toUserId,
                            Action = UserAction.Submit,
                            Type = FeedType.Flow,
                            InfoId = subTask.TaskId,
                            Title = "[任务审核] " + info.Title
                        };
                        Core.FeedManager.Save(feed);
                        Core.MessageManager.Add(feed);
                    }
                }
                var feed1 = new Feed
                {
                    FromUserId = Identity.ID,
                    ToUserId = subTask.ToUserId,
                    Action = UserAction.Submit,
                    Type = FeedType.Flow,
                    InfoId = subTask.TaskId,
                    Title = "[任务完成] " + subTask.Content
                };
                Core.FeedManager.Save(feed1);
                Core.MessageManager.Add(feed1);
            }
            else
            {
                subTask.Status = SubTaskStatus.Back;
                var parentFlowNodeData = Core.FlowNodeDataManager.GetModel(model.ParentId);
                toUserId = parentFlowNodeData.UserId;
                Core.FlowNodeDataManager.CreateChildNodeData(model, toUserId, subTask.ID);
                var feed = new Feed
                {
                    FromUserId = Identity.ID,
                    ToUserId = toUserId,
                    Action = UserAction.Submit,
                    Type = FeedType.Flow,
                    InfoId = subTask.TaskId,
                    Title = "[任务失败] " + subTask.Content
                };
                Core.FeedManager.Save(feed);
                Core.MessageManager.Add(feed);
            }
            if (toUserId > 0)
            {
                Core.UserFormInfoManager.Save(new UserFormInfo
                {
                    InfoId = subTask.TaskId,
                    FlowStatus = FlowStatus.Doing,
                    UserId = toUserId
                });
            }
        }

        [HttpPost]
        public void SaveTodo(TaskTodo model)
        {
            var subTask = Core.TaskManager.GetSubTask(model.SubTaskId);
            model.CreatorId = Identity.ID;
            Core.TaskManager.SaveTodo(model);

            Core.UserFormInfoManager.Save(new UserFormInfo
            {
                InfoId = subTask.TaskId,
                UserId = model.ToUserId,
                FlowStatus = FlowStatus.Doing
            });

            var feed = new Feed
            {
                InfoId = subTask.TaskId,
                ToUserId = model.ToUserId,
                Title = model.Content,
                FromUserId = Identity.ID,
                Type = FeedType.Task,
                Action = UserAction.Create,
            };
            Core.FeedManager.Save(feed);
            Core.MessageManager.Add(feed);
        }

        [HttpGet]
        public void UpdateTodoStatus(int id)
        {
            var model = Core.TaskManager.GetTodo(id);
            model.Completed = !model.Completed;
            model.UpdateTime = DateTime.Now;
            Core.TaskManager.SaveTodo(model);
        }

        [HttpDelete]
        public void DeleteTodo(int id)
        {
            var model = Core.TaskManager.GetTodo(id);
            var subTask = Core.TaskManager.GetSubTask(model.SubTaskId);
            var infoId = subTask.TaskId;
            Core.TaskManager.RemoveTodo(model);
            if (!subTask.Todos.Any(e => e.ToUserId == model.ToUserId))
            {
                Core.UserFormInfoManager.Remove(infoId, model.ToUserId);
            }
        }
    }
}
