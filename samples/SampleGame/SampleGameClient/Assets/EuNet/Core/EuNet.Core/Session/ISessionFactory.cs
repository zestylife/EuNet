using System.Threading.Tasks;

namespace EuNet.Core
{
    /// <summary>
    /// 세션을 생성하기 위한 세션팩토리.
    /// 풀링의 기능도 가능하게 하기 위해서 Create 와 Release 를 통해서 관리되도록 함
    /// </summary>
    public interface ISessionFactory
    {
        /// <summary>
        /// 세션 생성
        /// </summary>
        /// <returns>생성된 세션</returns>
        ISession Create();

        /// <summary>
        /// 세션 해제
        /// </summary>
        /// <param name="session">해제될 세션</param>
        /// <returns>성공여부</returns>
        bool Release(ISession session);

        /// <summary>
        /// 서버가 종료되면 호출됨
        /// </summary>
        Task ShutdownAsync();
    }
}
