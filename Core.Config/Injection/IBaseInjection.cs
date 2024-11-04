
using Core.Config.Config;

namespace Core.Config.Injection
{
    public interface IBaseInjection
    {
        public IConfigProject ConfigProject { get; set; }
    }
}
