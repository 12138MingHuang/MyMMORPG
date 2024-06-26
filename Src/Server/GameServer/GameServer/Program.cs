using System;
using System.IO;
using Common;

internal class Program
{
    private static void Main(string[] args)
    {
        FileInfo fi = new System.IO.FileInfo("log4net.xml");
        log4net.Config.XmlConfigurator.ConfigureAndWatch(fi);
        Log.Init("GameServer");
        Log.Info("Game Server Init");
    }
}