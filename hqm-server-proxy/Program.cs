using hqm_server_proxy;
using hqm_server_proxy.Classes;
using Newtonsoft.Json;
using System.Net;
using System.Reflection;

await Main();
async Task Main()
{
    var currentPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
    var path = Path.Combine(currentPath, "config.json");
    var configJson = File.ReadAllText(path);
    var mainConfig = JsonConvert.DeserializeObject<MainConfig>(configJson);

    var proxyTasks = new List<Task>();

    foreach (var config in mainConfig.Servers)
    {
        var proxyServer = new ProxyServer(config, mainConfig.Name);

        proxyTasks.Add(new Task(async () => await proxyServer.Run()));
    }

    Parallel.ForEach(proxyTasks, task =>
    {
        task.Start();
    });

    Task.WaitAll(proxyTasks.ToArray());
}