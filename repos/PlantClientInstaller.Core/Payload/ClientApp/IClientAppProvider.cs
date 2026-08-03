namespace PlantClientInstaller.Core.Payload.ClientApp;

public interface IClientAppProvider
{
    ClientAppModel Load();
    bool TryLoad(out ClientAppModel? model);
}
