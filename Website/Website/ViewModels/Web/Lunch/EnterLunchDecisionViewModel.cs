using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Website.Models;

namespace Website.ViewModels.Web
{
    public class EnterLunchDecisionViewModel
    {
        public ICollection<SelectListItem> LunchPolls { get; set; }

        [Required]
        public int SelectedPollId { get; set; }

        [Required]
        public string Decision { get; set; }

        public EnterLunchDecisionViewModel()
        {
            LunchPolls = new List<SelectListItem>();
        }

        public EnterLunchDecisionViewModel(IEnumerable<LunchPoll> polls)
        {
            LunchPolls = polls.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).ToList();
        }

    }
}