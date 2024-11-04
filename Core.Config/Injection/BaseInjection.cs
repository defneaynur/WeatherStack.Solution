using Core.Config.Config;


namespace Core.Config.Injection
{
    public class BaseInjection: IBaseInjection
    {
        public IConfigProject ConfigProject { get; set; }
    }
}
