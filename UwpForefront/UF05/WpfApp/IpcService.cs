﻿using System;
using System.ServiceModel;
using System.Windows;


// プロセス間通信 (WCF 利用)
// System.ServiceModel アセンブリへの参照追加が必要

namespace WpfApp
{
  // サービスの定義
  [ServiceContract]
  public interface INavigationService
  {
    [OperationContract]
    void Navigate(string url);
  }



  // サーバー側
  [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
  public class IpcService : INavigationService
  {
    public void Navigate(string url)
    {
      if (App.Current.MainWindow is MainWindow mainWindow)
      {
        // ウィンドウを最前面に持ってくる
        if (mainWindow.WindowState == WindowState.Minimized)
          mainWindow.WindowState = WindowState.Normal;
        mainWindow.Activate();
        mainWindow.Topmost = true; 
        mainWindow.Topmost = false;
        mainWindow.Focus();

        // URL の Web ページを表示
        if (!string.IsNullOrWhiteSpace(url))
          mainWindow.Navigate(url);
      }
    }

    private static ServiceHost _host;

    public static void StartService()
    {
      if (_host != null)
        _host.Close();

      _host = new ServiceHost(new IpcService(),
                              new Uri("net.pipe://localhost/UF05"));
      _host.AddServiceEndpoint(typeof(INavigationService),
                               new NetNamedPipeBinding(),
                               "NavigationService");
      _host.Open();
    }
  }



  // クライアント側
  public class IpcClient
  {
    public static bool RequestNavigation(string url)
    {
      try
      {
        var channelFactory
          = new ChannelFactory<INavigationService>(
              new NetNamedPipeBinding(),
              new EndpointAddress("net.pipe://localhost/UF05/NavigationService"));
        var navigationService = channelFactory.CreateChannel();
        navigationService.Navigate(url);
        return true;
      }
      catch
      {
        return false;
      }
    }
  }
}
