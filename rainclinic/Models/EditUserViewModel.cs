using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace rainclinic.Models
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }

        public List<string> Roles { get; set; } = new List<string>();

        public IList<string> SelectedRoles { get; set; } = new List<string>();
    }
}
