using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleToDoList.Domain.Interfaces
{
    public interface IDatabaseFileProvider
    {
        Task<ICollection<T>> LoadAsync<T>(string fileName, CancellationToken token);
        Task SaveAsync<T>(string fileName, IEnumerable<T> items, CancellationToken token);
    }
}
