using System.Threading.Tasks;

namespace EuNet.Core
{
    public interface ISessionFactory
    {
        ISession Create();
        bool Release(ISession session);
        Task ShutdownAsync();
    }
}
