using Website.Models;

namespace Website.ViewModels.Web
{
    public class TagViewModel
    {
        public TagViewModel()
        {
            
        }

        public TagViewModel(Tag tag)
        {
            Text = tag.Text;
        }

        public string Text { get; set; }
    }
}