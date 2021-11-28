using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SimpleToDoList.Domain.Interfaces;

namespace SimpleToDoList.Infrastructure.Providers
{
    public class JsonFileProvider: IDatabaseFileProvider
    {
        private readonly string _databasePath;

        public JsonFileProvider(string databasePath)
        {
            _databasePath = databasePath;
        }
        
        public async Task<ICollection<T>> LoadAsync<T>(string fileName, CancellationToken token)
        {
            var file = Path.Join(_databasePath, $"{fileName}.json");
            if (!File.Exists(file))
                return new List<T>();
            await using var openStream = File.OpenRead(file);
            return await JsonSerializer.DeserializeAsync<ICollection<T>>(openStream, cancellationToken: token);
        } 

        
        public async Task SaveAsync<T>(string fileName, IEnumerable<T> items, CancellationToken token)
        {
            await using var openStream = File.Create(Path.Join(_databasePath, $"{fileName}.json"));
            await JsonSerializer.SerializeAsync(openStream, items, cancellationToken: token);
        }
    }
}
