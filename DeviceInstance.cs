using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using PacketDotNet;
using PacketDotNet.Utils;
using SharpPcap;

namespace WakeOnLANServer;

public class DevicesInstance : IDisposable
{
    public List<ILiveDevice> Devices { get; private set; }
    private IConfiguration Configuration { get; }

    public DevicesInstance(IConfiguration configuration)
    {
        Devices = new List<ILiveDevice>();
        Configuration = configuration;
        OpenDevices();
    }

    public void OpenDevices()
    {
        var devices = (from device in CaptureDeviceList.Instance
                       where device.MacAddress != null
                       select device).ToArray();
        string? interfaceName = "";
        int i = 0;
        do
        {
            interfaceName = Configuration.GetSection($"WakeOnLanInterface:{i}").Value;
            if (interfaceName == null)
            {
                break;
            }
            else if (interfaceName == "*")
            {
                Devices = devices.ToList();
                break;
            }
            var selectDevice = from device in devices
                               where (device.Name == interfaceName)
                               select device;

            if (selectDevice.Any())
            {
                Devices.Add(selectDevice.First());
            }
            i += 1;
        } while (true);

        foreach (var item in Devices)
        {
            item.Open();
        }
        var notEthernet = (from device in devices
                           where (device.Name == interfaceName) && (device.LinkType != LinkLayers.Ethernet)
                           select device).ToArray();
        foreach (var item in notEthernet)
        {
            item.Close();
        }
        devices = (from device in devices
                   where (device.Name == interfaceName) && (device.LinkType == LinkLayers.Ethernet)
                   select device).ToArray();
    }

    public void Dispose()
    {
        foreach (var item in Devices)
        {
            item.Close();
        }
    }
}