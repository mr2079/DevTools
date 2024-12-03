using Cocona;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using Bogus;

namespace DevTools.Cli;

public class DevToolsCommands
{
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
            ProcessStartInfo psi = new("powershell.exe")
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                Verb = "runas",
                Arguments = arg
            };
            Process.Start(psi);
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    [Command(CommandStatics.Guid)]
    public void NewGuid(
        [Option(OptionStatics.Count)] int? count)
    {
        if (count is not null)
            for (var i = 0; i < count; i++)
                Console.WriteLine(Guid.NewGuid());
        else
            Console.WriteLine(Guid.NewGuid());
    }

    [Command(CommandStatics.Token)]
    public void GenerateToken(
        [Argument] int length,
        [Option(OptionStatics.Count)] int? count)
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

    [Command(CommandStatics.Dns)]
    public void DnsOperations(
        [Argument] DnsCommandEnum command,
        [Argument] string? preferred,
        [Argument] string? alternate)
    {
        var nic = GetActiveEthernetOrWifiNetworkInterface().Result;

        if (nic is null)
        {
            Console.WriteLine("there is a problem in get network interface information!\ncheck network connection");
            return;
        }

        switch (command)
        {
            case DnsCommandEnum.Status:
                {
                    var dnsAddresses = nic.GetIPProperties().DnsAddresses;

                    Console.WriteLine(dnsAddresses.Count >= 2
                        ? $"dns addresses: preferred = {dnsAddresses[0]} , alternate = {dnsAddresses[1]}"
                        : $"dns addresses: preferred = {dnsAddresses[0]}");

                    break;
                }
            case DnsCommandEnum.Reset:
                {
                    var reset = RunCommand($"netsh interface ipv4 set dns \"{nic!.Name}\" dhcp");

                    Console.WriteLine(reset
                        ? "dns addresses have been removed"
                        : "there is a problem in reset dns addresses!");

                    break;
                }
            case DnsCommandEnum.Set:
                {
                    if (string.IsNullOrWhiteSpace(preferred))
                    {
                        Console.WriteLine("enter preferred dns address at least!");
                        break;
                    }

                    var change = RunCommand(CreateSetCommand(nic.Name, preferred, alternate!));

                    Console.WriteLine(change
                        ? $"dns addresses have been changed to {preferred}, {alternate}"
                        : "there is a problem in change dns addresses!");

                    break;
                }
        }
    }

    [Command(CommandStatics.Lorem)]
    public void LoremIpsum(
        [Option(OptionStatics.Locale)] string? locale,
        [Option(OptionStatics.Word)] int? word,
        [Option(OptionStatics.Sentence)] int? sentence,
        [Option(OptionStatics.Paragraph)] int? paragraph)
    {
        var faker = new Faker();

        if (!string.IsNullOrWhiteSpace(locale))
            faker = new Faker(locale);

        if (paragraph != null)
        {
            if (sentence != null)
            {
                for (var i = 0; i < paragraph; i++)
                    Console.WriteLine(faker.Lorem.Paragraph((int)sentence));

                return;
            }

            Console.WriteLine(faker.Lorem.Paragraphs((int)paragraph));
            return;
        }

        if (sentence != null)
        {
            if (word != null)
            {
                for (var i = 0; i < sentence; i++)
                    Console.WriteLine(faker.Lorem.Sentence(word));

                return;
            }

            Console.WriteLine(faker.Lorem.Sentences(sentence));
            return;
        }

        if (word != null)
        {
            for (var i = 0; i < word; i++)
                Console.WriteLine(faker.Lorem.Word());

            return;
        }

        Console.WriteLine(faker.Lorem.Sentence());
    }
}