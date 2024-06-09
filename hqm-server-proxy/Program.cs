using hqm_server_proxy;
using hqm_server_proxy.Classes;
using Newtonsoft.Json;
using System.Reflection;

await Main();
async Task Main()
{
    var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    var path = Path.Combine(currentPath, "config.json");
    var configJson = File.ReadAllText(path);
    var mainConfig = JsonConvert.DeserializeObject<MainConfig>(configJson);

    var proxyServer = new ProxyServer(mainConfig.Config, mainConfig.Name);
    await proxyServer.Run();
}