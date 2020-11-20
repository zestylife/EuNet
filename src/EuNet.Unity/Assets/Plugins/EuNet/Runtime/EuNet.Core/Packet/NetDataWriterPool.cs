using System;

namespace EuNet.Core
{
    /// <summary>
    /// NetDataWriter 를 재활용하기 위한 풀
    /// </summary>
    public class NetDataWriterPool : ConcurrentObjectPool<NetDataWriter>
    {
        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="maxCount">풀링될 최대 개수. 최대 개수에 도달하면 풀링되지 않고 버려진다</param>
        public NetDataWriterPool(int maxCount)
            : base(0, maxCount)
        {

        }

        /// <summary>
        /// 풀에서 NetDataWriter 를 가져온다
        /// </summary>
        /// <returns></returns>
        public override NetDataWriter Alloc()
        {
            var writer = base.Alloc();
            writer.Reset();
            return writer;
        }

        /// <summary>
        /// 할당과 해제를 자동으로하여 편리하게 사용할 수 있는 함수
        /// Action을 선언하고 완료가 되면 자동으로 풀링된 NetDataWriter 가 해제된다
        /// </summary>
        /// <param name="action"></param>
        public void Use(Action<NetDataWriter> action)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                action(writer);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }
    }
}
