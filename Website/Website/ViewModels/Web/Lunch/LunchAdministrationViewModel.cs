using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Models;

namespace Website.ViewModels.Web
{
    public class LunchAdministrationViewModel
    {

        public ICollection<SelectListItem> AdministrationTasks { get; set; }
        public ICollection<SelectListItem> LunchPolls { get; set; }
        public int PollId { get; set; }
        public string UserName { get; set; }
        public string OptionName { get; set; }
        public string NewOptionName { get; set; }

        public LunchAdministrationViewModel()
        {
            AdministrationTasks = (
                from LunchAdministrationTaskEnum val in Enum.GetValues(typeof(LunchAdministrationTaskEnum))
                let name = val.GetDisplayName()
                let value = val.ToString()
                let selected = val == LunchAdministrationTaskEnum.EnterLunchDecision
                select new SelectListItem { Text = name, Value = value, Selected = selected }
            ).ToList();
            LunchPolls = new List<SelectListItem>();
        }

        public LunchAdministrationViewModel(IEnumerable<LunchPoll> polls, LunchAdministrationTaskEnum task) : this()
        {
            AdministrationTasks.Single(sli => sli.Value == task.ToString()).Selected = true;
            LunchPolls = polls.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).ToList();
        }
    }

    public enum LunchAdministrationTaskEnum
    {
        [Description("Enter Lunch Decision")] EnterLunchDecision,
        [Description("Add User to Crew")] AddUserToLunch,
        [Description("Remove User from Crew")] RemoveUserFromLunch,
        [Description("Rename Lunch Option")] RenameOption,
        [Description("Delete Lunch Option")] DeleteOption,
        [Description("Delete Crew")] DeletePoll,
    }
}