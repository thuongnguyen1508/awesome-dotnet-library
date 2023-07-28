using System.Threading;
using System.Threading.Tasks;

namespace TN.Warmups
{
    public interface IStartupTask
    {
        Task ExecuteAsync(CancellationToken cancellationToken = default);
    }
}
