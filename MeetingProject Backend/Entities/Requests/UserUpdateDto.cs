using Entities.IModels;
using System.ComponentModel.DataAnnotations;

namespace Entities.Requests
{
    public class UserUpdateDto : IEntity
    {

        public string? Name { get; set; }

        public string? Surname { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [MinLength(6)]
        public string? Password { get; set; }

    }
}
