﻿using Cocona;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;

namespace DevTools.Cli;

public class DevToolsCommands
{
    [Command("guid")]
    public void NewGuid([Option("c")] int? count)
    {
        if (count is not null)
            for (var i = 0; i < count; i++)
                Console.WriteLine(Guid.NewGuid());
        else
            Console.WriteLine(Guid.NewGuid());
    }

    [Command("token")]
    public void GenerateToken([Argument] int length, [Option("c")] int? count)
    {
        var valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var sb = new StringBuilder();

        if (count is not null)
            for (var i = 0; i < count; i++)
            {
                var l = length;

                using (var rng = new RNGCryptoServiceProvider())
                {
                    byte[] uintBuffer = new byte[sizeof(uint)];

                    while (l-- > 0)
                    {
                        rng.GetBytes(uintBuffer);
                        var num = BitConverter.ToUInt32(uintBuffer, 0);
                        sb.Append(valid[(int)(num % (uint)valid.Length)]);
                    }
                }
                Console.WriteLine(sb.ToString());
                sb.Clear();
            }
        else
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];

                while (length-- > 0)
                {
                    rng.GetBytes(uintBuffer);
                    var num = BitConverter.ToUInt32(uintBuffer, 0);
                    sb.Append(valid[(int)(num % (uint)valid.Length)]);
                }
            }
            Console.WriteLine(sb.ToString());
        }
    }

    [Command("dns-reset")]
    public void ResetDns()
    {
        var nic = GetActiveEthernetOrWifiNetworkInterface().Result;

        if (nic is null)
            Console.WriteLine("there is a problem in get network interface information!");

        var reset = RunCommand($"netsh interface ipv4 set dns \"{nic!.Name}\" dhcp");
        Console.WriteLine(reset
            ? "dns addresses have been removed"
            : "there is a problem in reset dns addresses!");
    }

    [Command("dns-change")]
    public void ChangeDns([Argument] string preferred, [Argument] string alternate)
    {
        var nic = GetActiveEthernetOrWifiNetworkInterface().Result;

        if (nic is null)
            Console.WriteLine("there is a problem in get network interface information!");

        var change = RunCommand(CreateSetCommand(nic!.Name, preferred, alternate));
        Console.WriteLine(change
            ? $"dns addresses have been changed to {preferred}, {alternate}"
            : "there is a problem in change dns addresses!");
    }

    #region dns methods

    private Task<NetworkInterface?> GetActiveEthernetOrWifiNetworkInterface()
    {
        return Task.FromResult(
            NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(a => a is { OperationalStatus: OperationalStatus.Up, NetworkInterfaceType: NetworkInterfaceType.Wireless80211 or NetworkInterfaceType.Ethernet }
                                     && a.GetIPProperties().GatewayAddresses
                                         .Any(g => g.Address.AddressFamily.ToString() == "InterNetwork")));
    }

    private string CreateSetCommand(string nicName, string preferred, string alternate)
        => $"netsh interface ipv4 add dnsservers \"{nicName}\" address={preferred} index=1 ; netsh interface ipv4 add dnsservers \"{nicName}\" address={alternate} index=2";

    private bool RunCommand(string arg)
    {
        try
        {
            ProcessStartInfo psi = new ProcessStartInfo("powershell.exe");
            psi.UseShellExecute = true;
            psi.WindowStyle = ProcessWindowStyle.Hidden;
            psi.Verb = "runas";
            psi.Arguments = arg;
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion
}