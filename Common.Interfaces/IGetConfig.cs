using Common.ModelInterfaces;

namespace Common.Interfaces
{
    public interface IGetConfig
    {
        T GetConfig<T>(string id = null) where T : IIdentifier;
    }
}
