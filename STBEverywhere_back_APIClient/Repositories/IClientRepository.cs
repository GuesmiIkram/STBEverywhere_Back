
using STBEverywhere_Back_SharedModels;
namespace STBEverywhere_back_APIClient.Repositories
{
    public interface IClientRepository
    {
        Client GetClientByEmail(string email);
        Task<Client> GetClientByIdAsync(int id);
        Client GetClientById(int id);

        Task UpdateClientAsync(Client client);
    }
}
