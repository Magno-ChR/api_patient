using System.Threading.Tasks;

namespace patient.domain.Entities.Contacts
{
    internal interface IContactRepository
    {
        Task UpdateAsync(Contact contact);
        Task DeleteAsync(Contact contact);
    }
}